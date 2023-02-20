using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DungeonAutomata._Project.Scripts._Common;
using DungeonAutomata._Project.Scripts._Interfaces;
using DungeonAutomata._Project.Scripts._Managers;
using DungeonAutomata._Project.Scripts.BulletHellPrototype;
using DungeonAutomata._Project.Scripts.Data;
using DungeonAutomata._Project.Scripts.GridComponents;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.Controllers
{
    public class FPSGridController : MonoBehaviour
    {
        private Camera _cam;
        private Player _player;
        [SerializeField] private Grid grid;
        [SerializeField] private bool canMove = true;        
        [SerializeField] private float moveCD = .2f;

        
        private bool _isMoving;
        private Tween _tween;
        private Vector3Int _currentPosition;
        private Vector3Int _currentTarget;
        private const float _speed = 4f;
        private const float _turnSpeed = 4f;
        private const float _headBob = 0.1f;
        private Direction _currentDirection = 0;
        private readonly float _turnCD = 1;
        private readonly Vector3 _centerOffset = new Vector3(.5f, 1f, .5f);
        
        private enum Direction
        {
            North = 0,
            East = 1,
            South = 2,
            West = 3
        }
        
        private void Awake()
        {
            //_manager = TopDownManager.Instance;
            //_eventManager = EventManager.Instance;
            //_mapManager = MapManager.Instance;
            //_unit = GetComponent<IUnit>();
        }

        private void Start()
        {
            _cam = Camera.main;
            _currentPosition = grid.WorldToCell(transform.position);
            _currentTarget = _currentPosition;
            _currentDirection = Direction.North;
        }

        private void Update()
        {
            GetMovementInput();
        }

        public void SetPosition(Vector3Int position)
        {
	        _currentPosition = position;
	        transform.position = grid.CellToWorld(_currentPosition);
	        //TODO: replace w/ fps events
	        //_eventManager.InvokeCellUpdate(previousCell);
	        //_eventManager.InvokeCellUpdate(currentCell);
        }

        public void GetMovementInput()
        {
            if (_isMoving) return;
            if (Input.GetButton("Up"))
            {
                _isMoving = true;
                StartCoroutine(MovePlayerToCell());
            }
            else if (Input.GetButtonDown("Down"))
            {
                var direction = ((int)_currentDirection + 2) % 4;
                //_currentDirection = (TopDownManager.Direction) direction;
                //_player._currentDirection = _currentDirection;
                //Debug.Log("Direction: " + _currentDirection);
                
                _isMoving = true;
                StartCoroutine(RotatePlayer(Vector3.down * 180));
            }
            else if (Input.GetButtonDown("Left"))
            {
                var direction = ((int) _currentDirection + 3) % 4;

                //_currentDirection = (TopDownManager.Direction) direction;
                //_player._currentDirection = _currentDirection;
                //Debug.Log("Direction: " + _currentDirection);
                _isMoving = true;
                StartCoroutine(RotatePlayer(Vector3.down * 90));
            }
            else if (Input.GetButtonDown("Right"))
            {
                var direction = ((int)_currentDirection + 1)%4;
                
                //_currentDirection = (TopDownManager.Direction) direction;
                //_player._currentDirection = _currentDirection;
                //Debug.Log("Direction: " + _currentDirection);
                _isMoving = true;
                StartCoroutine(RotatePlayer(Vector3.up * 90));
            }
            else if (Input.GetButton("Fire1"))
            {
            }
            else if (Input.GetButtonDown("Fire2"))
            {
                
            }
        }
        private IEnumerator RotatePlayer(Vector3 rotation)
        {
            if (!_isMoving) yield break;
            
            transform.DORotate(transform.eulerAngles + rotation, 1 / _turnSpeed, RotateMode.Fast);
            yield return new WaitForSeconds(_turnCD);
            _isMoving = false;
        }
    
        private IEnumerator MovePlayerToCell()
        {
            if (!_isMoving) yield break;
            
            _currentTarget = _currentDirection switch
            {
                Direction.North => _currentPosition + Vector3Int.up,
                Direction.East => _currentPosition + Vector3Int.right,
                Direction.South => _currentPosition + Vector3Int.down,
                Direction.West => _currentPosition + Vector3Int.left,
                _ => _currentPosition
            };
            var cell = Vector3.up;
            if (CheckCell(cell))
            {
                var bodyPosition = new Vector3Int(_currentTarget.x, _currentTarget.z, _currentTarget.y);
                transform.DOMove(bodyPosition + _centerOffset,
                    1 / _speed, false);
            }
            else
            {
                _cam.DOShakePosition(strength: 0.1f, duration: .2f, randomness: 45f, vibrato: 45, fadeOut: true);
                _cam.transform.DOLocalMove(Vector3.zero, .1f, false);
            }
                //_playerUIManager.LogAction.Invoke("Moved to cell " + _currentPosition.x + ", " + _currentPosition.y);
            //_manager.UpdateTurn();
            yield return new WaitForSeconds(_turnCD);
            _isMoving = false;
        }
        private bool CheckCell(Vector3 cellPos)
        {
            return true;
            //TODO: reimplement movement checks at later time
            if (true) // if position is valid cell
            {
                var body_position = new Vector3Int(_currentTarget.x, _currentTarget.z, _currentTarget.y);
                if (true) // if cell is not occupied or blocked
                {
                    //Debug.Log("Cell is blocked!");
                    var cellTag = "";
                    switch (cellTag)
                    {
                        case "Enemy":
                            _cam.DOShakePosition(strength: 0.1f, duration: .2f, randomness: 45f, vibrato: 45, fadeOut: true);
                            _cam.transform.DOLocalMove(Vector3.zero, .1f, false);
                            break;
                        case "Item":
                            //_events.pickUpItem.Invoke(cell);
                            transform.DOMove(body_position + _centerOffset,
                                1 / _speed, false);
                            //_manager.GetDungeonCell(_currentPosition).Free();
                            _currentPosition = _currentTarget;
                            //cell.Occupy(_player);
                            break;
                        case "Key":
                            //_events.pickUpItem.Invoke(cell);
                            transform.DOMove(body_position + _centerOffset,
                                1 / _speed, false);
                            //_manager.GetDungeonCell(_currentPosition).Free();
                            _currentPosition = _currentTarget;
                            //cell.Occupy(_player);
                            break;
                        case "Weapon":
                            //_events.pickUpItem.Invoke(cell);
                            transform.DOMove(body_position + _centerOffset,
                                1 / _speed, false);
                            //_manager.GetDungeonCell(_currentPosition).Free();
                            _currentPosition = _currentTarget;
                            //cell.Occupy(_player);
                            break;
                        case "Door":
                            _cam.DOShakePosition(strength: 0.1f, duration: .2f, randomness: 45f, vibrato: 45, fadeOut: true);
                            _cam.transform.DOLocalMove(Vector3.zero, .1f, false);
                            break;
                        case "Exit":
                            //_events.endRound.Invoke();
                            break;
                        default:
                            _cam.DOShakePosition(strength: 0.1f, duration: .2f, randomness: 45f, vibrato: 45, fadeOut: true);
                            _cam.transform.DOLocalMove(Vector3.zero, .1f, false);
                            break;
                    }
                }
                else
                {
                    transform.DOMove(body_position + _centerOffset, 1 / _speed, false);
                    //_manager.GetDungeonCell(_currentPosition).Free();
                    _currentPosition = _currentTarget;
                    //cell.Occupy(_player);
                }
                
            }
            else
            {
                _cam.DOShakePosition(strength: 0.1f, duration: .2f, randomness: 45f, vibrato: 45, fadeOut: true);
                _cam.transform.DOLocalMove(Vector3.zero, .1f, false);
            }
        }
    
        private IEnumerator PlayerAttack(Vector3Int targetCell)
        {
            if (!_isMoving) yield break;
            
            //_playerUIManager.LogAction.Invoke("Attacked cell " + targetCell.x + ", " + targetCell.y);
            //_player.activeWeapon?.Attack(targetCell);
            //_manager.UpdateTurn();
            yield return new WaitForSeconds(_turnCD);
            _isMoving = false;
        }
        
        //Not used due to disorientation
        private void HeadBob()
        {
            if (_cam == null) return;
            var camPos = _cam.transform.position;
            
            var sequence = DOTween.Sequence()
                .Append(_cam.transform.DOMoveY(camPos.y - _headBob/4, 1.5f, false).SetEase(Ease.InOutSine))
                .Append(_cam.transform.DOMoveY(camPos.y, 1, false).SetEase(Ease.InOutSine));
            sequence.SetLoops(-1, LoopType.Yoyo);
        }

    }
}
