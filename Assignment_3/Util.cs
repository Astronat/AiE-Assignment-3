using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Assignment_3 {
	class Util {
		/// <summary>
		/// Interpolates between two colors
		/// </summary>
		/// <param name="colorA">The first color</param>
		/// <param name="colorB">The second color</param>
		/// <param name="bAmount">The amount to interpolate; 0.0 for 100% color A, 1.0 for color B</param>
		/// <returns>The resulting Color</returns>
		public static Color ColorInterpolate (Color colorA, Color colorB, float bAmount) {
			var aAmount = 1.0f - bAmount;
			var r = (int)(colorA.R * aAmount + colorB.R * bAmount);
			var g = (int)(colorA.G * aAmount + colorB.G * bAmount);
			var b = (int)(colorA.B * aAmount + colorB.B * bAmount);

			return Color.FromNonPremultiplied(r, g, b, 255);
		}

		public static Rectangle RectFToRect(RectangleF input) {
			return new Rectangle((int)input.X, (int)input.Y, (int)input.Width, (int)input.Height);
		}

		//Effectively draws a white line between two points
		public static void DrawLine(SpriteBatch sb, Vector2 a, Vector2 b, float thickness, Color color) {
			var tan = b - a;
			var rotation = (float)Math.Atan2(tan.Y, tan.X);

			var middlePoint = new Vector2(0, Game1.OnePxWhite.Height / 2f);
			var scale = new Vector2(tan.Length(), thickness);

			sb.Draw(Game1.OnePxWhite, a, null, color, rotation, middlePoint, scale, SpriteEffects.None, 0f);
		}
	}

	public struct Size {
		public int Width;
		public int Height;
		public Size(int w, int h) {Width = w;Height = h;}
	}
}
