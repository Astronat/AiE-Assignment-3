using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment_3 {
	internal class Player {
		//private static Texture2D playerSprite;

		private static SoundEffect jumpBoop;
		private static SoundEffect landBoop;

		public Vector2 Position;
		public Vector2 CenterPosition { //Returns the center position of the player's sprite
			get { return new Vector2(Position.X + (PlayerSize.Width / 2f), Position.Y + (PlayerSize.Height / 2f));}
		}
		public static Size PlayerSize = new Size(20, 40);

		public float MovementSpeed = 5.0f;
		public float VertMomentum = 5f;

		private int ammoCount = 5;
		public int AmmoCount { 
			get { return ammoCount; } 
			//Don't allow this to be set higher than MaxAmmo for obvious reasons
			set { ammoCount = value < MaxAmmo ? value : MaxAmmo; }
		}
		public const int MaxAmmo = 5;

		public bool Alive = true;
		public bool Jumping = false;
		public bool Ducking = false;
		public bool FacingRight = true;
		public bool Firing = true;

		public Texture2D PlayerSprite;
		public static void LoadContent(ContentManager content) {
			//playerSprite = content.Load<Texture2D>("player");
			jumpBoop = content.Load<SoundEffect>("jump");
			landBoop = content.Load<SoundEffect>("land");
		}

		public Player(Vector2 startPos) {
			Position = startPos;
		}

		public void Update(KeyboardState ks, KeyboardState? prevState, float stageSpeed, Collisions collisions) {
			//Update player speed
			MovementSpeed = 3.0f + stageSpeed;

			//Keep the player moving with the world
			Position.X -= stageSpeed;

			Ducking = ks.IsKeyDown(Keys.Down);

			//Left and right movement
			if (ks.IsKeyDown(Keys.Left) && !collisions.Left) {
				if (!Ducking)
					Position.X -= MovementSpeed;
				
				FacingRight = false;
			}
			else if (ks.IsKeyDown(Keys.Right) && !collisions.Right) {
				if (!Ducking)
					Position.X += MovementSpeed;

				FacingRight = true;
			}

			//Make sure the player can't just run off screen
			if (Position.X < 0) Position.X = 0;
			if (Position.X + PlayerSize.Width > Game1.GameBounds.Width)
				Position.X = Game1.GameBounds.Width - PlayerSize.Width;
			
			//Ground collision and jumping
			if (collisions.Down) {
				if (Jumping) landBoop.Play();
				Jumping = false;

				//start jump upon pressing X
				if (ks.IsKeyDown(Keys.X) && prevState.Value.IsKeyUp(Keys.X) && !Jumping && !Ducking) {
					jumpBoop.Play();
					VertMomentum = -12f;
					Jumping = true;
				}
			}

			//Cancel momentum and stop rising if the button is released
			if (ks.IsKeyUp(Keys.X) && prevState.Value.IsKeyDown(Keys.X) && Jumping && VertMomentum < 0f) {
				VertMomentum = 0f;
			}
			
			//Keep vertical momentum updated
			if (VertMomentum < 12f)
				VertMomentum += 0.5f;
			if (collisions.Down && !Jumping)
				VertMomentum = 0f;

			Position.Y += VertMomentum;
			
			//Make sure the player can't fall through the floor
			if (Position.Y + PlayerSize.Height > collisions.Floor)
				Position.Y = collisions.Floor - PlayerSize.Height;
		}

		public void Draw(SpriteBatch sb) {
			var shadesColor = Color.FromNonPremultiplied(30, 30, 30, 255);

			//Doubled up drawing so that if the player is facing left, the gun draws first so it's behind the character
			if (FacingRight) {
				//Draw player
				Util.DrawCube(sb, HitBox, (int) (PlayerSize.Width*0.8), 0.2f, -0.5f, Color.LightGreen,
				              Util.MuteColor(Color.LightGreen, 0.5f), Util.MuteColor(Color.LightGreen, 0.3f));

				//Draw stunna shades
				Util.DrawCube(sb,
					new Rectangle(HitBox.Right, HitBox.Y + 4, (int)((PlayerSize.Width * 0.8) * 0.2), 6), 
					(int)(PlayerSize.Width * 0.8), 0.2f, -0.5f, shadesColor, shadesColor, shadesColor);
				//And shade arm
				Util.DrawCube(sb,
					new Rectangle(HitBox.Right - (HitBox.Width / 2), HitBox.Y + 4, (HitBox.Width / 2), 2), 
					2, 0.2f, -0.5f, shadesColor, shadesColor, shadesColor);
				Util.DrawCube(sb,
					new Rectangle(HitBox.Right - (HitBox.Width / 2), HitBox.Y + 6, 2, 2), 
					2, 0.2f, -0.5f, shadesColor, shadesColor, shadesColor);

				//Draw gun handle
				Util.DrawCube(sb,
							  new Rectangle((int)CenterPosition.X,
											(int)(CenterPosition.Y - Bullet.BulletSize.Height + (Ducking ? HitBox.Height / 2 : 0) + (int)(Bullet.BulletSize.Height * 0.6f)),
											(int)(Bullet.BulletSize.Height * 0.6f), (int)(Bullet.BulletSize.Height * 0.6f)),
							  4, .5f, -.5f, 
							  Color.DarkGray, Util.MuteColor(Color.DarkGray, 0.5f),
							  Util.MuteColor(Color.DarkGray, 0.3f));

				//Draw gun
				Util.DrawCube(sb,
				              new Rectangle((int) CenterPosition.X,
				                            (int) (CenterPosition.Y - Bullet.BulletSize.Height + (Ducking ? HitBox.Height/2 : 0)),
				                            18, (int) (Bullet.BulletSize.Height*0.6f)),
				              4, .5f, -.5f, 
							  Color.DarkGray, Util.MuteColor(Color.DarkGray, 0.5f), Util.MuteColor(Color.DarkGray, 0.3f));

				//Draw very :3 muzzle flash
				if (Firing) {
					//Draw main flash
					Util.DrawCube(sb,
							  new Rectangle((int)CenterPosition.X + 18,
											(int)(CenterPosition.Y - Bullet.BulletSize.Height + (Ducking ? HitBox.Height / 2 : 0)) - 2,
											(int)(Bullet.BulletSize.Height * 0.6f), (int)(Bullet.BulletSize.Height * 0.6f) + 4),
							  4, .5f, -.5f,
							   Color.Yellow, Util.MuteColor(Color.Orange, 0.5f), Util.MuteColor(Color.Orange, 0.3f));

					//Draw smaller part
					Util.DrawCube(sb,
							  new Rectangle((int)CenterPosition.X + 18 + (int)(Bullet.BulletSize.Height * 0.6f),
											(int)(CenterPosition.Y - Bullet.BulletSize.Height + (Ducking ? HitBox.Height / 2 : 0)),
											(int)(Bullet.BulletSize.Height * 0.6f), (int)(Bullet.BulletSize.Height * 0.6f)),
							  4, .5f, -.5f,
							  Color.Yellow, Util.MuteColor(Color.Orange, 0.5f), Util.MuteColor(Color.Orange, 0.3f));
				}
			}
			else {
				//Draw very :3 muzzle flash
				if (Firing) {
					//Draw small bit of flash
					Util.DrawCube(sb,
							  new Rectangle((int)CenterPosition.X - 18 - (int)(Bullet.BulletSize.Height * 0.6f) - 4,
											(int)(CenterPosition.Y - Bullet.BulletSize.Height + (Ducking ? HitBox.Height / 2 : 0)),
											(int)(Bullet.BulletSize.Height * 0.6f), (int)(Bullet.BulletSize.Height * 0.6f)),
							  4, .5f, -.5f,
							  Color.Yellow, Util.MuteColor(Color.Orange, 0.5f), Util.MuteColor(Color.Orange, 0.3f));

					//Draw main flash
					Util.DrawCube(sb,
							  new Rectangle((int)CenterPosition.X - 18 - 4,
											(int)(CenterPosition.Y - Bullet.BulletSize.Height + (Ducking ? HitBox.Height / 2 : 0)) - 2,
											(int)(Bullet.BulletSize.Height * 0.6f), (int)(Bullet.BulletSize.Height * 0.6f) + 4),
							  4, .5f, -.5f,
							   Color.Yellow, Util.MuteColor(Color.Orange, 0.5f), Util.MuteColor(Color.Orange, 0.3f));
				}

				//Draw gun, mostly just the tip
				Util.DrawCube(sb,
							  new Rectangle((int)CenterPosition.X - 18,
											(int)(CenterPosition.Y - Bullet.BulletSize.Height + (Ducking ? HitBox.Height / 2 : 0)),
											18, (int)(Bullet.BulletSize.Height * 0.6f)),
							  4, .2f, -.5f, 
							  Color.DarkGray, Util.MuteColor(Color.DarkGray, 0.5f), Util.MuteColor(Color.DarkGray, 0.3f));

				//Draw stunna shades
				Util.DrawCube(sb,
					new Rectangle(HitBox.X - (int)((PlayerSize.Width * 0.8) * 0.2), HitBox.Y + 4, (int)((PlayerSize.Width * 0.8) * 0.2), 6),
					(int)(PlayerSize.Width * 0.8), 0.2f, -0.5f, shadesColor, shadesColor, shadesColor);

				//Draw player
				Util.DrawCube(sb, HitBox, (int)(PlayerSize.Width * 0.8), 0.2f, -0.5f, Color.LightGreen,
							  Util.MuteColor(Color.LightGreen, 0.5f), Util.MuteColor(Color.LightGreen, 0.3f));

				//Stunna shade arm
				Util.DrawCube(sb,
					new Rectangle(HitBox.X, HitBox.Y + 4, (HitBox.Width / 2), 2), 
					2, 0.2f, -0.5f, shadesColor, shadesColor, shadesColor);
				Util.DrawCube(sb,
					new Rectangle(HitBox.X + (HitBox.Width / 2) - 2, HitBox.Y + 6, 2, 2), 
					2, 0.2f, -0.5f, shadesColor, shadesColor, shadesColor);
			}
		}

		//Hitbox rectangles
		public Rectangle HitBox { get {
			return new Rectangle((int)Position.X, (int)Position.Y + (Ducking ? PlayerSize.Height / 2 : 0), PlayerSize.Width, PlayerSize.Height / (Ducking ? 2 : 1));
		} }
		public Rectangle BottomBox { get { return new Rectangle((int)(Position.X + 2.5f), (int)Position.Y + PlayerSize.Height, (int)(PlayerSize.Width - 5f), 1); } }
		public Rectangle LeftBox { get { return new Rectangle((int) Position.X, (int) Position.Y, 1, PlayerSize.Height - 5); } }
		public Rectangle RightBox { get { return new Rectangle((int)Position.X + PlayerSize.Width, (int)Position.Y, 1, PlayerSize.Height - 5); } }
	}

	public struct Collisions {
		public bool Left, Right, Down;
		public int Floor, RightSide, LeftSide;
	}
}
