using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using DungeonAutomata._Project.Scripts.Utilities;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Tilemaps;
using static DungeonAutomata._Project.Scripts._Common.GridUtils;

namespace DungeonAutomata._Project.Scripts.Controllers
{
	public class GridController2D : MonoBehaviour
	{
		[SerializeField] private float moveCD = .2f;
		//private Tilemap tilemap;
		private CellData[,] cellMap;

		//Using feedbacks from MoreMountains for movement effects
		[SerializeField] private MMFeedbacks feedbacks;
		private TopDownManager _turnManager;
		private EventManager _eventManager;
		private MapManager _mapManager;
		private Grid _grid;

		private Tween _tween;
		private Vector3Int _currentPosition;
		private Vector3Int _currentTarget;
		
		private List<Vector3Int> _optimalPath;
		private Stack<Vector3Int> _optimalPathStack;
		private IUnit _unit;
		private SpriteRenderer _unitSpriteRenderer;
		private Transform _unitSpriteTransform;

		private void Awake()
		{
			_turnManager = TopDownManager.Instance;
			_eventManager = EventManager.Instance;
			_mapManager = MapManager.Instance;
			_unit = GetComponent<IUnit>();
			_unitSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
			_unitSpriteTransform = GetComponentInChildren<SpriteRenderer>().transform;
		}

		public void InitializeGrid()
		{
			//tilemap = _mapManager.GetTileMap();
			cellMap = _mapManager.GetCellMap();
			_grid = _mapManager.GetTileMap().layoutGrid;
		}

		public void SetPosition(Vector3Int position)
		{
			var previousCell = cellMap[_unit.CurrentPos.x, _unit.CurrentPos.y];
			if (previousCell != null)
			{
				previousCell.Occupant = null;
				previousCell.isWalkable = true;
			}
			
			var currentCell = cellMap[position.x, position.y];
			if (currentCell != null)
			{
				currentCell.Occupant = _unit;
				currentCell.isWalkable = false;
			}

			//var cellPos = _grid.WorldToCell( position);
			var cellPos = position;
			
			//if(isometric)
			//var cellPos = _grid.WorldToCell(GetIsometricPos(position));
			transform.position = cellPos;
			_currentPosition = cellPos;
			//transform.position = position;

			//var gridPos = _mapManager.IsIsometric() ? GetIsometricPos(position) : position;
			//transform.position =  position;
			//StartCoroutine(MoveToPosition(transform, gridPos, moveCD));
			
			_eventManager.InvokeCellUpdate(previousCell);
			_eventManager.InvokeCellUpdate(currentCell);
			//_mapManager.UpdateCellMap(cellMap);
		}

		public void MoveUnit(Vector3Int pos)
		{
			_currentPosition = _unit.CurrentPos;
			cellMap = _mapManager.GetCellMap();
			//if (tilemap.HasTile(pos))
			if (cellMap[pos.x, pos.y] != null)
			{
				var previousCell = cellMap[_currentPosition.x, _currentPosition.y];
				var cell = cellMap[pos.x, pos.y];
				if (!IsFreeCell(cell)) 
					return;
				cell.isWalkable = false;
				cell.Occupant = _unit;
				previousCell.isWalkable = true;
				previousCell.Occupant = null;
				if(pos.x != _currentPosition.x)
					_unitSpriteRenderer.flipX =  pos.x < _currentPosition.x;
				_currentPosition = pos;
				_unit.CurrentPos = pos;
				//var gridPos = _mapManager.IsIsometric() ? GetIsometricPos(pos) : pos;
				//_grid.CellToWorld(gridPos);
				StartCoroutine(MoveToPosition(transform, _grid.CellToWorld(pos), moveCD));
			}
		}

		//TODO: Refactor and move logic out of this method
		private bool IsFreeCell(CellData cell)
		{
			//TODO: optimize and organize
			if (cell.Occupant != null)
			{
				//Enemy collision logic
				if (_unit.GetType() == typeof(EnemyUnit) )
				{
					//Rework later into command pattern and AoE SO's
					if (cell.Occupant.GetType() == typeof(PlayerUnit)
					    || cell.Occupant.GetType() == typeof(EnemyUnit))
					{
						var positions = new List<Vector3Int>();
						positions.Add(cell.Occupant.CurrentPos);
						//Debug.Log("Attacking: " + cell.gridPosition);
						_unitSpriteRenderer.flipX =  cell.gridPosition.x < _currentPosition.x;
						StartCoroutine(GridUtils.PunchToPosition(_unitSpriteTransform, 
							_currentPosition, 
							cell.gridPosition, 
							moveCD));
						_eventManager.InvokeAttack(_unit, positions);
						return false;
					}
				}
				
				//Player collision logic
				if (_unit.GetType() == typeof(PlayerUnit))
				{
					Debug.Log("Picking up item: " + cell.gridPosition);
					if (cell.Occupant.GetType() == typeof(ItemUnit))
					{
						_eventManager.InvokePickup(cell.Occupant as IItem);
						//Hide sprite
						cell.Occupant.Die();
						cell.Occupant = null;
						cell.isWalkable = true;
						return true;
					}
					//Rework later into command pattern and AoE SO's
					if (cell.Occupant.GetType() == typeof(EnemyUnit))
					{
						var positions = new List<Vector3Int>();
						positions.Add(cell.Occupant.CurrentPos);
						Debug.Log("Attacking: " + cell.gridPosition);
						_unitSpriteRenderer.flipX =  cell.gridPosition.x < _currentPosition.x;
						StartCoroutine(GridUtils.PunchToPosition(_unitSpriteTransform, 
							_currentPosition, 
							cell.gridPosition, 
							moveCD));
						_eventManager.InvokeAttack(_unit, positions);
						//Replace with actual feedbacks/animations later);
						return false;
					}
					if (cell.cellType == CellTypes.Exit)
					{
						_eventManager.InvokePlayerExit();
						return false;
					}
				}
			}

			if (cell.isWalkable && cell.Occupant == null)
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

        private IEnumerator FindOptimalPath(Vector3Int position)
        {
            var search_cells = new List<Vector3Int>();
            var processed_cells = new List<Vector3Int>();
            //Used to track connections between neighbor cells while pathfinding
            var previous_cells = new Dictionary<Vector3Int, Vector3Int>();
            
            var start = _currentPosition;
            var h_score = GetCellDistance(_currentPosition, position); // n to target
            var g_score = GetCellDistance(start, _currentPosition); // start to n
            var f_score = h_score + g_score;
            
            search_cells.Add(start);
            while (search_cells.Any())
            {
                var current = search_cells[0];
                foreach (var cell in search_cells)
                {
                    var new_h = GetCellDistance(cell, position);
                    var new_g = GetCellDistance(start, cell);
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
                
                var neighbors = GridUtils.GetAdjacentCells(current, cellMap);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor.isWalkable && !processed_cells.Contains(neighbor.gridPosition))
                    {
                        var tentative_g = g_score + GetCellDistance(current, neighbor.gridPosition);
                        var neighbor_g = GetCellDistance(neighbor.gridPosition, position);
                        var in_search = search_cells.Contains(neighbor.gridPosition);
                        if (!in_search || tentative_g < neighbor_g)
                        {
                            previous_cells[neighbor.gridPosition] = current;
                            g_score = tentative_g;
                            if (!in_search)
                            {
                                h_score = GetCellDistance(neighbor.gridPosition, position);
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