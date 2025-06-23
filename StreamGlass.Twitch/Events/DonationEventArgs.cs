using CorpseLib;
using CorpseLib.DataNotation;
using CorpseLib.StructuredText;
using TwitchCorpse.API;

namespace StreamGlass.Twitch.Events
{
    public class DonationEventArgs(TwitchUser user, float amount, string currency, Text message)
    {
        public class DataSerializer : ADataSerializer<DonationEventArgs>
        {
            protected override OperationResult<DonationEventArgs> Deserialize(DataObject reader)
            {
                if (reader.TryGet("message", out Text? message) &&
                    reader.TryGet("user", out TwitchUser? user) &&
                    reader.TryGet("currency", out string? currency) &&
                    reader.TryGet("amount", out float? amount))
                    return new(new(user!, (float)amount!, currency!, message!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(DonationEventArgs obj, DataObject writer)
            {
                writer["message"] = obj.m_Message;
                writer["user"] = obj.m_User;
                writer["currency"] = obj.m_Currency;
                writer["amount"] = obj.m_Amount;
            }
        }

        private readonly Text m_Message = message;
        private readonly TwitchUser m_User = user;
        private readonly string m_Currency = currency;
        private readonly float m_Amount = amount;

        public TwitchUser User => m_User;
        public float Amount => m_Amount;
        public string Currency => m_Currency;
        public Text Message => m_Message;
    }
}
