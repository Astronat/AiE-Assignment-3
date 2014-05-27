﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Assignment_3 {
	class Stage {
		public List<RectangleF> GroundChunks = new List<RectangleF>();

		public List<Ammo> AmmoPickups = new List<Ammo>();
		public List<Enemy> Enemies = new List<Enemy>();

		public BulletFactory Bullets = new BulletFactory();

		private readonly ExplosionFactory exFactory = new ExplosionFactory();
		
		public Player PlayerOne;

		private static SoundEffect nameEntryBoop;
		private static SoundEffect enemyExplodeBoop;
		private static SoundEffect playerExplodeBoop;
		private static SoundEffect bulletWallBoop;
		private static SoundEffect ammoPickupBoop;
		private static SoundEffect playerFireBoop;
		private static SoundEffect enemyFireBoop;

		private const float LineWidth = 8f;

		public float ScrollSpeed = 2f;
		public float XPosition = 0f;
		
		private float deathWallIntesity = 0.5f;

		public long Score = 0;

		private double lastShotTime = 0;
		private double lastSparkTime = 0;
		private double sparkdelay = 0;

		private readonly double levelStartTime = 0;

		private bool deathWallGrowing = false;
		private bool levelStart = true;

		public bool LevelFinished = false;
		public bool NameEntryFinished = false;
		public bool ScoreIsHigh = false;

		private readonly int[] highScoreName = {0, 0, 0};

		private int highScoreNameSelected = 0;
		
		public Stage(double startTime) {
			//Starting chunk
			GroundChunks.Add(new RectangleF(0, Game1.GameBounds.Height - 80, Game1.GameBounds.Width * 1.5f, 80));

			PlayerOne = new Player(new Vector2(Game1.GameBounds.Width / 2f, Game1.GameBounds.Height - 80 - Player.PlayerSize.Height - (LineWidth / 2f)));

			levelStartTime = startTime;
		}

		public static void LoadContent(ContentManager content) {
			Player.LoadContent(content);

			nameEntryBoop = content.Load<SoundEffect>("menuselect");
			enemyExplodeBoop = content.Load<SoundEffect>("enemyexplode");
			playerExplodeBoop = content.Load<SoundEffect>("playerexplode");
			bulletWallBoop = content.Load<SoundEffect>("bulletwall");
			ammoPickupBoop = content.Load<SoundEffect>("ammopickup");
			playerFireBoop = content.Load<SoundEffect>("playerfire");
			enemyFireBoop = content.Load<SoundEffect>("enemyfire");
		}
		
		public void Draw(SpriteBatch sb) {
			//Draw ammo and enemies
			foreach (var a in AmmoPickups) a.Draw(sb);
			foreach (var e in Enemies) e.Draw(sb);
			
			//Draw the Ominous Wall of Death
			Util.DrawLine(sb, new Vector2(0, 0), new Vector2(0, Game1.GameBounds.Height), 8f * ScrollSpeed,
				Util.ColorInterpolate(Color.White, Color.Red, deathWallIntesity));

			//Draw explosions
			exFactory.Draw(sb);
			
			//Draw "pits will kill you" line thingy
			Util.DrawLine(sb, new Vector2(0, Game1.GameBounds.Height), 
				new Vector2(Game1.GameBounds.Width, Game1.GameBounds.Height), 16f,
				Util.ColorInterpolate(Color.White, Color.Red, deathWallIntesity));

			//Draw each section's background
			//This is separate from the below so that it doesn't end up drawing over the vertical sections
			foreach (var t in GroundChunks) {
				sb.Draw(Game1.OnePxWhite,
						new Rectangle((int)(t.X - XPosition), (int)(Game1.GameBounds.Height - t.Height), (int)t.Width,
				                      (int)t.Height), Color.FromNonPremultiplied(50,50,50,255));
			}

			//Default chunkStart to the X position of the first chunk
			var chunkStart = GroundChunks[0].X - XPosition;

			//Draw the individual lines that make up the floor
			for (var i = 0; i < GroundChunks.Count; i++) {
				//Draw horizontal lines
				Util.DrawLine(sb, new Vector2(chunkStart, Game1.GameBounds.Height - GroundChunks[i].Height),
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
				Util.DrawLine(sb, new Vector2(chunkStart, Game1.GameBounds.Height - bottom + (LineWidth / 2)),
						 new Vector2(chunkStart, Game1.GameBounds.Height - bottom - leng - (LineWidth / 2)), LineWidth, Color.White);
			}

			//Draw bullets
			Bullets.Draw(sb);

			//Draw the player
			if (PlayerOne.Alive) PlayerOne.Draw(sb);

			//Draw the stage start "RUN!" text
			if (levelStart)
				Util.DrawFontMultiLine(sb, "run!", new Vector2(Game1.GameBounds.Width / 2f, Game1.GameBounds.Height / 2f), 
					Color.Red, Game1.GameBounds.Width, 64f, StringAlignment.Center, StringAlignmentVert.Center );
			else if (PlayerOne.Alive) { //And if the level has finished uh, starting, draw the entire HUD
				//Ammo text
				Util.DrawFont(sb, "Ammo", new Vector2(12, 3), Color.LightGreen);
				
				//Ammo icons
				for (var i = 0; i < PlayerOne.AmmoCount; i++ ) {
					sb.Draw(Game1.OnePxWhite, new Rectangle(16 + (i * 24), 35, 16, 16), Color.Yellow);
				}
				
				//Score text
				Util.DrawFont(sb, "Score", new Vector2(Game1.GameBounds.Width - 12, 3), Color.LightGreen, 32, StringAlignment.Right);
				Util.DrawFont(sb, Score / 10, new Vector2(Game1.GameBounds.Width - 12, 35), Color.White, 32, StringAlignment.Right);
			}

			//Draw the end screen text
			if (!PlayerOne.Alive && LevelFinished) {
				//Draw a semitransparent black box over everything to make the text show up better
				sb.Draw(Game1.OnePxWhite, new Rectangle(0, 0, Game1.GameBounds.Width, Game1.GameBounds.Height), Color.FromNonPremultiplied(0, 0, 0, 128));

				//All non-high score related text
				Util.DrawFontMultiLine(sb, "You died!", new Vector2(Game1.ScreenCenter.X, 50f), Color.White, Game1.GameBounds.Width,
									   32f, StringAlignment.Center);
				Util.DrawFontMultiLine(sb, "However,you made it to", new Vector2(Game1.ScreenCenter.X, 82f), Color.White, Game1.GameBounds.Width,
									   32f, StringAlignment.Center);
				Util.DrawFontMultiLine(sb, Score / 10, new Vector2(Game1.ScreenCenter.X, 114f), Color.White, Game1.GameBounds.Width,
									   64f, StringAlignment.Center);
				Util.DrawFontMultiLine(sb, "points!", new Vector2(Game1.ScreenCenter.X, 178f), Color.White, Game1.GameBounds.Width,
									   32f, StringAlignment.Center);

				//High score related
				if (ScoreIsHigh) {
					Util.DrawFontMultiLine(sb, "a new high score!", new Vector2(Game1.ScreenCenter.X, 230f), Color.White,
					                       Game1.GameBounds.Width,
					                       32f, StringAlignment.Center);
					Util.DrawFontMultiLine(sb, "input name", new Vector2(Game1.ScreenCenter.X, 256f), Color.White,
					                       Game1.GameBounds.Width,
					                       32f, StringAlignment.Center);

					//Draw Name selection letter box thingy yeah woo
					//Lots of magic numbers here; it's mostly just fine-tuned positioning stuff
					Util.DrawFontMultiLine(sb, HighScores.HighScoreIntArrayToString(highScoreName) + "~",
					                       new Vector2(Game1.ScreenCenter.X, 310f), Color.White, Game1.GameBounds.Width,
					                       48f, StringAlignment.Center);

					var boxX = Game1.ScreenCenter.X - 96 + (highScoreNameSelected*48);
					Util.DrawBox(sb, new Rectangle((int) boxX - 3, 310, 49, 44), 4f,
					             Util.ColorInterpolate(Color.White, Color.Red, deathWallIntesity));
				} else {
					Util.DrawFontMultiLine(sb, "Press x to continue",
										   new Vector2(Game1.ScreenCenter.X, 310f), Color.White, Game1.GameBounds.Width,
										   48f, StringAlignment.Center);
				}
			} 
		}

		public void Update (KeyboardState kState, KeyboardState? prevState, GameTime gTime) {
			if (gTime.TotalGameTime.TotalMilliseconds > levelStartTime + 3000)
				levelStart = false;

			//The sum of each of the chunks' lengths
			var endWidth = GroundChunks[GroundChunks.Count - 1].X + GroundChunks[GroundChunks.Count - 1].Width;
			
			//Check if a new chunk is required, generate and add it if it is
			if (endWidth - XPosition < Game1.GameBounds.Width + 8) {
				//Randomized chunk height and width

				//Determine if the current chunk should be a pit chunk
				var isPit = (Game1.GameRand.NextDouble() > 0.9);

				var rndHeight = Game1.GameRand.Next(70, 160);
				while (rndHeight < GroundChunks[GroundChunks.Count - 1].Height + 15 
				&& rndHeight > GroundChunks[GroundChunks.Count - 1].Height - 15) {
					rndHeight = Game1.GameRand.Next(70, 160);
				}

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
					if (spawnChance > 0.6) {
						AmmoPickups.Add(new Ammo(
							                new Vector2(
								                Game1.GameBounds.Width + LineWidth +
								                (float) ((GroundChunks[GroundChunks.Count - 1].Width - Ammo.Size)*Game1.GameRand.NextDouble()),
								                Game1.GameBounds.Height - rndHeight - 30)));
					}
						//Same as above, but for enemies
					else if (spawnChance > 0.3) {
						Enemies.Add(new Enemy(
							            new Vector2(
								            Game1.GameBounds.Width + LineWidth +
								            (float) ((GroundChunks[GroundChunks.Count - 1].Width - Enemy.SpriteSize.Width)*Game1.GameRand.NextDouble()),
								            Game1.GameBounds.Height - rndHeight)));
						Enemies[Enemies.Count - 1].LastShotMs = gTime.TotalGameTime.TotalMilliseconds;
					}
				}
			}

			//Switches between increasing and decreasing the death wall's intensity, then does so
			if (deathWallIntesity >= 0.9f || deathWallIntesity <= 0.1f) deathWallGrowing = !deathWallGrowing;
			deathWallIntesity = deathWallIntesity + (deathWallGrowing ? 0.025f : -0.025f);

			if (PlayerOne.Alive) {
				//Update each Ammo pickup
				foreach (var a in AmmoPickups) {
					a.Update(ScrollSpeed);

					if (a.HitBox.Intersects(PlayerOne.HitBox)) {
						a.Alive = false;
						PlayerOne.AmmoCount += 1;
						ammoPickupBoop.Play();
					}
				}

				//Update enemies
				foreach (var e in Enemies) {
					//Player bullet/enemy collisions
					foreach (var b in Bullets.Bullets) {
						if (b.HitBox.Intersects(e.HitBox) && b.Friendly) {
							b.Alive = e.Alive = false;

							enemyExplodeBoop.Play();
							
							//Explode enemy
							exFactory.Explode(new Vector2(e.HitBox.X + (e.HitBox.Width/2), e.HitBox.Y + (e.HitBox.Height/2)),
							                  Color.LightCoral, gTime);
						}
					}

					//Fire a bullet once every 2 seconds, minus some so the game doesn't end up getting easier as time passes
					if (e.LastShotMs + (2000 - (ScrollSpeed * 100)) < gTime.TotalGameTime.TotalMilliseconds
					&& e.HitBox.Intersects(new Rectangle(0, 0, Game1.GameBounds.Width, Game1.GameBounds.Height))) {
						enemyFireBoop.Play();

						Bullets.FireBullet(e.CenterPosition + (e.AimDirection*30), e.AimDirection, false, ScrollSpeed);
						e.LastShotMs = gTime.TotalGameTime.TotalMilliseconds;
					}

					//Player/enemy hit detection
					if (e.HitBox.Intersects(PlayerOne.HitBox)) {
						playerExplodeBoop.Play(0.7f, 0, 0);
						enemyExplodeBoop.Play();

						PlayerOne.Alive = false;
						exFactory.Explode(PlayerOne.CenterPosition, Color.Green, gTime);
					}

					e.Update(ScrollSpeed, PlayerOne.CenterPosition);
				}

				//Player/enemy bullet collisions
				foreach (var b in Bullets.Bullets.Where(item => !item.Friendly)) {
					if (!b.HitBox.Intersects(PlayerOne.HitBox)) continue;

					b.Alive = false;
					PlayerOne.Alive = false;

					playerExplodeBoop.Play(0.7f, 0, 0);
					exFactory.Explode(PlayerOne.CenterPosition, Color.Green, gTime);
				}

				//Update bullets
				Bullets.Update();

				//Remove all dead objects
				AmmoPickups.RemoveAll(item => !item.Alive);
				Enemies.RemoveAll(item => !item.Alive);

				//If the first chunk in the array is completely off screen, remove it
				if (GroundChunks[0].X + GroundChunks[0].Width <= XPosition) {
					GroundChunks.RemoveAt(0);
				}

				//Update world position
				XPosition += ScrollSpeed;

				/*** COLLISIONS ***/

				//Player + world collisions
				var col = new Collisions {Left = false, Right = false, Down = false, Floor = 500};
				var lastChunk = Util.RectFToRect(GroundChunks[0]);

				//Iterate through each chunk
				for (var i = 0; i < GroundChunks.Count; i++) {
					//The current chunk, slightly changed for collision purposes
					var chunk = new Rectangle((int) (GroundChunks[i].X - XPosition), (int) (GroundChunks[i].Y - LineWidth/2),
					                          (int) GroundChunks[i].Width, 500);

					if (PlayerOne.BottomBox.Intersects(chunk)) {
						col.Down = true;
						col.Floor = chunk.Y;
					}

					//Test player left side collision
					if (PlayerOne.LeftBox.Intersects(lastChunk)) {
						col.Left = true;
						col.LeftSide = lastChunk.Right + 1;
					}

					//Test player right side collision
					if (i < GroundChunks.Count - 1) {
						//The next chunk in the world
						var cNext = new Rectangle((int) (GroundChunks[i + 1].X - XPosition - (LineWidth/2f)),
						                          (int) (GroundChunks[i + 1].Y - LineWidth/2), (int) GroundChunks[i + 1].Width, 500);

						//Actual collision check
						if (PlayerOne.RightBox.Intersects(cNext)) {
							col.Right = true;
							col.RightSide = cNext.Left - 56;
						}
					}

					//Reset the previous chunk
					lastChunk = new Rectangle((int) (chunk.X + (LineWidth/2)), chunk.Y, (int) (chunk.Width + (LineWidth/2)),
					                          chunk.Height);
					
					//Particle collisions and removal
					foreach (var e in exFactory.Explosions)
						e.Particles.RemoveAll(part => part.HitBox.Intersects(chunk));

					//Remove bullets hitting "terrain"
					foreach (var b in Bullets.Bullets.Where(item => item.HitBox.Intersects(chunk))) {
						bulletWallBoop.Play();

						//Cause an explosion effect slightly away from the wall, so the particles aren't immediately destroyed
						exFactory.Explode(b.Position - (b.Direction * 5), (b.Friendly ? Color.Green : Color.Red), gTime, 100, 300);
						b.Alive = false;
					}

					//Death wall and pit death detection
					if (PlayerOne.HitBox.Intersects(new Rectangle(0, 0, (int)(8f * ScrollSpeed) / 2, Game1.GameBounds.Height))
					|| PlayerOne.BottomBox.Y > Game1.GameBounds.Height) {
						playerExplodeBoop.Play();
						PlayerOne.Alive = false;
						exFactory.Explode(PlayerOne.CenterPosition, Color.Green, gTime);
					}
				}

				//This is outside of player.cs to avoid having to make BulletFactory static and as such avoid some dodgy code
				if (kState.IsKeyDown(Keys.Z) && gTime.TotalGameTime.TotalMilliseconds > lastShotTime + 300 &&
				    PlayerOne.AmmoCount > 0) {
					//Reset the shot delay timer
					lastShotTime = gTime.TotalGameTime.TotalMilliseconds;
					
					playerFireBoop.Play();

					//Fire a bullet
					Bullets.FireBullet(
						new Vector2(
							PlayerOne.CenterPosition.X - (Bullet.BulletSize.Width/2f) +
							(PlayerOne.FacingRight ? PlayerOne.HitBox.Width/2 : -(PlayerOne.HitBox.Width/2)),
							PlayerOne.CenterPosition.Y - (Bullet.BulletSize.Height/2f) + (PlayerOne.Ducking ? PlayerOne.HitBox.Height/2 : 0)),
						new Vector2(PlayerOne.FacingRight ? 1 : -1, 0), true, ScrollSpeed);

					//Remove 1 ammo from player
					PlayerOne.AmmoCount -= 1;
				}
				
				//Throw "sparks" off the bottom of the ominous death wall for effect
				if (lastSparkTime + sparkdelay < gTime.TotalGameTime.TotalMilliseconds) {
					lastSparkTime = gTime.TotalGameTime.TotalMilliseconds;
					sparkdelay = Game1.GameRand.Next(20, 200);

					//Shoot particles to the right of the wall
					exFactory.Explode(new Vector2((float)(((8f * ScrollSpeed) / 2) * Game1.GameRand.NextDouble()), GroundChunks[0].Top - 5f), Color.Red, -2f, gTime, 50, 200, 3, 15);
				}

				//Update the player
				if (!levelStart)
					PlayerOne.Update(kState, prevState, ScrollSpeed, col);
			}

			//Increase level speed and score
			if (!levelStart && PlayerOne.Alive) {
				ScrollSpeed += 0.0004f;
				Score += (int)(ScrollSpeed * 1.2);
			}

			//Update explosions
			exFactory.Update(gTime, (PlayerOne.Alive ? ScrollSpeed : 0f));

			//Consider level finished once the player is dead and no explosions need updating
			//The idea being once the player dies, everything stops and they explode
			if (!PlayerOne.Alive && exFactory.NoUpdates) {
				LevelFinished = true;
			}

			//Name entry button press stuff
			if (!PlayerOne.Alive && LevelFinished) {
				if (Score / 10 > Game1.HighScoreList[9].Points) {
					ScoreIsHigh = true;
				}

				if (ScoreIsHigh) {
					if (kState.IsKeyDown(Keys.Left) && prevState.Value.IsKeyUp(Keys.Left) && highScoreNameSelected > 0) {
						highScoreNameSelected--;
					}
					if (kState.IsKeyDown(Keys.Right) && prevState.Value.IsKeyUp(Keys.Right) && highScoreNameSelected < 3) {
						highScoreNameSelected++;
					}

					if (highScoreNameSelected < 3) {
						if (kState.IsKeyDown(Keys.Up) && prevState.Value.IsKeyUp(Keys.Up)) {
							if (highScoreName[highScoreNameSelected] < 39)
								highScoreName[highScoreNameSelected]++;
							else
								highScoreName[highScoreNameSelected] = 0;
						}
						if (kState.IsKeyDown(Keys.Down) && prevState.Value.IsKeyUp(Keys.Down)) {
							if (highScoreName[highScoreNameSelected] > 0)
								highScoreName[highScoreNameSelected]--;
							else
								highScoreName[highScoreNameSelected] = 39;
						}
					}

					if (kState.IsKeyDown(Keys.X) && prevState.Value.IsKeyUp(Keys.X) && highScoreNameSelected == 3) {
						nameEntryBoop.Play();

						HighScores.InsertScore(HighScores.HighScoreIntArrayToString(highScoreName), (int)(Score / 10), Game1.HighScoreList);
						HighScores.SerializeScores("highscores", Game1.HighScoreList);

						NameEntryFinished = true;
					}
				}
				else {
					if (kState.IsKeyDown(Keys.X) && prevState.Value.IsKeyUp(Keys.X)) {
						nameEntryBoop.Play();
						NameEntryFinished = true;
					}
				}
			} 
		}

	}
}
