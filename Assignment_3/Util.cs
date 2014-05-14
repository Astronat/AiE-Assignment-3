using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Assignment_3 {
	class Util {
		public static Color ColorInterpolate (Color colorA, Color colorB, float bAmount) {
			var aAmount = 1.0f - bAmount;
			var r = (int)(colorA.R * aAmount + colorB.R * bAmount);
			var g = (int)(colorA.G * aAmount + colorB.G * bAmount);
			var b = (int)(colorA.B * aAmount + colorB.B * bAmount);

			return Color.FromNonPremultiplied(r, g, b, 255);
		}
	}
}
