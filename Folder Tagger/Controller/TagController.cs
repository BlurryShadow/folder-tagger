using System.Collections.Generic;
using System.Linq;

namespace Folder_Tagger
{
    class TagController
    {
        public List<Tag> GetTagList(string location, string orderBy = "alphabet")
        {
            using (var db = new Model1())
                if (location == "all")
                {
                    var query = db.Tags.Select(t => t);

                    if (orderBy == "alphabet")
                        query = query.OrderBy(t => t.TagName);
                    if (orderBy == "mostUsed")
                        query = query.OrderByDescending(t => t.Folders.Count);

                    return query.ToList();
                }
                else
                    return db.Tags
                        .OrderBy(t => t.TagName)
                        .Where(t => t.Folders.Any(f => f.Location == location))
                        .Select(t => t)
                        .ToList();
        }

        public Tag GetTagByName(string tagName, Model1 db)
        {
            return db.Tags.Where(t => t.TagName == tagName).FirstOrDefault();
        }
    }
}
