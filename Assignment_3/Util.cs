using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Assignment_3 {
	class Util {
		#region Color functions
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

		public static Color SimilarColor (Color inputColor, float maxDifference) {
			var diff = (int) (maxDifference*255);
			
			var r = Limit((int) (inputColor.R + diff*((Game1.GameRand.NextDouble()*2f) - 1.0f)), 0, 255);
			var g = Limit((int) (inputColor.G + diff*((Game1.GameRand.NextDouble()*2f) - 1.0f)), 0, 255);
			var b = Limit((int) (inputColor.B + diff*((Game1.GameRand.NextDouble()*2f) - 1.0f)), 0, 255);

			return Color.FromNonPremultiplied(r, g, b, 255);
		}

		public static Color MuteColor(Color input, float amount) {
			return Color.FromNonPremultiplied(
				Limit((int) (input.R*(1.0f - amount)), 0, 255),
				Limit((int) (input.G*(1.0f - amount)), 0, 255),
				Limit((int) (input.B*(1.0f - amount)), 0, 255),
				input.A);
		}

		public static Color RandomShadeOfGrey(float fromWhite) {
			var val = 255 - ((int)(Game1.GameRand.NextDouble() * (255 * fromWhite)));
			return Color.FromNonPremultiplied(val, val, val, 255);
		}
		#endregion

		#region Math/Conversion functions
		public static T Limit<T>(T input, T min, T max) {
			if (Comparer<T>.Default.Compare(input, max) > 0) return max;
			return Comparer<T>.Default.Compare(min, input) > 0 ? min : input;
		}

		public static Rectangle RectFToRect(RectangleF input) {
			return new Rectangle((int)input.X, (int)input.Y, (int)input.Width, (int)input.Height);
		}
		#endregion

		#region Drawing functions
		//Effectively draws a white line between two points
		public static void DrawLine(SpriteBatch sb, Vector2 a, Vector2 b, float thickness, Color color) {
			var tan = b - a;
			var rotation = (float)Math.Atan2(tan.Y, tan.X);

			var middlePoint = new Vector2(0, Game1.OnePxWhite.Height / 2f);
			var scale = new Vector2(tan.Length(), thickness);

			sb.Draw(Game1.OnePxWhite, a, null, color, rotation, middlePoint, scale, SpriteEffects.None, 0f);
		}

		public static void DrawSkewedRectHor(SpriteBatch sb, Rectangle rect, float skewRightByPix, Color col) {
			for (var i = 0; i < rect.Height; i++) {
				var skewAmount = Math.Round((Convert.ToDouble(i) / Convert.ToDouble(rect.Height)) * skewRightByPix, MidpointRounding.AwayFromZero);

				sb.Draw(Game1.OnePxWhite,
					new Rectangle(rect.X + (int)(skewRightByPix - skewAmount),
						rect.Y + i, rect.Width, 1), col);
			}
		}
		public static void DrawSkewedRectVert(SpriteBatch sb, Rectangle rect, float skewUpByPix, Color col) {
			for (var i = 0; i < rect.Width; i++) {
				var skewAmount = (Convert.ToDouble(i) / Convert.ToDouble(rect.Width)) * skewUpByPix;

				sb.Draw(Game1.OnePxWhite, new Rectangle(rect.X + i, rect.Y - (int)skewAmount, 1, rect.Height), col);
			}
		}

		public static void DrawCube(SpriteBatch sb, Rectangle frontRect, int depth, float horiSkew, float vertSkew, Color front, Color top, Color side) {
			var depthVertSkew = (int)Math.Abs(depth * vertSkew);
			var depthHoriSkew = (int)Math.Abs(depth * horiSkew);

			//Draw top
			if (vertSkew < 0f) {
				DrawSkewedRectHor(sb, new Rectangle(frontRect.X, (frontRect.Y - depthVertSkew), frontRect.Width, depthVertSkew),
				                       depth * horiSkew, top);
				if (horiSkew > 0f)
					sb.Draw(Game1.OnePxWhite, new Rectangle(frontRect.Right, frontRect.Top - depthVertSkew, depthHoriSkew, depthVertSkew), top) ;
			}
			else { //Draw bottom
				DrawSkewedRectHor(sb, new Rectangle((int)(frontRect.X + depth * horiSkew), frontRect.Bottom, frontRect.Width, depthVertSkew),
									   -(depth * horiSkew), top);

				if (horiSkew > 0f)
					sb.Draw(Game1.OnePxWhite, new Rectangle(frontRect.Right, frontRect.Bottom, depthHoriSkew, depthVertSkew), top);
			}
			
			if (horiSkew > 0f) { //Right
				DrawSkewedRectVert(sb,
								   new Rectangle(frontRect.Right, frontRect.Y, depthHoriSkew, frontRect.Height),
								   (vertSkew > 0f ? -depthVertSkew : depthVertSkew), side);
			}
			else { //Left
				DrawSkewedRectVert(sb,
								   new Rectangle((frontRect.X - depthHoriSkew), (frontRect.Y - (vertSkew > 0f ? -depthVertSkew : depthVertSkew)), depthHoriSkew, frontRect.Height),
								   (vertSkew < 0f ? -depthVertSkew : depthVertSkew), side);
			}

			sb.Draw(Game1.OnePxWhite, frontRect, front);
		}
		//Draws a box
		public static void DrawBox(SpriteBatch sb, Rectangle rect, float lineWidth, Color col) {
			DrawLine(sb, new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width, rect.Y), lineWidth, col); //Top
			DrawLine(sb, new Vector2(rect.X, rect.Y + rect.Height), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), lineWidth, col); //Bottom

			DrawLine(sb, new Vector2(rect.X, rect.Y), new Vector2(rect.X, rect.Y + rect.Height), lineWidth, col); //Left
			DrawLine(sb, new Vector2(rect.X + rect.Width, rect.Y), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), lineWidth, col); //Right
		}

		//Draws a polygon out of lines
		public static void DrawPoly(SpriteBatch sb, float lineWidth, Color col, params Vector2[] points) {
			if (points.Length <= 1) return;

			for (var i = 0; i < points.Length-1; i++) {
				var p = points[i];
				var pP = points[i + 1];

				DrawLine(sb, p, pP, lineWidth, col);
			}
		}
		
		//Uses the below DrawFont() function to draw multiple lines to the screen
		public static void DrawFontMultiLine(SpriteBatch sb, object text, Vector2 location, Color color, float maxWidth, float size = 32f,
			StringAlignment stringAlignment = StringAlignment.Left, StringAlignmentVert stringAlignmentVert = StringAlignmentVert.Below
		) {
			var maxLineChars = (int)(maxWidth / size);
			var rows = new List<string>();
			var splitWords = text.ToString().Split(new[] { ' ' });
			var currentLine = "";
			var negOffset = 0f;

			//Goes through each of the words in the string and adds them to the rows list
			//to split the words into rows that fit into MaxWidth
			foreach (var word in splitWords) {
				if (currentLine.Length < maxLineChars && (currentLine + " " + word).Length < maxLineChars) {
					currentLine += " " + word;
				}
				else {
					rows.Add(currentLine.Trim());
					currentLine = word;
				}
			}

			//Add the final row
			rows.Add(currentLine.Trim());

			//Add offsets for each of the types of vertical string alignment
			switch (stringAlignmentVert) {
				case StringAlignmentVert.Above: negOffset = rows.Count * size; break;
				case StringAlignmentVert.Center: negOffset = (size * rows.Count) / 2; break;
			}

			//Draw each of the rows
			for (var i = 0; i < rows.Count; i++) {
				DrawFont(sb, rows[i],
					new Vector2(location.X, location.Y - negOffset + (i * size)), color, size, stringAlignment);
			}
		}

		//Draws a string to the screen using the specified SpriteBatch
		public static void DrawFont(SpriteBatch sb, object text, Vector2 location, Color color, float size = 32f, StringAlignment stringAlignment = StringAlignment.Left) {
			//Convert to a char array of uppercase characters to simplify the below process
			var inputCharacters = text.ToString().ToUpper().ToCharArray();

			//Sets the distance the string's placement should be moved to the left
			//based on the stringAlignment parameter - if it's set to align Right, 
			//move the entire string to the left side of the draw position,
			//if it's centered, move it half way to the left, otherwise don't do anything
			//Saves time and magic numbers with setting the position for drawn strings
			var negOffset = 0f;
			switch (stringAlignment) {
				case StringAlignment.Right: negOffset = text.ToString().Length * size; break;
				case StringAlignment.Center: negOffset = (text.ToString().Length * size) / 2; break;
			}

			//Iterate through each character
			for (var i = 0; i < inputCharacters.Length; i++) {
				int charValue;
				//Use the ASCII spec to work out what type of character this is
				//0-9 fall on ASCII 48-57, A-Z are on 65-90, a-z are on 97-122
				//Due to the ToUpper() above, checking a-z is not required
				if (inputCharacters[i] > 47 && inputCharacters[i] < 58) //Character is a number
					charValue = 1 + (inputCharacters[i] - 48);
				else if (inputCharacters[i] > 64 && inputCharacters[i] < 91) //Capitalized letters
					charValue = 11 + (inputCharacters[i] - 65);
				else switch (inputCharacters[i]) {
					case '!':
						charValue = 37;
						break;
					case '.':
						charValue = 38;
						break;
					case ',':
						charValue = 39;
						break;
					case '?':
						charValue = 40;
						break;
					case '~': //Special character; draws small "end" diagonally, for name input
						charValue = 41;
						break;
					default:
						charValue = 0;
						break;
				}

				//Draw the character to the screen
				sb.Draw(Game1.GameFont,
					new Rectangle((int)((location.X - negOffset) + (i * size)), (int)location.Y, (int)size, (int)size),
					new Rectangle((charValue * 32), 0, 32, 32),
					color);
			}
		}
		#endregion
	}

	//Enums
	public enum StringAlignment { Left, Center, Right }
	public enum StringAlignmentVert { Above, Center, Below }

	public struct Size {
		public int Width;
		public int Height;
		public Size(int w, int h) {Width = w;Height = h;}
	}
}
