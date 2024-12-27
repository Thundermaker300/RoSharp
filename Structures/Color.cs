namespace RoSharp.Structures
{
    /// <summary>
    /// Struct representing a color.
    /// </summary>
    public readonly struct Color
    {

        private readonly string hexCode;
        private readonly (byte, byte, byte) rgbCombo;

        /// <summary>
        /// Gets the hex code representing the color.
        /// </summary>
        public string HexCode => hexCode;

        /// <summary>
        /// Gets the red component of the color.
        /// </summary>
        public byte R => rgbCombo.Item1;

        /// <summary>
        /// Gets the green component of the color.
        /// </summary>
        public byte G => rgbCombo.Item2;

        /// <summary>
        /// Gets the blue component of the color.
        /// </summary>
        public byte B => rgbCombo.Item3;

        /// <summary>
        /// Gets a string that can be used to display the R, G, and B values.
        /// </summary>
        public string RGBString => $"{R},{G},{B}";

        /// <summary>
        /// Gets an integer representing the specific <see cref="HexCode"/>.
        /// </summary>
        public int HexInt => Convert.ToInt32(HexCode, 16);

        private void Validate()
        {
            if (HexInt < 0 || HexInt > 16777215)
            {
                throw new InvalidOperationException("Invalid color.");
            }
        }

        private (byte, byte, byte) GetRGB()
        {
            byte red = (byte)((HexInt >> 16) & 255);
            byte green = (byte)((HexInt >> 8) & 255);
            byte blue = (byte)(HexInt & 255);
            return (red, green, blue);
        }

        private string GetHex()
        {
            return $"{R:X2}{G:X2}{B:X2}";
        }

        public Color() : this("000000") { }

        public Color(string hex)
        {
            hex = hex.Replace("#", string.Empty);

            hexCode = hex;

            rgbCombo = GetRGB();

            Validate();
        }

        public Color(byte r, byte g, byte b)
        {
            rgbCombo = (r, g, b);

            hexCode = GetHex();

            Validate();
        }


        // Static colors

        /// <summary>
        /// Represents a color with HexCode <c>000000</c>.
        /// </summary>
        public static Color Black { get; } = new();

        /// <summary>
        /// Represents a color with HexCode <c>FF0000</c>.
        /// </summary>
        public static Color Red { get; } = new("FF0000");
    }
}
