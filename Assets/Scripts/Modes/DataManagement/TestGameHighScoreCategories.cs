using System.Collections.Generic;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.Data;
using Multimorphic.P3App.Modes.Data;

namespace PinballClub.TestGame.Modes.Data
{
	public class TestGameHighScoreCategories
	{
		public static List<HighScoreCategory> GetCategories()
		{
			List<HighScoreCategory> cats = new List<HighScoreCategory>();
			cats.Add (Score ());
			cats.Add (RightRamps ());
			cats.Add (LeftRamps ());
			return cats;
		}

		const int HIGH_SCORE_COUNT = 10;
		const float DEFAULT_HIGH_SCORE = 1000f;
		const float DEFAULT_HIGH_SCORE_DECREMENT = 100f;

		private static HighScoreCategory Score()
		{
			HighScoreCategory hsCat = new HighScoreCategory("Score", "Score", HIGH_SCORE_COUNT);

			List<float> values = new List<float> ();
			float startingValue = DEFAULT_HIGH_SCORE;
			for (int i=0; i<HIGH_SCORE_COUNT; i++)
			{
				values.Add(startingValue - i * DEFAULT_HIGH_SCORE_DECREMENT);
			}
			hsCat.SetDefaultValues(values);
			return (hsCat);
		}

		private static HighScoreCategory RightRamps()
		{
			HighScoreCategory hsCat = new HighScoreCategory("Evt_RightRampInc", "Right Ramps", 10);
			
			List<float> values = new List<float> ();
			float startingValue = 30f;
			for (int i=0; i<HIGH_SCORE_COUNT; i++)
			{
				values.Add(startingValue - i);
			}
			hsCat.SetDefaultValues(values);
			return (hsCat);
		}

		private static HighScoreCategory LeftRamps()
		{
			HighScoreCategory hsCat = new HighScoreCategory("Evt_LeftRampInc", "Left Ramps", 10);
			
			List<float> values = new List<float> ();
			float startingValue = 30f;
			for (int i=0; i<HIGH_SCORE_COUNT; i++)
			{
				values.Add(startingValue - i);
			}
			hsCat.SetDefaultValues(values);
			return (hsCat);
		}

	}

}

