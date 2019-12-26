using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
