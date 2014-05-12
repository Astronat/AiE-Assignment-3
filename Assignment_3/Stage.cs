using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Size = System.Drawing.Size;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment_3 {
	class Stage {
		public static Texture2D FloorTexture;
		public static void LoadContent() {
			FloorTexture = new Texture2D(new GraphicsDevice(), 1, 1);
			FloorTexture.SetData(new[] { Color.White });
		}

		private Rectangle bounds;
		private readonly List<Size> groundChunks = new List<Size>();

		private const float LineWidth = 8f;

		public Stage(Rectangle gameBounds) {
			bounds = gameBounds;

			//Testing additions
			groundChunks.Add(new Size(gameBounds.Width / 2, 80));
			groundChunks.Add(new Size(gameBounds.Width / 2, 150));
		}

		public void Draw(SpriteBatch sb) {
			var currentStart = 0;

			//Draw each section's background
			//This is separate from the below so that it doesn't end up drawing over the vertical sections
			foreach (var t in groundChunks) {
				sb.Draw(FloorTexture,
				        new Rectangle(currentStart, bounds.Bottom - t.Height, t.Width,
				                      t.Height), Color.DarkGray);
				currentStart += t.Width;
			}

			//Reset the current start position
			currentStart = 0;

			//Draw the individual lines that make up the floor
			for (var i = 0; i < groundChunks.Count; i++ ) {
				//Draw horizontal lines
				DrawLine(sb, new Vector2(currentStart, bounds.Bottom - groundChunks[i].Height),
						 new Vector2(currentStart + groundChunks[i].Width, bounds.Bottom - groundChunks[i].Height), LineWidth);

				//Add to the current start position before the vertical lines to simplify the below very slightly
				currentStart += groundChunks[i].Width;

				//Draw vertical lines
				if (i < groundChunks.Count - 1) {
					DrawLine(sb, new Vector2(currentStart, bounds.Bottom - groundChunks[i].Height + (LineWidth / 2)),
							 new Vector2(currentStart, bounds.Bottom - groundChunks[i+1].Height - (LineWidth / 2)), LineWidth);
				}
			}
		}
		public void Update () {
			
		}

		public void DrawLine(SpriteBatch sb, Vector2 a, Vector2 b, float thickness) {
			//Effectively draws a white line between two points
			var tan = b - a;
			var rotation = (float)Math.Atan2(tan.Y, tan.X);

			var middlePoint = new Vector2(0, FloorTexture.Height / 2f);
			var scale = new Vector2(tan.Length(), thickness);

			sb.Draw(FloorTexture, a, null, Color.White, rotation, middlePoint, scale, SpriteEffects.None, 0f);
		}
	}
}
