using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class FullEditWindow : Window
    {
        private readonly string location;

        private readonly TagController tc = new TagController();
        private readonly FolderController fc = new FolderController();

        public FullEditWindow(string location)
        {
            InitializeComponent();
            this.location = location;
            listboxTag.ItemsSource = tc.GetTags(location);

            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void TextBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();
        }

        private void TextBoxInputInsideListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //To the parent ListBoxItem if it was selected
            //To the first ListBoxItem if none were selected
            if (e.Key == Key.Enter)
                MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        /* If there is a light blue / dotted line box around tag that means a ListBoxItem is being seleteced for light blue, focused for dotted line, or both if there are both
         * Pay no mind to it since it does not affect event handler below
         * When TextBoxes receive keyboard focus (by clicking them or pressing tab), they will also have logical focus
         * LostFocus only triggers when IsFocused changes from true to false, which means logical focus is gone
         * There are 2 ways to make this happens:
         *   + Click another TextBox, shifting both keyboard and logical focus to it
         *   + Move logical focus to another object by pressing Enter (Thanks to event handler TextBoxInputInsideListBox_PreviewKeyDown above)
         *     This will make keyboard focus disappears but there will be one ListBoxItem with both selection and logical focus, it will be:
         *       + Parent ListBoxItem if it was selected before pressing Enter
         *       + The first ListBoxItem in ListBox if none were selected before
         *       + The ListBox itself if it is empty now
         *     Its logical focus is permanent and cannot be removed
         *     However we can still edit normally because although there must be only one keyboard focus but there can be multiple logical ones
         *     So we can set new logical focus to other TextBoxes, then remove them to trigger LostFocus event handler
         * Reference: https://docs.microsoft.com/en-us/dotnet/api/system.windows.uielement.lostfocus?view=netframework-4.8
         *            https://docs.microsoft.com/en-us/dotnet/api/system.windows.uielement.isfocused?view=netframework-4.8
        */
        private void TextBoxInputInsideListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            string oldTagName = tb.Tag.ToString().ToLower().Trim();
            string newTagName = tb.Text.ToLower().Trim();

            if (newTagName == oldTagName)
            {
                tb.Text = oldTagName;
                return;
            }

            using (var db = new Model1())
            {
                Folder folder = fc.GetFolderByLocation(location, db);
                Tag newTag = tc.GetTagByName(newTagName, db);
                if (folder.Tags.Any(t => t == newTag))
                {
                    tb.Text = oldTagName;
                    return;
                }

                //Updating tag by removing old tag, then add new tag
                Tag oldTag = tc.GetTagByName(oldTagName, db);
                folder.Tags.Remove(oldTag);
                //Just remove old tag if users input empty string
                if (string.IsNullOrEmpty(newTagName))
                {
                    db.SaveChanges();
                    listboxTag.ItemsSource = tc.GetTags(location);
                    return;
                }
                if (newTag == null)
                {
                    MessageBox.Show('\"' + newTagName + "\" tag does not exist yet!");
                    tb.Text = oldTagName;
                    return;
                }
                folder.Tags.Add(newTag);
                db.SaveChanges();
                tb.Text = newTagName;
                //The newTagName will be the next oldTagName
                tb.Tag = newTagName;
            }
        }
    }
}
