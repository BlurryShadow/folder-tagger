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

        private int imagesPerPage = 50;
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
                int foldersCount = db.Folders.GroupBy(f => f.Thumbnail).Count();
                int pageCount = (int)Math.Ceiling(((double)foldersCount / imagesPerPage));
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

                var thumbnailList = takeDataOffset(query, foldersCount, 1);
                DataContext = thumbnailList;
            }
        }

        private dynamic takeDataOffset(System.Linq.IQueryable<Folder> query, int foldersCount, int page)
        {
            return query
                .OrderBy(f => f.Name)
                .Skip((page - 1) * imagesPerPage)
                .Take(imagesPerPage)
                .Distinct()
                .ToList();
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
    }
}
