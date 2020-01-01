using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class AddTagWindow : Window
    {
        private readonly List<string> folderList = new List<string>();
        private readonly FolderController fc = new FolderController();
        private readonly TagController tc = new TagController();
        public AddTagWindow(List<string> folderList)
        {
            InitializeComponent();
            this.folderList = folderList;

            Loaded += (sender, e) => tbInput.Focus();
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
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
