using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts.GridComponents;
using DungeonAutomata._Project.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using static DungeonAutomata._Project.Scripts._Common.CommonTypes;
using static DungeonAutomata._Project.Scripts._Common.CommonUtils;

namespace DungeonAutomata._Project.Scripts._Managers
{
	public class TopDownManager : MonoBehaviour
	{
		public static TopDownManager Instance { get; private set; }
		
		//[SerializeField] private GameObject _enemyPrefab;
		//[SerializeField] private Item[] _itemPrefabs;
		//[SerializeField] private List<Vector3Int> _enemySpawnPoints;
		//[SerializeField] private List<Vector3Int> _itemSpawnPoints;
		[SerializeField] private CinemachineVirtualCamera _camera;
		[SerializeField] private bool _populateOnLoad;
		[SerializeField] private PlayerUnit _player;
		private EventManager _eventManager;
		private MapManager _mapManager;
		private UIManager _uiManager;
		private List<ItemUnit> _inventory;
		private GameState _previousState;
		private GameState _state;
		private int _turnCount = 0;
		private List<ICommand> _actionList;

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
			//_eventManager.OnPlayerMove += UpdatePlayerPosition;
			_eventManager.OnMenu += ToggleMenu;
			_eventManager.OnPlayerExit += NextStage;
			_eventManager.OnPlayerDeath += PlayerDeath;
			_eventManager.OnTurnEnd += EndTurn;
			_inventory = new List<ItemUnit>();
			_actionList = new List<ICommand>();
			_state = GameState.PlayerTurn;
			_previousState = _state;
			_turnCount = 0;
		}

		[SerializeField] private MapGenerator _generator;
		[Button]
		private void GenerateNewMap()
		{
			_generator.GenerateSpriteMap();
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
			if (_populateOnLoad)
			{
				_mapManager.PopulateGridMap();
				_player = GetPlayer();
			}
			_eventManager.InvokeUpdateHUD();
			//UpdateGameState(_state);
		}
		private void Update()
		{
			//Highlight tile under mouse
			var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			_mapManager.HighLightCell(Vector3Int.FloorToInt(mouseWorldPos));
		}

		public void ResetCamera(Transform target)
		{
			_camera.Follow = target;
		}
		//-------------------------------------------------------------------
		// State functions
		//-------------------------------------------------------------------

		private void ToggleMenu()
		{
			if (_state == GameState.Menu)
			{
				UpdateGameState(_previousState);
				//StartCoroutine(UpdateGameState(_previousState));
			}
			else
			{
				_player.SetMoveState(false);
				UpdateGameState(GameState.Menu);
				//StartCoroutine(UpdateGameState(GameState.Menu));
			}
		}

		private void UpdateGameState(GameState newState)
		{
			_previousState = _state;
			Debug.Log("Prev State: " + _previousState);
			Debug.Log("Next State: " + newState);
			switch (newState)
			{
				case GameState.PlayerTurn:
					//Execute commands registered by enemies in last turn
					//ExecuteCommands();
					_eventManager.InvokePlayerTurnStart();
					_eventManager.InvokeUpdateHUD();
					//yield return GetWaitForSeconds(.05f);
					_player.SetMoveState(true);
					break;
				case GameState.EnemyTurn:
					//Execute commands registered by player in last turn
					_eventManager.InvokeEnemyTurnStart();
					_eventManager.InvokeUpdateHUD();
					//yield return GetWaitForSeconds(.05f);
					break;
				case GameState.Lose:
					_eventManager.InvokePlayerDeath();
					_player.SetMoveState(false);
					break;
				case GameState.Menu:
					_player.SetMoveState(false);
					break;
			}
			_state = newState;
		}

		private void PlayerDeath()
		{
			NextStage();
			UpdateGameState(GameState.PlayerTurn);
			//_state = GameState.Lose;
		}

		private void NextStage()
		{
			//UpdateGameState(GameState.Win);
			//StartCoroutine(UpdateGameState(GameState.Win));
			//UpdateGameState(GameState.Win);
			Debug.Log("Next Stage");
			_mapManager.ResetMap();
			_mapManager.InitializeMap();
			UpdateGameState(GameState.PlayerTurn);
		}

		//-------------------------------------------------------------------
		// Update functions
		//-------------------------------------------------------------------

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

		private bool _executing = false;

		private void EndTurn()
		{
			Debug.Log("Ending Turn");
			if (!_executing)
			{
				Debug.Log("Ended Turn");
				_executing = true;
				StartCoroutine(ExecuteCommands());
			}
		}

		public IEnumerator ExecuteCommands()
		{
			_turnCount++;
			var commands = _actionList.ToArray();
			//TODO: Add initiative system and lock registration of commands after turn end
			Debug.Log("Executing " + _actionList.Count + " commands");
			Debug.Log("Turn: " + _turnCount);
			foreach (var command in commands)
			{
				//StartCoroutine(command.Execute());
				command.Execute();
			}
			yield return GetWaitForSeconds(.1f);
			_actionList.Clear();
			if (_state == GameState.PlayerTurn)
			{
				//StartCoroutine(UpdateGameState(GameState.EnemyTurn));
				_executing = false;
				if (_player.CurrentEnergy > 0)
				{
					UpdateGameState(GameState.PlayerTurn);
				}
				else
				{
					UpdateGameState(GameState.EnemyTurn);
				}
			}
			else if (_state == GameState.EnemyTurn)
			{
				//StartCoroutine(UpdateGameState(GameState.PlayerTurn));
				_executing = false;
				_player.CurrentEnergy = _player.MaxEnergy;
				UpdateGameState(GameState.PlayerTurn);
			}
			//_eventManager.InvokeTurnEnd();
		}

		public void RegisterCommand(ICommand command)
		{
			// Play around with order execution via unit stats later
			_actionList.Add(command);
		}

		public GameState GetState()
		{
			return _state;
		}
	}
}