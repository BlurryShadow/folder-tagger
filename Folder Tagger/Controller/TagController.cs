﻿using System.Collections.Generic;
using System.Linq;

namespace Folder_Tagger
{
    class TagController
    {
        public List<Tag> GetTagList(string location)
        {
            using (var db = new Model1())
                if (location == "all")
                    return db.Tags
                        .Select(t => t)
                        .ToList();
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
