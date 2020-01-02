using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class InfoWindow : Window
    {
        private readonly string location = "";
        private string oldTagName = "";
        private readonly TagController tc = new TagController();
        private readonly FolderController fc = new FolderController();
        public InfoWindow()
        {
            InitializeComponent();
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
            int oldTagID = Int32.Parse(tb.Tag.ToString());
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
                Tag oldTag = tc.GetTagByID(oldTagID, db);
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
