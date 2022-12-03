using System.Windows.Media;
using System;

namespace StreamGlass.UI
{
    public class BrushPaletteManager : ManagedObject.Manager<BrushPalette>
    {
        public BrushPaletteManager() : base("./themes") { }

        public BrushPalette NewDefaultPalette(string id, string name, BrushPalette.Type type)
        {
            BrushPalette palette = new(id, name, type);
            AddObject(palette);
            return palette;
        }

        public BrushPalette NewPalette(string name, BrushPalette.Type type)
        {
            BrushPalette palette = new(name, type);
            AddObject(palette);
            return palette;
        }

        public bool TryGetColor(string name, out Brush color)
        {
            if (CurrentObject != null)
                return CurrentObject.TryGetColor(name, out color);
            color = new SolidColorBrush(Colors.Black);
            return false;
        }

        public BrushPalette.Type GetPaletteType()
        {
            if (CurrentObject != null)
                return CurrentObject.PaletteType;
            return BrushPalette.Type.NONE;
        }

        public void SetCurrentPalette(string id) => SetCurrentObject(id);

        protected override BrushPalette? DeserializeObject(Json obj) => new(obj);
    }
}
