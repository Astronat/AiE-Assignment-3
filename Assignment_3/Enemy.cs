using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment_3 {
	internal class Enemy {
		public Vector2 Position;
		public Vector2 CenterPosition;
		public Vector2 Crosshair;
		public Vector2 AimDirection = new Vector2(-1, 0);

		public static Size SpriteSize = new Size(40, 40);

		public double LastShotMs = 0;

		public bool Alive = true;
		
		public Enemy(Vector2 startPos) {
			Position = startPos;
			//Aim straight at the left side by default
			//As spawning is off screen the enemy will start tracking the player anyway
			//but should only be able to fire while on screen for fairness, obviously
			Crosshair = new Vector2(0, Position.Y);
		}

		public void Update(float scrollSpeed, Vector2 aimingAt) {
			CenterPosition = new Vector2(Position.X + (SpriteSize.Width / 2f), Position.Y - (SpriteSize.Height / 2f));

			//The direction for the turret "crosshair" to move to keep aiming at the player
			var crosshairMvDir = Crosshair - aimingAt;
			crosshairMvDir.Normalize();

			//Move the crosshair towards the player
			Crosshair = Crosshair - (crosshairMvDir*(scrollSpeed*1.5f));

			//And set the Aim direction for the turret
			AimDirection = Crosshair-CenterPosition;
			AimDirection.Normalize();

			//If the enemy has moved off the left side of the screen, proceed to kill it
			if (Position.X + SpriteSize.Width + 1 < 0) {
				Alive = false;
			}

			Position.X -= scrollSpeed;
		}
		public void Draw(SpriteBatch sb) {
			//Draw enemy box
			var col = Color.LightCoral;
			Util.DrawCube(sb, HitBox, 14, 0.4f, -0.5f, col, Util.MuteColor(col, 0.2f),
						  Util.MuteColor(col, 0.4f));
			
			//Draw gun
			Util.DrawLine(sb, CenterPosition, CenterPosition + (AimDirection * 30), 8f,
						  Color.Red);
		}

		//A hitbox rectangle representing the Enemy
		public Rectangle HitBox { get {
			return new Rectangle((int)Position.X, (int)Position.Y-(SpriteSize.Height - 3), SpriteSize.Width, SpriteSize.Height);
		} }
	}
}
