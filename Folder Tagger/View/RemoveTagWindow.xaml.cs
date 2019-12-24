using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class RemoveTagWindow : Window
    {
        private readonly List<string> folderList = new List<string>();
        public RemoveTagWindow(List<string> folderList)
        {
            InitializeComponent();
            this.folderList = folderList;
            Loaded += (sender, e) => tbInput.Focus();
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void RemoveTag(object sender, RoutedEventArgs e)
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
                        Tag deletedTag = db.Tags.Where(t => t.TagName == currentTag && t.Folders.Any(f => f.Location == location)).FirstOrDefault();

                        if (deletedTag == null) continue;

                        Folder folder = db.Folders.Where(f => f.Location == location).Select(f => f).First();
                        folder.Tags.Remove(deletedTag);
                        db.SaveChanges();
                    }
            Close();
        }
    }
}
