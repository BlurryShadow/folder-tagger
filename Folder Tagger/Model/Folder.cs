namespace Folder_Tagger
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Folder")]
    public partial class Folder
    {
        public int ID { get; set; }

        [Required]
        [StringLength(260)]
        public string Location { get; set; }

        [StringLength(260)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Group { get; set; }

        [StringLength(50)]
        public string Artist { get; set; }

        public int TagID { get; set; }

        [StringLength(260)]
        public string Thumbnail { get; set; }

        public virtual Tag Tag { get; set; }

        public Folder(string location, string name, string thumbnail = null, int tagID = 1)
        {
            Location = location;
            Name = name;
            TagID = tagID;
            Thumbnail = thumbnail;
        }

        public Folder() { }
    }
}
