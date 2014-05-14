using Microsoft.Xna.Framework;

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
	}
}
