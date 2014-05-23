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

		private Stage gameStage;

		private GameState gameState = GameState.Game;

		public Game1()
			: base() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}
		
		public void ResetObjects() {
			gameStage = new Stage();
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
			ResetObjects();
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

			//Stage.LoadContent();
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
			
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			switch (gameState) {
				case GameState.Game:
					gameStage.Update(Keyboard.GetState(), lastFrameState, gameTime);
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

			spriteBatch.Begin();

			switch (gameState) {
				case GameState.Game:
					gameStage.Draw(spriteBatch);
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
		GameOver
	}
}
