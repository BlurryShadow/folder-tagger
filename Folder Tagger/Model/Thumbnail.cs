namespace Folder_Tagger
{
    class Thumbnail
    {
        public string Folder { get; set; }
        public string Root { get; set; }
        public string Name { get; set; }

        public Thumbnail(string folder, string root, string name)
        {
            Folder = folder;
            Root = root;
            Name = name;
        }

        public Thumbnail() { }
    }
}
