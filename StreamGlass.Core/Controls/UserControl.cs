namespace StreamGlass.Core.Controls
{
    public class UserControl : System.Windows.Controls.UserControl, IUIElement
    {
        public Window? GetWindow() { try { return (Window)System.Windows.Window.GetWindow(this); } catch { return null; } }

        public void Update(BrushPaletteManager palette)
        {
            if (Content is IUIElement updatable)
                updatable.Update(palette);
            OnUpdate(palette);
        }

        protected virtual void OnUpdate(BrushPaletteManager palette) { }
    }
}
