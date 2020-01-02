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
        private readonly TagController tc = new TagController();
        private readonly FolderController fc = new FolderController();

        public FullEditWindow(string location)
        {
            InitializeComponent();
            this.location = location;
            DataContext = tc.GetTagList(location);

            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void TextBoxInput_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            oldTagName = tb.Text;
        }

        private void TextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Keyboard.ClearFocus();
        }

        private void TextBoxInput_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string newTagName = tb.Text.Trim().ToLower();

            if (newTagName == oldTagName.Trim().ToLower())
            {
                tb.Text = oldTagName;
                return;
            }

            using (var db = new Model1())
            {
                if (db.Folders.Any(f => f.Location == location && f.Tags.Any(t => t.TagName == newTagName)))
                {
                    tb.Text = oldTagName;
                    return;
                }

                Folder folder = fc.GetFolderByLocation(location, db);
                Tag oldTag = tc.GetTagByName(oldTagName, db);
                folder.Tags.Remove(oldTag);

                if (string.IsNullOrWhiteSpace(newTagName))
                {
                    db.SaveChanges();
                    DataContext = tc.GetTagList(location);
                    return;
                }

                Tag newTag = tc.GetTagByName(newTagName, db);
                if (newTag == null)
                    newTag = new Tag(newTagName);

                folder.Tags.Add(newTag);
                db.SaveChanges();
                tb.Text = newTagName;
                tb.Tag = newTag.TagID;
            }
        }
    }
}
