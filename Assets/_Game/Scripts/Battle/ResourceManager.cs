// Assets/_Game/Scripts/Battle/ResourceManager.cs
using UnityEngine;
using MedievalRTS.Core;

namespace MedievalRTS.Battle
{
    public class ResourceManager : MonoBehaviour
    {
        [SerializeField] private int startingGold = 100;
        [SerializeField] private float goldPerSecond = 5f;

        public int Gold { get; private set; }

        private float _accumulator;

        private void Start()
        {
            Gold = startingGold;
            EventBus.Publish(new GoldChangedEvent(Gold));
        }

        private void Update()
        {
            _accumulator += goldPerSecond * Time.deltaTime;
            if (_accumulator < 1f) return;
            int gained = Mathf.FloorToInt(_accumulator);
            _accumulator -= gained;
            AddGold(gained);
        }

        public bool TrySpend(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            EventBus.Publish(new GoldChangedEvent(Gold));
            return true;
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            EventBus.Publish(new GoldChangedEvent(Gold));
        }
    }
}
