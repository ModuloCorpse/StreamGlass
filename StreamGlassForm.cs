using Quicksand.Web;
using System.Diagnostics;
using Timer = System.Windows.Forms.Timer;

namespace StreamGlass
{
    public partial class StreamGlassForm : Form
    {
        private readonly Stopwatch m_Watch = new();
        private readonly Server m_WebServer = new();

        public StreamGlassForm()
        {
            InitializeComponent();
            m_WebServer.AddResource<Resources.Index>("/");
            m_WebServer.AddResource<Resources.TestCodeDrivenHTMLPage>("/test", "Document");
            m_WebServer.AddResource<Resource>("/ramo");
            m_WebServer.AddResource("/js", "D:\\Programmation\\WebOverlay\\Js");
            m_WebServer.AddResource("/", "D:\\Programmation\\WebOverlay\\favicon.ico", true);
            m_WebServer.StartListening();

            Timer timer = new()
            {
                Interval = 50 //milliseconds
            };
            timer.Tick += StreamGlassForm_Tick;
            m_Watch.Start();
            timer.Start();
        }

        private void StreamGlassForm_Tick(object? sender, EventArgs e)
        {
            m_Watch.Stop();
            m_WebServer.Update(m_Watch.ElapsedMilliseconds);
            m_Watch.Reset();
            m_Watch.Start();
        }
    }
}