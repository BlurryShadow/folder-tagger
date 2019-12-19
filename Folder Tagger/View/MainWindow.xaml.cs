using System;
using System.Windows;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace Folder_Tagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private int imagesPerPage = 30;
        private int maxPage = 1;
        private int currentPage = 1;
        List<List<Thumbnail>> thumbnailList = new List<List<Thumbnail>>();
        public MainWindow()
        {
            InitializeComponent();
            GenerateAGList();
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

        private void AddFolder(string location, string name, string type = "multiple")
        {
            using (var db = new Model1())
            {
                if (db.Folders.Any(f => f.Location == location))
                {
                    if (type == "single")
                    {
                        System.Windows.MessageBox.Show("This Folder Has Already Been Added!");
                    }
                    return;
                }

                string thumbnail = null;
                DirectoryInfo dr = new DirectoryInfo(location);
                thumbnail = dr.EnumerateFiles()
                            .Select(t => t.FullName)
                            .FirstOrDefault(FullName => (FullName.ToLower() == "folder.jpg")
                                                     || (FullName.ToLower().Contains(".png"))
                                                     || (FullName.ToLower().Contains(".jpg"))
                                                     || (FullName.ToLower().Contains(".jpeg"))
                                                     || (FullName.ToLower().Contains(".bmp")));

                var folder = new Folder(location, name, thumbnail);
                db.Folders.Add(folder);                
                db.SaveChanges();
            }
        }

        private void Search()
        {
            thumbnailList.Clear();
            string artist = cbBoxArtist.Text;
            string group = cbBoxGroup.Text;
            List<string> tagList = new List<string>();
            if (!string.IsNullOrWhiteSpace(tbTag.Text)) tagList = tbTag.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList();

            using (var db = new Model1())
            {                
                var query = (System.Linq.IQueryable<Folder>)db.Folders;

                if (!string.IsNullOrWhiteSpace(artist))
                    query = query.Where(f => f.Artist == artist);

                if (!string.IsNullOrWhiteSpace(group))
                    query = query.Where(f => f.Group == group);

                if (tagList.Any())
                    foreach (string tag in tagList)
                        query = query.Where(f => f.Tag.TagName.ToLower() == tag.ToLower());

                int foldersCount = query.GroupBy(f => f.Thumbnail).Count();
                maxPage = (int)Math.Ceiling(((double)foldersCount / imagesPerPage));                

                for (int i = 0; i < maxPage; i++)
                {
                    thumbnailList.Add(query
                                        .OrderBy(f => f.Name)
                                        .Skip(i * imagesPerPage)
                                        .Take(imagesPerPage)
                                        .Select(f => new Thumbnail 
                                        { 
                                            Folder = f.Location, 
                                            Root = f.Thumbnail, 
                                            Name = f.Name 
                                        })
                                        .Distinct()
                                        .ToList()
                    );
                }

                if (thumbnailList.Count > 0)
                    DataContext = thumbnailList.ElementAt(0);
            }
        }

        private void AddOneFolder(object sender, RoutedEventArgs e)
        {
            using(var fbd = new FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string location = fbd.SelectedPath;
                    string name = Path.GetFileName(location);
                    AddFolder(location, name, "single");
                }
            }
        }

        private void AddManyFolders(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string location = fbd.SelectedPath;
                    DirectoryInfo parentFolder = new DirectoryInfo(location);
                    foreach (DirectoryInfo subFolder in parentFolder.GetDirectories())
                    {
                        AddFolder(subFolder.FullName, subFolder.Name, "multiple");
                    }                    
                }
            }
        }

        private void SearchFolders(object sender, RoutedEventArgs e)
        {
            Search();
            currentPage = 1;
            if (maxPage > 1)
                lblCurrentPage.Content = currentPage + ".." + maxPage;
        }

        private void ToFirstPage(object sender, RoutedEventArgs e)
        {
            if (currentPage == 1) return;

            currentPage = 1;
            lblCurrentPage.Content = currentPage + ".." + maxPage;
            DataContext = thumbnailList.ElementAt(1 - 1);
            listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
        }

        private void ToPreviousPage(object sender, RoutedEventArgs e)
        {
            if (currentPage == 1) return;

            currentPage--;
            lblCurrentPage.Content = currentPage + ".." + maxPage;
            DataContext = thumbnailList.ElementAt(currentPage - 1);
            listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
        }

        private void ToNextPage(object sender, RoutedEventArgs e)
        {
            if (currentPage == maxPage) return;

            currentPage++;
            lblCurrentPage.Content = currentPage + ".." + maxPage;
            DataContext = thumbnailList.ElementAt(currentPage - 1);
            listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
        }

        private void ToLastPage(object sender, RoutedEventArgs e)
        {
            if (currentPage == maxPage) return;

            currentPage = maxPage;
            lblCurrentPage.Content = currentPage + ".." + maxPage;
            DataContext = thumbnailList.ElementAt(maxPage - 1);
            listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
        }

        private void OpenSmallEditWindow(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem itemClicked = (System.Windows.Controls.MenuItem)sender;
            string type = itemClicked.Header.ToString().Replace("Edit ", "");
            string folder = itemClicked.Tag.ToString();
            Window smallEditWindow = new SmallEditWindow(type, folder);
            smallEditWindow.Owner = App.Current.MainWindow;
            smallEditWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            smallEditWindow.ShowInTaskbar = false;
            smallEditWindow.Closed += SmallEditWindow_Closed;
            smallEditWindow.ShowDialog();            
        }

        private void SmallEditWindow_Closed(object sender, EventArgs e)
        {
            GenerateAGList();
        }

        private void OpenFullEditWindow(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem itemClicked = (System.Windows.Controls.MenuItem)sender;
            string folder = itemClicked.Tag.ToString();
            Window fullEditWindow = new FullEditWindow(folder);
            fullEditWindow.Owner = App.Current.MainWindow;
            fullEditWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            fullEditWindow.ShowInTaskbar = false;
            fullEditWindow.ShowDialog();
        }
    }
}
