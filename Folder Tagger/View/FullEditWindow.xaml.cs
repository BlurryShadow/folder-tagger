using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Folder_Tagger
{
    /// <summary>
    /// Interaction logic for FullEditWindow.xaml
    /// </summary>
    public partial class FullEditWindow : Window
    {
        private string folder = "";
        public FullEditWindow(string folder)
        {
            InitializeComponent();
            this.folder = folder;
            loadTag();
        }

        private void loadTag()
        {
            using (var db = new Model1())
            {
                var query = db.Folders
                              .Join(db.Tags,
                                    f => f.TagID,
                                    t => t.TagID,
                                    (f, t) => new { Folder = f, Tag = t })
                              .Where(q => q.Folder.Location == folder)
                              .Select(q => q.Tag.TagName);

                var result = query.ToList();
                DataContext = result;
            }
        }
    }
}
