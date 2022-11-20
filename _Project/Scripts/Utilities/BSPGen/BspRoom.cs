namespace DungeonAutomata._Project.Scripts.Utilities.BSPGen
{
	public class BspRoom
	{
		protected int left, right, top, bottom;

		protected int GetWidth() {
			return right - left + 1;
		}

		protected int GetHeight() {
			return top - bottom + 1;
		}

		public BspRoom(int left, int right, int top, int bottom) {
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}

		public int GetLeft() { return left; }
		public int GetRight() { return right; }
		public int GetTop() { return top; }
		public int GetBottom() { return bottom; }
	}
}