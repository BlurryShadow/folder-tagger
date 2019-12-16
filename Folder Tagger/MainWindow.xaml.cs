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
        private int pagesCount = 1;
        List<List<Thumbnail>> thumbnailList = new List<List<Thumbnail>>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddFolder(string location, string name, string type = "multiple")
        {
            using (var db = new Model1())
            {
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

                if (db.Folders.Any(f => f.Location == location))
                {
                    if (type == "single")
                    {
                        System.Windows.MessageBox.Show("This Folder Has Already Been Added!");                       
                    }
                    return;
                }
                db.SaveChanges();
            }
        }

        private void Search()
        {
            string artist = cbBoxArtist.Text;
            string group = cbBoxGroup.Text;            
            string type = cbBoxType.Text;
            bool isTranslated = (bool)checkBTranslated.IsChecked;
            List<string> tagList = new List<string>();
            if (!string.IsNullOrWhiteSpace(tbTag.Text)) tagList = tbTag.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList();

            using (var db = new Model1())
            {                
                var query = db.Folders.Where(f => f.Translated == isTranslated);

                if (!string.IsNullOrWhiteSpace(artist))
                    query = query.Where(f => f.Artist == artist);

                if (!string.IsNullOrWhiteSpace(group))
                    query = query.Where(f => f.Group == group);

                if (!string.IsNullOrEmpty(type))
                    query = query.Where(f => f.Type == type);

                if (tagList.Any())
                    foreach (string tag in tagList)
                        query = query.Where(f => f.Tag.TagName.ToLower() == tag.ToLower());

                int foldersCount = query.GroupBy(f => f.Thumbnail).Count();
                pagesCount = (int)Math.Ceiling(((double)foldersCount / imagesPerPage));                

                for (int i = 0; i < pagesCount; i++)
                {
                    thumbnailList.Add(query
                                        .OrderBy(f => f.Name)
                                        .Skip(i * imagesPerPage)
                                        .Take(imagesPerPage)
                                        .Select(f => new Thumbnail { Root = f.Thumbnail, Name = f.Name })
                                        .Distinct()
                                        .ToList()
                    );
                }

                DataContext = thumbnailList.ElementAt(0);
            }
        }

        private void MenuAddFolder_Click(object sender, RoutedEventArgs e)
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

        private void MenuAddParentFolder_Click(object sender, RoutedEventArgs e)
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void btnFirstPage_Click(object sender, RoutedEventArgs e)
        {
            if (lblCurrentPage.Content.Equals("1")) return;

            lblCurrentPage.Content = "1";
            DataContext = thumbnailList.ElementAt(1 - 1);
            listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
        }

        private void btnPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (lblCurrentPage.Content.Equals("1")) return;

            int previousPage = Int32.Parse(lblCurrentPage.Content.ToString()) - 1;
            lblCurrentPage.Content = previousPage.ToString();
            DataContext = thumbnailList.ElementAt(previousPage - 1);
            listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (lblCurrentPage.Content.Equals(pagesCount.ToString())) return;

            int nextPage =  Int32.Parse(lblCurrentPage.Content.ToString()) + 1;
            lblCurrentPage.Content = nextPage.ToString();
            DataContext = thumbnailList.ElementAt(nextPage - 1);
            listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
        }

        private void btnLastPage_Click(object sender, RoutedEventArgs e)
        {
            if (lblCurrentPage.Content.Equals(pagesCount.ToString())) return;

            lblCurrentPage.Content = pagesCount.ToString();
            DataContext = thumbnailList.ElementAt(pagesCount - 1);
            listboxGallery.ScrollIntoView(listboxGallery.Items[0]);
        }
    }
}
