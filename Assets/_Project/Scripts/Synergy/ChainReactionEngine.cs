using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Synergy
{
    /// <summary>
    /// Strongly typed event model for cascading elemental and item triggers.
    /// Tracks recursion depth, source identity, and execution metadata.
    /// </summary>
    [Serializable]
    public class ChainEvent
    {
        public string EventId { get; private set; }
        public string SourceId { get; private set; }
        public string TargetId { get; private set; }
        public int Depth { get; private set; }
        public float Timestamp { get; private set; }
        public string EventType { get; private set; }
        public int Value { get; private set; }
        public string RootTriggerId { get; private set; }

        public ChainEvent(
            string eventId,
            string sourceId,
            string targetId,
            int depth,
            float timestamp,
            string eventType,
            int value,
            string rootId = null)
        {
            EventId = eventId ?? Guid.NewGuid().ToString();
            SourceId = sourceId;
            TargetId = targetId;
            Depth = depth;
            Timestamp = timestamp;
            EventType = eventType;
            Value = value;
            RootTriggerId = string.IsNullOrEmpty(rootId) ? (sourceId ?? EventId) : rootId;
        }
    }

    /// <summary>
    /// Master execution engine and loop guard for cascading item & rune chain reactions.
    /// Strictly adheres to PLAN.md Section 8.1:
    /// 1. Frame-Tick Propagation Cap (0.02s interval per source trigger)
    /// 2. Recursion Depth Limit (N <= 4)
    /// 3. Sequential Queue<ChainEvent> processing (zero stack recursion).
    /// </summary>
    public class ChainReactionEngine : MonoBehaviour
    {
        public const int MAX_CHAIN_DEPTH = 4;
        public const float PROPAGATION_TICK_INTERVAL = 0.02f;

        private readonly Queue<ChainEvent> _eventQueue = new Queue<ChainEvent>();
        private readonly Dictionary<string, float> _lastTriggeredTimestampBySource = new Dictionary<string, float>();
        private readonly HashSet<string> _processedEventIds = new HashSet<string>();
        private readonly List<ChainEvent> _history = new List<ChainEvent>();

        public event Action<ChainEvent> OnChainEventProcessed;
        public event Action<ChainEvent, string> OnChainEventRejected;

        public int QueueCount => _eventQueue.Count;
        public int ProcessedCount => _history.Count;
        public IReadOnlyList<ChainEvent> History => _history;

        /// <summary>
        /// Enqueues a chain event subject to depth limits (Depth <= 4) and tick rate caps.
        /// Returns true if enqueued; false if rejected by loop guards.
        /// </summary>
        public bool EnqueueEvent(ChainEvent evt, float currentTime = 0f)
        {
            if (evt == null) return false;

            // 1. Enforce Hard Recursion Depth Limit: N <= 4
            if (evt.Depth > MAX_CHAIN_DEPTH)
            {
                OnChainEventRejected?.Invoke(evt, $"Exceeded maximum chain depth ({evt.Depth} > {MAX_CHAIN_DEPTH})");
                return false;
            }

            // 2. Enforce Frame-Tick Propagation Cap (0.02s per source trigger)
            if (!string.IsNullOrEmpty(evt.SourceId))
            {
                if (_lastTriggeredTimestampBySource.TryGetValue(evt.SourceId, out float lastTime))
                {
                    if (currentTime > 0f && (currentTime - lastTime) < PROPAGATION_TICK_INTERVAL)
                    {
                        OnChainEventRejected?.Invoke(evt, $"Rate limit cap violated ({currentTime - lastTime:F4}s < {PROPAGATION_TICK_INTERVAL}s)");
                        return false;
                    }
                }
            }

            // 3. Duplicate / Cyclic loop guard
            if (_processedEventIds.Contains(evt.EventId))
            {
                OnChainEventRejected?.Invoke(evt, "Duplicate event ID");
                return false;
            }

            _eventQueue.Enqueue(evt);
            return true;
        }

        /// <summary>
        /// Processes all enqueued chain events sequentially, updating rate limits and notifying listeners.
        /// </summary>
        public int ProcessQueue(float currentTime = 0f)
        {
            int processedThisPass = 0;

            while (_eventQueue.Count > 0)
            {
                ChainEvent evt = _eventQueue.Dequeue();
                if (evt == null) continue;

                if (!string.IsNullOrEmpty(evt.SourceId))
                {
                    _lastTriggeredTimestampBySource[evt.SourceId] = currentTime;
                }

                _processedEventIds.Add(evt.EventId);
                _history.Add(evt);
                processedThisPass++;

                OnChainEventProcessed?.Invoke(evt);
            }

            return processedThisPass;
        }

        public void ClearQueue()
        {
            _eventQueue.Clear();
        }

        public void ResetEngine()
        {
            _eventQueue.Clear();
            _lastTriggeredTimestampBySource.Clear();
            _processedEventIds.Clear();
            _history.Clear();
        }
    }
}
