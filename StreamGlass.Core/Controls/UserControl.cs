namespace StreamGlass.Core.Controls
{
    public class UserControl : System.Windows.Controls.UserControl, IUIElement
    {
        public Window GetWindow() => (Window)System.Windows.Window.GetWindow(this);

        public void Update(BrushPaletteManager palette)
        {
            if (Content is IUIElement updatable)
                updatable.Update(palette);
            OnUpdate(palette);
        }

        protected virtual void OnUpdate(BrushPaletteManager palette) { }
    }
}
