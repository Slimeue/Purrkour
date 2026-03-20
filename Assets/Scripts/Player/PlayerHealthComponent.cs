using Interface;
using UnityEngine;

namespace Player
{
    public class PlayerHealthComponent : MonoBehaviour , IKillable
    {
        public delegate void HealthChanged(int currentHealth);
        public event HealthChanged OnHealthChangedEvent;

        private int _currentHealth;
        [SerializeField] private int maxHealth = 3;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => _currentHealth;

        private void Awake()
        {
            _currentHealth = maxHealth;
            NotifyHealthChanged();
        }
        
        public void AddHealth(int amount)
        {
            _currentHealth += amount;
            NotifyHealthChanged();
        }
        
        public void ReduceHealth(int amount)
        {
            _currentHealth -= amount;
            if (_currentHealth < 0)
            {
                _currentHealth = 0;
                Kill();
            }
            NotifyHealthChanged();
        }

        private protected void NotifyHealthChanged()
        {
            OnHealthChangedEvent?.Invoke(_currentHealth);
        }

        public void TakeDamage(int damage)
        {
            ReduceHealth(damage);
        }

        public void Kill()
        {
            
        }
    }
}
