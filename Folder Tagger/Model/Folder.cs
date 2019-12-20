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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Folder()
        {
            Tags = new HashSet<Tag>();
        }

        [Key]
        [StringLength(260)]
        public string Location { get; set; }

        [StringLength(260)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Group { get; set; }

        [StringLength(50)]
        public string Artist { get; set; }

        [StringLength(260)]
        public string Thumbnail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tag> Tags { get; set; }

        public Folder(string location, string name, string thumbnail = null)
        {
            Tags = new HashSet<Tag>();

            Location = location;
            Name = name;
            Thumbnail = thumbnail;
        }
    }
}
