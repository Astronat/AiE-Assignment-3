using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Assignment_3 {
	class Background {
		private readonly List<Block> blocks = new List<Block>();

		public static float Rand() { return (float)Game1.GameRand.NextDouble(); }
		
		public Background(float scrollSpeed) {
			for (var i = 0; i < 600; i++)
				Update(scrollSpeed);
		}

		public void Update(float scrollSpeed) {
			//Add new floating background blocks as required
			if (Game1.GameRand.NextDouble() < 0.02) {
				var rndWidth = 200*Rand();

				var bRect = new RectangleF(Game1.GameBounds.Width + 55, 
					Rand()*Game1.GameBounds.Height, rndWidth,
					rndWidth*Rand());

				blocks.Add(new Block {
					BlockColor = Util.RandomShadeOfGrey(0.7f), 
					ScrollDepth = Util.Limit(Rand(), 0.01f, 1.0f), 
					BlockRect = bRect, 
					BlockDepth = Game1.GameRand.Next(10, 50)});

				blocks.Sort((a, b) => Comparer<double>.Default.Compare(a.ScrollDepth, b.ScrollDepth));
			}

			//Add new little debris "stars"
			if (Game1.GameRand.NextDouble() < 0.02) {
				var rndSize = 6f * (Rand());

				var bRect = new RectangleF(Game1.GameBounds.Width + 20,
					Rand() * Game1.GameBounds.Height, rndSize,
					rndSize);

				blocks.Add(new Block  {
					BlockColor = Util.RandomShadeOfGrey(0.4f),
					ScrollDepth = rndSize / 6f,
					BlockDepth = (int)rndSize,
					BlockRect = bRect});
			}

			//Update all blocks
			foreach(var bl in blocks) {
				bl.BlockRect.X -= scrollSpeed*(float)bl.ScrollDepth;
			}
			blocks.RemoveAll(item => item.BlockRect.Right + item.BlockDepth < 0);
		}

		public void Draw(SpriteBatch sb) {
			//Draw each block
			foreach (var bl in blocks) {
				var topPer = (bl.BlockRect.Top / Game1.GameBounds.Height) - 0.3f;
				var sidePer = ((Game1.GameBounds.Width - bl.BlockRect.X)/Game1.GameBounds.Width) - 0.5f;
				
				Util.DrawCube(sb, Util.RectFToRect(bl.BlockRect), bl.BlockDepth, sidePer, -topPer, bl.BlockColor,
				              Util.MuteColor(bl.BlockColor, 0.2f), Util.MuteColor(bl.BlockColor, 0.5f));
			}

			//Draw a transparent black overlay to dull the background a tad
			sb.Draw(Game1.OnePxWhite, new Rectangle(0, 0, Game1.GameBounds.Width, Game1.GameBounds.Height), Color.FromNonPremultiplied(0, 0, 0, 128));
		}
	}

	class Block {
		public RectangleF BlockRect;
		public Color BlockColor;
		public double ScrollDepth;
		public int BlockDepth;
	}
}
