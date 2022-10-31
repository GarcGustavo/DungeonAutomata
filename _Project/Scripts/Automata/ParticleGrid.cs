using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Automata
{
	public class ParticleGrid : MonoBehaviour
	{
		public static ParticleGrid Instance { get; private set; }
		
		[SerializeField] Material _material;
		private RectTransform _rectTransform;
		private Texture2D _caTexture;
		private int[,] _grid;
		private int[,] _buffer;
		[SerializeField] private int _width = 10;
		[SerializeField] private int _height = 10;
		[SerializeField] private float _fillPercent = 0.5f;
		private float _cellSize = 1f;
		private bool _wrapEdges = true;

		private void Awake()
		{
			//Singleton initialization
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
			
			_caTexture = new Texture2D(_width, _height);
			_caTexture.filterMode = FilterMode.Point;
			Reset();
			UpdateTexture();
			_material.SetTexture("_MainTex", _caTexture);
		}

		private void Update()
		{
			if (Input.GetMouseButton(0))
			{
				//Vector3 mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
				//Raycast
				var pixelWidth = _width;
				var pixelHeight = _height;
				var unitsToPixels = 100.0;
				var pos = Camera.main.ViewportToWorldPoint(Input.mousePosition);
				pos = transform.InverseTransformPoint(pos);
				
				//var xPixel = Mathf.RoundToInt((float) (pos.x * unitsToPixels));
				//var yPixel = Mathf.RoundToInt((float) (pos.y * unitsToPixels));
				var xPixel = Mathf.RoundToInt((float) (pos.x));
				var yPixel = Mathf.RoundToInt((float) (pos.y));
     
				Debug.Log("("+xPixel+", "+yPixel+")");
				SetCell(xPixel,yPixel, true);
			}
			UpdateTexture();
			for (int x = _width-1; x > 0; x--)
			{
				for (int y = _height-1; y > 0; y--)
				{
					UpdateParticle(x, y);
				}
			}
		}

		void Reset()
		{
			_grid = new int[_width, _height];
			_buffer = new int[_width, _height];
			for (int x = 0; x < _width; ++x)
			{
				for (int y = 0; y < _height; ++y)
				{
					//_grid[x, y] = Random.value > _fillPercent ? 0 : 1;
					_grid[x, y] = 0;
				}
			}
		}

		void UpdateTexture()
		{
			var pixels = _caTexture.GetPixels();
			for (int i = 0; i < pixels.Length; ++i)
			{
				var value = _grid[i % _width, i / _height];
				pixels[i] = value * Color.blue;
			}

			_caTexture.SetPixels(pixels);
			_caTexture.Apply();
		}

		private void UpdateParticle(int x, int y)
		{
			//below
			if (IsEmpty(x, y - 1))
			{
				SwapCells(x, y, x, y-1);
			}
			//left diagonal
			else if (IsEmpty(x - 1, y - 1))
			{
				SwapCells(x, y, x-1, y-1);
			}
			//right diagonal
			else if (IsEmpty(x + 1, y - 1))
			{
				SwapCells(x, y, x+1, y-1);
			}
		}
		
		public void SetCell(int x, int y, bool state)
		{
			_grid[x, y] = state ? 1 : 0;
		}
		
		public void SwapCells(int x1, int y1, int x2, int y2)
		{
			// ReSharper disable once SwapViaDeconstruction
			int tmp = _grid[x1, y1];
			_grid[x1, y1] = _grid[x2, y2];
			_grid[x2, y2] = tmp;
		}
		
		public bool IsEmpty(int x, int y)
		{
			return _grid[x, y] == 0;
		}
	}
}