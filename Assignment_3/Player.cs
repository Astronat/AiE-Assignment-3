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
		public static Size PlayerSize = new Size(32, 64);

		public float MovementSpeed = 5.0f;
		public float Momentum = 0f;

		public bool Alive = true;

		public Texture2D PlayerSprite;
		public void LoadContent(ContentManager content) { 
			//TODO: Player sprite
			/*PlayerSprite = content.Load<Texture2D>("player");
			Size = new Vector2(PlayerSprite.Width, PlayerSprite.Height;*/ 
		}

		public Player(Vector2 startPos) {
			Position = startPos;
		}

		public void Update(KeyboardState ks, float stageSpeed, MovementAllowed freedoms) {
			Position.X -= stageSpeed;

			if (ks.IsKeyDown(Keys.Left) && freedoms.Left) {
				Position.X -= MovementSpeed;
			}
			else if (ks.IsKeyDown(Keys.Right) && freedoms.Right) {
				Position.X += MovementSpeed;
				if (Position.X + PlayerSize.Width > Game1.GameBounds.Width) 
					Position.X = Game1.GameBounds.Width - PlayerSize.Width;
			}

			if (Position.X < 0) Position.X = 0;
			//if (ks.IsKeyDown(Keys.X)) { } //Jump?
			//if (ks.IsKeyDown(Keys.Z)) { } //Shoot?
		}

		public void Draw(SpriteBatch sb) { sb.Draw(Game1.OnePxWhite, HitBox, Color.LightGreen); }

		//A hitbox rectangle representing the Player sprite
		public Rectangle HitBox { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)PlayerSize.Width, (int)PlayerSize.Height); } }
		
	}

	public struct MovementAllowed {
		public bool Left, Right, Down;
	}
}
