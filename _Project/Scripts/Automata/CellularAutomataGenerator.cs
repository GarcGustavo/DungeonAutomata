using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Automata
{
	public class CellularAutomataGenerator : MonoBehaviour
	{
		[SerializeField] Material _material;
		Texture2D _caTexture;
		int[,] _cellularAutomata;

		[SerializeField] int _width;
		[SerializeField] int _height;
		[SerializeField] float _fillPercent = 0.5f;
		[SerializeField] private int _liveNeighboursRequired = 3;

		void OnEnable()
		{
			ResetAutomata();
			_caTexture = new Texture2D(_width, _height);
			_caTexture.filterMode = FilterMode.Point;
			UpdateTexture();
			_material.SetTexture("_MainTex", _caTexture);
		}
	
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				ResetAutomata();
				UpdateTexture();
			}
			if (Input.GetKeyDown(KeyCode.Space))
			{
				RainFall(_cellularAutomata);
				UpdateTexture();
			}
		}

		void ResetAutomata()
		{
			_cellularAutomata = new int[_width, _height];
			for (int x = 0; x < _width; ++x)
			{
				for (int y = 0; y < _height; ++y)
				{
					_cellularAutomata[x, y] = Random.value > _fillPercent ? 0 : 1;
				}
			}
		}

		void UpdateTexture()
		{
			var pixels = _caTexture.GetPixels();
			for (int i = 0; i < pixels.Length; ++i)
			{
				var value = _cellularAutomata[i % _width, i / _height];
				pixels[i] = value * Color.blue;
			}

			_caTexture.SetPixels(pixels);
			_caTexture.Apply();
		}
	
		int GetBelow(int x, int y)
		{
			int neighbourValue = 0;

			if (y > 0)
			{
				neighbourValue = _cellularAutomata[x, y - 1];
			}

			return neighbourValue;
		}
	
		int GetAbove(int x, int y)
		{
			int neighbourValue = 0;

			if (y < _height - 1)
			{
				neighbourValue = _cellularAutomata[x, y + 1];
			}

			return neighbourValue;
		}
	
		int GetRight(int x, int y)
		{
			int neighbourValue = 0;

			if (x < _width - 1)
			{
				neighbourValue = _cellularAutomata[x + 1, y];
			}

			return neighbourValue;
		}
	
		int GetLeft(int x, int y)
		{
			int neighbourValue = 0;

			if (x > 0)
			{
				neighbourValue = _cellularAutomata[x - 1, y];
			}

			return neighbourValue;
		}
	
		int GetNeighbourCellCount(int x, int y)
		{
			int neighbourCellCount = 0;
			if (x > 0)
			{
				neighbourCellCount += _cellularAutomata[x - 1, y];
				if (y > 0)
				{
					neighbourCellCount += _cellularAutomata[x - 1, y - 1];
				}
			}

			if (y > 0)
			{
				neighbourCellCount += _cellularAutomata[x, y - 1];
				if (x < _width - 1)
				{
					neighbourCellCount += _cellularAutomata[x + 1, y - 1];
				}
			}

			if (x < _width - 1)
			{
				neighbourCellCount += _cellularAutomata[x + 1, y];
				if (y < _height - 1)
				{
					neighbourCellCount += _cellularAutomata[x + 1, y + 1];
				}
			}

			if (y < _height - 1)
			{
				neighbourCellCount += _cellularAutomata[x, y + 1];
				if (x > 0)
				{
					neighbourCellCount += _cellularAutomata[x - 1, y + 1];
				}
			}

			return neighbourCellCount;
		}
	
		void Step(int[,] cellularAutomata)
		{
			int[,] caBuffer = new int[_width, _height];

			for (int x = 0; x < _width; ++x)
			{
				for (int y = 0; y < _height; ++y)
				{
					int liveCellCount = cellularAutomata[x, y] + GetNeighbourCellCount(x, y);
					caBuffer[x, y] = liveCellCount > _liveNeighboursRequired ? 1 : 0;
				}
			}

			for (int x = 0; x < _width; ++x)
			{
				for (int y = 0; y < _height; ++y)
				{
					cellularAutomata[x, y] = caBuffer[x, y];
				}
			}
		}
	
		void RainFall(int[,] cellularAutomata)
		{
			int[,] caBuffer = new int[_width, _height];

			for (int x = 0; x < _width; ++x)
			{
				for (int y = 0; y < _height; ++y)
				{
					if (y <= 0)
					{
						caBuffer[x, y] = cellularAutomata[x, y];
					}
					else
					{
						var pixelBelow = GetBelow(x, y);
						var rightNeighbor = GetRight(x, y-1);
						var leftNeighbor = GetLeft(x, y-1);
						var rightBias = Random.value > 0.5f ? 1 : 0;
					
						if (pixelBelow == 0)
						{
							caBuffer[x, y] = 0;
							caBuffer[x, y-1] = cellularAutomata[x, y];
						}
						else if (rightNeighbor == 0 && leftNeighbor == 0 && x > 0 && x < _width - 1)
						{
							caBuffer[x, y] = 0;
							if (rightBias == 1)
							{
								caBuffer[x+1, y-1] = cellularAutomata[x, y];
							}
							else
							{
								caBuffer[x-1, y-1] = cellularAutomata[x, y];
							}
						}
						else if (rightNeighbor == 0 && x < _width-1)
						{
							caBuffer[x, y] = 0;
							caBuffer[x+1, y-1] = cellularAutomata[x, y];
						}
						else if (leftNeighbor == 0 && x > 0)
						{
							caBuffer[x, y] = 0;
							caBuffer[x-1, y-1] = cellularAutomata[x, y];
						}
						else
						{
							caBuffer[x, y] = cellularAutomata[x, y];
						}
					}
				}
			}

			for (int x = 0; x < _width; ++x)
			{
				for (int y = 0; y < _height; ++y)
				{
					cellularAutomata[x, y] = caBuffer[x, y];
				}
			}
		}
	}
}
