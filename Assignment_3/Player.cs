using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment_3 {
	internal class Player {
		public Vector2 Position;
		public Vector2 Size;

		public bool Alive = true;

		public Texture2D PlayerSprite;
		public void LoadContent(ContentManager content) { 
			/*PlayerSprite = content.Load<Texture2D>("player");
			Size = new Vector2(PlayerSprite.Width, PlayerSprite.Height;*/ 
		}

		public Player(Vector2 startPos) {
			Position = startPos;
		}

		public void Update() {}
		public void Draw(SpriteBatch sb) {}

		//A hitbox rectangle representing the Player sprite
		public Rectangle HitBox { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y); } }
	}
}
