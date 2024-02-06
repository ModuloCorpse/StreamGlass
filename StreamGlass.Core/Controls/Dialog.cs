namespace StreamGlass.Core.Controls
{
    public class Dialog : Window
    {
        public Dialog(Window parent): base(parent.GetBrushPalette())
        {
            Owner = parent;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
        }

        protected void OnOkClick()
        {
            DialogResult = true;
            Close();
        }

        protected void OnCancelClick()
        {
            Close();
        }
    }
}
