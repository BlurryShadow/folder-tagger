using System;
using System.Windows;
using System.Windows.Forms;

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

        private void AddFolder(string location)
        {
            using (var db = new Model1())
            {
                var folder = new Folder(location);
                db.Folders.Add(folder);
                
                try
                {
                    db.SaveChanges();
                }
                catch
                {
                    System.Windows.MessageBox.Show("This Folder Has Already Been Added!");
                }
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
                    AddFolder(location);
                }
            }
        }
    }
}
