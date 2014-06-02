using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Assignment_3 {
	class HighScores {
		//Insert a score if it fits anywhere
		public static void InsertScore (string name, int points, List<HighScore> scores) {
			if (points > scores[0].Points) scores.Insert(0, new HighScore{Name = name, Points = points});
			else {
				for (var i = 0; i < scores.Count - 1; i++) {
					if (points < scores[i].Points && points > scores[i + 1].Points) {
						scores.Insert(i + 1, new HighScore{Name = name, Points = points});
					}
				}
			}

			//Remove excess scores
			if (scores.Count > 10) {
				scores.RemoveRange(scores.Count-1, scores.Count - 10);
			}
		}

		//Only used in Stage.cs to print the current initals for high score entry
		public static string HighScoreIntArrayToString(int[] input) {
			var output = "";
			foreach (var i in input) {
				if (i > -1 && i < 26) { //A-Z
					output += (char)(65 + i);
				}
				else if (i > 25 && i < 36) { //Followed by 0-9
					output += (char)(48 + (i - 26));
				}
				else switch (i) { //Special characters
					case 36:
						output += "!";
						break;
					case 37:
						output += ".";
						break;
					case 38:
						output += ",";
						break;
					case 39:
						output += "?";
						break;
				}
			}
			return output;
		}

		//Deserialize a high score file
		public static bool DeserializeScores(string fileName, out List<HighScore> highScores) {
			highScores = null;

			if (File.Exists(fileName)) {
				var scores = File.OpenRead(fileName);
				var bin = new BinaryFormatter();

				highScores = (List<HighScore>)bin.Deserialize(scores);
				return true;
			}

			return false;
		}

		//Creates a set of 10 randomized names with scores from 1000 to 10000 and return a new high score list
		public static List<HighScore> CreateScores() {
			var newScores = new List<HighScore>(10);

			for (var i = 0; i < 10; i++) {
				var h = new HighScore {Name = ThreeCharRndString(), Points = 5000 - i * 500};
				newScores.Add(h);
			}

			return newScores;
		}

		//Serialize a set of scores to a file
		public static void SerializeScores(string fileName, List<HighScore> scores) {
			using (var strm = File.OpenWrite(fileName)) {
				var bFormatter = new BinaryFormatter();

				bFormatter.Serialize(strm, scores);
			}
		}

		//Return a 3 character long randomized string of letters
		private static string ThreeCharRndString() {
			int[] output = {Game1.GameRand.Next(0, 39), Game1.GameRand.Next(0, 39), Game1.GameRand.Next(0, 39)};
			return HighScoreIntArrayToString(output);
		}
	}

	[Serializable]
	public struct HighScore {
		public string Name;
		public int Points;
	}
}
