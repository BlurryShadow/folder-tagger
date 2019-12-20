namespace Folder_Tagger
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
        {
        }

        public virtual DbSet<Folder> Folders { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Folder>()
                .HasMany(e => e.Tags)
                .WithMany(e => e.Folders)
                .Map(m => m.ToTable("FolderTag").MapLeftKey("FolderLocation").MapRightKey("TagID"));
        }
    }
}
