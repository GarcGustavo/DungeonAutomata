using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Utilities.BSPGen
{
	public class BspLeaf
	{
		private int _x;
		private int _y;
		private int _width;
		private int _height;
		
		private BspLeaf _leftChild;
		private BspLeaf _rightChild;
		private BspRoom _room;
		private Vector3Int _hallways;
		
		public BspLeaf(int x, int y, int width, int height)
		{
			_x = x;
			_y = y;
			_width = width;
			_height = height;
		}

		public bool Split()
		{
			if (_leftChild != null || _rightChild != null)
			{
				return false;
			}

			bool splitH = Random.Range(0, 2) == 1;

			if (_width > _height && _width / _height >= 1.25f)
			{
				splitH = false;
			}
			else if (_height > _width && _height / _width >= 1.25f)
			{
				splitH = true;
			}

			int max = (splitH ? _height : _width) - 24;
			if (max <= 24)
			{
				return false;
			}

			int split = Random.Range(24, max);

			if (splitH)
			{
				_leftChild = new BspLeaf(_x, _y, _width, split);
				_rightChild = new BspLeaf(_x, _y + split, _width, _height - split);
			}
			else
			{
				_leftChild = new BspLeaf(_x, _y, split, _height);
				_rightChild = new BspLeaf(_x + split, _y, _width - split, _height);
			}

			return true;
		}
	}
}