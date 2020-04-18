using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class AddTagWindow : Window
    {
        private readonly List<string> locations;
        private readonly List<Tag> allTags;
        private readonly int numberCharactersToActiveSuggestion = 3;

        private readonly FolderController fc = new FolderController();
        private readonly TagController tc = new TagController();
        public AddTagWindow(List<string> locations)
        {
            InitializeComponent();
            this.locations = locations;
            allTags = tc.GetTags("all", "mostUsed");

            Loaded += (sender, e) => tbInput.Focus();
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
            listboxAutoComplete.PreviewMouseUp += (sender, e) => ApplySuggestion();
        }

        protected override void OnLocationChanged(EventArgs e) //Make popup stick with textbox
        {
            popupAutoComplete.HorizontalOffset += 1;
            popupAutoComplete.HorizontalOffset -= 1;
            base.OnLocationChanged(e);
        }

        private void ApplySuggestion()
        {
            int indexPosition = tbInput.Text.LastIndexOf(", ");
            try
            {
                Tag t = listboxAutoComplete.SelectedItem as Tag;
                //indexPosition == -1 means TextBox does not have any tag yet
                tbInput.Text = (indexPosition == -1) ? t.TagName : tbInput.Text.Substring(0, indexPosition + 2) + t.TagName;
                tbInput.Text += ", ";
                tbInput.CaretIndex = tbInput.Text.Length;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            tbInput.Focus();
        }

        private void TextBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbInput.Text))
            {
                //The last part of TextBox Text, behind ', '
                //Not the whole TextBox string
                //Nor where the cursor is located at
                //Example: "a1, a2, a3|, a4, a5"    | = cursor   currentTag = a5
                string currentTag = tbInput.Text.Split(new string[] { ", " }, StringSplitOptions.None).Last().ToLower().Trim();
                if (string.IsNullOrEmpty(currentTag) || currentTag.Length < numberCharactersToActiveSuggestion)
                {
                    popupAutoComplete.IsOpen = false;
                    return;
                }

                var suggestedTags = allTags.Where(t => t.TagName.Contains(currentTag)).ToList();
                int suggestedTagCount = suggestedTags.Count;
                if (suggestedTagCount == 0)
                {
                    popupAutoComplete.IsOpen = false;
                    return;
                }

                popupAutoComplete.Height = (suggestedTagCount > 10) ? 250 : suggestedTagCount * 25 + 10;
                listboxAutoComplete.ItemsSource = suggestedTags;
                popupAutoComplete.IsOpen = true;
            }
        }

        private void TextBoxInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Jump to suggestion box when press Down or Up Key
            if (popupAutoComplete.IsOpen)
            {
                if (e.Key == Key.Down)
                    listboxAutoComplete.SelectedIndex = 0;
                if (e.Key == Key.Up)
                    listboxAutoComplete.SelectedIndex = listboxAutoComplete.Items.Count - 1;
                if (e.Key == Key.Down || e.Key == Key.Up)
                {
                    var item = (ListBoxItem)listboxAutoComplete.ItemContainerGenerator.ContainerFromItem(listboxAutoComplete.SelectedItem);
                    item.Focus();
                    //Prevent focus from jumping to next element
                    e.Handled = true;
                }
            }

            if (e.Key == Key.OemComma)
            {
                tbInput.Text += ", ";
                e.Handled = true;
                tbInput.CaretIndex = tbInput.Text.Length;
            }
        }

        private void ListBoxAutoComplete_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Jump back to TextBox when press Up Key at first element or Backspace key
            if ((e.Key == Key.Up && listboxAutoComplete.SelectedIndex == 0) || e.Key == Key.Back)
                tbInput.Focus();

            //Return to first element when pressed at last one
            //Trying to mimic S Key default behavior
            if (e.Key == Key.Down && listboxAutoComplete.SelectedIndex == listboxAutoComplete.Items.Count - 1)
            {
                var item = (ListBoxItem)listboxAutoComplete.ItemContainerGenerator.ContainerFromItem(listboxAutoComplete.Items[0]);
                item.Focus();
                e.Handled = true;
            }

            //Disable 'S' key default event
            //Which is the same as Down key
            //But return to first element when pressed at last one
            if (e.Key == Key.S)
                e.Handled = true;

            if (e.Key == Key.Enter)
            {
                ApplySuggestion();
                //Add Button is default
                //Cancel Enter Key Down event to prevent premature trigger
                e.Handled = true;
            }
        }

        private void ButtonAddTag_Clicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbInput.Text))
            {
                Close();
                return;
            }

            List<string> tags = tbInput.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList();
            using (var db = new Model1())
                foreach (string location in locations)
                    foreach (string tag in tags)
                    {
                        string currentTag = tag.Trim().ToLower();
                        if (string.IsNullOrEmpty(currentTag))
                            continue;
                        if (db.Folders.Any(f => f.Location == location && f.Tags.Any(t => t.TagName == currentTag)))
                            continue;

                        Folder folder = fc.GetFolderByLocation(location, db);
                        Tag newTag = tc.GetTagByName(currentTag, db);
                        if (newTag == null)
                        {
                            newTag = new Tag(currentTag);
                            db.Tags.Add(newTag);
                        }
                        folder.Tags.Add(newTag);
                        db.SaveChanges();
                    }
            Close();
        }
    }
}
