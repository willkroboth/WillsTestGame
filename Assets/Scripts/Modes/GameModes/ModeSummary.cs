using System.Collections;

namespace PinballClub.TestGame.Modes {

	/// <summary>
	/// A descriptor for the various achievements from a mode or scene.  Holds the content for the ModeSummary display in the GUI layer.
	/// </summary>
	public class ModeSummary
	{
		public string Title;
		public string [] Items;
		public string [] Values;
		private bool completed;
		
		public bool useCompleted { get; private set; }
		public bool Completed { get { return(completed); } set { completed = value; useCompleted = true;} }
		
		public ModeSummary ()
		{
			Items = new string[] {"a", "b", "c"};
			Values = new string[] {"1", "2", "3"};
			completed = false;
			useCompleted = false;
		}
		
		public void SetItemAndValue( int index, string item, string value )
		{
			if (index < 3)
			{
				Items[index] = item;
				Values[index] = value;
			}
		}		
	}
}
