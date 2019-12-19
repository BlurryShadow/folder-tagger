using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class FullEditWindow : Window
    {
        private string location = "";
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
                var query = db.Folders
                    .Where(f => f.Location == location)
                    .Select(f => f.Tag);

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
            {
                TextBox tb = (TextBox)sender;
                TraversalRequest tr = new TraversalRequest(FocusNavigationDirection.Next);
                tb.MoveFocus(tr);
            }
        }

        private void EditTag(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int tagID = Int32.Parse(tb.Tag.ToString());
            string newTagName = tb.Text.Trim();

            if (string.IsNullOrWhiteSpace(newTagName))
            {
                tb.Text = oldTagName;
                return;
            }

            using (var db = new Model1())
            {
                if (db.Folders.Any(f => f.Location == location && f.Tag.TagName.ToLower() == newTagName.ToLower()))
                {
                    tb.Text = oldTagName;
                    return;
                }
                int newTagID;

                if (!db.Tags.Any(t => t.TagName.ToLower() == newTagName.ToLower()))
                {
                    Tag t = new Tag(newTagName);
                    db.Tags.Add(t);
                    db.SaveChanges();
                    newTagID = t.TagID;
                } else
                {
                    newTagID = db.Tags
                        .Where(t => t.TagName.ToLower() == newTagName.ToLower())
                        .Select(t => t.TagID)
                        .First();                    
                }

                Folder newTagFolder = db.Folders
                    .Where(f => f.Location == location)
                    .Where(f => f.TagID == tagID)
                    .Select(f => f)
                    .First();

                newTagFolder.TagID = newTagID;
                tb.Tag = newTagID.ToString();
                db.SaveChanges();                
            }
        }
    }
}
