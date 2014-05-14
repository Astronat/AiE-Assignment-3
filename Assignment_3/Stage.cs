using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment_3 {
	class Stage {
		private Rectangle bounds;
		public List<Rectangle> GroundChunks = new List<Rectangle>();

		private const float LineWidth = 8f;

		public float ScrollSpeed = 1f;
		public float XPosition = 0f;

		public Stage(Rectangle gameBounds) {
			bounds = gameBounds;

			//Starting chunk
			GroundChunks.Add(new Rectangle(0, gameBounds.Height - 80, (int)(gameBounds.Width * 1.5f), 80));
		}
		
		public void Draw(SpriteBatch sb) {
			var currentStart = -(int)XPosition;

			//Draw each section's background
			//This is separate from the below so that it doesn't end up drawing over the vertical sections
			foreach (var t in GroundChunks) {
				sb.Draw(Game1.OnePxWhite,
				        new Rectangle(currentStart, bounds.Bottom - t.Height, t.Width,
				                      t.Height), Color.FromNonPremultiplied(50,50,50,255));
				currentStart += t.Width;
			}

			//Reset the current start position
			currentStart = -(int)XPosition;

			//Draw the individual lines that make up the floor
			for (var i = 0; i < GroundChunks.Count; i++ ) {
				//Draw horizontal lines
				DrawLine(sb, new Vector2(currentStart, bounds.Bottom - GroundChunks[i].Height),
						 new Vector2(currentStart + GroundChunks[i].Width, bounds.Bottom - GroundChunks[i].Height), LineWidth, Color.White);

				//Add to the current start position before the vertical lines to simplify the below very slightly
				currentStart += GroundChunks[i].Width;

				//Only continue if a vertical line needs to be drawn
				if (i >= GroundChunks.Count - 1) continue;

				//Get the length of the vertical difference between the current and next chunk
				var leng = Math.Abs(GroundChunks[i].Height - GroundChunks[i + 1].Height);
				
				//and the bottom position for the current vertical line
				var bottom = Math.Min(GroundChunks[i].Height, GroundChunks[i + 1].Height);

				//Then draw the line
				DrawLine(sb, new Vector2(currentStart, bounds.Bottom - bottom + (LineWidth / 2)),
				         new Vector2(currentStart, bounds.Bottom - bottom - leng - (LineWidth / 2)), LineWidth, Color.White);
			}
		}

		public void Update () {
			//If the first chunk in the array is completely off screen, remove it and reset the scroll speed
			if (GroundChunks[0].Width < XPosition) {
				GroundChunks.RemoveAt(0);
				XPosition = ScrollSpeed;
			}

			//The sum of each of the chunks' lengths
			var totalWidth = GroundChunks.Sum(item => item.Width);
			
			//Check if a new chunk is required, generate and add it if it is
			if (totalWidth - XPosition < bounds.Width + LineWidth) {
				var rndWidth = Game1.gameRand.Next(70, 160);
				GroundChunks.Add(new Rectangle(0, bounds.Height - rndWidth, Game1.gameRand.Next(150, bounds.Width / 2), rndWidth));
			}

			//Update the X position
			XPosition += ScrollSpeed;
		}

		//Effectively draws a white line between two points
		public void DrawLine(SpriteBatch sb, Vector2 a, Vector2 b, float thickness, Color color) {
			var tan = b - a;
			var rotation = (float)Math.Atan2(tan.Y, tan.X);

			var middlePoint = new Vector2(0, Game1.OnePxWhite.Height / 2f);
			var scale = new Vector2(tan.Length(), thickness);

			sb.Draw(Game1.OnePxWhite, a, null, color, rotation, middlePoint, scale, SpriteEffects.None, 0f);
		}
	}
}
