using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Folder_Tagger
{
    public partial class AddTagWindow : Window
    {
        private readonly string location;
        public AddTagWindow(string location)
        {
            InitializeComponent();
            this.location = location;
            Loaded += (sender, e) => tbInput.Focus();
            PreviewKeyDown += (sender, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private void AddTag(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbInput.Text))
                Close();

            List<string> tagList = tbInput.Text.Split(new string[] { ", " }, StringSplitOptions.None).ToList();

            using (var db = new Model1())
                foreach (string tag in tagList)
                {
                    if (db.Folders.Any(f => f.Location == location && f.Tags.Any(t => t.TagName.ToLower() == tag.ToLower())))
                        continue;

                    Folder folder = db.Folders.Where(f => f.Location == location).Select(f => f).Single();

                    Tag newTag = db.Tags.Where(t => t.TagName.ToLower() == tag.ToLower()).Select(t => t).FirstOrDefault();
                    if (newTag == null)
                    {
                        newTag = new Tag(tag);
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
