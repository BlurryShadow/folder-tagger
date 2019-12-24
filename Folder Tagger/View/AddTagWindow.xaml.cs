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
        public AddTagWindow(List<string> folderList)
        {
            InitializeComponent();
            this.folderList = folderList;
            Loaded += (sender, e) => tbInput.Focus();
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void AddTag(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbInput.Text))
                Close();

            List<string> tagList = tbInput.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList();

            using (var db = new Model1())
                foreach (string location in folderList)
                    foreach (string tag in tagList)
                    {
                        string currentTag = tag.Trim().ToLower();

                        if (db.Folders.Any(f => f.Location == location && f.Tags.Any(t => t.TagName == currentTag)))
                            continue;

                        Folder folder = db.Folders.Where(f => f.Location == location).Select(f => f).First();
                        Tag newTag = db.Tags.Where(t => t.TagName == currentTag).Select(t => t).FirstOrDefault();
                        if (newTag == null)
                        {
                            newTag = new Tag(currentTag);
                            db.Tags.Add(newTag);
                            db.SaveChanges();
                        }
                        Tag defaultTag = db.Tags.Where(t => t.TagID == 1 && t.Folders.Any(f => f.Location == location)).FirstOrDefault();
                        if (defaultTag != null)
                            folder.Tags.Remove(defaultTag);

                        folder.Tags.Add(newTag);
                        db.SaveChanges();
                    }
            Close();
        }
    }
}
