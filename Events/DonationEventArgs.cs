using CorpseLib.StructuredText;

namespace StreamGlass.Events
{
    public class DonationEventArgs
    {
        private readonly Text m_Message;
        private readonly string m_Name;
        private readonly string m_Currency;
        private readonly float m_Amount;

        public string Name => m_Name;
        public float Amount => m_Amount;
        public string Currency => m_Currency;
        public Text Message => m_Message;

        public DonationEventArgs(string name, float amount, string currency, Text message)
        {
            m_Name = name;
            m_Amount = amount;
            m_Currency = currency;
            m_Message = message;
        }
    }
}
