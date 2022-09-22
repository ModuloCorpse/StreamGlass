using Newtonsoft.Json.Linq;
using System.Windows.Media;
using System;
using System.Xml.Linq;

namespace StreamGlass
{
    public class BrushPaletteManager : ObjectManager<BrushPalette>
    {
        public BrushPaletteManager() : base("chat_palettes.json") { }

        public BrushPalette NewPalette(string name, BrushPalette.Type type)
        {
            BrushPalette palette = new(name, type);
            AddObject(palette);
            return palette;
        }
        public Brush GetColor(string name)
        {
            if (CurrentObject != null)
                return CurrentObject.GetColor(name);
            throw new Exception();
        }

        public BrushPalette.Type GetPaletteType()
        {
            if (CurrentObject != null)
                return CurrentObject.PaletteType;
            throw new Exception();
        }

        public void SetCurrentPalette(string id) => SetCurrentObject(id);

        protected override BrushPalette? DeserializeObject(JObject obj) => new(obj);
    }
}
