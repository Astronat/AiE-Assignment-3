using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment_3 {
	internal class Player {
		public Vector2 Position;
		public Vector2 Size = new Vector2(32, 64);

		public bool Alive = true;

		public Texture2D PlayerSprite;
		public void LoadContent(ContentManager content) { 
			//TODO: Player sprite
			/*PlayerSprite = content.Load<Texture2D>("player");
			Size = new Vector2(PlayerSprite.Width, PlayerSprite.Height;*/ 
		}

		public Player(Vector2 startPos) {
			Position = startPos;
		}

		public void Update(KeyboardState ks) {
			/*TODO: Player movement and action code*/
			if (ks.IsKeyDown(Keys.Left)) { }
			if (ks.IsKeyDown(Keys.Right)) { }
			//if (ks.IsKeyDown(Keys.X)) { } //Jump?
			//if (ks.IsKeyDown(Keys.Z)) { } //Shoot?
		}

		public void Draw(SpriteBatch sb) { sb.Draw(Game1.OnePxWhite, HitBox, Color.LightGreen); }

		//A hitbox rectangle representing the Player sprite
		public Rectangle HitBox { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y); } }
	}
}
