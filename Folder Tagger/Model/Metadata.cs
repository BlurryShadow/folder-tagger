using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folder_Tagger
{
    class Metadata
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Group { get; set; }
        public List<string> Tags { get; set; }
    }
}
