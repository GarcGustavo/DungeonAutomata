using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts.GridComponents;
using DungeonAutomata._Project.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using static DungeonAutomata._Project.Scripts._Common.CommonUtils;

namespace DungeonAutomata._Project.Scripts._Managers
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance { get; private set; }
		
		//[SerializeField] private GameObject _enemyPrefab;
		//[SerializeField] private Item[] _itemPrefabs;
		//[SerializeField] private List<Vector3Int> _enemySpawnPoints;
		//[SerializeField] private List<Vector3Int> _itemSpawnPoints;
		[SerializeField] private CinemachineVirtualCamera _camera;
		private EventManager _eventManager;
		private MapManager _mapManager;
		private UIManager _uiManager;
		private List<ItemUnit> _inventory;
		private PlayerUnit _player;
		private GameState _previousState;
		private GameState _state;
		private int _turnCount = 0;

		private enum GameState
		{
			PlayerTurn,
			EnemyTurn,
			Lose,
			Win,
			Menu
		}

		// Unity event functions
		//-------------------------------------------------------------------
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

			//Get the grid and tilemaps
			_eventManager = EventManager.Instance;
			_uiManager = UIManager.Instance;
			_mapManager = MapManager.Instance;
			_eventManager.OnStartGame += SpawnPlayer;
			_eventManager.OnPlayerMove += UpdatePlayerPosition;
			_eventManager.OnPlayerTurnEnd += PlayerTurnEnd;
			_eventManager.OnMenu += ToggleMenu;
			_eventManager.OnPlayerGoalReached += NextStage;
			_eventManager.OnPlayerDeath += PlayerDeath;
			_eventManager.OnEnemyTurnEnd += EnemyTurnEnd;
			_inventory = new List<ItemUnit>();
			_state = GameState.PlayerTurn;
			_previousState = _state;
		}

		[SerializeField] private TileMapGenerator _generator;
		[Button]
		private void GenerateNewMap()
		{
			_generator.GenerateMap();
		}
		
		[Button]
		private void ClearCurrentMap()
		{
			_generator.ClearMap();
		}

		private void Start()
		{
			Debug.Log("Game Started");
			_eventManager.InvokeStartGame();
			_mapManager.InitializeMap();
			_player = GetPlayer();
			//UpdateGameState(_state);
		}
		private void Update()
		{
			if (_player != null && _state == GameState.PlayerTurn)
			{
				_player.GetPlayerInput();
			}
			
			//Highlight tile under mouse
			var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			_mapManager.HighLightCell(mouseWorldPos);
		}

		public void ResetCamera(Transform target)
		{
			_camera.Follow = target;
		}
		//-------------------------------------------------------------------
		// State functions
		//-------------------------------------------------------------------

		private void PlayerTurnEnd()
		{
			if (_player.CurrentEnergy <= 0)
			{
				_player.SetMoveState(false);
				StartCoroutine(UpdateGameState(GameState.EnemyTurn));
			}
		}

		private void EnemyTurnEnd()
		{
			StartCoroutine(UpdateGameState(GameState.PlayerTurn));
		}

		private void ToggleMenu()
		{
			if (_state == GameState.Menu)
			{
				StartCoroutine(UpdateGameState(_previousState));
			}
			else
			{
				_player.SetMoveState(false);
				StartCoroutine(UpdateGameState(GameState.Menu));
			}
		}

		private IEnumerator UpdateGameState(GameState newState)
		{
			_previousState = _state;
			_state = newState;
			Debug.Log("State: " + _state);
			switch (_state)
			{
				case GameState.PlayerTurn:
					yield return GetWaitForSeconds(.2f);
					_eventManager.InvokePlayerTurnStart();
					_player.SetMoveState(true);
					break;
				case GameState.EnemyTurn:
					yield return GetWaitForSeconds(.2f);
					_player.SetMoveState(false);
					_eventManager.InvokeEnemyTurnStart();
					break;
				case GameState.Lose:
					_eventManager.InvokePlayerDeath();
					_player.SetMoveState(false);
					break;
				case GameState.Menu:
					_player.SetMoveState(false);
					break;
			}
		}

		private void PlayerDeath()
		{
			_state = GameState.Lose;
		}

		private void NextStage()
		{
			UpdateGameState(GameState.Win);
			_mapManager.ResetMap();
			_mapManager.InitializeMap();
		}

		//-------------------------------------------------------------------
		// Update functions
		//-------------------------------------------------------------------

		private void UpdatePlayerPosition(Vector3Int player_pos)
		{
			//PlayerTurnEnd();
		}

		private void SpawnPlayer()
		{
			_eventManager.InvokePlayerSpawn();
		}
		
		public void SetPlayer(PlayerUnit player)
		{
			_player = player;
		}

		public PlayerUnit GetPlayer()
		{
			return _mapManager.GetPlayer();
		}
	}
}