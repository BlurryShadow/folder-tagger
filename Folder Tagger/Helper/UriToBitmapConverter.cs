using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Folder_Tagger
{
    class UriToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            else
            {
                int heigth = (int)(double)App.Current.MainWindow.Resources["ThumbnailHeigth"];
                int width = (int)(double)App.Current.MainWindow.Resources["ThumbnailWidth"];

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.DecodePixelWidth = heigth;
                bi.DecodePixelWidth = width;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(value.ToString());
                bi.EndInit();
                return bi;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
