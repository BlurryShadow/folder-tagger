using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Folder_Tagger
{
    class FolderController
    {
        public Folder GetFolderByLocation(string location, Model1 db)
        {
            return db.Folders.Where(f => f.Location == location).FirstOrDefault();
        }

        public List<Folder> GetFoldersByArtist(string artist, Model1 db)
        {
            return db.Folders.Where(f => f.Artist == artist).ToList();
        }

        public List<Folder> GetFoldersByGroup(string group, Model1 db)
        {
            return db.Folders.Where(f => f.Group == group).ToList();
        }

        public List<List<Thumbnail>> SearchFolders(
            string paramArtist = null, 
            string paramGroup = null, 
            string paramName = null, 
            List<string> tags = null, 
            int imagesPerPage = 60)
        {
            using (var db = new Model1())
            {
                string artist = paramArtist?.ToLower().Trim();
                string group = paramGroup?.ToLower().Trim();
                string name = paramName?.ToLower().Trim();
                var query = (IQueryable<Folder>)db.Folders;

                if (!string.IsNullOrEmpty(artist))
                    query = query.Where(f => f.Artist == artist);

                if (!string.IsNullOrEmpty(group))
                    query = query.Where(f => f.Group == group);

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(f => f.Name.Contains(name));

                if (tags.Count() > 0)
                {
                    string firstTag = tags[0].ToLower().Trim();
                    switch (firstTag)
                    {
                        case "no tag":
                            query = query.Where(f => f.Tags.Count() == 0);
                            break;
                        case "no artist":
                            query = query.Where(f => f.Artist == null);
                            break;
                        case "no group":
                            query = query.Where(f => f.Group == null);
                            break;
                        case "no artist no group":
                        case "no group no artist":
                            query = query.Where(f => f.Artist == null && f.Group == null);
                            break;
                        default:
                            foreach (string tag in tags)
                            {
                                string currentTag = tag.ToLower().Trim();
                                if (string.IsNullOrEmpty(currentTag))
                                    continue;
                                //Excluding tags
                                if (currentTag[0].Equals('-') && currentTag.Length > 1)
                                    query = query.Where(f => f.Tags.All(t => t.TagName != currentTag.Substring(1)));
                                //Including tags
                                else
                                    query = query.Where(f => f.Tags.Any(t => t.TagName == currentTag));
                            }
                            break;
                    }
                }

                //Pagination
                int totalPages = (int)Math.Ceiling(((double)query.Count() / imagesPerPage));
                if (totalPages < 1)
                    return null;

                List<List<Thumbnail>> thumbnailsList = new List<List<Thumbnail>>();
                for (int i = 0; i < totalPages; i++)
                    thumbnailsList.Add(
                        query
                            .OrderBy(f => f.Name)
                            .Skip(i * imagesPerPage)
                            .Take(imagesPerPage)
                            .Select(f => new Thumbnail
                            {
                                Folder = f.Location,
                                Root = f.Thumbnail,
                                Name = f.Name
                            })
                            .ToList()
                    );
                return thumbnailsList;
            }
        }

        public void AddFolder(string location, string name, bool showWarning)
        {
            using (var db = new Model1())
            {
                var folder = GetFolderByLocation(location, db);
                if (folder != null)
                {
                    if (showWarning)
                        System.Windows.MessageBox.Show("This Folder Has Already Been Added!");
                    return;
                }

                string thumbnail = PickNewThumbnail(location);
                folder = new Folder(location, name, thumbnail);
                db.Folders.Add(folder);
                db.SaveChanges();
            }
        }

        public void RemoveFolder(List<string> locations)
        {
            using (var db = new Model1())
                foreach (string location in locations)
                {
                    Folder folder = db.Folders.Where(f => f.Location == location).FirstOrDefault();
                    db.Folders.Remove(folder);
                    db.SaveChanges();
                }
        }

        public void RemoveNonexistentFolders()
        {
            using (var db = new Model1())
            {
                List<Folder> folders = db.Folders.ToList();
                foreach (Folder folder in folders)
                    if (!Directory.Exists(folder.Location))
                        db.Folders.Remove(folder);
                    else if (!File.Exists(folder.Thumbnail))
                        folder.Thumbnail = PickNewThumbnail(folder.Location);
                db.SaveChanges();
            }
        }

        public string PickNewThumbnail(string location)
        {
            DirectoryInfo dr = new DirectoryInfo(location);
            return dr.EnumerateFiles()
                .Select(t => t.FullName)
                .FirstOrDefault(FullName => (FullName.ToLower() == "folder.jpg")
                                            || (FullName.ToLower().Contains(".png"))
                                            || (FullName.ToLower().Contains(".jpg"))
                                            || (FullName.ToLower().Contains(".jpeg"))
                                            || (FullName.ToLower().Contains(".bmp")));
        }

        public List<string> GetArtists(string location = "all")
        {
            using (var db =  new Model1())
                if (location == "all")
                    return db.Folders
                        .OrderBy(f => f.Artist)
                        .Where(f => f.Artist != null)
                        .Select(f => f.Artist)
                        .Distinct()
                        .ToList();
                else
                    return db.Folders
                        .Where(f => f.Location == location)
                        .Select(f => f.Artist)
                        .ToList();
        }

        public void UpdateArtist(string location, string newArtist)
        {
            using (var db = new Model1())
            {
                var folder = GetFolderByLocation(location, db);
                folder.Artist = newArtist;
                db.SaveChanges();
            }
        }

        public List<string> GetGroups(string location = "all")
        {
            using (var db = new Model1())
                if (location == "all")
                    return db.Folders
                        .OrderBy(f => f.Group)
                        .Where(f => f.Group != null)
                        .Select(f => f.Group)
                        .Distinct()
                        .ToList();
                else
                    return db.Folders
                        .Where(f => f.Location == location)
                        .Select(f => f.Group)
                        .ToList();
        }

        public void UpdateGroup(string location, string newGroup)
        {
            using (var db = new Model1())
            {
                var folder = GetFolderByLocation(location, db);
                folder.Group = newGroup;
                db.SaveChanges();
            }
        }

        public void RemoveTagFromFolders(Model1 db, Tag tag, string location = "all")
        {
            if (location == "all")
                db.Folders.ToList().ForEach(f => f.Tags.Remove(tag));
            else
            {
                var folder = GetFolderByLocation(location, db);
                folder.Tags.Remove(tag);
            }
            db.SaveChanges();
        }

        public void RemoveAllTagsFromFolders(string location = "all")
        {
            using (var db = new Model1())
            {
                if (location == "all")
                    db.Folders.ToList().ForEach(f => f.Tags.Clear());
                else
                {
                    var folder = GetFolderByLocation(location, db);
                    folder.Tags.Clear();
                }
                db.SaveChanges();
            }
        }

        public IQueryable GetMetadataToExport(Model1 db)
        {
            return db.Folders
                .OrderBy(f => f.Name)
                .Select(f => new {
                    f.Name,
                    f.Artist,
                    f.Group,
                    Tags = f.Tags.OrderBy(t => t.TagName).Select(t => t.TagName)
                });
        }

        public void ImportMetadata(string json)
        {
            using (var db = new Model1())
            {
                List<Metadata> metadatas = JsonConvert.DeserializeObject<List<Metadata>>(json);
                foreach (Metadata metadata in metadatas)
                {
                    string name = metadata.Name.ToLower().Trim();
                    var folder = db.Folders.Where(f => f.Name.ToLower().Trim() == name).FirstOrDefault();
                    if (folder == null) continue;

                    string artist = metadata.Artist?.ToLower().Trim();
                    string group = metadata.Group?.ToLower().Trim();
                    var tags = metadata.Tags;

                    if (!string.IsNullOrEmpty(artist))
                        folder.Artist = artist;
                    if (!string.IsNullOrEmpty(group))
                        folder.Group = group;
                    db.SaveChanges();

                    foreach (string tag in tags)
                    {
                        string currentTag = tag.ToLower().Trim();
                        Tag newTag = db.Tags.Where(t => t.TagName == currentTag).FirstOrDefault();
                        if (newTag == null)
                        {
                            newTag = new Tag(currentTag);
                            db.Tags.Add(newTag);
                        }
                        folder.Tags.Add(newTag);
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}
