#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Assignment_3 {
	public class Game1 : Game {
		private readonly GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public static Random GameRand = new Random();
		public static Texture2D OnePxWhite;
		public static Size GameBounds;
		public static Vector2 ScreenCenter;
		public static Texture2D GameFont;
		public static List<HighScore> HighScoreList;
 
		private Stage gameStage;
		
		private GameState gameState = GameState.Menu;

		//Menu variables
		private float glowIntesity = 0.5f;
		private bool glowGrowing = false;
		private bool startSelected = true;

		public Game1()
			: base() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize() {
			graphics.PreferredBackBufferWidth = 1000;

			GameBounds = new Size(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
			ScreenCenter = new Vector2(GameBounds.Width / 2f, GameBounds.Height / 2f);

			
			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			
			OnePxWhite = new Texture2D(new GraphicsDevice(), 1, 1);
			OnePxWhite.SetData(new[] { Color.White });

			GameFont = Content.Load<Texture2D>("7px3bus");
			List<HighScore> tmpScores;
			if (HighScores.DeserializeScores("highscores", out tmpScores)) {
				HighScoreList = tmpScores;
			}
			else {
				HighScoreList = HighScores.CreateScores();
				HighScores.SerializeScores("highscores", HighScoreList);
			}

			Stage.LoadContent(Content);
		}
		protected override void UnloadContent() {}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		private KeyboardState? lastFrameState = null;
		protected override void Update(GameTime gameTime) {
			if (lastFrameState == null)
				lastFrameState = Keyboard.GetState();

			var currState = Keyboard.GetState();
			
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			switch (gameState) {
				case GameState.Menu:
					if (glowIntesity >= 0.9f || glowIntesity <= 0.1f) glowGrowing = !glowGrowing;
					glowIntesity = glowIntesity + (glowGrowing ? 0.025f : -0.025f);

					if (lastFrameState.Value.IsKeyUp(Keys.Enter) && currState.IsKeyDown(Keys.Enter)) {
						if (startSelected) {
							gameStage = new Stage(gameTime.TotalGameTime.TotalMilliseconds);
							gameState = GameState.Game;
						}
						else Exit();
					}

					if ((lastFrameState.Value.IsKeyUp(Keys.Up) && currState.IsKeyDown(Keys.Up)) 
					|| (lastFrameState.Value.IsKeyUp(Keys.Down) && currState.IsKeyDown(Keys.Down))) {
						startSelected = !startSelected;
					}
					break;
				case GameState.Game:
					gameStage.Update(Keyboard.GetState(), lastFrameState, gameTime);
					if (gameStage.NameEntryFinished) {
						gameState = GameState.HighScores;
					}
					break;
				case GameState.HighScores:
					break;
			}

			lastFrameState = Keyboard.GetState();
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.Black);

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

			switch (gameState) {
				case GameState.Menu:
					var glowyCol = Util.ColorInterpolate(Color.White, Color.Red, glowIntesity);

					Util.DrawFontMultiLine(spriteBatch, "game name", new Vector2(graphics.PreferredBackBufferWidth / 2f, 30), 
						glowyCol, graphics.PreferredBackBufferWidth, 80f, StringAlignment.Center);

					Util.DrawFontMultiLine(spriteBatch, "Start Game", new Vector2(graphics.PreferredBackBufferWidth / 2f, graphics.PreferredBackBufferHeight - 190),
						startSelected ? glowyCol : Color.White, graphics.PreferredBackBufferWidth, 32f, StringAlignment.Center);
					Util.DrawFontMultiLine(spriteBatch, "exit", new Vector2(graphics.PreferredBackBufferWidth / 2f, graphics.PreferredBackBufferHeight - 150),
						!startSelected ? glowyCol : Color.White, graphics.PreferredBackBufferWidth, 32f, StringAlignment.Center);


					Util.DrawFontMultiLine(spriteBatch, "z to shoot, x to jump", new Vector2(graphics.PreferredBackBufferWidth / 2f, graphics.PreferredBackBufferHeight - 30),
						Color.White, graphics.PreferredBackBufferWidth, 24f, StringAlignment.Center);

					break;
				case GameState.Game:
					gameStage.Draw(spriteBatch);
					break;
				case GameState.HighScores:
					break;
			}

			spriteBatch.End();

			base.Draw(gameTime);
		}
	}

	//Contains each of the game's possible states
	enum GameState {
		Menu,
		Game,
		HighScores
	}
}
