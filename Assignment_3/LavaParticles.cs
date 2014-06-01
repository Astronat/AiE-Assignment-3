using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment_3 {
	class LavaParticles {
		public List<LParticle> Particles = new List<LParticle>(); 

		//Create a new lava spark
		public void Spark(Vector2 position, float vertMomentum, float horiMomentum, float size) {
			Particles.Add(new LParticle(position, vertMomentum, horiMomentum, size));
		}

		public void Update(float scrollSpeed) {
			foreach (var p in Particles)
				p.Update(scrollSpeed);

			Particles.RemoveAll(item => item.Position.Y > Game1.GameBounds.Height + 30 && item.Sparks.Count == 0);
		}
		public void Draw(SpriteBatch sb) {
			foreach (var p in Particles)
				p.Draw(sb);
		}

	}
	class LParticle {
		private float vertMomentum, colorIntensity;
		private readonly float horiMomentum, size;
		private bool colorGrowing = false;
		public Vector2 Position;

		public List<MiniParticle> Sparks = new List<MiniParticle>();

		public LParticle(Vector2 pos, float vertMo, float horiMo, float partSize) {
			Position = pos;
			vertMomentum = vertMo;
			horiMomentum = horiMo;
			size = partSize;
		}

		public void Update(float scrollSpeed) {
			Position.X -= scrollSpeed;
			Position.X += horiMomentum;
			Position.Y -= vertMomentum;

			if (colorIntensity >= 0.9f || colorIntensity <= 0.1f) colorGrowing = !colorGrowing;
			colorIntensity = colorIntensity + (colorGrowing ? 0.125f : -0.125f);

			if (vertMomentum > -15f)
				vertMomentum -= 0.5f;

			if (Game1.GameRand.NextDouble() > 0.7) {
				Sparks.Add(new MiniParticle(Position, (float)(Game1.GameRand.NextDouble() * 4f) - 2f, horiMomentum / 2f, 2f, Util.ColorInterpolate(Color.Red, Color.Orange, colorIntensity)));
			}

			foreach (var s in Sparks)
				s.Update(scrollSpeed);

			Sparks.RemoveAll(item => item.Position.Y > Game1.GameBounds.Height);
		}
		public void Draw(SpriteBatch sb) {
			sb.Draw(Game1.OnePxWhite, new Rectangle((int)Position.X, (int)Position.Y, (int)size, (int)size), Util.ColorInterpolate(Color.Red, Color.Orange, colorIntensity));

			foreach (var s in Sparks)
				s.Draw(sb);
		}
	}
	class MiniParticle {
		private float vertMomentum; 
		private readonly float horiMomentum, size;
		private readonly Color col;
		public Vector2 Position;
		
		public MiniParticle(Vector2 pos, float vertMo, float horiMo, float partSize, Color color) {
			Position = pos;
			vertMomentum = vertMo;
			horiMomentum = horiMo;
			size = partSize;
			col = color;
		}

		public void Update(float scrollSpeed) {
			Position.X -= scrollSpeed;
			Position.X += horiMomentum;
			Position.Y -= vertMomentum;

			if (vertMomentum > -15f)
				vertMomentum -= 0.5f;
		}
		public void Draw(SpriteBatch sb) {
			sb.Draw(Game1.OnePxWhite, new Rectangle((int)Position.X, (int)Position.Y, (int)size, (int)size), col);
		}
	}
}
