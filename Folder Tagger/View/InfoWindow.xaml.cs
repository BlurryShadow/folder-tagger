using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class InfoWindow : Window
    {
        private readonly string infoType = "";
        private string oldInput = "";
        private List<string> infoList;

        private readonly TagController tc = new TagController();
        public InfoWindow(string infoType)
        {
            InitializeComponent();
            this.infoType = infoType;
            switch (infoType)
            {
                case "TagInfo":
                    infoList = tc.GetTagList("all").Select(t => t.TagName).ToList();
                    Title = "Tag Info";
                    break;
            }
            listboxInfo.ItemsSource = infoList;

            Loaded += (sender, e) => tbSuggest.Focus();
        }

        private void TextBoxSort_TextChanged(object sender, TextChangedEventArgs e)
        {
            string currentSort = tbSuggest.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(currentSort))
                listboxInfo.ItemsSource = infoList;
            else
                listboxInfo.ItemsSource = infoList.Where(i => i.Contains(currentSort));
        }
        private void TextBoxInput_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            oldInput = tb.Text;
        }

        private void TextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Keyboard.ClearFocus();
        }

        private void TextBoxInput_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string newInput = tb.Text.Trim().ToLower();

            if (newInput == oldInput.Trim().ToLower())
            {
                tb.Text = oldInput;
                return;
            }

            using (var db = new Model1())
            {
                switch (infoType)
                {
                    case "TagInfo":
                        Tag oldTag = tc.GetTagByName(oldInput, db);
                        if (string.IsNullOrEmpty(newInput))
                            db.Tags.Remove(oldTag);
                        else
                        {
                            Tag newTag = tc.GetTagByName(newInput, db);
                            if (newTag == null)
                                oldTag.TagName = newInput;

                            if (newTag != null)
                            {
                                var folderList = db.Folders.Where(f => f.Tags.Any(t => t.TagName == oldInput)).ToList();
                                db.Tags.Remove(oldTag);
                                folderList.ForEach(f => f.Tags.Add(newTag));
                            }
                        }
                        break;
                }

                db.SaveChanges();
                switch (infoType)
                {
                    case "TagInfo":
                        infoList = tc.GetTagList("all").Select(t => t.TagName).ToList();
                        listboxInfo.ItemsSource = infoList;
                        break;
                }
            }
        }
    }
}
