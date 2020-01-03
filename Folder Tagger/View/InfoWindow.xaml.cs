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
        private readonly FolderController fc = new FolderController();
        public InfoWindow(string infoType)
        {
            InitializeComponent();
            this.infoType = infoType;
            Title = infoType + " Info";
            switch (infoType)
            {
                case "Tag":
                    infoList = tc.GetTagList("all").Select(t => t.TagName).ToList();                    
                    break;
                case "Artist":
                    infoList = fc.GetArtists();
                    break;
                case "Group":
                    infoList = fc.GetGroups();
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
            string newInput = string.IsNullOrWhiteSpace(tb.Text) ? null : tb.Text.Trim().ToLower();

            if (newInput == oldInput.Trim().ToLower())
            {
                tb.Text = oldInput;
                return;
            }

            using (var db = new Model1())
            {
                List<Folder> folderList;
                switch (infoType)
                {                    
                    case "Tag":
                        Tag oldTag = tc.GetTagByName(oldInput, db);
                        if (newInput == null)
                            db.Tags.Remove(oldTag);
                        else
                        {
                            Tag newTag = tc.GetTagByName(newInput, db);
                            if (newTag == null)
                                oldTag.TagName = newInput;
                            if (newTag != null)
                            {
                                folderList = db.Folders.Where(f => f.Tags.Any(t => t.TagName == oldInput)).ToList();
                                db.Tags.Remove(oldTag);
                                folderList.ForEach(f => f.Tags.Add(newTag));
                            }
                        }
                        break;
                    case "Artist":
                        folderList = fc.GetFoldersByArtist(oldInput, db);
                        folderList.ForEach(f => fc.UpdateArtist(f.Location, newInput));
                        break;
                    case "Group":
                        folderList = fc.GetFoldersByGroup(oldInput, db);
                        folderList.ForEach(f => fc.UpdateGroup(f.Location, newInput));
                        break;
                }

                db.SaveChanges();
                switch (infoType)
                {
                    case "Tag":
                        infoList = tc.GetTagList("all").Select(t => t.TagName).ToList();
                        break;
                    case "Artist":
                        infoList = fc.GetArtists();
                        break;
                    case "Group":
                        infoList = fc.GetGroups();
                        break;
                }
                listboxInfo.ItemsSource = infoList;
            }
        }
    }
}
