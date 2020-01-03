using Microsoft.VisualBasic.FileIO;
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

        public List<Folder> GetFolderListByArtist(string artist, Model1 db)
        {
            return db.Folders.Where(f => f.Artist == artist).ToList();
        }

        public List<Folder> GetFolderListByGroup(string group, Model1 db)
        {
            return db.Folders.Where(f => f.Group == group).ToList();
        }

        public List<List<Thumbnail>> SearchFolder(
            string artist = null, 
            string group = null, 
            string name = null, 
            List<string> tagList = null, 
            int imagesPerPage = 60)
        {
            using (var db = new Model1())
            {
                var query = (IQueryable<Folder>)db.Folders;

                if (!string.IsNullOrWhiteSpace(artist))
                    query = query.Where(f => f.Artist == artist);

                if (!string.IsNullOrWhiteSpace(group))
                    query = query.Where(f => f.Group == group);

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(f => f.Name.Contains(name));

                if (tagList.Count() > 0)
                {
                    string firstTag = tagList.ElementAt(0).ToLower().Trim();
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
                        case "no artist group":
                        case "no group artist":
                            query = query.Where(f => f.Artist == null && f.Group == null);
                            break;
                        default:
                            foreach (string tag in tagList)
                            {
                                string currentTag = tag.ToLower().Trim();
                                if (currentTag[0].Equals('-') && currentTag.Length > 1)
                                    query = query.Where(f => f.Tags.All(t => t.TagName != currentTag.Substring(1)));
                                else
                                    query = query.Where(f => f.Tags.Any(t => t.TagName == currentTag));
                            }
                            break;
                    }
                }

                int maxPage = (int)Math.Ceiling(((double)query.Count() / imagesPerPage));
                if (maxPage < 1)
                    return null;

                List<List<Thumbnail>> thumbnailList = new List<List<Thumbnail>>();
                for (int i = 0; i < maxPage; i++)
                    thumbnailList.Add(
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
                return thumbnailList;
            }
        }
        public void AddFolder(string location, string name, string type = "multiple")
        {
            using (var db = new Model1())
            {
                var folder = GetFolderByLocation(location, db);
                if (folder != null)
                {
                    if (type == "single")
                        System.Windows.MessageBox.Show("This Folder Has Already Been Added!");
                    return;
                }

                string thumbnail = null;
                DirectoryInfo dr = new DirectoryInfo(location);
                thumbnail = dr.EnumerateFiles()
                            .Select(t => t.FullName)
                            .FirstOrDefault(FullName => (FullName.ToLower() == "folder.jpg")
                                                     || (FullName.ToLower().Contains(".png"))
                                                     || (FullName.ToLower().Contains(".jpg"))
                                                     || (FullName.ToLower().Contains(".jpeg"))
                                                     || (FullName.ToLower().Contains(".bmp")));

                folder = new Folder(location, name, thumbnail);
                db.Folders.Add(folder);
                db.SaveChanges();
            }
        }

        public void DeleteRealFolder(List<string> folderList)
        {
            using (var db = new Model1())
                foreach (string folder in folderList)
                {
                    Folder deletedFolder = db.Folders.Where(f => f.Location == folder).First();
                    db.Folders.Remove(deletedFolder);
                    db.SaveChanges();
                    try
                    {
                        FileSystem.DeleteDirectory(
                            folder,
                            UIOption.OnlyErrorDialogs,
                            RecycleOption.SendToRecycleBin
                        );
                    }
                    catch (Exception)
                    {
                        System.Windows.Forms.MessageBox.Show("The folder " + folder + " is being used.");
                        continue;
                    }
                }
        }

        public void RemoveNonexistFolder()
        {
            using (var db = new Model1())
            {
                List<Folder> folderList = db.Folders.Select(f => f).ToList();
                foreach (Folder folder in folderList)
                {
                    if (!Directory.Exists(folder.Location))
                        db.Folders.Remove(folder);
                }
                db.SaveChanges();
            }
        }

        public List<string> GetArtistList(string location)
        {
            using (var db =  new Model1())
            {
                if (location == "all")
                {
                    return db.Folders
                        .OrderBy(f => f.Artist)
                        .Where(f => f.Artist != null)
                        .Select(f => f.Artist)
                        .Distinct()
                        .ToList();
                } else
                    return db.Folders
                        .Where(f => f.Location == location)
                        .Select(f => f.Artist)
                        .ToList();
            }
        }

        public void UpdateArtist(string location, string input)
        {
            using (var db = new Model1())
            {
                var folder = db.Folders.Where(f => f.Location == location).FirstOrDefault();
                folder.Artist = input;
                db.SaveChanges();
            }
        }

        public List<string> GetGroupList(string location)
        {
            using (var db = new Model1())
            {
                if (location == "all")
                {
                    return db.Folders
                        .OrderBy(f => f.Group)
                        .Where(f => f.Group != null)
                        .Select(f => f.Group)
                        .Distinct()
                        .ToList();
                }
                else
                    return db.Folders
                        .Where(f => f.Location == location)
                        .Select(f => f.Group)
                        .ToList();
            }
        }

        public void UpdateGroup(string location, string input)
        {
            using (var db = new Model1())
            {
                var folder = db.Folders.Where(f => f.Location == location).FirstOrDefault();
                folder.Group = input;
                db.SaveChanges();
            }
        }

        public IQueryable ExportMetadata(Model1 db)
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
                List<Metadata> metadataList = JsonConvert.DeserializeObject<List<Metadata>>(json);
                foreach (Metadata metadata in metadataList)
                {
                    var folder = db.Folders.Where(f => f.Name.ToLower().Trim() == metadata.Name.ToLower().Trim()).FirstOrDefault();
                    if (folder == null) continue;

                    if (!string.IsNullOrWhiteSpace(metadata.Artist))
                        folder.Artist = metadata.Artist.ToLower().Trim();
                    if (!string.IsNullOrWhiteSpace(metadata.Group))
                        folder.Group = metadata.Group.ToLower().Trim();
                    db.SaveChanges();

                    foreach (string tag in metadata.Tags)
                    {
                        Tag newTag = db.Tags.Where(t => t.TagName == tag).FirstOrDefault();
                        if (newTag == null)
                        {
                            newTag = new Tag(tag.ToLower().Trim());
                            db.Tags.Add(newTag);
                            folder.Tags.Add(newTag);
                            db.SaveChanges();
                        } else
                        {
                            if (db.Folders
                                .Where(f => f.Name.ToLower().Trim() == metadata.Name.ToLower().Trim()
                                    && f.Tags.Any(t => t.TagName == tag)).FirstOrDefault() == null)
                            {
                                folder.Tags.Add(newTag);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
        }
    }
}
