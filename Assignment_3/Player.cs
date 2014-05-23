using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment_3 {
	internal class Player {
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

		public Texture2D PlayerSprite;
		public void LoadContent(ContentManager content) { 
			/*PlayerSprite = content.Load<Texture2D>("player");
			Size = new Vector2(PlayerSprite.Width, PlayerSprite.Height;*/ 
		}

		public Player(Vector2 startPos) {
			Position = startPos;
		}

		public void Update(KeyboardState ks, KeyboardState? prevState, float stageSpeed, Collisions collisions) {
			//Update player speed
			MovementSpeed = 3.0f + stageSpeed;

			//Keep the player moving with the world while grounded
			//if (collisions.Down) //Wait no, make that just whenever
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
				Jumping = false;

				//start jump upon pressing X
				if (ks.IsKeyDown(Keys.X) && prevState.Value.IsKeyUp(Keys.X) && !Jumping && !Ducking) {
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
			sb.Draw(Game1.OnePxWhite, HitBox, Color.LightGreen);
			sb.Draw(Game1.OnePxWhite, new Rectangle((int)CenterPosition.X - (FacingRight ? 0 : 18), (int)(CenterPosition.Y - Bullet.BulletSize.Height + (Ducking ? HitBox.Height / 2 : 0)), 18, Bullet.BulletSize.Height), Color.DarkGreen); 
			/* Debug hitbox drawing 
			sb.Draw(Game1.OnePxWhite, BottomBox, Color.Red);
			sb.Draw(Game1.OnePxWhite, LeftBox, Color.Red);
			sb.Draw(Game1.OnePxWhite, RightBox, Color.Red);*/
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
