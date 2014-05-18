using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment_3 {
	internal class Player {
		public Vector2 Position;
		public static Size PlayerSize = new Size(20, 40);

		public float MovementSpeed = 5.0f;
		public float VertMomentum = 5f;

		public bool Alive = true;
		public bool Jumping = false;

		public Texture2D PlayerSprite;
		public void LoadContent(ContentManager content) { 
			//TODO: Player sprite
			/*PlayerSprite = content.Load<Texture2D>("player");
			Size = new Vector2(PlayerSprite.Width, PlayerSprite.Height;*/ 
		}

		public Player(Vector2 startPos) {
			Position = startPos;
		}

		public void Update(KeyboardState ks, KeyboardState? prevState, float stageSpeed, Collisions collisions) {
			Position.X -= stageSpeed;

			if (ks.IsKeyDown(Keys.Left) && !collisions.Left) {
				Position.X -= MovementSpeed;
			}
			else if (ks.IsKeyDown(Keys.Right) && !collisions.Right) {
				Position.X += MovementSpeed;
			}

			if (Position.X < 0) Position.X = 0;
			if (Position.X + PlayerSize.Width > Game1.GameBounds.Width)
				Position.X = Game1.GameBounds.Width - PlayerSize.Width;

			if (collisions.Down) {
				Jumping = false;

				if (ks.IsKeyDown(Keys.X) && !Jumping) {
					VertMomentum = -12f;
					Jumping = true;
				}
			}

			if (VertMomentum < 12f)
				VertMomentum += 0.5f;

			Position.Y += VertMomentum;
			
			if (Position.Y + PlayerSize.Height > collisions.Floor)
				Position.Y = collisions.Floor - PlayerSize.Height;

			//if (ks.IsKeyDown(Keys.Z)) { } //Shoot?
		}

		public void Draw(SpriteBatch sb) { sb.Draw(Game1.OnePxWhite, HitBox, Color.LightGreen); 
			/* Debug hitbox drawing /**/
			sb.Draw(Game1.OnePxWhite, BottomBox, Color.Red);
			sb.Draw(Game1.OnePxWhite, LeftBox, Color.Red);
			sb.Draw(Game1.OnePxWhite, RightBox, Color.Red);
		}

		//Hitbox rectangles
		public Rectangle HitBox { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)PlayerSize.Width, (int)PlayerSize.Height); } }
		public Rectangle BottomBox { get { return new Rectangle((int)(Position.X + 2.5f), (int)Position.Y + PlayerSize.Height, (int)(PlayerSize.Width - 5f), 1); } }
		public Rectangle LeftBox { get { return new Rectangle((int) Position.X, (int) Position.Y, 1, (int) PlayerSize.Height - 5); } }
		public Rectangle RightBox { get { return new Rectangle((int)Position.X + (int)PlayerSize.Width, (int)Position.Y, 1, (int)PlayerSize.Height - 5); } }
	}

	public struct Collisions {
		public bool Left, Right, Down;
		public int Floor, RightSide, LeftSide;
	}
}
