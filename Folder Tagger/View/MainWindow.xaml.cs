using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

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
            GenerateAGList();            

            Loaded += (sender, e) => tbName.Focus();
            ContentRendered += (sender, e) => Search(null, null, null, new List<string>() { "no tag" });
        }

        private void GenerateAGList()
        {
            using (var db = new Model1())
            {
                var artistList = db.Folders
                    .OrderBy(f => f.Artist)
                    .Where(f => f.Artist != null)
                    .Select(f => f.Artist)
                    .Distinct()
                    .ToList();
                var groupList = db.Folders
                    .OrderBy(f => f.Group)
                    .Where(f => f.Group != null)
                    .Select(f => f.Group)
                    .Distinct()
                    .ToList();

                artistList.Insert(0, "");
                groupList.Insert(0, "");

                cbBoxArtist.ItemsSource = artistList;
                cbBoxGroup.ItemsSource = groupList;
            }
        }

        private void Search(string artist = null, string group = null, string name = null, List<string> tagList = null)
        {
            thumbnailList.Clear();
            currentPage = 1;

            using (var db = new Model1())
            {
                var query = (System.Linq.IQueryable<Folder>)db.Folders;

                if (!string.IsNullOrWhiteSpace(artist))
                    query = query.Where(f => f.Artist == artist);

                if (!string.IsNullOrWhiteSpace(group))
                    query = query.Where(f => f.Group == group);

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(f => f.Name.Contains(name));

                if (tagList.Count() > 0)
                {
                    if (tagList.ElementAt(0).Trim().ToLower().Equals("no tag"))
                        query = query.Where(f => f.Tags.Count() == 0);
                    else
                        foreach (string tag in tagList)
                        {
                            string currentTag = tag.Trim().ToLower();
                            query = query.Where(f => f.Tags.Any(t => t.TagName == currentTag));
                        }                            
                }

                totalFolders = query.Count();
                maxPage = (int)Math.Ceiling(((double)totalFolders / imagesPerPage));
                ChangeCurrentPageTextBlock();
                ChangeFolderFoundTextBlock();

                if (maxPage < 1)
                {
                    DataContext = null;                    
                    maxPage = 1;
                    return;
                }

                for (int i = 0; i < maxPage; i++)
                    thumbnailList.Add(
                        query
                            .OrderBy(f => f.Name)
                            .Skip(i * imagesPerPage)
                            .Take(imagesPerPage)
                            .Select(f => new Thumbnail
                            {
                                Folder = f.Location,
                                Root = f.Thumbnail,
                                Name = f.Name
                            })
                            .ToList()
                    );

                DataContext = thumbnailList.ElementAt(0);
                listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
            }
        }

        private void ChangeCurrentPageTextBlock()
        {
            textblockCurrentPage.Text = maxPage > 1 ? currentPage + ".." + maxPage : "1";
        }

        private void ChangeFolderFoundTextBlock()
        {
            textblockTotalFolder.Text = "Folders Found: " + totalFolders;
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
            listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
        }

        private void MenuItemOpenWindow_Clicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menuItem = (System.Windows.Controls.MenuItem)sender;
            string folder;
            List<string> folderList = new List<string>();
            Window newWindow;

            switch (menuItem.Name)
            {
                case "miEditArtist":
                case "miEditGroup":
                    folder = menuItem.Tag.ToString();
                    string type = menuItem.Header.ToString().Replace("Edit ", "");
                    newWindow = new SmallEditWindow(type, folder);
                    newWindow.Closed += (newWindowSender, newWindowEvent) => GenerateAGList();
                    break;
                case "miEditTag":
                    folder = menuItem.Tag.ToString();
                    newWindow = new FullEditWindow(folder);
                    break;
                default: //Add Tag
                    foreach (Thumbnail t in listboxGallery.SelectedItems)
                        folderList.Add(t.Folder);
                    newWindow = new AddTagWindow(folderList);
                    break;
                case "miRemoveTag":
                    foreach (Thumbnail t in listboxGallery.SelectedItems)
                        folderList.Add(t.Folder);
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
            System.Windows.Controls.MenuItem itemClicked = (System.Windows.Controls.MenuItem)sender;
            string folder = itemClicked.Tag.ToString();
            Process.Start(folder);
        }

        private void MenuItemOpenInMangareader_Clicked(object sender, RoutedEventArgs e)
        {
            string mangaReaderRoot = @"E:\Data\Programs\Basic Programs\Mangareader\mangareader.exe";
            System.Windows.Controls.MenuItem itemClicked = (System.Windows.Controls.MenuItem)sender;
            string folder = itemClicked.Tag.ToString();
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
                    DataContext = thumbnailList.Count == 0 ? null : thumbnailList.ElementAt(0);
                    if (listboxGallery.Items.Count > 0)
                        listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
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

            DataContext = thumbnailList.ElementAt(0);
            ChangeCurrentPageTextBlock();
        }
    }
}
