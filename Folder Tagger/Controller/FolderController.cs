using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Folder_Tagger
{
    class FolderController
    {
        public bool FolderExist(string location)
        {
            using (var db = new Model1())
                if (db.Folders.Any(f => f.Location == location))
                    return true;
                return false;
        }

        public Folder GetFolderByLocation(string location, Model1 db)
        {
            return db.Folders.Where(f => f.Location == location).FirstOrDefault();
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
                var query = (System.Linq.IQueryable<Folder>)db.Folders;

                if (!string.IsNullOrWhiteSpace(artist))
                    query = query.Where(f => f.Artist == artist);

                if (!string.IsNullOrWhiteSpace(group))
                    query = query.Where(f => f.Group == group);

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(f => f.Name.Contains(name));

                if (tagList.Count() > 0)
                {
                    if (tagList.ElementAt(0).Trim().ToLower().Equals("no tag"))
                        query = query.Where(f => f.Tags.Count() == 0);
                    else
                        foreach (string tag in tagList)
                        {
                            string currentTag = tag.Trim().ToLower();
                            query = query.Where(f => f.Tags.Any(t => t.TagName == currentTag));
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
                if (FolderExist(location))
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

                var folder = new Folder(location, name, thumbnail);
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

        public List<string> GetArtistList(string location = null)
        {
            using (var db =  new Model1())
            {
                if (location != null)
                    return db.Folders
                        .Where(f => f.Location == location)
                        .Select(f => f.Artist)
                        .ToList();
                else
                {
                    var artistList = db.Folders
                        .OrderBy(f => f.Artist)
                        .Where(f => f.Artist != null)
                        .Select(f => f.Artist)
                        .Distinct()
                        .ToList();
                    artistList.Insert(0, "");
                    return artistList;
                }
            }
        }

        public void UpdateArtist(string location, string input)
        {
            using (var db = new Model1())
            {
                db.Folders
                    .Where(f => f.Location == location)
                    .ToList()
                    .ForEach(f => f.Artist = input);
                db.SaveChanges();
            }
        }

        public List<string> GetGroupList(string location = null)
        {
            using (var db = new Model1())
            {
                if (location != null)
                    return db.Folders
                        .Where(f => f.Location == location)
                        .Select(f => f.Group)
                        .ToList();
                else
                {
                    var artistList = db.Folders
                        .OrderBy(f => f.Group)
                        .Where(f => f.Group != null)
                        .Select(f => f.Group)
                        .Distinct()
                        .ToList();
                    artistList.Insert(0, "");
                    return artistList;
                }
            }
        }

        public void UpdateGroup(string location, string input)
        {
            using (var db = new Model1())
            {
                db.Folders
                    .Where(f => f.Location == location)
                    .ToList()
                    .ForEach(f => f.Group = input);
                db.SaveChanges();
            }
        }
    }
}
