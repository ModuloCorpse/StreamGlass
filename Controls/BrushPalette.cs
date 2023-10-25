using CorpseLib.Json;
using CorpseLib.ManagedObject;
using System.Collections.Generic;
using System.Windows.Media;

namespace StreamGlass.Controls
{
    public class BrushPalette : Object<BrushPalette>
    {
        public enum Type
        {
            DARK,
            LIGHT,
            NONE
        }

        private Type m_Type;
        private readonly BrushConverter m_Converter = new();
        private readonly Dictionary<string, Brush> m_Palette = new();

        public Type PaletteType => m_Type;

        public BrushPalette(string id, string name, Type type) : base(id, name, false) => m_Type = type;
        public BrushPalette(string name, Type type) : base(name) => m_Type = type;
        internal BrushPalette(JObject json) : base(json) { }

        public void AddHexColor(string name, string hex)
        {
            if (hex[0] != '#')
                hex = "#" + hex;
            Brush? color = (Brush?)m_Converter.ConvertFrom(hex);
            if (color != null)
                m_Palette[name] = color;
        }

        public void AddColor(string name, Brush color) => m_Palette[name] = color;

        public bool TryGetColor(string name, out Brush color)
        {
            if (m_Palette.TryGetValue(name, out var ret))
            {
                color = ret;
                return true;
            }
            else if (Parent != null)
                return Parent.TryGetColor(name, out color);
            color = new SolidColorBrush(Colors.Black);
            return false;
        }

        protected override void Save(ref JObject json)
        {
            JObject obj = new();
            foreach (var color in m_Palette)
                obj.Add(color.Key, m_Converter.ConvertToString(color.Value));
            json.Add("colors", obj);
            json.Add("type", (int)m_Type);
        }

        protected override void Load(JObject json)
        {
            m_Type = json.GetOrDefault("type", Type.NONE);
            if (json.TryGet("colors", out JObject? colors))
            {
                foreach (var color in colors!)
                {
                    Brush? brush = (Brush?)m_Converter.ConvertFrom(color.Value.Cast<string>()!);
                    if (brush != null)
                        m_Palette[color.Key] = brush;
                }
            }
        }
    }
}
