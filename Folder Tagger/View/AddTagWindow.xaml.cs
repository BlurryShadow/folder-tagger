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
        private readonly List<string> folderList = new List<string>();
        private readonly List<Tag> allTag = new List<Tag>();
        private readonly FolderController fc = new FolderController();
        private readonly TagController tc = new TagController();
        public AddTagWindow(List<string> folderList)
        {
            InitializeComponent();
            this.folderList = folderList;
            allTag = tc.GetTagList("all", "mostUsed");

            Loaded += (sender, e) => tbInput.Focus();
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        protected override void OnLocationChanged(EventArgs e) //Make popup stick with textbox
        {
            popupAutoComplete.HorizontalOffset += 1;
            popupAutoComplete.HorizontalOffset -= 1;
            base.OnLocationChanged(e);
        }

        private void TextBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbInput.Text))
            {
                string currentTag = tbInput.Text.Split(new string[] { ", " }, StringSplitOptions.None).Last().ToLower().Trim();
                if (string.IsNullOrEmpty(currentTag) || currentTag.Length < 3)
                {
                    popupAutoComplete.IsOpen = false;
                    return;
                }

                var suggestedTag = allTag.Where(t => t.TagName.Contains(currentTag)).Select(t => t).ToList();
                int suggestedTagCount = suggestedTag.Count;
                if (suggestedTagCount == 0)
                {
                    popupAutoComplete.IsOpen = false;
                    return;
                }

                popupAutoComplete.Height = suggestedTagCount > 10 ? 250 : suggestedTagCount * 25 + 10;
                listboxAutoComplete.ItemsSource = suggestedTag;
                popupAutoComplete.IsOpen = true;
            }
        }

        private void TextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && popupAutoComplete.IsOpen)
            {
                listboxAutoComplete.SelectedIndex = 0;
                var item = (ListBoxItem)listboxAutoComplete.ItemContainerGenerator.ContainerFromItem(listboxAutoComplete.SelectedItem);
                item.Focus();
                e.Handled = true;
            }

            if (e.Key == Key.OemComma)
            {
                tbInput.Text += ", ";
                e.Handled = true;
                tbInput.CaretIndex = tbInput.Text.Length;
            }
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && listboxAutoComplete.SelectedIndex == 0)
                tbInput.Focus();

            if (e.Key == Key.S) //Disable 'S' key default event
                e.Handled = true;

            if (e.Key == Key.Enter)
            {
                Tag t = listboxAutoComplete.SelectedItem as Tag;
                int lastPosition = tbInput.Text.LastIndexOf(", ");
                try
                {
                    tbInput.Text = lastPosition == -1 ? t.TagName : tbInput.Text.Substring(0, lastPosition + 2) + t.TagName;
                    tbInput.Text += ", ";
                    tbInput.CaretIndex = tbInput.Text.Length;
                } catch(Exception exception)
                {
                    System.Windows.Forms.MessageBox.Show(exception.Message);
                }
                tbInput.Focus();
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

            List<string> tagList = tbInput.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList();

            using (var db = new Model1())
                foreach (string location in folderList)
                    foreach (string tag in tagList)
                    {
                        string currentTag = tag.Trim().ToLower();
                        if (string.IsNullOrEmpty(currentTag)) continue;

                        if (db.Folders.Any(f => f.Location == location && f.Tags.Any(t => t.TagName == currentTag)))
                            continue;

                        Folder folder = fc.GetFolderByLocation(location, db);
                        Tag newTag = tc.GetTagByName(currentTag, db);
                        if (newTag == null)
                        {
                            newTag = new Tag(currentTag);
                            db.Tags.Add(newTag);
                            db.SaveChanges();
                        }

                        folder.Tags.Add(newTag);
                        db.SaveChanges();
                    }
            Close();
        }
    }
}
