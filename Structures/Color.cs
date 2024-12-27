namespace RoSharp.Structures
{
    public readonly struct Color
    {
        private readonly string hexCode;
        private readonly (byte, byte, byte) rgbCombo;

        public string HexCode => hexCode;
        public byte R => rgbCombo.Item1;
        public byte G => rgbCombo.Item2;
        public byte B => rgbCombo.Item3;

        private void Validate()
        {
            int rgbInt = Convert.ToInt32(HexCode, 16);
            if (rgbInt < 0 || rgbInt > 16777215)
            {
                throw new InvalidOperationException("Invalid color.");
            }
        }

        private (byte, byte, byte) GetRGB()
        {
            int rgbInt = Convert.ToInt32(HexCode, 16);
            byte red = (byte)((rgbInt >> 16) & 255);
            byte green = (byte)((rgbInt >> 8) & 255);
            byte blue = (byte)(rgbInt & 255);
            return (red, green, blue);
        }

        private string GetHex()
        {
            return $"{R:X2}{G:X2}{B:X2}";
        }

        public Color() : this("000000") { }

        public Color(string hex)
        {
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
    }
}
