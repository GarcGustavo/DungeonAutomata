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
        private PlayerUnit _player;
        public Vector3Int CurrentPos => _currentPosition;
        
        [SerializeField] private Grid _grid;
        [SerializeField] private bool isMoving;
        [SerializeField] private Direction currentDirection = 0;
        [SerializeField] private float moveCD = .1f;
        private Vector3Int _currentPosition;
        private Vector3Int _currentTarget;
        [SerializeField] private float _speed = 4f;
        [SerializeField] private float _turnSpeed = 4f;
        [SerializeField] private float _headBob = 0.1f;
        [SerializeField] private Vector3 _centerOffset = new Vector3(.5f, 0f, .5f);
        private Tween _tween;
        private CellData[,] _cellMap;
        
        private enum Direction
        {
            North = 0,
            East = 1,
            South = 2,
            West = 3
        }
        
        private void Awake()
        {
            _grid = FindObjectOfType<Grid>();
            _player = GetComponent<PlayerUnit>();
            _currentPosition = _grid.WorldToCell(transform.position + _centerOffset);
            _currentTarget = _currentPosition;
            currentDirection = Direction.North;
            isMoving = false;
            _tween = transform.DOMove(_grid.CellToWorld(_currentPosition), 0f);
        }

        private void Start()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            GetMovementInput();
        }

        public void InitializeGrid(CellData[,] cellMap)
        {
            _cellMap = cellMap;
        }

        public void SetPosition(Vector3Int position)
        {
	        _currentPosition = position;
	        transform.position = _grid.CellToWorld(_currentPosition);
	        //TODO: replace w/ fps events
	        //_eventManager.InvokeCellUpdate(previousCell);
	        //_eventManager.InvokeCellUpdate(currentCell);
        }

        public void GetMovementInput()
        {
            if(_tween == null) return;
            if (_tween.IsPlaying()) return;
            
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            if (vertical > 0)
            {
                //Debug.Log("Moving Forward using playerUnit interface");
                //StartCoroutine(MovePlayerToCell());
            }
            else if (vertical < 0)
            {
                var direction = ((int)currentDirection + 2) % 4;
                currentDirection = (Direction) direction;
                
                StartCoroutine(RotatePlayer(Vector3.down * 180));
            }
            else if (horizontal < 0)
            {
                var direction = ((int) currentDirection + 3) % 4;
                currentDirection = (Direction) direction;
                StartCoroutine(RotatePlayer(Vector3.down * 90));
            }
            else if (horizontal > 0)
            {
                var direction = ((int)currentDirection + 1)%4;
                currentDirection = (Direction) direction;
                StartCoroutine(RotatePlayer(Vector3.up * 90));
            }
        }
        private IEnumerator RotatePlayer(Vector3 rotation)
        {
            if (_tween.IsPlaying())
            {
                _tween.Complete();
            }
            _tween = transform.DORotate(transform.eulerAngles + rotation, 1 / _turnSpeed, RotateMode.Fast);
            yield return new WaitForSeconds(moveCD);
        }
    
        public IEnumerator MovePlayerToCell()
        {
            if (_tween.IsPlaying())
            {
                _tween.Kill();
            }
            _currentTarget = currentDirection switch
            {
                Direction.North => _currentPosition + Vector3Int.forward,
                Direction.East => _currentPosition + Vector3Int.right,
                Direction.South => _currentPosition + Vector3Int.back,
                Direction.West => _currentPosition + Vector3Int.left,
                _ => _currentPosition
            };
            
            var cell = Vector3.zero;
            if (CheckCell(cell))
            {
                var worldPos = new Vector3Int(1, 0, 1);
                //_currentTarget.z = _currentTarget.y;
                //_currentTarget.y = 0;
                //TODO: reimplement
                _tween = transform.DOMove(_currentTarget * worldPos + _centerOffset, 
                    1 / _speed, 
                    false);
                _currentPosition = _currentTarget;
                _player.CurrentPos = _currentPosition;
            }
            else
            {
                _cam.DOShakePosition(strength: 0.1f, duration: .2f, randomness: 45f, vibrato: 45, fadeOut: true);
                _tween = _cam.transform.DOLocalMove(Vector3.zero, .1f, false);
            }

            //_playerUIManager.LogAction.Invoke("Moved to cell " + _currentPosition.x + ", " + _currentPosition.y);
            //_manager.UpdateTurn();
            yield return new WaitForSeconds(moveCD);
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
