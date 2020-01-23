using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class MainWindow : Window
    {
        private readonly string mangaReaderRoot = @"E:\Data\Programs\Basic Programs\Mangareader\mangareader.exe";
        private readonly int[] pageCapacity = { 60, 300, 600 };
        private readonly int numberCharactersToActiveSuggestion = 3;
        private int imagesPerPage;
        private int totalPages = 1;
        private int currentPage = 1;
        private int totalFolders;

        List<List<Thumbnail>> thumbnailsList = new List<List<Thumbnail>>();
        List<Tag> allTags;

        private readonly FolderController fc = new FolderController();
        private readonly TagController tc = new TagController();
        public MainWindow()
        {
            InitializeComponent();
            SetConnectionString();
            listboxAutoComplete.PreviewMouseUp += (sender, e) => ApplySuggestion();
            cbBoxImagesPerPage.ItemsSource = pageCapacity;
            fc.RemoveNonexistentFolders();

            //For shortcut binding
            commandbindingAddOneFolder.Executed += (sender, e) => MenuItemAddFolder_Clicked(miAddOneFolder, new RoutedEventArgs());
            commandbindingAddManyFolders.Executed += (sender, e) => MenuItemAddFolder_Clicked(miAddManyFolders, new RoutedEventArgs());
            commandbindingOpenFolder.Executed += (sender, e) => MenuItemOpenFolder_Clicked("miOpenFolderInExplorer", new RoutedEventArgs());
            commandbindingOpenInMangareader.Executed += (sender, e) => MenuItemOpenFolder_Clicked("miOpenFolderInMangareader", new RoutedEventArgs());
            commandbindingAddTag.Executed += (sender, e) => MenuItemContextMenu_Clicked("miAddTag", new RoutedEventArgs());
            commandbindingEditArtist.Executed += (sender, e) => MenuItemContextMenu_Clicked("miEditArtist", new RoutedEventArgs());
            commandbindingEditGroup.Executed += (sender, e) => MenuItemContextMenu_Clicked("miEditGroup", new RoutedEventArgs());
            commandbindingEditTag.Executed += (sender, e) => MenuItemContextMenu_Clicked("miEditTag", new RoutedEventArgs());
            commandbindingRemoveTag.Executed += (sender, e) => MenuItemContextMenu_Clicked("miRemoveTag", new RoutedEventArgs());

            Loaded += (sender, e) => tbName.Focus();
            ContentRendered += (sender, e) =>
            {
                Search(null, null, null, new List<string>() { "no tag" });
                cbBoxArtist.ItemsSource = fc.GetArtists();
                cbBoxGroup.ItemsSource = fc.GetGroups();
                allTags = tc.GetTags("all", "mostUsed");
            };
        }

        private void SetConnectionString()
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string newConnectionString = @"data source=(LocalDB)\MSSQLLocalDB;AttachDbFilename="
                + currentPath
                + @"Database\Database1.mdf;Integrated Security=True;MultipleActiveResultSets=True;App=EntityFramework";
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringsSection = config.ConnectionStrings;
            connectionStringsSection.ConnectionStrings["Model1"].ConnectionString = newConnectionString;
            config.Save();
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        private void Search(string artist = null, string group = null, string name = null, List<string> tagList = null)
        {
            currentPage = 1;
            thumbnailsList = fc.SearchFolders(artist, group, name, tagList, imagesPerPage);
            totalPages = thumbnailsList != null ? thumbnailsList.Count() : 1;
            totalFolders = thumbnailsList != null ? thumbnailsList.Sum(th => th.Count) : 0;
            UpdateCurrentPageTextBlock();
            UpdateFoldersFoundTextBlock();
            listboxGallery.ItemsSource = thumbnailsList?.ElementAtOrDefault(0);
            ResetScroll();
        }

        private void ApplySuggestion()
        {
            int indexPosition = tbTag.Text.LastIndexOf(", ");
            bool hasExcludeMark;
            //We don't need to check string null nor empty because to trigger this method
            //Suggestion Box must be visible, which is only when the string in Tag Text Box has more than 2 characters
            //indexPosition == -1 means TextBox does not have any tag yet
            if (indexPosition == -1)
                hasExcludeMark = (tbTag.Text[0] == '-') ? true : false;
            else
                hasExcludeMark = (tbTag.Text[indexPosition + 2] == '-') ? true : false;
            try
            {
                Tag t = listboxAutoComplete.SelectedItem as Tag;
                if (hasExcludeMark)
                    tbTag.Text = (indexPosition == -1) ? '-' + t.TagName : tbTag.Text.Substring(0, indexPosition) + ", -" + t.TagName;
                else
                    tbTag.Text = (indexPosition == -1) ? t.TagName : tbTag.Text.Substring(0, indexPosition) + ", " + t.TagName;
                tbTag.Text += ", ";
                tbTag.CaretIndex = tbTag.Text.Length;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            tbTag.Focus();
        }

        private void UpdateCurrentPageTextBlock()
        {
            textblockCurrentPage.Text = totalPages > 1 ? currentPage + ".." + totalPages : "1";
        }

        private void UpdateFoldersFoundTextBlock()
        {
            textblockTotalFolder.Text = "Folders Found: " + totalFolders;
        }

        private void ResetScroll()
        {
            if (listboxGallery.Items.Count > 0)
                listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
        }

        private void MenuItemAddFolder_Clicked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            using(var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string location = fbd.SelectedPath;
                    if (menuItem.Name == "miAddOneFolder")
                    {
                        string name = Path.GetFileName(location);
                        fc.AddFolder(location, name, true);
                    } else
                    {
                        DirectoryInfo parentFolder = new DirectoryInfo(location);
                        foreach (DirectoryInfo subFolder in parentFolder.GetDirectories())
                            fc.AddFolder(subFolder.FullName, subFolder.Name, false);
                    }                    
                    Search(null, null, null, new List<string>() { "no tag" });
                }
            }
        }

        private void MenuItemImportMetadata_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fd = new System.Windows.Forms.OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*json",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Metadata"
            };
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MessageBoxResult mbr = MessageBox.Show(
                    "The process might take a long time, are you sure?",
                    "Import Folders Metadata",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );
                if (mbr == MessageBoxResult.Yes)
                    using (StreamReader sr = new StreamReader(fd.FileName))
                    {
                        string json = sr.ReadToEnd();
                        fc.ImportMetadata(json);
                        MessageBox.Show("Done");
                    }
            }
        }

        private void MenuItemExportMetadata_Clicked(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory("Metadata");
            using (var db = new Model1())
            {
                var json = JsonConvert.SerializeObject(fc.GetMetadataToExport(db), Formatting.Indented);
                string newJSON = "Metadata " + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".json";
                using (var sw = File.AppendText(@"Metadata\" + newJSON))
                    sw.Write(json);
            }
            MessageBox.Show("Metadata Exported Successfully!");
        }

        private void MenuItemInfo_Clicked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            string type = menuItem.Name.Replace("miInfo", "");
            Window newWindow = new InfoWindow(type);
            newWindow.Closed += (newWindowSender, newWindowEvent) =>
            {
                if (type == "Tag")
                    allTags = tc.GetTags("all", "mostUsed");
                if (type == "Artist")
                    cbBoxArtist.ItemsSource = fc.GetArtists();
                if (type == "Group")
                    cbBoxGroup.ItemsSource = fc.GetGroups();
            };
            newWindow.Owner = App.Current.MainWindow;
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            newWindow.ShowInTaskbar = false;
            newWindow.ShowDialog();
        }

        private void MenuItemHelp_Clicked(object sender, RoutedEventArgs e)
        {
            string str = "Search Tag:\n" +
                "  + \"cheerleader\" to include cheerleader tag\n" +
                "  + \"-nurse\" to exlcude nurse tag\n" +
                "  + \"no tag\" to search folders that have no tags\n" +
                "  + \"no artist\" to search folders that do not have artists\n" +
                "  + \"no group\" to search folders that do not have groups\n" +
                "  + \"no artist no group\" or \"no group no artist\" to search\n" +
                "       folders that do not have arists nor groups\n\n" +
                "Remove Tag:\n" +
                "  + \"everything\" to remove all tags from selected folders\n";
            MessageBox.Show(str, "Special Code", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TextBoxTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbTag.Text))
            {
                //The last part of TextBox Text, behind ', '
                //Not the whole TextBox string
                //Nor where the cursor is located at
                //Example: "a1, a2, a3|, a4, a5"    | = cursor   currentTag = a5
                string currentTag = tbTag.Text.Split(new string[] { ", " }, StringSplitOptions.None).Last().Replace("-", "").ToLower().Trim();
                if (string.IsNullOrEmpty(currentTag) || currentTag.Length < numberCharactersToActiveSuggestion)
                {
                    popupAutoComplete.IsOpen = false;
                    return;
                }

                var suggestedTags = allTags.Where(t => t.TagName.Contains(currentTag)).ToList();
                int suggestedTagCount = suggestedTags.Count;
                if (suggestedTagCount == 0)
                {
                    popupAutoComplete.IsOpen = false;
                    return;
                }

                popupAutoComplete.Height = (suggestedTagCount > 10) ? 250 : suggestedTagCount * 25 + 10;
                listboxAutoComplete.ItemsSource = suggestedTags;
                popupAutoComplete.IsOpen = true;
            }
        }

        private void TextBoxTag_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Jump to suggestion box when press Down or Up Key
            if (popupAutoComplete.IsOpen)
            {
                if (e.Key == Key.Down)
                    listboxAutoComplete.SelectedIndex = 0;
                if (e.Key == Key.Up)
                    listboxAutoComplete.SelectedIndex = listboxAutoComplete.Items.Count - 1;
                if (e.Key == Key.Down || e.Key == Key.Up)
                {
                    var item = (ListBoxItem)listboxAutoComplete.ItemContainerGenerator.ContainerFromItem(listboxAutoComplete.SelectedItem);
                    item.Focus();
                    //Prevent focus from jumping to next element
                    e.Handled = true;
                }
            }

            if (e.Key == Key.OemComma)
            {
                tbTag.Text += ", ";
                e.Handled = true;
                tbTag.CaretIndex = tbTag.Text.Length;
            }
        }

        private void ListBoxAutoComplete_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Jump back to TextBox when press Up Key at first element
            if (e.Key == Key.Up && listboxAutoComplete.SelectedIndex == 0)
                tbTag.Focus();

            //Return to first element when pressed at last one
            //Trying to mimic S Key default behavior
            if (e.Key == Key.Down && listboxAutoComplete.SelectedIndex == listboxAutoComplete.Items.Count - 1)
            {
                var item = (ListBoxItem)listboxAutoComplete.ItemContainerGenerator.ContainerFromItem(listboxAutoComplete.Items[0]);
                item.Focus();
                e.Handled = true;
            }

            //Disable 'S' key default event
            //Which is the same as Down key
            //But return to first element when pressed at last one
            if (e.Key == Key.S)
                e.Handled = true;

            if (e.Key == Key.Enter)
            {
                ApplySuggestion();
                //Add Button is default
                //Cancel Enter Key Down event to prevent premature trigger
                e.Handled = true;
            }
        }

        private void ButtonSearch_Clicked(object sender, RoutedEventArgs e)
        {
            string artist = cbBoxArtist.Text;
            string group = cbBoxGroup.Text;
            string name = tbName.Text;
            List<string> tagList = new List<string>();
            if (!string.IsNullOrWhiteSpace(tbTag.Text)) tagList =
                    tbTag.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList();
            Search(artist, group, name, tagList);
        }

        private void ComboBoxPageCapacity_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            imagesPerPage = pageCapacity[comboBox.SelectedIndex];
            if (totalFolders == 0)
                return;
            currentPage = 1;
            totalPages = (int)Math.Ceiling(((double)totalFolders / imagesPerPage));

            List<Thumbnail> newThumbnails = new List<Thumbnail>();
            foreach (List<Thumbnail> thumbnails in thumbnailsList)
                newThumbnails.AddRange(thumbnails);

            thumbnailsList.Clear();
            for (int i = 0; i < totalPages; i++)
                thumbnailsList.Add(
                    newThumbnails
                        .Skip(i * imagesPerPage)
                        .Take(imagesPerPage)
                        .ToList()
                );
            listboxGallery.ItemsSource = thumbnailsList.ElementAtOrDefault(0);
            UpdateCurrentPageTextBlock();
        }

        private void MenuItemOpenFolder_Clicked(object sender, RoutedEventArgs e)
        {
            //From keyboard shortcut
            string openIn = sender.ToString().Replace("miOpenFolderIn", "");
            //From mouse click
            if (sender.GetType().Equals(typeof(MenuItem)))
            {
                MenuItem menuItem = (MenuItem)sender;
                openIn = menuItem.Name.Replace("miOpenFolderIn", "");
            }

            int selectedThumbnailsCount = listboxGallery.SelectedItems.Count;
            if (selectedThumbnailsCount == 0)
                return;
            Thumbnail selectedThumbnail = listboxGallery.SelectedItems[selectedThumbnailsCount - 1] as Thumbnail;
            string location = selectedThumbnail.Folder;

            if (openIn == "Explorer")
                Process.Start(location);
            if (openIn == "Mangareader")
                Process.Start(mangaReaderRoot, '\"' + location + '\"');
        }

        private void MenuItemContextMenu_Clicked(object sender, RoutedEventArgs e)
        {
            List<string> locations = listboxGallery.SelectedItems.Cast<Thumbnail>().Select(th => th.Folder).ToList();
            if (locations.Count == 0) return;

            //From keyboard shortcut
            string menuItemType = sender.ToString();
            Window newWindow;

            //From mouse click
            if (sender.GetType().Equals(typeof(MenuItem)))
            {
                MenuItem menuItem = (MenuItem)sender;
                menuItemType = menuItem.Name;
            }

            string type = menuItemType.Replace("miEdit", "");
            switch (menuItemType)
            {
                case "miEditArtist":
                case "miEditGroup":
                    newWindow = new SmallEditWindow(type, locations);
                    newWindow.Closed += (newWindowSender, newWindowEvent) =>
                    {
                        if (type == "Artist")
                            cbBoxArtist.ItemsSource = fc.GetArtists();
                        if (type == "Group")
                            cbBoxGroup.ItemsSource = fc.GetGroups();
                    };
                    break;
                case "miEditTag":
                    newWindow = new FullEditWindow(locations.Last());
                    break;
                default: //Add Tag
                    newWindow = new AddTagWindow(locations);
                    newWindow.Closed += (newWindowSender, newWindowEvent) => allTags = tc.GetTags("all", "mostUsed");
                    break;
                case "miRemoveTag":
                    newWindow = new RemoveTagWindow(locations);
                    break;
            }

            newWindow.Owner = App.Current.MainWindow;
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            newWindow.ShowInTaskbar = false;
            newWindow.ShowDialog();
        }

        private void MenuItemRemoveFolder_Clicked(object sender, RoutedEventArgs e)
        {
            totalFolders -= listboxGallery.SelectedItems.Count;
            UpdateFoldersFoundTextBlock();
            List<string> removedFolders = 
                listboxGallery.SelectedItems.Cast<Thumbnail>().ToList()
                .Select(th => th.Folder).ToList();

            fc.RemoveFolder(removedFolders);
            thumbnailsList[currentPage - 1].RemoveAll(th => removedFolders.Contains(th.Folder));

            //A whole page was removed
            if (thumbnailsList[currentPage - 1].Count == 0)
            {
                thumbnailsList.RemoveAll(th => th.Count == 0);
                thumbnailsList.TrimExcess();
                currentPage = 1;
                totalPages = thumbnailsList.Count() == 0 ? 1 : thumbnailsList.Count();
                UpdateCurrentPageTextBlock();
                listboxGallery.ItemsSource = thumbnailsList.ElementAtOrDefault(0);
                ResetScroll();
            }
            else
                listboxGallery.Items.Refresh();
        }

        private void TextBlockFolderName_Clicked(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            Clipboard.SetText(textBlock.Text);
            textblockClipboard.Text = "Copied To Clipboard: " + textBlock.Text;
        }

        private void ButtonSwitchPage_Clicked(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if ((button.Name == "btnFirstPage" || button.Name == "btnPreviousPage") && currentPage == 1)
                return;
            if ((button.Name == "btnLastPage" || button.Name == "btnNextPage") && currentPage == totalPages)
                return;

            switch (button.Name)
            {
                case "btnFirstPage":
                    currentPage = 1;
                    break;
                case "btnPreviousPage":
                    currentPage--;
                    break;
                case "btnNextPage":
                    currentPage++;
                    break;
                case "btnLastPage":
                    currentPage = totalPages;
                    break;
            }
            listboxGallery.ItemsSource = thumbnailsList[currentPage - 1];
            UpdateCurrentPageTextBlock();
            ResetScroll();
        }
    }
}
