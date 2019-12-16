using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folder_Tagger
{
    class Thumbnail
    {
        public string Root { get; set; }
        public string Name { get; set; }

        public Thumbnail(string root, string name)
        {
            Root = root;
            Name = name;
        }

        public Thumbnail() { }
    }
}
