// FileIconConverter.cs
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Lab_27 // Убедитесь, что пространство имен совпадает
{
    public class FileIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DirectoryInfo)
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resources/folder.png"));
            }
            else if (value is FileInfo fileInfo)
            {
                switch (fileInfo.Extension.ToLower())
                {
                    case ".txt":
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/textfile.png"));
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/imagefile.png"));
                    default:
                        return new BitmapImage(new Uri("pack://application:,,,/Resources/file.png"));
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}