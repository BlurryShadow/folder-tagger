using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class InfoWindow : Window
    {
        private readonly string infoType;
        private List<string> infos;

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
                    infos = tc.GetTags().Select(t => t.TagName).ToList();                    
                    break;
                case "Artist":
                    infos = fc.GetArtists();
                    break;
                case "Group":
                    infos = fc.GetGroups();
                    break;
            }            
            listboxInfo.ItemsSource = infos;

            Loaded += (sender, e) => tbSuggestion.Focus();
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void TextBoxSuggestion_TextChanged(object sender, TextChangedEventArgs e)
        {
            string currentSuggestion = tbSuggestion.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(currentSuggestion))
                listboxInfo.ItemsSource = infos;
            else
                listboxInfo.ItemsSource = infos.Where(i => i.Contains(currentSuggestion));
        }

        private void TextBoxInputInsideListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e) //For editing with mouse
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();
        }

        private void TextBoxInputInsideListBox_GotFocus(object sender, RoutedEventArgs e) //For editing with keyboard
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();
        }

        private void TextBoxInputInsideListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        //Refer to FullEditWindow for more information
        private void TextBoxInputInsideListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string oldInput = tb.Tag.ToString();
            string newInput = string.IsNullOrWhiteSpace(tb.Text) ? null : tb.Text.Trim().ToLower();

            if (newInput == oldInput.Trim().ToLower())
            {
                tb.Text = oldInput;
                return;
            }

            using (var db = new Model1())
            {
                List<Folder> folders;
                switch (infoType)
                {
                    case "Tag":
                        Tag oldTag = tc.GetTagByName(oldInput, db);
                        //Delete oldTag if users input empty string
                        if (newInput == null)
                            db.Tags.Remove(oldTag);
                        else
                        {
                            Tag newTag = tc.GetTagByName(newInput, db);
                            //If there is not any tag that has the same name as newInput
                            //Simply update oldTag TagName into newInput
                            //We need to check this because the primary key is TagID, not TagName
                            //Therefore multiple tags with the same name is a possibility
                            if (newTag == null)
                                oldTag.TagName = newInput;
                            //If there is already a tag with TagName == newInput
                            //Transfer all folders belong to oldTag to newTag and then delete oldTag
                            //Again because the primary key is TagID, not TagName
                            if (newTag != null)
                            {
                                folders = db.Folders.Where(f => f.Tags.Any(t => t.TagName == oldInput)).ToList();
                                db.Tags.Remove(oldTag);
                                folders.ForEach(f => f.Tags.Add(newTag));
                            }
                        }
                        break;
                    //Unlike Tag which has many-to-many relationship with Folder
                    //Artist and Group are property of Folder
                    //Therefore no need to check duplicate and deleting just means update them to null
                    //But also when updating we have to update each folder individually
                    case "Artist":
                        folders = fc.GetFoldersByArtist(oldInput, db);
                        folders.ForEach(f => fc.UpdateArtist(f.Location, newInput));
                        break;
                    case "Group":
                        folders = fc.GetFoldersByGroup(oldInput, db);
                        folders.ForEach(f => fc.UpdateGroup(f.Location, newInput));
                        break;
                }
                db.SaveChanges();

                switch (infoType)
                {
                    case "Tag":
                        infos = tc.GetTags().Select(t => t.TagName).ToList();
                        break;
                    case "Artist":
                        infos = fc.GetArtists();
                        break;
                    case "Group":
                        infos = fc.GetGroups();
                        break;
                }
                //ItemsSource will be re-generated anyway so updating TextBox Tag for new oldInput is unnecessary
                listboxInfo.ItemsSource = infos;
            }
        }
    }
}
