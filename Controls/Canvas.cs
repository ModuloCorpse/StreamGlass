using System.Windows;

namespace StreamGlass.Controls
{
    public class Canvas : System.Windows.Controls.Canvas, IUIElement
    {
        public void Update(BrushPaletteManager palette)
        {
            foreach (UIElement element in Children)
            {
                if (element is IUIElement updatable)
                    updatable.Update(palette);
            }
        }
    }
}
