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
    /// Interaction logic for SmallEditWindow.xaml
    /// </summary>
    public partial class SmallEditWindow : Window
    {
        private string type = "";
        private string folder = "";
        public SmallEditWindow(string type, string folder)
        {
            InitializeComponent();
            this.type = type;
            this.folder = folder;
            tbInput.Text = type + " - " + folder;
        }
    }
}
