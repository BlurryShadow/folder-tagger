using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class FullEditWindow : Window
    {
        private readonly string location = "";
        private string oldTagName = "";

        public FullEditWindow(string location)
        {
            InitializeComponent();
            this.location = location;
            LoadTag();
        }

        private void LoadTag()
        {
            using (var db = new Model1())
            {
                var query = db.Tags
                    .Where(t => t.Folders.Any(f => f.Location == location))
                    .Select(t => t);

                var result = query.ToList();
                DataContext = result;
            }
        }

        private void GetCurrentTagName(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            oldTagName = tb.Text;
        }

        private void TextBoxPressEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Keyboard.ClearFocus();
        }

        private void EditTag(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int oldTagID = Int32.Parse(tb.Tag.ToString());
            string newTagName = tb.Text.Trim();

            if (string.IsNullOrWhiteSpace(newTagName) || newTagName == oldTagName)
            {
                tb.Text = oldTagName;
                return;
            }

            using (var db = new Model1())
            {
                if (db.Folders.Any(f => f.Location == location && f.Tags.Any(t => t.TagName.ToLower() == newTagName.ToLower())))
                {
                    tb.Text = oldTagName;
                    return;
                }

                Tag oldTag = db.Tags
                    .Where(t => t.TagID == oldTagID)
                    .Select(t => t)
                    .First();

                Tag newTag = db.Tags
                        .Where(t => t.TagName.ToLower() == newTagName.ToLower())
                        .Select(t => t)
                        .FirstOrDefault();

                if (newTag == null)
                    newTag = new Tag(newTagName);

                Folder folder = db.Folders
                    .Where(f => f.Location == location)
                    .Select(f => f)
                    .First();

                folder.Tags.Remove(oldTag);
                folder.Tags.Add(newTag);
                db.SaveChanges();
            }
        }
    }
}
