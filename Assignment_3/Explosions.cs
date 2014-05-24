using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment_3 {
	public class ExplosionFactory {
		public List<Explosion> Explosions = new List<Explosion>();

		public bool NoUpdates = true;

		public void Explode(Vector2 startPosition, Color color, GameTime gt) {
			Explosions.Add(new Explosion(startPosition, gt.TotalGameTime.TotalMilliseconds, color));
		}
		public void Explode(Vector2 startPosition, Color color, GameTime gt, int minDecay, int maxDecay) {
			Explosions.Add(new Explosion(startPosition, gt.TotalGameTime.TotalMilliseconds, color, minDecay, maxDecay));
		}

		public void Update(GameTime gt, float scrollSpeed) {
			foreach (var e in Explosions) e.Update(gt, scrollSpeed);
			Explosions.RemoveAll(item => item.Particles.Count == 0);

			NoUpdates = (Explosions.Count < 1);
		}
		public void Draw(SpriteBatch sb) {
			foreach (var e in Explosions) e.Draw(sb);
		}
	}
	public class Explosion {
		public Vector2 Origin;
		public List<Particle> Particles = new List<Particle>();
		public double CreatedTime;
		public Color ParticleColor;

		public Explosion(Vector2 position, double created, Color col, int minDecay = 200, int maxDecay = 500) {
			Origin = position;
			CreatedTime = created;

			var partCount = Game1.GameRand.Next(5, 20);

			for (var i = 0; i < partCount; i++) {
				var decayTime = Game1.GameRand.Next(minDecay, maxDecay);
				var hoDir = (float)(Game1.GameRand.NextDouble()*2f) - 1.0f;
				var veDir = (float)(Game1.GameRand.NextDouble()*2f) - 1.0f;
				var direction = new Vector2(hoDir, veDir);

				var p = new Particle {DecayTime = decayTime, Dir = direction, Pos = position, Speed = 3.0f, Col = Util.SimilarColor(col, 0.2f)};

				Particles.Add(p);
			}
		}

		public void Draw(SpriteBatch sb) {
			foreach (var p in Particles) p.Draw(sb);
		}
		public void Update(GameTime gt, float scrollSpeed) {
			foreach (var p in Particles) p.Update(scrollSpeed);

			Particles.RemoveAll(item => item.DecayTime + CreatedTime < gt.TotalGameTime.TotalMilliseconds);
		}
	}
	public class Particle {
		public double DecayTime;
		public float Speed;
		public Vector2 Pos, Dir;
		public Color Col;

		public void Draw(SpriteBatch sb) {
			sb.Draw(Game1.OnePxWhite, HitBox, Col);
		}
		public void Update(float scrollSpeed) {
			Pos += Dir*Speed;
			Pos.X -= scrollSpeed;
		}

		public Rectangle HitBox { get { return new Rectangle((int) Pos.X - 2, (int) Pos.Y - 2, 4, 4); }}
	}
}
