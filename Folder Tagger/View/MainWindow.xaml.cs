using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Folder_Tagger
{
    public partial class MainWindow : Window
    {
        private readonly string mangaReaderRoot = @"E:\Data\Programs\Basic Programs\Mangareader\mangareader.exe";
        private readonly int[] pageCapacity = { 60, 300, 600 };
        private int imagesPerPage;
        private int totalPages = 1;
        private int currentPage = 1;
        private int totalFolders;

        List<List<Thumbnail>> thumbnailsList = new List<List<Thumbnail>>();

        private readonly FolderController fc = new FolderController();
        public MainWindow()
        {
            InitializeComponent();
            SetConnectionString();
            cbBoxImagesPerPage.ItemsSource = pageCapacity;
            fc.RemoveNonexistentFolders();
            cbBoxArtist.ItemsSource = fc.GetArtists();
            cbBoxGroup.ItemsSource = fc.GetGroups();

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
            ContentRendered += (sender, e) => Search(null, null, null, new List<string>() { "no tag" });
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
            listboxGallery.ItemsSource = thumbnailsList.ElementAtOrDefault(0);
            ResetScroll();
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
            System.Windows.Controls.MenuItem menuItem = (System.Windows.Controls.MenuItem)sender;
            using(var fbd = new FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
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
            OpenFileDialog fd = new OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*json",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Metadata"
            };
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show(
                    "The process might take a long time, are you sure?",
                    "Import Folders Metadata",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question
                );
                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                    using (StreamReader sr = new StreamReader(fd.FileName))
                    {
                        string json = sr.ReadToEnd();
                        fc.ImportMetadata(json);
                        System.Windows.Forms.MessageBox.Show("Done");
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
            System.Windows.Forms.MessageBox.Show("Metadata Exported Successfully!");
        }

        private void MenuItemInfo_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menuItem = (System.Windows.Controls.MenuItem)sender;
            string type = menuItem.Name.Replace("miInfo", "");
            Window newWindow = new InfoWindow(type);
            newWindow.Closed += (newWindowSender, newWindowEvent) =>
            {
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

        private void MenuItemContextMenu_Clicked(object sender, RoutedEventArgs e)
        {
            List<string> locations = listboxGallery.SelectedItems.Cast<Thumbnail>().Select(th => th.Folder).ToList();
            if (locations.Count == 0) return;

            //From keyboard shortcut
            string menuItemType = sender.ToString();
            Window newWindow;

            //From mouse click
            if (sender.GetType().Equals(typeof(System.Windows.Controls.MenuItem)))
            {
                System.Windows.Controls.MenuItem menuItem = (System.Windows.Controls.MenuItem)sender;
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

        private void MenuItemOpenFolder_Clicked(object sender, RoutedEventArgs e)
        {
            //From keyboard shortcut
            string openIn = sender.ToString().Replace("miOpenFolderIn", "");
            //From mouse click
            if (sender.GetType().Equals(typeof(System.Windows.Controls.MenuItem)))
            {
                System.Windows.Controls.MenuItem menuItem = (System.Windows.Controls.MenuItem)sender;
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

        private void MenuItemDeleteFolder_Clicked(object sender, RoutedEventArgs e)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show(
                "Are you sure you want to delete these folders?",
                "Confirm",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                totalFolders -= listboxGallery.SelectedItems.Count;
                UpdateFoldersFoundTextBlock();
                List<string> deletedFolders = 
                    listboxGallery.SelectedItems.Cast<Thumbnail>().ToList()
                    .Select(th => th.Folder).ToList();

                fc.DeleteRealFolders(deletedFolders);
                thumbnailsList[currentPage - 1].RemoveAll(th => deletedFolders.Contains(th.Folder));

                //A whole page was deleted
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
        }

        private void TextBlockFolderName_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            System.Windows.Clipboard.SetText(textBlock.Text);
            textblockClipboard.Text = "Copied To Clipboard: " + textBlock.Text;
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

        private void ButtonSwitchPage_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;
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

        private void ComboBoxPageCapacity_SelectionChanged(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ComboBox comboBox = (System.Windows.Controls.ComboBox)sender;
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
    }
}
