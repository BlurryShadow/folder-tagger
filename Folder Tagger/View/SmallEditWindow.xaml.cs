using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class SmallEditWindow : Window
    {
        private readonly string type = "";
        private readonly string location = "";
        private string artist = "";
        private string group = "";
        public SmallEditWindow(string type, string location)
        {
            InitializeComponent();
            this.type = type;
            this.location = location;
            getFolderData();
            Loaded += (sender, e) => tbInput.Focus();
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void getFolderData()
        {
            using (var db = new Model1())
            {
                var query = db.Folders
                    .Where(f => f.Location == location)
                    .Select(f => new { f.Artist, f.Group })
                    .Single();

                artist = query.Artist;
                group = query.Group;

                if (type.Equals("Artist")) tbInput.Text = artist;
                if (type.Equals("Group")) tbInput.Text = group;
            }
        }

        private void UpdateData(object sender, RoutedEventArgs e)
        {
            string input = tbInput.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(input)) input = null;

            using (var db = new Model1())
            {
                var query = db.Folders
                    .Where(f => f.Location == location)
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
