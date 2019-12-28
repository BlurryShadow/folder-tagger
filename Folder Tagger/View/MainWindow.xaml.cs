using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class MainWindow : Window
    {
        private int[] pageCapacity = { 60, 300, 600 };
        private int imagesPerPage;
        private int maxPage = 1;
        private int currentPage = 1;
        private int totalFolders;
        List<List<Thumbnail>> thumbnailList = new List<List<Thumbnail>>();

        private readonly FolderController fc = new FolderController();
        public MainWindow()
        {
            InitializeComponent();
            cbBoxImagesPerPage.ItemsSource = pageCapacity;
            fc.RemoveNonexistFolder();
            cbBoxArtist.ItemsSource = fc.GetArtistList();
            cbBoxGroup.ItemsSource = fc.GetGroupList();
            commandbindingAddOneFolder.Executed += (sender, e) => MenuItemAddFolder_Clicked(miAddOneFolder, new RoutedEventArgs());
            commandbindingAddManyFolders.Executed += (sender, e) => MenuItemAddFolder_Clicked(miAddManyFolders, new RoutedEventArgs());
            commandbindingOpenFolder.Executed += (sender, e) => MenuItemOpenFolder_Clicked(null, new RoutedEventArgs());
            commandbindingOpenInMangareader.Executed += (sender, e) => MenuItemOpenInMangareader_Clicked(null, new RoutedEventArgs());
            commandbindingAddTag.Executed += (sender, e) => MenuItemOpenWindow_Clicked("miAddTag", new RoutedEventArgs());
            commandbindingEditArtist.Executed += (sender, e) => MenuItemOpenWindow_Clicked("miEditArtist", new RoutedEventArgs());
            commandbindingEditGroup.Executed += (sender, e) => MenuItemOpenWindow_Clicked("miEditGroup", new RoutedEventArgs());
            commandbindingEditTag.Executed += (sender, e) => MenuItemOpenWindow_Clicked("miEditTag", new RoutedEventArgs());
            commandbindingRemoveTag.Executed += (sender, e) => MenuItemOpenWindow_Clicked("miRemoveTag", new RoutedEventArgs());

            Loaded += (sender, e) => tbName.Focus();
            ContentRendered += (sender, e) => Search(null, null, null, new List<string>() { "no tag" });
        }

        private void Search(string artist = null, string group = null, string name = null, List<string> tagList = null)
        {
            currentPage = 1;
            thumbnailList = fc.SearchFolder(artist, group, name, tagList, imagesPerPage);
            maxPage = thumbnailList != null ? thumbnailList.Count() : 1;
            totalFolders = thumbnailList != null ? thumbnailList.Sum(th => th.Count) : 0;
            ChangeCurrentPageTextBlock();
            ChangeFolderFoundTextBlock();
            DataContext = thumbnailList?.ElementAt(0);
            ResetScroll();
        }

        private void ChangeCurrentPageTextBlock()
        {
            textblockCurrentPage.Text = maxPage > 1 ? currentPage + ".." + maxPage : "1";
        }

        private void ChangeFolderFoundTextBlock()
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
                        fc.AddFolder(location, name, "single");
                    } else
                    {
                        DirectoryInfo parentFolder = new DirectoryInfo(location);
                        foreach (DirectoryInfo subFolder in parentFolder.GetDirectories())
                            fc.AddFolder(subFolder.FullName, subFolder.Name, "multiple");
                    }                    
                    Search(null, null, null, new List<string>() { "no tag" });
                }
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

        private void ButtonSwitchPage_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;

            if ((button.Name == "btnFirstPage" || button.Name == "btnPreviousPage") && currentPage == 1)
                return;

            if ((button.Name == "btnLastPage" || button.Name == "btnNextPage") && currentPage == maxPage)
                return;

            switch (button.Name)
            {
                case "btnFirstPage":
                    currentPage = 1;
                    DataContext = thumbnailList.ElementAt(0);
                    break;
                case "btnPreviousPage":
                    currentPage--;
                    DataContext = thumbnailList.ElementAt(currentPage - 1);
                    break;
                case "btnNextPage":
                    currentPage++;
                    DataContext = thumbnailList.ElementAt(currentPage - 1);
                    break;
                case "btnLastPage":
                    currentPage = maxPage;
                    DataContext = thumbnailList.ElementAt(maxPage - 1);
                    break;
            }

            ChangeCurrentPageTextBlock();
            ResetScroll();
        }

        private void MenuItemOpenWindow_Clicked(object sender, RoutedEventArgs e)
        {
            List<string> folderList = listboxGallery.SelectedItems.Cast<Thumbnail>().Select(th => th.Folder).ToList();
            if (folderList.Count == 0) return;

            string target = sender.ToString();
            string folder = folderList.Last();
            Window newWindow;

            if (sender.GetType().Equals(typeof(System.Windows.Controls.MenuItem)))
            {
                System.Windows.Controls.MenuItem menuItem = (System.Windows.Controls.MenuItem)sender;
                target = menuItem.Name;
                folder = menuItem.Tag?.ToString();
            }

            string type = target.Replace("miEdit", "");

            switch (target)
            {
                case "miEditArtist":
                case "miEditGroup":
                    newWindow = new SmallEditWindow(type, folder);
                    newWindow.Closed += (newWindowSender, newWindowEvent) =>
                    {
                        if (type == "Artist")
                            cbBoxArtist.ItemsSource = fc.GetArtistList();
                        else
                            cbBoxGroup.ItemsSource = fc.GetGroupList();
                    };
                    break;
                case "miEditTag":
                    newWindow = new FullEditWindow(folder);
                    break;
                default: //Add Tag
                    newWindow = new AddTagWindow(folderList);
                    break;
                case "miRemoveTag":
                    newWindow = new RemoveTagWindow(folderList);
                    break;
            }

            newWindow.Owner = App.Current.MainWindow;
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            newWindow.ShowInTaskbar = false;
            newWindow.ShowDialog();
        }

        private void MenuItemOpenFolder_Clicked(object sender, RoutedEventArgs e)
        {
            string folder;
            if (sender != null)
            {
                System.Windows.Controls.MenuItem itemClicked = (System.Windows.Controls.MenuItem)sender;
                folder = itemClicked.Tag.ToString();
            } else
            {
                int selectedItemsCount = listboxGallery.SelectedItems.Count;
                if (selectedItemsCount == 0) return;
                Thumbnail selectedThumbnail = listboxGallery.SelectedItems[selectedItemsCount - 1] as Thumbnail;
                folder = selectedThumbnail.Folder;
            }
            Process.Start(folder);
        }

        private void MenuItemOpenInMangareader_Clicked(object sender, RoutedEventArgs e)
        {
            string folder;
            string mangaReaderRoot = @"E:\Data\Programs\Basic Programs\Mangareader\mangareader.exe";
            if (sender != null)
            {
                System.Windows.Controls.MenuItem itemClicked = (System.Windows.Controls.MenuItem)sender;
                folder = itemClicked.Tag.ToString();
            } else
            {
                int selectedItemsCount = listboxGallery.SelectedItems.Count;
                if (selectedItemsCount == 0) return;
                Thumbnail selectedThumbnail = listboxGallery.SelectedItems[selectedItemsCount - 1] as Thumbnail;
                folder = selectedThumbnail.Folder;
            }            
            Process.Start(mangaReaderRoot, '\"' + folder + '\"');
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
                ChangeFolderFoundTextBlock();
                List<string> deletedFolderList = 
                    listboxGallery.SelectedItems.Cast<Thumbnail>().ToList()
                    .Select(th => th.Folder).ToList();

                fc.DeleteRealFolder(deletedFolderList);
                thumbnailList.ElementAt(currentPage - 1).RemoveAll(th => deletedFolderList.Contains(th.Folder));

                if (thumbnailList.ElementAt(currentPage - 1).Count == 0)
                {
                    thumbnailList.RemoveAll(th => th.Count == 0);
                    thumbnailList.TrimExcess();
                    currentPage = 1;
                    maxPage = thumbnailList.Count() == 0 ? 1 : thumbnailList.Count();
                    ChangeCurrentPageTextBlock();
                    DataContext = thumbnailList.ElementAtOrDefault(0);
                    ResetScroll();
                }
                else
                    listboxGallery.Items.Refresh();
            }
        }

        private void ComboBoxPageCapacity_SelectionChanged(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ComboBox comboBox = (System.Windows.Controls.ComboBox)sender;
            imagesPerPage = pageCapacity[comboBox.SelectedIndex];
            if (totalFolders == 0) return;
            currentPage = 1;
            maxPage = (int)Math.Ceiling(((double)totalFolders / imagesPerPage));

            List<Thumbnail> newSubThumbnailList = new List<Thumbnail>();
            foreach (List<Thumbnail> subThumbnailList in thumbnailList)
                newSubThumbnailList.AddRange(subThumbnailList);

            thumbnailList.Clear();
            for (int i = 0; i < maxPage; i++)
                thumbnailList.Add(
                    newSubThumbnailList
                        .Skip(i * imagesPerPage)
                        .Take(imagesPerPage)
                        .ToList()
                );

            DataContext = thumbnailList.ElementAtOrDefault(0);
            ChangeCurrentPageTextBlock();
        }
    }
}
