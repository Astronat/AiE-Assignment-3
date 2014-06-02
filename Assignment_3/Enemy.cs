using System;
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

		public void Draw(SpriteBatch sb, float colorIntensity) {
			var x = 0;
			var y = 0;

			if (AimDirection.X >= -1f && AimDirection.X <= -0.5f) {
				x = HitBox.X - 8;
				y = Util.Limit((int)(HitBox.Y + (HitBox.Height / 2) + (HitBox.Height * AimDirection.Y)) - 5, HitBox.Y, HitBox.Y + HitBox.Height);
			}
			else if (AimDirection.X >= 0.5f && AimDirection.X <= 1f) {
				x = HitBox.Right + 2;
				y = Util.Limit((int)(HitBox.Y + (HitBox.Height / 2) + (HitBox.Height * AimDirection.Y)) - 5, HitBox.Y, HitBox.Y + HitBox.Height);
			}
			else {
				y = HitBox.Y - 12;
				x = Util.Limit((int)(HitBox.X + (HitBox.Width / 2) + (HitBox.Width * (AimDirection.X))), HitBox.X, HitBox.X + HitBox.Width);
			}

			//Draw enemy box
			//Draw before the cannon if the cannon is not facing left
			if (!(AimDirection.X > -1f && AimDirection.X < -0.5f)) {
				DrawTurretBox(sb, colorIntensity);
			}

			//Draw the gun
			Util.DrawCube(sb, new Rectangle(x, y, 10, 10), 8, 0.2f, -0.5f,
						  Color.Red, Util.MuteColor(Color.Red, 0.3f), Util.MuteColor(Color.Red, 0.5f));

			//Draw after the cannon if it IS facing left
			if (AimDirection.X > -1f && AimDirection.X < -0.5f) {
				DrawTurretBox(sb, colorIntensity);
			}
			
			//Draw crosshair
			sb.Draw(Game1.OnePxWhite, new Rectangle((int)Crosshair.X, (int) Crosshair.Y, 10, 10),Color.Fuchsia);
		}

		private void DrawTurretBox(SpriteBatch sb, float colorIntensity) {
			Util.DrawCube(sb, HitBox, 30, 0.2f, -0.5f,
						  Color.FromNonPremultiplied(50, 50, 50, 255),
						  Color.FromNonPremultiplied(150, 150, 150, 255),
						  Color.FromNonPremultiplied(100, 100, 100, 255));

			var sideGlowRect = HitBox;
			sideGlowRect.Inflate(-10, -10);
			Util.DrawBox(sb, sideGlowRect, 2f, Util.ColorInterpolate(Color.FromNonPremultiplied(30, 30, 30, 255), Color.Red, colorIntensity));
		}

		//A hitbox rectangle representing the Enemy
		public Rectangle HitBox { get {
			return new Rectangle((int)Position.X, (int)Position.Y-(SpriteSize.Height - 3), SpriteSize.Width, SpriteSize.Height);
		} }
	}
}
