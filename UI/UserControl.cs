namespace StreamGlass.UI
{
    public class UserControl : System.Windows.Controls.UserControl, IUIElement
    {
        public void Update(BrushPaletteManager palette, TranslationManager translation)
        {
            if (Content is IUIElement updatable)
                updatable.Update(palette, translation);
            OnUpdate(palette, translation);
        }

        protected virtual void OnUpdate(BrushPaletteManager palette, TranslationManager translation) { }
    }
}
