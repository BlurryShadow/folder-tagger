using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class RemoveTagWindow : Window
    {
        private readonly List<string> locations;
        private readonly string deleteEveryTagKeyword = "everything";

        private readonly FolderController fc = new FolderController();
        public RemoveTagWindow(List<string> locations)
        {
            InitializeComponent();
            this.locations = locations;

            Loaded += (sender, e) => tbInput.Focus();
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void ButtonRemoveTag_Clicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbInput.Text))
            {
                Close();
                return;
            }

            string str = tbInput.Text.ToLower().Trim();
            using (var db = new Model1())
                foreach (string location in locations)
                    if (str == deleteEveryTagKeyword)
                        fc.RemoveAllTagsFromFolders(location);
                    else
                    {
                        List<string> tags = tbInput.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList();
                        foreach (string tag in tags)
                        {
                            string currentTag = tag.ToLower().Trim();
                            Tag deletedTag = db.Tags.Where(t => t.TagName == currentTag).FirstOrDefault();
                            if (deletedTag == null) continue;
                            fc.RemoveTagFromFolders(deletedTag);
                        }
                    }
            Close();
        }
    }
}
