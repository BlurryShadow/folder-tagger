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
        [Key]
        [Column(Order = 0)]
        [StringLength(260)]
        public string Location { get; set; }

        [StringLength(50)]
        public string Group { get; set; }

        [StringLength(50)]
        public string Artist { get; set; }

        public bool Translated { get; set; }

        [StringLength(50)]
        public string Type { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TagID { get; set; }

        [StringLength(260)]
        public string Thumbnail { get; set; }

        public virtual Tag Tag { get; set; }

        public Folder(string location)
        {
            Location = location;
            Translated = false;
            TagID = 1;
        }
    }
}
