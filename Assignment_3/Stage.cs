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
		public List<Chunk> GroundChunks = new List<Chunk>();

		public List<Ammo> AmmoPickups = new List<Ammo>();
		public List<Enemy> Enemies = new List<Enemy>();

		public BulletFactory Bullets = new BulletFactory();

		private readonly ExplosionFactory exFactory = new ExplosionFactory();
		private readonly LavaParticles lParticles = new LavaParticles();

		public Player PlayerOne;

		private readonly Background bGround;

		public static Texture2D LevelGlow;
		public static Texture2D CircleGlow;

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

		private float deathWallIntensity = 0.5f;
		private float deathFloorIntensity = 0.5f;

		public long Score = 0;

		private double lastShotTime = 0;
		private double lastSparkTime = 0;
		private double sparkdelay = 0;
		private double lastLavaSparkTime = 0;
		private double lavaSparkdelay = 0;

		private readonly double levelStartTime = 0;

		private bool deathWallGrowing = false;
		private bool deathFloorGrowing = false;
		private bool levelStart = true;

		public bool LevelFinished = false;
		public bool NameEntryFinished = false;
		public bool ScoreIsHigh = false;

		private readonly int[] highScoreName = {0, 0, 0};

		private int highScoreNameSelected = 0;
		
		public Stage(double startTime) {
			//Starting chunk
			GroundChunks.Add(new Chunk(0, Game1.GameBounds.Height - 80, Game1.GameBounds.Width * 1.5f, 80));

			PlayerOne = new Player(new Vector2(Game1.GameBounds.Width / 2f, Game1.GameBounds.Height - 80 - Player.PlayerSize.Height - (LineWidth / 2f)));

			bGround = new Background(ScrollSpeed);

			levelStartTime = startTime;
		}

		public static void LoadContent(ContentManager content) {
			Player.LoadContent(content);

			//Create glow sprite for the lava
			LevelGlow = new Texture2D(new GraphicsDevice(), 1, 255);
			var glowData = new Color[255];
			for (var i = 0; i < 255; i++) {
				glowData[i] = Color.FromNonPremultiplied(255, 255, 255, i);
			}

			LevelGlow.SetData(glowData);

			//Create circular gradient texture
			CircleGlow = new Texture2D(new GraphicsDevice(), 255, 255);

			//Create pixel array
			glowData = new Color[255 * 255];
			//Simply a vector pointing at the middle of the image
			var centerVect = new Vector2(0.5f, 0.5f);
			
			//Iterate through each pixel
			var rowCount = 0;
			for (var v = 0; v < 255*255; v++) {
				//xPosition and 0.0-1.0 representation of it
				var xP = (float)(v % 255);
				var x = xP/255f; 

				//Same as above but for the Y axis
				var yP = (rowCount);
				var y = yP/255f;

				//Vector pointing to the current pixel
				var gradVect = new Vector2(x, y);

				//Limited representation of the current pixel's length from the center of the texture
				var len = Util.Limit((1.0f - (centerVect - gradVect).Length() * 2f), 0.0f, 1f);
				
				//Set the pixel's color data
				glowData[v] = Color.FromNonPremultiplied(255, 255, 255, (int)(len * 100));

				//Increment the row count if required
				if (v % 255 == 0 && v > 0) rowCount++;
			}

			//Apply the above pixel array's data to the texture
			CircleGlow.SetData(glowData);

			nameEntryBoop = content.Load<SoundEffect>("menuselect");
			enemyExplodeBoop = content.Load<SoundEffect>("enemyexplode");
			playerExplodeBoop = content.Load<SoundEffect>("playerexplode");
			bulletWallBoop = content.Load<SoundEffect>("bulletwall");
			ammoPickupBoop = content.Load<SoundEffect>("ammopickup");
			playerFireBoop = content.Load<SoundEffect>("playerfire");
			enemyFireBoop = content.Load<SoundEffect>("enemyfire");
		}
		
		public void Draw(SpriteBatch sb) {
			//Draw background
			bGround.Draw(sb);

			//Draw "land" behind lava so it doesn't just drop off into black
			Util.DrawCube(sb,
				new Rectangle(0, Game1.GameBounds.Height - 80, Game1.GameBounds.Width,
								  40),
								  40, 0.2f, -0.3f,
								  Color.FromNonPremultiplied(30, 30, 30, 255),
								  Color.FromNonPremultiplied(130, 130, 130, 255),
								  Color.FromNonPremultiplied(80, 80, 80, 255));

			//Draw glow on background lava wall thing
			sb.Draw(LevelGlow, new Rectangle(0, Game1.GameBounds.Height - 80, Game1.GameBounds.Width,
								  20), Color.FromNonPremultiplied(255, 0, 0, (int)(230 * deathFloorIntensity)));

			//Draw "pits will kill you" line thingy
			Util.DrawLine(sb, new Vector2(0, Game1.GameBounds.Height - 30),
				new Vector2(Game1.GameBounds.Width, Game1.GameBounds.Height - 30), 66f,
				Util.ColorInterpolate(Color.FromNonPremultiplied(30, 30, 30, 255), Color.Red, deathFloorIntensity));

			//Draw each section's background
			//This is separate from the below so that it doesn't end up drawing over the vertical sections
			foreach (var t in GroundChunks) {
				var chunkLeft = t.X - XPosition;
				var chunkRight = chunkLeft + t.Width;

				//New 3D hotness
				if (t.Top < Game1.GameBounds.Height) { //Not a pit
					//Draw each level chunk
					Util.DrawCube(sb,
					              new Rectangle((int)chunkLeft, (int)(Game1.GameBounds.Height - t.Height), (int) t.Width,
					                            (int) t.Height - 8),
					              40, 0.2f, -0.5f,
					              Color.FromNonPremultiplied(50, 50, 50, 255),
					              Color.FromNonPremultiplied(150, 150, 150, 255),
					              Color.FromNonPremultiplied(100, 100, 100, 255));

					foreach(var r in t.SideDetail) {
						sb.Draw(Game1.OnePxWhite, new Rectangle((int)(chunkLeft + r.Rect.X), (int)(Game1.GameBounds.Height - t.Height) + r.Rect.Y, r.Rect.Width, r.Rect.Height), r.Col);
					}

					//Draw glow on each level chunk
					sb.Draw(LevelGlow, new Rectangle((int)chunkLeft, Game1.GameBounds.Height - 50, (int)t.Width,
					                                 50), Color.FromNonPremultiplied(255, 0, 0, (int) (230*deathFloorIntensity)));


					//Draw player shadow
					if (PlayerOne.Position.X > chunkLeft && PlayerOne.Position.X + Player.PlayerSize.Width < chunkRight && PlayerOne.Alive) {
						var playerDist = 100 - Util.Limit((int)(Game1.GameBounds.Height - t.Height) - 8 - (PlayerOne.Position.Y + Player.PlayerSize.Height), 0, 100);
						var distFloat = playerDist/100f;

						Util.DrawSkewedRectHor(sb,
							new Rectangle((int)(PlayerOne.Position.X - 3 + ((1.0f - distFloat) * Player.PlayerSize.Width)), (int)(Game1.GameBounds.Height - t.Height - 11),
											  (int)(Player.PlayerSize.Width - ((1.0f - distFloat) * Player.PlayerSize.Width / 2)), 8), 4, Color.FromNonPremultiplied(0, 0, 0, (int)(150 * distFloat)));
					}

					//Draw ammo shadows
					foreach (var a in AmmoPickups.Where(a => a.HitBox.X > chunkLeft && a.HitBox.Right < chunkRight)) {
						Util.DrawSkewedRectHor(sb,
						                       new Rectangle(a.HitBox.X, (int)(Game1.GameBounds.Height - t.Height) - 10,
						                                     a.HitBox.Width, 6), 2, Color.FromNonPremultiplied(0, 0, 0, 130));
					}

					//Draw bullet glow
					foreach (var b in Bullets.Bullets.Where(item => item.HitBox.X > chunkLeft && item.HitBox.Right < chunkRight)) {
						var ammoDist = 100 - Util.Limit((int)(Game1.GameBounds.Height - t.Height) - 8 - b.HitBox.Bottom, 0, 100);
						var distFloat = ammoDist / 100f;

						sb.Draw(CircleGlow,
								new Rectangle(b.HitBox.X - (b.HitBox.Width / 2), (int)(Game1.GameBounds.Height - t.Height) - 10,
											  b.HitBox.Width * 2, 6), 
											  Color.FromNonPremultiplied((!b.Friendly ? 255 : 0), (b.Friendly ? 255 : 0), 0, 50 + (int)(distFloat * 200)));
					}
				}
			}

			//Draw the player
			if (PlayerOne.Alive) PlayerOne.Draw(sb);

			//Draw the Ominous Wall of Death
			Util.DrawLine(sb, new Vector2(0, 0), new Vector2(0, GroundChunks[0].Top - 5f), 8f * ScrollSpeed,
				Util.ColorInterpolate(Color.White, Color.Red, deathWallIntensity));

			//Draw sparks
			lParticles.Draw(sb);

			//Draw secondary lava line
			Util.DrawLine(sb, new Vector2(0, Game1.GameBounds.Height),
				new Vector2(Game1.GameBounds.Width, Game1.GameBounds.Height), 16f,
				Util.ColorInterpolate(Color.FromNonPremultiplied(30, 30, 30, 255), Color.Red, deathFloorIntensity));

			//Draw explosions
			exFactory.Draw(sb);

			//Draw bullets
			Bullets.Draw(sb);
			
			//Draw ammo and enemies
			foreach (var a in AmmoPickups) a.Draw(sb);
			foreach (var e in Enemies) e.Draw(sb);

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
					             Util.ColorInterpolate(Color.White, Color.Red, deathWallIntensity));

					if (highScoreNameSelected != 3) {
						Util.DrawPoly(sb, 2f, Util.ColorInterpolate(Color.White, Color.Red, deathWallIntensity),
						              new Vector2(boxX + 29, 305), new Vector2(boxX + 13, 305), new Vector2(boxX + 21, 295), //Draw the three arrow points
						              new Vector2(boxX + 29, 305)); //Then go back to the start
						Util.DrawPoly(sb, 2f, Util.ColorInterpolate(Color.White, Color.Red, deathWallIntensity),
						              new Vector2(boxX + 29, 359), new Vector2(boxX + 13, 359), new Vector2(boxX + 21, 369),
						              new Vector2(boxX + 29, 359));
					}

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
				var isPit = (Game1.GameRand.NextDouble() > 0.8 && GroundChunks[GroundChunks.Count - 1].Top < Game1.GameBounds.Height);

				var rndHeight = Game1.GameRand.Next(70, 160);
				while (rndHeight < GroundChunks[GroundChunks.Count - 1].Height + 15 
				&& rndHeight > GroundChunks[GroundChunks.Count - 1].Height - 15) {
					rndHeight = Game1.GameRand.Next(70, 160);
				}

				//Pit chunks should always be smaller than normal chunks so jumping them is actually possible
				var rndWidth = (!isPit ? Game1.GameRand.Next(150, Game1.GameBounds.Width/2) 
										: Game1.GameRand.Next(50, 130));

				//Add a new chunk, if it's a pit make it a good bit lower than the screen
				GroundChunks.Add(!isPit
									 ? new Chunk(endWidth, Game1.GameBounds.Height - rndHeight, rndWidth, rndHeight)
									 : new Chunk(endWidth, Game1.GameBounds.Height + 200, rndWidth, -200));

				var spawnChance = Game1.GameRand.NextDouble();

				//Not a pit, so go ahead and maybe add ammo or an enemy
				if (!isPit) {
					//Chance to add an ammo pickup to the new chunk; This may need to be tweaked
					if (spawnChance > 0.6) {
						AmmoPickups.Add(new Ammo(
							                new Vector2(
								                Game1.GameBounds.Width + LineWidth +
								                (float) ((GroundChunks[GroundChunks.Count - 1].Width - Ammo.Size)*Game1.GameRand.NextDouble()),
								                Game1.GameBounds.Height - rndHeight - 40)));
					}
						//Same as above, but for enemies
					else if (spawnChance > 0.3) {
						Enemies.Add(new Enemy(
							            new Vector2(
								            Game1.GameBounds.Width + LineWidth +
								            (float) ((GroundChunks[GroundChunks.Count - 1].Width - Enemy.SpriteSize.Width)*Game1.GameRand.NextDouble()),
								            Game1.GameBounds.Height - rndHeight - 4)));
						Enemies[Enemies.Count - 1].LastShotMs = gTime.TotalGameTime.TotalMilliseconds;
					}
				}
			}

			//Switches between increasing and decreasing the death wall's intensity, then does so
			if (deathWallIntensity >= 0.9f || deathWallIntensity <= 0.1f) deathWallGrowing = !deathWallGrowing;
			deathWallIntensity = deathWallIntensity + (deathWallGrowing ? 0.025f : -0.025f);

			if (deathFloorIntensity >= 0.9f || deathFloorIntensity <= 0.1f) deathFloorGrowing = !deathFloorGrowing;
			deathFloorIntensity = deathFloorIntensity + (deathFloorGrowing ? 0.005f : -0.005f);

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
				bGround.Update(ScrollSpeed);

				/*** COLLISIONS ***/

				//Player + world collisions
				var col = new Collisions {Left = false, Right = false, Down = false, Floor = 500};
				var lastChunk = GroundChunks[0].ToRect();

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
						playerExplodeBoop.Play(0.4f, 0, 0);
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
				
				if (lastLavaSparkTime + lavaSparkdelay < gTime.TotalGameTime.TotalMilliseconds) {
					lastLavaSparkTime = gTime.TotalGameTime.TotalMilliseconds;
					lavaSparkdelay = Game1.GameRand.Next(100, 1500);

					lParticles.Spark(new Vector2((float)(Game1.GameBounds.Width * Game1.GameRand.NextDouble()), Game1.GameBounds.Height - 4), (float)(10f * Game1.GameRand.NextDouble()), (float)(10f * (0.5 - Game1.GameRand.NextDouble())), 5f);
				}

				lParticles.Update(ScrollSpeed);

				//Update the player
				if (!levelStart)
					PlayerOne.Update(kState, prevState, ScrollSpeed, col);
			}

			//Increase level speed and score
			if (!levelStart && PlayerOne.Alive) {
				ScrollSpeed += 0.0007f;
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

	//This mostly exists as I wanted to add things, like the side details, to each 
	//Chunk without having to rewrite a good bit of code
	class Chunk {
		private RectangleF rect;
		public List<ChunkDetail> SideDetail = new List<ChunkDetail>();

		public Chunk(float x, float y, float width, float height) {
			rect = new RectangleF(x, y, width, height);

			//Create side of chunk details
			var detailCount = Game1.GameRand.Next(10, 40);
			for(var i = 0; i < detailCount; i++) {
				var detailSize = (int)(Game1.GameRand.NextDouble()*15);
				var xPos = (int)((Width - detailSize)*Game1.GameRand.NextDouble());
				var yPos = (int)((Height - detailSize)*Game1.GameRand.NextDouble());

				var detColor = Game1.GameRand.Next(10, 45);

				SideDetail.Add(
					new ChunkDetail { Rect = new Rectangle(xPos, yPos, detailSize, detailSize), 
						Col = Color.FromNonPremultiplied(detColor, detColor, detColor, 255) });
			}
		}

		public float X { get { return rect.X; } set { rect.X = value; } }
		public float Y { get { return rect.Y; } set { rect.Y = value; } }
		public float Width { get { return rect.Width; } set { rect.Width = value; } }
		public float Height { get { return rect.Height; } set { rect.Height = value; } }
		public float Left { get { return rect.Left; }}
		public float Right { get { return rect.Right; }}
		public float Top { get { return rect.Top; }}
		public float Bottom { get { return rect.Bottom; } }
		public Rectangle ToRect() { return new Rectangle((int)X, (int)Y, (int)Width, (int)Height); }
	}
	struct ChunkDetail { public Rectangle Rect; public Color Col; }
}

