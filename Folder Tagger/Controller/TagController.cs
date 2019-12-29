using System.Collections.Generic;
using System.Linq;

namespace Folder_Tagger
{
    class TagController
    {
        public List<Tag> GetTagList(string location)
        {
            using (var db = new Model1())
                return db.Tags
                    .Where(t => t.Folders.Any(f => f.Location == location))
                    .Select(t => t)
                    .ToList();
        }

        public Tag GetTagByID(int tagID, Model1 db)
        {
            return db.Tags.Where(t => t.TagID == tagID).FirstOrDefault();
        }

        public Tag GetTagByName(string tagName, Model1 db)
        {
            return db.Tags.Where(t => t.TagName == tagName).FirstOrDefault();
        }
    }
}
