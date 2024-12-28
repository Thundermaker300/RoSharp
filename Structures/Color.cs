using System;

namespace RoSharp.Structures
{
    /// <summary>
    /// Struct representing a color.
    /// </summary>
    public struct Color
    {
        private static Dictionary<string, Color> BuiltIn { get; } = new()
        {
            ["Black"] = FromHex("000000"),
            ["Red"] = FromHex("FF0000"),
            ["Green"] = FromHex("00FF00"),
            ["Blue"] = FromHex("0000FF"),
        };

        private string hexCode = "000000";
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

        /// <summary>
        /// Gets the hue component of the color.
        /// </summary>
        public int H => hsvCombo.Item1;

        /// <summary>
        /// Gets the saturation component of the color.
        /// </summary>
        public int S => hsvCombo.Item2;

        /// <summary>
        /// Gets the value component of the color.
        /// </summary>
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

        /// <summary>
        /// Gets this color's name if <see cref="IsNamed"/> is <see langword="true"/>.
        /// </summary>
        public string? Name
        {
            get
            {
                int hex = HexInt;
                KeyValuePair<string, Color>? pair = BuiltIn.FirstOrDefault(x => x.Value.HexInt == hex);
                if (pair.HasValue)
                    return pair.Value.Key;
                return null;
            }
        }

        /// <summary>
        /// Gets if this color is named.
        /// </summary>
        public bool IsNamed => Name is not null;

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
            // Code sample from: https://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
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

        /// <summary>
        /// Creates a new <see cref="Color"/>. Identical to <see cref="Black"/>.
        /// </summary>
        public Color() { }

        /// <summary>
        /// Creates a new color given a color hexadecimal code.
        /// </summary>
        /// <param name="hex">The code.</param>
        /// <returns>A new <see cref="Color"/> object.</returns>
        /// <exception cref="InvalidOperationException">Invalid color.</exception>
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

        /// <summary>
        /// Creates a new color given the color's Red, Blue, and Green components.
        /// </summary>
        /// <param name="r">The red component of the color.</param>
        /// <param name="g">The green component of the color.</param>
        /// <param name="b">The blue component of the color.</param>
        /// <returns>A new <see cref="Color"/> object.</returns>
        /// <exception cref="InvalidOperationException">Invalid color.</exception>
        public static Color FromRGB(byte r, byte g, byte b)
        {
            Color c = new();
            c.rgbCombo = (r, g, b);

            c.hexCode = c.GetHex();
            c.hsvCombo = c.GetHSV();

            c.Validate();
            return c;
        }

        /// <summary>
        /// Creates a new color given the color's Hue, Saturation, and Value components.
        /// </summary>
        /// <param name="h">The hue component of the color.</param>
        /// <param name="s">The saturation component of the color.</param>
        /// <param name="v">The value component of the color.</param>
        /// <returns>A new <see cref="Color"/> object.</returns>
        /// <exception cref="InvalidOperationException">Invalid color.</exception>
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
        public static Color Black { get; } = BuiltIn["Black"];

        /// <summary>
        /// Represents a color with HexCode <c>FF0000</c>.
        /// </summary>
        public static Color Red { get; } = BuiltIn["Red"];

        /// <summary>
        /// Represents a color with HexCode <c>00FF00</c>.
        /// </summary>
        public static Color Green { get; } = BuiltIn["Green"];

        /// <summary>
        /// Represents a color with HexCode <c>0000FF</c>.
        /// </summary>
        public static Color Blue { get; } = BuiltIn["Blue"];


        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Color [{HexCode}] ({RGBString})";
        }
    }
}
