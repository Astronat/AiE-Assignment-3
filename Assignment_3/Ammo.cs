using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment_3 {
	class Ammo {
		public Vector2 Position = new Vector2();

		public const int Size = 16;

		//Used for Stage object removal checking
		public bool Alive = true;

		//Default constructor
		public Ammo (Vector2 startPos) {
			Position = startPos;
		}

		//Draws a SizexSize yellow box
		public void Draw(SpriteBatch sb) {
			sb.Draw(Game1.OnePxWhite, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), Color.Yellow);
		}

		public void Update(float speed) {
			//If the pickup has moved off the left side of the screen
			//proceed to kill it
			if (Position.X + Size + 1 < 0) {
				Alive = false;
			}

			//Move the ammo hitbox left
			Position.X -= speed;
		}
	}
}
