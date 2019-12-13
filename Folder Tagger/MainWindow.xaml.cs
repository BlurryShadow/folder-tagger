using System;
using System.Windows;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace Folder_Tagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddFolder(string location, string type = "multiple")
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

                var folder = new Folder(location, thumbnail);
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

        private void MenuAddFolder_Click(object sender, RoutedEventArgs e)
        {
            using(var fbd = new FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string location = fbd.SelectedPath;
                    AddFolder(location, "single");
                }
            }
        }
    }
}
