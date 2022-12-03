namespace StreamGlass.Profile
{
    public class UpdateStreamInfoArgs
    {
        public readonly string Title;
        public readonly string Description;
        public readonly string Game;
        public readonly string Language;

        public UpdateStreamInfoArgs(string title, string description, string game, string language)
        {
            Title = title;
            Description = description;
            Game = game;
            Language = language;
        }
    }
}
