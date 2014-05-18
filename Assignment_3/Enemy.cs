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
		public Vector2 Crosshair;

		public static Vector2 Size = new Vector2(40, 40);

		public bool Alive = true;
		
		public Enemy(Vector2 startPos) {
			Position = startPos;
			//Aim straight at the left side by default
			//As spawning is off screen the enemy will start tracking the player anyway
			//but should only be able to fire while on screen for fairness, obviously
			Crosshair = new Vector2(0, Position.Y);
		}

		public void Update(float scrollSpeed, Vector2 aimingAt) {
			//TODO: Update aim direction

			//If the enemy has moved off the left side of the screen, proceed to kill it
			if (Position.X + Size.X + 1 < 0) {
				Alive = false;
			}

			Position.X -= scrollSpeed;
		}
		public void Draw(SpriteBatch sb) {
			sb.Draw(Game1.OnePxWhite, HitBox, Color.LightCoral);

			/*TODO: Draw turret*/
		}

		//A hitbox rectangle representing the Enemy
		public Rectangle HitBox { get { return new Rectangle((int)Position.X, (int)Position.Y-(int)Size.Y, (int)Size.X, (int)Size.Y); } }
	}
}
