﻿#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
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
		private int menuSelected = 0;

		private SoundEffect menuBoop;
		private SoundEffect menuSelect;

		private ExplosionFactory eFactory;

		private readonly LavaParticles lParticles = new LavaParticles();

		private Background bGround = new Background(2.0f);

		private bool deathFloorGrowing = false;
		private float deathFloorIntensity = 0.5f;

		private double lastLavaSparkTime = 0;
		private double lavaSparkdelay = 0;

		public Game1()
			: base() {
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			Window.Title = "RUN";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize() {
			//Set the game width to be a bit wider
			graphics.PreferredBackBufferWidth = 1000;

			//Simply setting a couple of static variables to make offscreen checking and such in other files easier
			GameBounds = new Size(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
			ScreenCenter = new Vector2(GameBounds.Width / 2f, GameBounds.Height / 2f);

			//Load an explosion factory for the high scores screen and a background for the main menu
			eFactory = new ExplosionFactory();
			bGround = new Background(3.0f);
			
			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			
			//Creates a new 1x1 white texture
			OnePxWhite = new Texture2D(new GraphicsDevice(), 1, 1);
			OnePxWhite.SetData(new[] { Color.White });

			//Load the game's font into memory
			GameFont = Content.Load<Texture2D>("7px3bus");
			
			//Attempt to load the list of high scores
			List<HighScore> tmpScores;
			//Attempt to deserialize from the file called "highscores"
			if (HighScores.DeserializeScores("highscores", out tmpScores)) {
				//If it's successful, set HighScoreList to tmpScores
				HighScoreList = tmpScores;
			}
			else {
				//If it fails, create a new randomized high score list
				HighScoreList = HighScores.CreateScores();
				//Then serialize them to a file to use later
				HighScores.SerializeScores("highscores", HighScoreList);
			}

			//Load menu audio
			menuBoop = Content.Load<SoundEffect>("menublip");
			menuSelect = Content.Load<SoundEffect>("menuselect");

			//Load stage content
			Stage.LoadContent(Content);
		}
		protected override void UnloadContent() {}

		//The previous frame's keyboard state
		private KeyboardState? lastFrameState = null;

		protected override void Update(GameTime gameTime) {
			//If the last frame's keystate is null, it needs to be set up
			if (lastFrameState == null)
				lastFrameState = Keyboard.GetState();

			//Set currState to the current state to avoid numerous calls in here
			var currState = Keyboard.GetState();
			
			//If esc is pressed, go back to the menu
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				gameState = GameState.Menu;

			//Main gamestate switch
			switch (gameState) {
				case GameState.Menu:
					//Update the background
					bGround.Update(1f);

					//Create new lava sparks
					if (lastLavaSparkTime + lavaSparkdelay < gameTime.TotalGameTime.TotalMilliseconds) {
						lastLavaSparkTime = gameTime.TotalGameTime.TotalMilliseconds;
						lavaSparkdelay = GameRand.Next(100, 1500);

						lParticles.Spark(new Vector2((float)(GameBounds.Width * GameRand.NextDouble()), GameBounds.Height - 4), (float)(16f * GameRand.NextDouble()), (float)(10f * (0.5 - GameRand.NextDouble())), 5f);
					}

					//Update main menu lava sparks
					lParticles.Update(1f);

					//Update death floor color
					if (deathFloorIntensity >= 0.9f || deathFloorIntensity <= 0.1f) deathFloorGrowing = !deathFloorGrowing;
					deathFloorIntensity = deathFloorIntensity + (deathFloorGrowing ? 0.005f : -0.005f);

					//X key checking for menu button presses
					if (lastFrameState.Value.IsKeyUp(Keys.X) && currState.IsKeyDown(Keys.X)) {
						menuSelect.Play();
						switch (menuSelected) {
							case 0:
								gameStage = new Stage(gameTime.TotalGameTime.TotalMilliseconds);
								gameState = GameState.Game;
								break;
							case 1:
								gameState = GameState.HighScores;
								break;
							default:
								Exit();
								break;
						}
					}

					//Up/down button checking
					if (lastFrameState.Value.IsKeyUp(Keys.Up) && currState.IsKeyDown(Keys.Up)) {
						menuBoop.Play();
						if (menuSelected > 0)
							menuSelected--;
						else
							menuSelected = 2;
					}
					if (lastFrameState.Value.IsKeyUp(Keys.Down) && currState.IsKeyDown(Keys.Down)) {
						menuBoop.Play();
						if (menuSelected < 2)
							menuSelected++;
						else
							menuSelected = 0;
					}
					break;
				case GameState.Game:
					//Main game update
					gameStage.Update(Keyboard.GetState(), lastFrameState, gameTime);

					//Level's over, go to high scores
					if (gameStage.NameEntryFinished) {
						gameState = GameState.HighScores;
					}
					break;
				case GameState.HighScores:
					//X was pressed, go back to menu
					if (lastFrameState.Value.IsKeyUp(Keys.X) && currState.IsKeyDown(Keys.X)) {
						menuSelect.Play();
						gameState = GameState.Menu;
					}

					//Cause a bunch of explosions every 2 seconds or so
					if (gameTime.TotalGameTime.TotalMilliseconds % 2000 < 200)
						eFactory.Explode(new Vector2(GameRand.Next(0, GameBounds.Width), GameRand.Next(0, GameBounds.Height)), Color.LightGreen, gameTime, 400, 2000, 50, 150);

					//Update background explosions
					eFactory.Update(gameTime, 0f);
					break;
			}

			//Update previous frame state
			lastFrameState = Keyboard.GetState();
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.Black);

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

			switch (gameState) {
				case GameState.Menu:
					//Draw background
					bGround.Draw(spriteBatch);

					//Draw background wall
					Util.DrawCube(spriteBatch,
						new Rectangle(-50, GameBounds.Height - 80, GameBounds.Width + 50,
										  40),
										  40, 0.2f, -0.3f,
										  Color.FromNonPremultiplied(30, 30, 30, 255),
										  Color.FromNonPremultiplied(130, 130, 130, 255),
										  Color.FromNonPremultiplied(80, 80, 80, 255));

					//Draw glow on wall
					spriteBatch.Draw(Stage.LevelGlow, new Rectangle(0, GameBounds.Height - 80, GameBounds.Width,
										  20), Color.FromNonPremultiplied(255, 0, 0, (int)(230 * deathFloorIntensity)));

					//Draw lava
					Util.DrawLine(spriteBatch, new Vector2(0, GameBounds.Height - 30),
						new Vector2(GameBounds.Width, GameBounds.Height - 30), 66f,
						Util.ColorInterpolate(Color.FromNonPremultiplied(30, 30, 30, 255), Color.Red, deathFloorIntensity));

					//Draw each lava spark's glow
					foreach (var s in lParticles.Particles) {
						spriteBatch.Draw(Stage.CircleGlow,
								new Rectangle((int)s.Position.X - 10, (int)s.Position.Y - 10, 24, 24), s.Color);
					}

					//Draw lava sparks
					lParticles.Draw(spriteBatch);

					//Draw front lava
					Util.DrawLine(spriteBatch, new Vector2(0, GameBounds.Height),
						new Vector2(GameBounds.Width, GameBounds.Height), 24f,
						Util.ColorInterpolate(Color.FromNonPremultiplied(30, 30, 30, 255), Color.Red, deathFloorIntensity));

					//Draw transparent black square on entire screen
					spriteBatch.Draw(OnePxWhite, new Rectangle(0, 0, GameBounds.Width, GameBounds.Height), Color.FromNonPremultiplied(0, 0, 0, 90));

					//var glowyCol = Util.ColorInterpolate(Color.White, Color.Red, deathFloorIntensity);
					var glowyCol = Color.Red;

					//Draw all menu text
					Util.DrawFontMultiLine(spriteBatch, "RUN", new Vector2(GameBounds.Width / 2f, 30),
						glowyCol, GameBounds.Width, 80f, StringAlignment.Center);

					Util.DrawFontMultiLine(spriteBatch, "Start Game", new Vector2(GameBounds.Width / 2f, GameBounds.Height - 190),
						menuSelected == 0 ? glowyCol : Color.White, GameBounds.Width, 32f, StringAlignment.Center);

					Util.DrawFontMultiLine(spriteBatch, "high scores", new Vector2(GameBounds.Width / 2f, GameBounds.Height - 150),
						menuSelected == 1 ? glowyCol : Color.White, GameBounds.Width, 32f, StringAlignment.Center);

					Util.DrawFontMultiLine(spriteBatch, "exit", new Vector2(GameBounds.Width / 2f, GameBounds.Height - 110),
						menuSelected == 2 ? glowyCol : Color.White, GameBounds.Width, 32f, StringAlignment.Center);


					Util.DrawFontMultiLine(spriteBatch, "z to shoot, x to jump", new Vector2(GameBounds.Width / 2f, GameBounds.Height - 30),
						Color.White, GameBounds.Width, 24f, StringAlignment.Center);

					break;
				case GameState.Game:
					//Draw the game itself
					gameStage.Draw(spriteBatch);
					break;
				case GameState.HighScores:
					//Draw background 'fireworks'
					eFactory.Draw(spriteBatch);

					//Draw transparent black square on entire screen
					spriteBatch.Draw(OnePxWhite, new Rectangle(0, 0, GameBounds.Width, GameBounds.Height), Color.FromNonPremultiplied(0, 0, 0, 90));

					//Draw high score text
					Util.DrawFontMultiLine(spriteBatch, "High Scores", new Vector2(GameBounds.Width / 2f, 10),
						Color.Red, GameBounds.Width, 60f, StringAlignment.Center);

					//Draw high scores
					for (var i = 0; i < HighScoreList.Count; i++) {
						var hs = HighScoreList[i];
						
						Util.DrawFontMultiLine(spriteBatch, string.Format("#{0}. {1}  {2}", (i < 9 ? " " : "") + (i+1), hs.Name, hs.Points.ToString().PadLeft(7, ' ')), new Vector2(GameBounds.Width / 2f - 32, 70 + i * 34),
							Color.White, GameBounds.Width, 32f, StringAlignment.Center);
					}

					//Draw Press X text
					Util.DrawFontMultiLine(spriteBatch, "Press X", new Vector2(GameBounds.Width / 2f, GameBounds.Height - 2),
						Color.White, GameBounds.Width, 24f, StringAlignment.Center, StringAlignmentVert.Above);
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
