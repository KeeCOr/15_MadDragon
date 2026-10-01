// Assets/_Game/Scripts/Core/EventBus.cs
using System;
using System.Collections.Generic;

namespace MedievalRTS.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.ContainsKey(type))
                _handlers[type] = Delegate.Combine(_handlers[type], handler);
            else
                _handlers[type] = handler;
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var existing)) return;
            var updated = Delegate.Remove(existing, handler);
            if (updated == null) _handlers.Remove(type);
            else _handlers[type] = updated;
        }

        public static void Publish<T>(T evt)
        {
            if (_handlers.TryGetValue(typeof(T), out var handler))
                ((Action<T>)handler).Invoke(evt);
        }

        /// <summary>테스트 후 정리용</summary>
        public static void Clear() => _handlers.Clear();
    }
}
