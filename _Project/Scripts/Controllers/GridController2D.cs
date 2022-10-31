using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using DungeonAutomata._Project.Scripts.Utilities;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonAutomata._Project.Scripts.Controllers
{
	public class GridController2D : MonoBehaviour
	{
		[SerializeField] private float moveCD = .2f;
		private Tilemap tilemap;
		private CellData[,] cells;

		//Using feedbacks from MoreMountains for movement effects
		[SerializeField] private MMFeedbacks feedbacks;
		private EventManager _eventManager;
		private MapManager _mapManager;

		private Tween _tween;
		private Vector3Int _currentPosition;
		private Vector3Int _currentTarget;
		
		private List<Vector3Int> _optimalPath;
		private Stack<Vector3Int> _optimalPathStack;
		private IUnit _unit;

		//Public variables and events
		public bool CanMove { get; set; }

		private void Awake()
		{
			_eventManager = EventManager.Instance;
			_mapManager = MapManager.Instance;
			CanMove = true;
			_unit = GetComponent<IUnit>();
		}

		public void InitializeGrid()
		{
			tilemap = _mapManager.GetTileMap();
			cells = _mapManager.GetGridMap();
		}

		public void SetPosition(Vector3Int position)
		{
			_currentPosition = position;
			transform.position = position;
		}

		public void MoveUnit(Vector3Int pos)
		{
			_currentPosition = _unit.CurrentTile;
			if (tilemap.HasTile(pos))
			{
				cells = _mapManager.GetGridMap();
				var previousTile = cells[_currentPosition.x, _currentPosition.y];
				var tile = cells[pos.x, pos.y];
				if (!CheckTile(tile)) 
					return;
				StartCoroutine(GridUtils.MoveToPosition(transform, pos, moveCD));
				tile.isEmpty = false;
				tile.Occupant = _unit;
				previousTile.isEmpty = true;
				previousTile.Occupant = null;
				_currentPosition = pos;
				_unit.CurrentTile = _currentPosition;
				
				if (_unit.GetType() == typeof(PlayerUnit))
				{
					//Debug.Log("Move player to tile: " + pos);
					_eventManager.InvokePlayerMove(pos);
					_eventManager.InvokePlayerAction();
					//_eventManager.InvokePlayerTurnEnd();
					//feedbacks.PlayFeedbacks();
				}
			}
		}

		private bool CheckTile(CellData tile)
		{
			//TODO: optimize and organize
			if (!tile.isEmpty)
			{
				//Enemy collision logic
				if (_unit.GetType() == typeof(EnemyUnit) && tile.Occupant != null)
				{
					if (tile.Occupant.GetType() == typeof(PlayerUnit))
					{
						var enemy = tile.Occupant;
						_eventManager.InvokeAttack(_unit, enemy);
						return false;
					}
				}
				//Player collision logic
				if (_unit.GetType() == typeof(PlayerUnit) && tile.Occupant != null)
				{
					if (tile.Occupant.GetType() == typeof(ItemUnit))
					{
						_eventManager.InvokePickup(tile.Occupant as IItem);
						//Hide sprite
						tile.Occupant.Die();
						tile.Occupant = null;
						tile.isEmpty = true;
						return true;
					}
					if (tile.Occupant.GetType() == typeof(EnemyUnit))
					{
						var enemy = tile.Occupant;
						_eventManager.InvokeAttack(_unit, enemy);
						_eventManager.InvokePlayerMove(_currentPosition);
						return false;
					}
				}
			}

			if (tile.isEmpty && tile.Occupant == null)
			{
				return true;
			}
			return false;
		}

		public void MoveTowards(Vector3Int targetPosition)
		{
			//Debug.Log("Moving towards: " + targetPosition);
			if (_currentTarget == targetPosition && _optimalPathStack.Count > 0)
			{
				var nextMove = _optimalPathStack.Pop() - _currentPosition;
				MoveUnit(nextMove);
			}
			else
			{
				StartCoroutine(FindOptimalPath(targetPosition));
			}
		}
		
		public void MoveAwayFrom(Vector3Int targetPosition)
		{
			Debug.Log("Moving away from: " + targetPosition);
			var nextMove = _currentPosition - targetPosition;
			MoveUnit(nextMove);
		}

		private float GetDistance(Vector3Int startPosition, Vector3Int targetPosition)
		{
			var distance = Mathf.Abs(startPosition.x - targetPosition.x) 
			               + Mathf.Abs(startPosition.y - targetPosition.y);
			return distance;
		}

        private IEnumerator FindOptimalPath(Vector3Int position)
        {
            var search_cells = new List<Vector3Int>();
            var processed_cells = new List<Vector3Int>();
            //Used to track connections between neighbor cells while pathfinding
            var previous_cells = new Dictionary<Vector3Int, Vector3Int>();
            
            var start = _currentPosition;
            var h_score = GetDistance(_currentPosition, position); // n to target
            var g_score = GetDistance(start, _currentPosition); // start to n
            var f_score = h_score + g_score;
            
            search_cells.Add(start);
            while (search_cells.Any())
            {
                var current = search_cells[0];
                foreach (var cell in search_cells)
                {
                    var new_h = GetDistance(cell, position);
                    var new_g = GetDistance(start, cell);
                    var new_f = new_h + new_g;
                    if ( new_f < f_score || (new_h < h_score && new_f.Equals(f_score)) )
                    {
                        current = cell;
                        h_score = new_h;
                        f_score = new_f;
                    }
                }
                
                processed_cells.Add(current);
                search_cells.Remove(current);
                
                if (current == position)
                {
                    var current_path_cell = position;
                    var path_stack = new Stack<Vector3Int>();
                    var count = 100;
                    while (current_path_cell != start)
                    {
                        path_stack.Push(current_path_cell);
                        count--;
                        if (count < 0 || !previous_cells.ContainsKey(current_path_cell))
                            break;
                        current_path_cell = previous_cells[current_path_cell];
                    }
                    _optimalPathStack = path_stack;
                    yield return path_stack;
                    MoveUnit(path_stack.Pop() - _currentPosition);
                }
                
                var neighbors = _mapManager.GetNeighborTiles(current);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor.isEmpty && !processed_cells.Contains(neighbor.gridPosition))
                    {
                        var tentative_g = g_score + GetDistance(current, neighbor.gridPosition);
                        var neighbor_g = GetDistance(neighbor.gridPosition, position);
                        var in_search = search_cells.Contains(neighbor.gridPosition);
                        if (!in_search || tentative_g < neighbor_g)
                        {
                            previous_cells[neighbor.gridPosition] = current;
                            g_score = tentative_g;
                            if (!in_search)
                            {
                                h_score = GetDistance(neighbor.gridPosition, position);
                                search_cells.Add(neighbor.gridPosition);
                            }
                        }
                    }
                }
            }

            //Will need to find a better way of handling unavailable paths
            //Debug.Log("returning null");
            var no_path = new Stack<Vector3Int>();
            _optimalPathStack = no_path;
            yield return no_path;
        }
	}
}