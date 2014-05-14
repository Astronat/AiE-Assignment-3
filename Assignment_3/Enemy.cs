using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment_3 {
	internal class Enemy {
		public Vector2 Position;
		public Vector2 Size;

		public bool Alive = true;
		
		public Enemy(Vector2 startPos) {
			Position = startPos;
		}

		public void Update(float scrollSpeed, Vector2 aimingAt) {
			//TODO: Update aim direction

			//If the enemy has moved off the left side of the screen, proceed to kill it
			if (Position.X + Size.X + 1 < 0) {
				Alive = false;
			}

			Position.X -= scrollSpeed;
		}
		public void Draw(SpriteBatch sb) { /*TODO: All drawing*/ }

		//A hitbox rectangle representing the Enemy
		public Rectangle HitBox { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y); } }
	}
}
