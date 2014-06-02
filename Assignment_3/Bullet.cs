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
		public void Update(GameTime gt) {
			foreach (var b in Bullets) b.Update(gt);
			
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

		public List<BulletSpark> Sparks = new List<BulletSpark>(); 

		public Bullet(Vector2 startPos, Vector2 dir, bool friendly, float scrollSpeed) {
			Position = new Vector2(startPos.X - (BulletSize.Width / 2f), startPos.Y - (BulletSize.Height / 2f));
			Direction = dir;
			Friendly = friendly;
			negSpeed = scrollSpeed;

			if (!Friendly) Speed /= 2;
		}

		public void Draw(SpriteBatch sb) {
			var col = Friendly ? Color.Green : Color.Red;

			foreach (var s in Sparks) s.Draw(sb, col);

			Util.DrawCube(sb, HitBox, 6, 0.4f, -0.5f, col, Util.MuteColor(col, 0.2f),
						  Util.MuteColor(col, 0.4f));
		}
		public void Update(GameTime gt) {
			Position += (Direction*(Speed + (negSpeed / 2)));
			Position.X -= negSpeed;
			
			if (Alive) Alive = (new Rectangle(0, 0, Game1.GameBounds.Width, Game1.GameBounds.Height).Intersects(HitBox)) || Sparks.Count > 0;
			
			var tmpDir = Vector2.Negate(Direction);
			var varianceX = (float)(tmpDir.X + 0.5f * (0.5f * Game1.GameRand.NextDouble()));
			var varianceY = (float)(tmpDir.Y + 2f * (0.5f * Game1.GameRand.NextDouble()));

			Console.WriteLine(varianceX);

			if (Game1.GameRand.NextDouble() < 0.2) {
				Sparks.Add(new BulletSpark(Position, 
					new Vector2(varianceX, varianceY), 
					Game1.GameRand.Next(50, 300), 
					gt.TotalGameTime.TotalMilliseconds, 
					(float)(2 + (5 * Game1.GameRand.NextDouble()))));
			}

			foreach (var s in Sparks) s.Update(gt, negSpeed);
			Sparks.RemoveAll(item => !item.Alive);
		}

		public Rectangle HitBox { get { return new Rectangle((int)Position.X, (int)Position.Y, BulletSize.Width, BulletSize.Height); } }
	}
	public class BulletSpark {
		public Vector2 Position, Direction;

		private readonly float speed;

		private readonly int size = 4;

		public int Decay;

		private readonly double spawnedTime;

		public bool Alive = true;

		public BulletSpark(Vector2 pos, Vector2 dir, int decay, double spawnTime, float partSpeed) {
			Decay = decay;
			Position = pos;
			Direction = dir;
			spawnedTime = spawnTime;
			speed = partSpeed;

			size = (int)(1 + (3*Game1.GameRand.NextDouble()));
		}

		public void Draw(SpriteBatch sb, Color col) {
			sb.Draw(Game1.OnePxWhite, new Rectangle((int)Position.X, (int)Position.Y, size, size), col);
		}

		public void Update(GameTime gt, float negSpeed) {
			Position += (Direction*(speed/2 - negSpeed/2));

			if (gt.TotalGameTime.TotalMilliseconds > spawnedTime + Decay) {
				Alive = false;
			}
		}
	}
}
