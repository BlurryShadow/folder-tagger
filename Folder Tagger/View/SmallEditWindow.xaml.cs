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
        FolderController fc = new FolderController();
        public SmallEditWindow(string type, string location)
        {
            InitializeComponent();
            this.type = type;
            this.location = location;

            switch (type)
            {
                case "Artist":
                    artist = fc.GetArtistList(location).ElementAtOrDefault(0);
                    tbInput.Text = artist;
                    break;
                case "Group":
                    group = fc.GetGroupList(location).ElementAtOrDefault(0);
                    tbInput.Text = group;
                    break;
            }

            Loaded += (sender, e) => tbInput.Focus();
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void ButtonUpdate_Clicked(object sender, RoutedEventArgs e)
        {
            string input = tbInput.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(input)) input = null;
            
            switch (type)
            {
                case "Artist":
                    fc.UpdateArtist(location, input);
                    break;
                case "Group":
                    fc.UpdateGroup(location, input);
                    break;
            }

            Close();
        }
    }
}
