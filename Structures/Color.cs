using System;

namespace RoSharp.Structures
{
    /// <summary>
    /// Struct representing a color.
    /// </summary>
    public struct Color
    {

        private string hexCode;
        private (byte, byte, byte) rgbCombo;
        private (int, int, int) hsvCombo;

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

        public int H => hsvCombo.Item1;
        public int S => hsvCombo.Item2;
        public int V => hsvCombo.Item3;

        /// <summary>
        /// Gets a string that can be used to display the R, G, and B values.
        /// </summary>
        public string RGBString => $"{R},{G},{B}";

        /// <summary>
        /// Gets a string that can be used to display the H, S, and V values.
        /// </summary>
        public string HSVString => $"{H},{S},{V}";

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

        // Requires hex code
        private (byte, byte, byte) GetRGB()
        {
            byte red = (byte)((HexInt >> 16) & 255);
            byte green = (byte)((HexInt >> 8) & 255);
            byte blue = (byte)(HexInt & 255);
            return (red, green, blue);
        }

        // Requires HSV
        private (byte, byte, byte) GetRGBFromHSV()
        {
            var rgb = new int[3];

            int baseColor = (H + 60) % 360 / 120;
            int shift = (H + 60) % 360 - (120 * baseColor + 60);
            var secondaryColor = (baseColor + (shift >= 0 ? 1 : -1) + 3) % 3;

            //Setting Hue
            rgb[baseColor] = 255;
            rgb[secondaryColor] = (int)((Math.Abs(shift) / 60.0f) * 255.0f);

            //Setting Saturation
            for (var i = 0; i < 3; i++)
                rgb[i] += (int)((255 - rgb[i]) * ((100 - S) / 100.0f));

            //Setting Value
            for (var i = 0; i < 3; i++)
                rgb[i] -= (int)(rgb[i] * (100 - V) / 100.0f);

            byte r = (byte)rgb[0];
            byte g = (byte)rgb[1];
            byte b = (byte)rgb[2];

            return (r, g, b);
        }

        // Requires rgb
        private string GetHex()
        {
            return $"{R:X2}{G:X2}{B:X2}";
        }

        // Requires rgb
        private (int, int, int) GetHSV()
        {
            // Code sample from: https://www.geeksforgeeks.org/program-change-rgb-color-model-hsv-color-model/

            // R, G, B values are divided by 255 
            // to change the range from 0..255 to 0..1 
            double r = R / 255.0;
            double g = G / 255.0;
            double b = B / 255.0;

            // h, s, v = hue, saturation, value 
            double cmax = Math.Max(r, Math.Max(g, b)); // maximum of r, g, b 
            double cmin = Math.Min(r, Math.Min(g, b)); // minimum of r, g, b 
            double diff = cmax - cmin; // diff of cmax and cmin. 
            double h = -1, s = -1;

            // if cmax and cmax are equal then h = 0 
            if (cmax == cmin)
                h = 0;

            // if cmax equal r then compute h 
            else if (cmax == r)
                h = (60 * ((g - b) / diff) + 360) % 360;

            // if cmax equal g then compute h 
            else if (cmax == g)
                h = (60 * ((b - r) / diff) + 120) % 360;

            // if cmax equal b then compute h 
            else if (cmax == b)
                h = (60 * ((r - g) / diff) + 240) % 360;

            // if cmax equal zero 
            if (cmax == 0)
                s = 0;
            else
                s = (diff / cmax) * 100;

            // compute v 
            double v = cmax * 100;

            return ((int)Math.Round(h), (int)Math.Round(s), (int)Math.Round(v));
        }

        public Color() { }

        public static Color FromHex(string hex)
        {
            Color c = new();
            hex = hex.Replace("#", string.Empty);

            c.hexCode = hex;

            c.rgbCombo = c.GetRGB();
            c.hsvCombo = c.GetHSV();

            c.Validate();
            return c;
        }

        public static Color FromRGB(byte r, byte g, byte b)
        {
            Color c = new();
            c.rgbCombo = (r, g, b);

            c.hexCode = c.GetHex();
            c.hsvCombo = c.GetHSV();

            c.Validate();
            return c;
        }

        public static Color FromHSV(int h, int s, int v)
        {
            Color c = new();
            c.hsvCombo = (h, s, v);

            c.rgbCombo = c.GetRGBFromHSV();
            c.hexCode = c.GetHex();

            c.Validate();
            return c;
        }


        // Static colors

        /// <summary>
        /// Represents a color with HexCode <c>000000</c>.
        /// </summary>
        public static Color Black { get; } = FromHex("000000");

        /// <summary>
        /// Represents a color with HexCode <c>FF0000</c>.
        /// </summary>
        public static Color Red { get; } = FromHex("FF0000");


        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Color [{HexCode}] ({RGBString})";
        }
    }
}
