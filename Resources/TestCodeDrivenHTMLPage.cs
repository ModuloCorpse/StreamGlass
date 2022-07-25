using Quicksand.Web.Html;

namespace StreamGlass.Resources
{
    public class TestCodeDrivenHTMLPage : Overlay
    {
        private long m_DeltaTime = 0;
        private Div m_BlinkDiv;

        public TestCodeDrivenHTMLPage()
        {
            m_BlinkDiv = NewElement<Div>("blink");
            m_BlinkDiv.AddContent("Blink");
        }

        protected override void GenerateOverlay()
        {
            Body body = m_Page.GetBody();
            body.AddAttribute("onload", "wof.main()");
            body.AddChild(m_BlinkDiv);
            body.AddLineBreak();
            Tag gifDiv = NewElement("img", "slowpoke-gif");
            gifDiv.AddAttribute("src", "http://i.stack.imgur.com/SBv4T.gif");
            gifDiv.AddAttribute("alt", "this slowpoke moves");
            gifDiv.AddAttribute("width", 250);
            body.AddChild(gifDiv);
            body.AddLineBreak();
            body.AddContent("This is a ");
            Tag link = NewElement("a", "test-link");
            link.AddAttribute("href", "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            link.AddContent("link");
            body.AddChild(link);
            body.AddContent(" without line break");
            body.AddContent("\nThis is line 1\nThis is line 2\n");
            body.AddContent("This is another line");
        }

        protected override void UpdateOverlay(long deltaTime)
        {
            m_DeltaTime += deltaTime;
            while (m_DeltaTime >= 1000)
            {
                m_BlinkDiv.SetHidden(!m_BlinkDiv.IsHidden());
                m_DeltaTime -= 1000;
            }
        }

        protected override void WebsocketMessage(int clientID, string message)
        {
            if (message == "ping")
                Send(clientID, "pong");
            else
                MessageBox.Show(message);
        }
    }
}
