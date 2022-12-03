namespace StreamGlass.UI
{
    public class Dialog : Window
    {
        public Dialog(Window parent): base(parent.GetBrushPalette(), parent.GetTranslations())
        {
            Owner = parent;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
        }
    }
}
