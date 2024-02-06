using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace StreamGlass
{
    /// <summary>
    /// Interaction logic for StreamGlassSplashScreen.xaml
    /// </summary>
    public partial class StreamGlassSplashScreen : Window
    {
        public StreamGlassSplashScreen()
        {
            InitializeComponent();
        }

        public new void Close() => Dispatcher.Invoke(base.Close);

        public void UpdateProgressBar(float percent)
        {
            Dispatcher.Invoke(() =>
            {
                DoubleAnimation doubleanimation = new(100 - percent, new(TimeSpan.FromMilliseconds(100)));
                SplashProgressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
            });
        }
    }
}
