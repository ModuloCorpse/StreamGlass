using System;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace StreamGlass
{
    public partial class SplashScreen : System.Windows.Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        public new void Close() => Dispatcher.Invoke(base.Close);

        public void UpdateProgressBar(float percent)
        {
            Dispatcher.Invoke(() =>
            {
                DoubleAnimation doubleanimation = new(100 - percent, new(TimeSpan.FromMilliseconds(100)));
                SplashProgressBar.BeginAnimation(RangeBase.ValueProperty, doubleanimation);
            });
        }
    }
}
