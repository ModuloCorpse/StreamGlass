using System.Windows.Media;

namespace StreamGlass.Core.StreamChat
{
    public class ChatUserInfo(string name, uint userType)
    {
        private readonly string m_Name = name;
        private readonly uint m_UserType = userType;
        private Color? m_Color;

        public string Name => m_Name;
        public uint UserType => m_UserType;
        public Color? Color => m_Color;

        public void SetColor(Color? color) => m_Color = color;
    }
}
