using CorpseLib.DataNotation;
using CorpseLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StreamGlass.Core.StreamChat
{
    internal class ColorDataSerializer : ADataSerializer<Color>
    {
        protected override OperationResult<Color> Deserialize(DataObject reader)
        {
            if (reader.TryGet("r", out byte? r) &&
                reader.TryGet("g", out byte? g) &&
                reader.TryGet("b", out byte? b) &&
                reader.TryGet("a", out byte? a))
                return new(Color.FromArgb((byte)a!, (byte)r!, (byte)g!, (byte)b!));
            return new("Bad json", string.Empty);
        }

        protected override void Serialize(Color obj, DataObject writer)
        {
            writer["r"] = obj.R;
            writer["g"] = obj.G;
            writer["b"] = obj.B;
            writer["a"] = obj.A;
        }
    }
}
