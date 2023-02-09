using System;
using DungeonAutomata._Project.Scripts.BulletHellPrototype.Interfaces;
using DungeonAutomata._Project.Scripts.Utilities.ObjectPooling;
using UnityEngine;

namespace DungeonAutomata._Project.Scripts.BulletHellPrototype
{
    public class Bullet : MonoBehaviour, IPoolObject<Bullet>
    {
        private float _speed;
        private float _damage;
        private Vector3 _direction;
        private Action<Bullet> _returnToPool;
    

        public void Initialize(Action<Bullet> returnAction)
        {
            _returnToPool = returnAction;
        }

        public void ReturnToPool()
        {
            _returnToPool?.Invoke(this);
        }
    
        public void InitializeBullet(Vector3 direction, float speed, float damage)
        {
            throw new System.NotImplementedException();
        }
    
        public void DamageTarget(IBUnit unit, float damage)
        {
            throw new System.NotImplementedException();
        }

        public void DisableBullet()
        {
            throw new System.NotImplementedException();
        }
    }
}
