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

		/// <summary>
		/// Generates a randomized color which is similar to the input color
		/// </summary>
		/// <param name="inputColor">the input color</param>
		/// <param name="maxDifference">a float from 0.0-1.0 determining the maximum difference</param>
		/// <returns></returns>
		public static Color SimilarColor (Color inputColor, float maxDifference) {
			var diff = (int) (maxDifference*255);
			
			var r = Limit((int) (inputColor.R + diff*((Game1.GameRand.NextDouble()*2f) - 1.0f)), 0, 255);
			var g = Limit((int) (inputColor.G + diff*((Game1.GameRand.NextDouble()*2f) - 1.0f)), 0, 255);
			var b = Limit((int) (inputColor.B + diff*((Game1.GameRand.NextDouble()*2f) - 1.0f)), 0, 255);

			return Color.FromNonPremultiplied(r, g, b, 255);
		}

		/// <summary>
		/// Generates a muted version of the input color
		/// </summary>
		/// <param name="input">the color to mute</param>
		/// <param name="amount">the amount to mute by</param>
		/// <returns></returns>
		public static Color MuteColor(Color input, float amount) {
			return Color.FromNonPremultiplied(
				Limit((int) (input.R*(1.0f - amount)), 0, 255),
				Limit((int) (input.G*(1.0f - amount)), 0, 255),
				Limit((int) (input.B*(1.0f - amount)), 0, 255),
				input.A);
		}

		/// <summary>
		/// Generates a monochrome color between white and black
		/// </summary>
		/// <param name="fromWhite">The maximum amount of difference from 255/255/255, flat white</param>
		/// <returns></returns>
		public static Color RandomShadeOfGrey(float fromWhite) {
			var val = 255 - ((int)(Game1.GameRand.NextDouble() * (255 * fromWhite)));
			return Color.FromNonPremultiplied(val, val, val, 255);
		}
		#endregion

		#region Math/Conversion functions
		/// <summary>
		/// Limits an input value to a minimum and maximum
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input">the input value</param>
		/// <param name="min">the minimum limit</param>
		/// <param name="max">the maximum limit</param>
		/// <returns></returns>
		public static T Limit<T>(T input, T min, T max) {
			if (Comparer<T>.Default.Compare(input, max) > 0) return max;
			return Comparer<T>.Default.Compare(min, input) > 0 ? min : input;
		}

		/// <summary>
		/// Effectively casts a RectangleF to a Rectangle
		/// </summary>
		/// <param name="input">the input RectangleF</param>
		/// <returns></returns>
		public static Rectangle RectFToRect(RectangleF input) {
			return new Rectangle((int)input.X, (int)input.Y, (int)input.Width, (int)input.Height);
		}
		#endregion

		#region Drawing functions
		/// <summary>
		/// Draws a line between two points
		/// </summary>
		/// <param name="sb">the SpriteBatch to draw with</param>
		/// <param name="a">the line's start position</param>
		/// <param name="b">the line's end position</param>
		/// <param name="thickness">the line thickness</param>
		/// <param name="color">the line color</param>
		public static void DrawLine(SpriteBatch sb, Vector2 a, Vector2 b, float thickness, Color color) {
			var tan = b - a;
			var rotation = (float)Math.Atan2(tan.Y, tan.X);

			var middlePoint = new Vector2(0, Game1.OnePxWhite.Height / 2f);
			var scale = new Vector2(tan.Length(), thickness);

			sb.Draw(Game1.OnePxWhite, a, null, color, rotation, middlePoint, scale, SpriteEffects.None, 0f);
		}

		/// <summary>
		/// Draws a line between two points
		/// </summary>
		/// <param name="sb">the SpriteBatch to draw with</param>
		/// <param name="a">the line's start position</param>
		/// <param name="b">the line's end position</param>
		/// <param name="thickness">the line thickness</param>
		/// <param name="color">the line color</param>
		public static void DrawGlowLine(SpriteBatch sb, Vector2 a, Vector2 b, float thickness, Color color) {
			var tan = b - a;
			var rotation = (float)Math.Atan2(tan.Y, tan.X);

			var middlePoint = new Vector2(0, Stage.LineGlow.Height / 2f);
			var scale = new Vector2(tan.Length(), thickness);

			sb.Draw(Stage.LineGlow, a, null, color, rotation, middlePoint, scale, SpriteEffects.None, 0f);
		}
		/// <summary>
		/// Draws a horizontally skewed rectangle
		/// </summary>
		/// <param name="sb">the SpriteBatch to draw to</param>
		/// <param name="rect">the base rectangle; at 0.0 skewRight this function will just draw this rectangle</param>
		/// <param name="skewRightByPix">the amount to skew the top by</param>
		/// <param name="col">the color to draw the rectangle as</param>
		public static void DrawSkewedRectHor(SpriteBatch sb, Rectangle rect, float skewRightByPix, Color col) {
			for (var i = 0; i < rect.Height; i++) {
				var skewAmount = Math.Round((Convert.ToDouble(i) / Convert.ToDouble(rect.Height)) * skewRightByPix, MidpointRounding.AwayFromZero);

				sb.Draw(Game1.OnePxWhite,
					new Rectangle(rect.X + (int)(skewRightByPix - skewAmount),
						rect.Y + i, rect.Width, 1), col);
			}
		}

		/// <summary>
		/// Draws a vertically skewed rectangle
		/// </summary>
		/// <param name="sb">the SpriteBatch to draw to</param>
		/// <param name="rect">the base rectangle; at 0.0 skewUp this function will just draw this rectangle</param>
		/// <param name="skewUpByPix">the amount to skew the right side up by</param>
		/// <param name="col">the color to draw the rectangle as</param>
		public static void DrawSkewedRectVert(SpriteBatch sb, Rectangle rect, float skewUpByPix, Color col) {
			for (var i = 0; i < rect.Width; i++) {
				var skewAmount = (Convert.ToDouble(i) / Convert.ToDouble(rect.Width)) * skewUpByPix;

				sb.Draw(Game1.OnePxWhite, new Rectangle(rect.X + i, rect.Y - (int)skewAmount, 1, rect.Height), col);
			}
		}

		/// <summary>
		/// Draws a very basic fake 3D "cube"
		/// </summary>
		/// <param name="sb">the SpriteBatch to draw to</param>
		/// <param name="frontRect">a rectangle representing the face facing the screen</param>
		/// <param name="depth">the "depth" of the cube</param>
		/// <param name="horiSkew">the amount to skew horizontally</param>
		/// <param name="vertSkew">the amount to skew vertically</param>
		/// <param name="front">the front face color</param>
		/// <param name="top">the top and bottom face colors</param>
		/// <param name="side">the side colors</param>
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
		
		/// <summary>
		/// Draws a rectangle
		/// </summary>
		/// <param name="sb">the SpriteBatch to draw to</param>
		/// <param name="rect">the rectangle to draw</param>
		/// <param name="lineWidth">the width of the outline</param>
		/// <param name="col">the color to draw the line with</param>
		public static void DrawBox(SpriteBatch sb, Rectangle rect, float lineWidth, Color col) {
			DrawLine(sb, new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width, rect.Y), lineWidth, col); //Top
			DrawLine(sb, new Vector2(rect.X, rect.Y + rect.Height), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), lineWidth, col); //Bottom

			DrawLine(sb, new Vector2(rect.X, rect.Y), new Vector2(rect.X, rect.Y + rect.Height), lineWidth, col); //Left
			DrawLine(sb, new Vector2(rect.X + rect.Width, rect.Y), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), lineWidth, col); //Right
		}

		/// <summary>
		/// Draws a rectangle
		/// </summary>
		/// <param name="sb">the SpriteBatch to draw to</param>
		/// <param name="rect">the rectangle to draw</param>
		/// <param name="lineWidth">the width of the outline</param>
		/// <param name="col">the color to draw the line with</param>
		public static void DrawGlowBox(SpriteBatch sb, Rectangle rect, float lineWidth, Color col) {
			DrawGlowLine(sb, new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width, rect.Y), lineWidth, col); //Top
			DrawGlowLine(sb, new Vector2(rect.X, rect.Y + rect.Height), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), lineWidth, col); //Bottom

			DrawGlowLine(sb, new Vector2(rect.X, rect.Y), new Vector2(rect.X, rect.Y + rect.Height), lineWidth, col); //Left
			DrawGlowLine(sb, new Vector2(rect.X + rect.Width, rect.Y), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), lineWidth, col); //Right
		}

		/// <summary>
		/// Draws an outline of a polygon
		/// </summary>
		/// <param name="sb">the SpriteBatch to draw to</param>
		/// <param name="lineWidth">the width of the line</param>
		/// <param name="col">the color to draw the line as</param>
		/// <param name="complete">if it's set to true, the line will automatically complete between the last and first point</param>
		/// <param name="points">each of the points to draw lines between</param>
		public static void DrawPoly(SpriteBatch sb, float lineWidth, Color col, bool complete, params Vector2[] points) {
			//If there's less than 2 points then just return
			if (points.Length <= 1) return;

			//else continue and draw lines between each of the points
			for (var i = 0; i < points.Length-1; i++) {
				var p = points[i];
				var pP = points[i + 1];

				DrawLine(sb, p, pP, lineWidth, col);
			}
			
			//And then finish the line if complete is true
			if (complete)
				DrawLine(sb, points[points.Length - 1], points[0], lineWidth, col);
		}
		
		/// <summary>
		/// Draws the game font to the SpriteBatch with basic line wrapping
		/// </summary>
		/// <param name="sb">the SpriteBatch to draw to</param>
		/// <param name="text">the text to draw</param>
		/// <param name="location">the starting location of the text</param>
		/// <param name="color">the color to draw the text with</param>
		/// <param name="maxWidth">the maximum line width before the text starts to wrap</param>
		/// <param name="size">the height/width of each character</param>
		/// <param name="stringAlignment">the anchor for the string's start position, Left makes the text go right from location</param>
		/// <param name="stringAlignmentVert">the vertical string alignment, Below makes the text draw below location</param>
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

		/// <summary>
		/// Draws a string using the game's font to the given SpriteBatch
		/// </summary>
		/// <param name="sb">the SpriteBatch to draw with</param>
		/// <param name="text">the text to draw</param>
		/// <param name="location">the position for the text to start at</param>
		/// <param name="color">the color to draw the text with</param>
		/// <param name="size">the height of each character</param>
		/// <param name="stringAlignment">the anchor for the string's start position, Left makes the text go right from location</param>
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
