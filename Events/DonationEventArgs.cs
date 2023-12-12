using CorpseLib.Json;
using CorpseLib;
using CorpseLib.StructuredText;
using TwitchCorpse;

namespace StreamGlass.Events
{
    public class DonationEventArgs
    {
        public class JSerializer : AJSerializer<DonationEventArgs>
        {
            protected override OperationResult<DonationEventArgs> Deserialize(JObject reader)
            {
                if (reader.TryGet("message", out Text? message) &&
                    reader.TryGet("user", out TwitchUser? user) &&
                    reader.TryGet("currency", out string? currency) &&
                    reader.TryGet("amount", out float? amount))
                    return new(new(user!, (float)amount!, currency!, message!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(DonationEventArgs obj, JObject writer)
            {
                writer["message"] = obj.m_Message;
                writer["user"] = obj.m_User;
                writer["currency"] = obj.m_Currency;
                writer["amount"] = obj.m_Amount;
            }
        }

        private readonly Text m_Message;
        private readonly TwitchUser m_User;
        private readonly string m_Currency;
        private readonly float m_Amount;

        public TwitchUser User => m_User;
        public float Amount => m_Amount;
        public string Currency => m_Currency;
        public Text Message => m_Message;

        public DonationEventArgs(TwitchUser user, float amount, string currency, Text message)
        {
            m_User = user;
            m_Amount = amount;
            m_Currency = currency;
            m_Message = message;
        }
    }
}
