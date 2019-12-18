using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Folder_Tagger
{
    /// <summary>
    /// Interaction logic for SmallEditWindow.xaml
    /// </summary>
    public partial class SmallEditWindow : Window
    {
        private string type = "";
        private string folder = "";
        private string artist = "";
        private string group = "";
        public SmallEditWindow(string type, string folder)
        {
            InitializeComponent();
            this.type = type;
            this.folder = folder;
            getFolderData();
        }

        private void getFolderData()
        {
            using (var db = new Model1())
            {
                var query = db.Folders
                              .Where(f => f.Location == folder)
                              .Select(f => new { f.Artist, f.Group })
                              .FirstOrDefault();

                artist = query.Artist;
                group = query.Group;

                if (type.Equals("Artist")) tbInput.Text = artist;
                if (type.Equals("Group")) tbInput.Text = group;
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            string input = tbInput.Text;
            if (string.IsNullOrWhiteSpace(input)) input = null;

            using (var db = new Model1())
            {
                var query = db.Folders
                              .Where(f => f.Location == folder)
                              .Select(f => f)
                              .ToList();

                query.ForEach(f => 
                { 
                    if (type.Equals("Artist")) f.Artist = input;
                    if (type.Equals("Group")) f.Group = input;
                });

                db.SaveChanges();
                Close();
            }
        }
    }
}
