using CorpseLib.Encryption;

namespace StreamGlass.Core
{
    public static class StreamGlassVault
    {
        private static readonly LocalVault ms_Instance = new(new WindowsEncryptionAlgorithm([95, 239, 5, 252, 160, 29, 242, 88, 31, 3]));
        public static LocalVault Vault => ms_Instance;
        public static void SetPassword(string password) => ms_Instance.SetPassword(password);
        public static string Load(string key) => ms_Instance.Load(key);
        public static void Store(string key, string value) => ms_Instance.Store(key, value);
    }
}
