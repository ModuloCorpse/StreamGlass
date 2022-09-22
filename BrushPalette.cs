using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace StreamGlass
{
    public class BrushPalette: ManagedObject<BrushPalette>
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

        public BrushPalette(string name, Type type): base(name) => m_Type = type;
        internal BrushPalette(JObject json): base(json) {}

        public void AddHexColor(string name, string hex)
        {
            if (hex[0] != '#')
                hex = "#" + hex;
            Brush? color = (Brush?)m_Converter.ConvertFrom(hex);
            if (color != null)
                m_Palette[name] = color;
        }

        public void AddColor(string name, Brush color) => m_Palette[name] = color;

        public Brush GetColor(string name)
        {
            if (m_Palette.TryGetValue(name, out var color))
                return color;
            if (Parent != null)
                return Parent.GetColor(name);
            throw new Exception();
        }

        protected override void Save(ref JObject json)
        {
            JObject obj = new();
            foreach (var color in m_Palette)
                obj[color.Key] = m_Converter.ConvertToString(color.Value);
            json["colors"] = obj;
            json["type"] = (int)m_Type;
        }

        protected override void Load(JObject json)
        {
            int? type = (int?)json["type"];
            if (type != null)
                m_Type = (Type)type;
            JObject? colors = (JObject?)json["colors"];
            if (colors != null)
            {
                foreach (var color in colors)
                {
                    string? colorValue = (string?)color.Value;
                    if (colorValue != null)
                    {
                        Brush? brush = (Brush?)m_Converter.ConvertFrom(colorValue);
                        if (brush != null)
                            m_Palette[color.Key] = brush;
                    }
                }
            }
        }
    }
}
