using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment_3 {
	class BulletFactory {
		public List<Bullet> Bullets = new List<Bullet>();

		//Draws all bullets
		public void Draw(SpriteBatch sb) {
			foreach (var b in Bullets) b.Draw(sb);
		}
		//Updates all bullets and removes dead bullets
		public void Update() {
			foreach (var b in Bullets) b.Update();
			
			Bullets.RemoveAll(item => !item.Alive);
		}
		//Creates a new bullet
		public void FireBullet(Vector2 startPos, Vector2 direction, bool friendly, float scrollSpeed) {
			Bullets.Add(new Bullet(startPos, direction, friendly, scrollSpeed));
		}

		//Bullet counts
		public int FriendlyBulletCount { get { return Bullets.FindAll(item => item.Friendly).Count; } }
		public int EnemyBulletCount { get { return Bullets.FindAll(item => !item.Friendly).Count; } }
	}
	class Bullet {
		public Vector2 Position, Direction;

		public static Size BulletSize = new Size(8,8);

		public float Speed = 8.0f;
		private readonly float negSpeed;

		public bool Friendly = false;
		public bool Alive = true;
		
		public Bullet(Vector2 startPos, Vector2 dir, bool friendly, float scrollSpeed) {
			Position = new Vector2(startPos.X - (BulletSize.Width / 2f), startPos.Y - (BulletSize.Height / 2f));
			Direction = dir;
			Friendly = friendly;
			negSpeed = scrollSpeed;

			if (!Friendly) Speed /= 2;
		}

		public void Draw(SpriteBatch sb) {
			sb.Draw(Game1.OnePxWhite, HitBox, Color.Red);
		}
		public void Update() {
			Position += (Direction*(Speed + (negSpeed / 2)));
			Position.X -= negSpeed;
			
			if (Alive) Alive = (new Rectangle(0, 0, Game1.GameBounds.Width, Game1.GameBounds.Height).Intersects(HitBox));
		}

		public Rectangle HitBox { get { return new Rectangle((int)Position.X, (int)Position.Y, BulletSize.Width, BulletSize.Height); } }
	}
}
