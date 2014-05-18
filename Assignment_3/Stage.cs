using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Assignment_3 {
	class Stage {
		public List<RectangleF> GroundChunks = new List<RectangleF>();

		public List<Ammo> AmmoPickups = new List<Ammo>();
		public List<Enemy> Enemies = new List<Enemy>();
 
		private const float LineWidth = 8f;

		public Player PlayerOne;

		//Used to move the first Chunk in the Chunk List to the left and off the screen
		//The idea is that XPosition is the negative position of the first chunk
		//and every chunk following is just "stuck" to the end of the first chunk
		//hence, as the first chunk goes completely off screen, Xposition is reset
		//and the first chunk is removed, making the second chunk become the leading chunk
		//again.
		//Could be simpler. Chunk movement handling should probably be rewritten.
		public float ScrollSpeed = 2f;
		public float XPosition = 0f; 

		private float deathWallIntesity = 0.5f;
		private bool deathWallGrowing = false;

		public Stage() {
			//Starting chunk
			GroundChunks.Add(new RectangleF(0, Game1.GameBounds.Height - 80, Game1.GameBounds.Width * 1.5f, 80));

			PlayerOne = new Player(new Vector2(150, Game1.GameBounds.Height - 80 - Player.PlayerSize.Height - 500));
		}
		
		public void Draw(SpriteBatch sb) {

			//Draw ammo and enemies
			foreach (var a in AmmoPickups) a.Draw(sb);
			foreach (var e in Enemies) e.Draw(sb);

			//Draw the Ominous Wall of Death
			DrawLine(sb, new Vector2(0, 0), new Vector2(0, Game1.GameBounds.Height), 16f,
				Util.ColorInterpolate(Color.White, Color.Red, deathWallIntesity));

			//Draw "pits will kill you" line thingy
			DrawLine(sb, new Vector2(0, Game1.GameBounds.Height), 
				new Vector2(Game1.GameBounds.Width, Game1.GameBounds.Height), 16f,
				Util.ColorInterpolate(Color.White, Color.Red, deathWallIntesity));

			//Draw each section's background
			//This is separate from the below so that it doesn't end up drawing over the vertical sections
			foreach (var t in GroundChunks) {
				sb.Draw(Game1.OnePxWhite,
						new Rectangle((int)(t.X - XPosition), (int)(Game1.GameBounds.Height - t.Height), (int)t.Width,
				                      (int)t.Height), Color.FromNonPremultiplied(50,50,50,255));
			}


			var chunkStart = GroundChunks[0].X - XPosition;

			//Draw the individual lines that make up the floor
			for (var i = 0; i < GroundChunks.Count; i++) {
				//Draw horizontal lines
				DrawLine(sb, new Vector2(chunkStart, Game1.GameBounds.Height - GroundChunks[i].Height),
						 new Vector2(chunkStart + GroundChunks[i].Width, Game1.GameBounds.Height - GroundChunks[i].Height), LineWidth, Color.White);

				//Add to the current start position before the vertical lines to simplify the below very slightly
				chunkStart += GroundChunks[i].Width;

				//Only continue if a vertical line needs to be drawn
				if (i >= GroundChunks.Count - 1) continue;

				//Get the length of the vertical difference between the current and next chunk
				var leng = Math.Abs(GroundChunks[i].Height - GroundChunks[i + 1].Height);
				
				//and the bottom position for the current vertical line
				var bottom = Math.Min(GroundChunks[i].Height, GroundChunks[i + 1].Height);

				//Then draw the line
				DrawLine(sb, new Vector2(chunkStart, Game1.GameBounds.Height - bottom + (LineWidth / 2)),
						 new Vector2(chunkStart, Game1.GameBounds.Height - bottom - leng - (LineWidth / 2)), LineWidth, Color.White);
			}

			//Draw the player
			PlayerOne.Draw(sb);
		}

		public void Update (KeyboardState kState, KeyboardState? prevState) {
			//The sum of each of the chunks' lengths
			var endWidth = GroundChunks[GroundChunks.Count - 1].X + GroundChunks[GroundChunks.Count - 1].Width;
			
			//Check if a new chunk is required, generate and add it if it is
			if (endWidth - XPosition < Game1.GameBounds.Width + 8) {
				//Randomized chunk height and width
				//TODO: Make the height always be n pixels above or below the last chunk for variance

				//Determine if the current chunk should be a pit chunk
				var isPit = (Game1.GameRand.NextDouble() > 0.9);

				var rndHeight = Game1.GameRand.Next(70, 160);
				//Pit chunks should always be smaller than normal chunks so jumping them is actually possible
				var rndWidth = (!isPit ? Game1.GameRand.Next(150, Game1.GameBounds.Width/2) 
										: Game1.GameRand.Next(50, 150));

				//Add a new chunk, if it's a pit make it a good bit lower than the screen
				GroundChunks.Add(!isPit
									 ? new RectangleF(endWidth, Game1.GameBounds.Height - rndHeight, rndWidth, rndHeight)
									 : new RectangleF(endWidth, Game1.GameBounds.Height + 200, rndWidth, -200));

				var spawnChance = Game1.GameRand.NextDouble();

				//Not a pit, so go ahead and maybe add ammo or an enemy
				if (!isPit) {
					//Chance to add an ammo pickup to the new chunk; This may need to be tweaked
					if (spawnChance > 0.7) {
						AmmoPickups.Add(new Ammo(
							                new Vector2(
								                Game1.GameBounds.Width + LineWidth +
								                (float) ((GroundChunks[GroundChunks.Count - 1].Width - Ammo.Size)*Game1.GameRand.NextDouble()),
								                Game1.GameBounds.Height - rndHeight - 60)));
					}
						//Same as above, but for enemies
					else if (spawnChance > 0.5) {
						Enemies.Add(new Enemy(
							            new Vector2(
								            Game1.GameBounds.Width + LineWidth +
								            (float) ((GroundChunks[GroundChunks.Count - 1].Width - Enemy.SpriteSize.Width)*Game1.GameRand.NextDouble()),
								            Game1.GameBounds.Height - rndHeight)));
					}
				}
			}

			//Switches between increasing and decreasing the death wall's intensity, then does so
			if (deathWallIntesity >= 0.9f || deathWallIntesity <= 0.1f) deathWallGrowing = !deathWallGrowing;
			deathWallIntesity = deathWallIntesity + (deathWallGrowing ? 0.025f : -0.025f);

			//Update each object
			foreach (var a in AmmoPickups) a.Update(ScrollSpeed);
			foreach (var e in Enemies) e.Update(ScrollSpeed, new Vector2(0, 0)); /*TODO: Replace placeholder Vector with player location*/

			//Remove all dead objects
			AmmoPickups.RemoveAll(item => !item.Alive);
			Enemies.RemoveAll(item => !item.Alive);
			
			//The above 2 ammo-related chunks are separate as when they were both in
			//a for-loop, for some reason it was doing strange double-dips into their 
			//Update()s on occasion (!?) and basically randomly skipping pickups
			//forwards a few pixels.

			//If the first chunk in the array is completely off screen, remove it and reset the scroll position
			if (GroundChunks[0].X + GroundChunks[0].Width <= XPosition) {
				//XPosition = XPosition + GroundChunks[0].Width - (LineWidth / 4f);
				GroundChunks.RemoveAt(0);
			} //else //Else just continue to update the scroll position
				XPosition += ScrollSpeed;

			var col = new Collisions {Left = false, Right = false, Down = false};

			foreach (var c in GroundChunks) {
				var chunk = new Rectangle((int)(c.X - XPosition), (int)(c.Y - LineWidth), (int)c.Width, 500);
				if (PlayerOne.BottomBox.Intersects(chunk)) {
					col.Down = true;
				}
			}

			PlayerOne.Update(kState, prevState, ScrollSpeed, col);
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
