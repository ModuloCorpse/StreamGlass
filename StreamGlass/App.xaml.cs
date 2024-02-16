using System.Threading;
using System.Windows;

namespace StreamGlass
{
    public partial class App : Application
    {
        private SplashScreen? m_SplashScreen = null;
        private volatile bool m_IsSplashScreenOpen = false;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Thread newWindowThread = new(new ThreadStart(ThreadStartingPoint));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
            while (!m_IsSplashScreenOpen)
                Thread.Sleep(100);
            MainWindow window = new(m_SplashScreen!);
            window.Show();
        }

        private void ThreadStartingPoint()
        {
            m_SplashScreen = new();
            m_SplashScreen.Show();
            m_IsSplashScreenOpen = true;
            System.Windows.Threading.Dispatcher.Run();
        }
    }
}
