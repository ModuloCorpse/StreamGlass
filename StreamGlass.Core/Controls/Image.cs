using CorpseLib.Wpf;
using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Core.Controls
{
    public class Image : System.Windows.Controls.Image
    {
        public delegate string ConversionDelegate(object obj);


        #region SourcePath
        public static readonly DependencyProperty SourcePathProperty = Helper.NewProperty("SourcePath", string.Empty, (Image instance, string value) => instance.Property_SetSourcePath(value));
        [Description("Path of the image"), Category("Common Properties")]
        public string SourcePath { get => (string)GetValue(SourcePathProperty); set => SetValue(SourcePathProperty, value); }
        internal void Property_SetSourcePath(string source)
        {
            if (!string.IsNullOrWhiteSpace(source))
                Source = ImageLoader.LoadStaticImage(source)?.Source;
        }
        #endregion SourcePath
    }
}
