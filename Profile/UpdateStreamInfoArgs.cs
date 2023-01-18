namespace StreamGlass.Profile
{
    public class UpdateStreamInfoArgs
    {
        public readonly string Title;
        public readonly string Description;
        public readonly CategoryInfo Category;
        public readonly string Language;

        public UpdateStreamInfoArgs(string title, string description, CategoryInfo category, string language)
        {
            Title = title;
            Description = description;
            Category = category;
            Language = language;
        }
    }
}
