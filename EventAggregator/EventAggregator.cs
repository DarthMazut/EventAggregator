using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventAggregator
{
    public class EventAggregator<TEventArgs>
    {
        private readonly object _syncLock = new();
        private readonly List<TEventArgs> _eventQueue = [];

        private DateTimeOffset _lastEventReceivedTimestamp = DateTimeOffset.MinValue;
        private bool _isLoopRunning;

        public event EventHandler<AggregatedEventArgs<TEventArgs>>? Aggregated;

        public TimeSpan LoopTimeout { get; init; } = TimeSpan.FromMilliseconds(1000);

        public TimeSpan LoopDelay { get; init; } = TimeSpan.FromMilliseconds(10);

        public TimeSpan AggregationDelay { get; init; } = TimeSpan.FromMilliseconds(100);

        public void Consume(TEventArgs eventArgs)
        {
            lock (_syncLock)
            {
                _eventQueue.Add(eventArgs);
                _lastEventReceivedTimestamp = DateTimeOffset.Now;
                if (!_isLoopRunning)
                {
                    RunLoop();
                }
            }
        }

        private void RunLoop()
        {
            _isLoopRunning = true;
            _ = Task.Run(async () =>
            {
                while (_isLoopRunning)
                {
                    if (ShouldAggregate())
                    {
                        Aggregate();
                    }

                    if (ShouldTimeout())
                    {
                        break;
                    }

                    await Task.Delay(LoopDelay);
                }
                _isLoopRunning = false;
            });
        }

        private void Aggregate()
        {
            lock (_syncLock)
            {
                IReadOnlyList<TEventArgs> eventCopy = _eventQueue.ToList().AsReadOnly();
                _eventQueue.Clear();
                _ = Task.Run(() => Aggregated?.Invoke(this, new AggregatedEventArgs<TEventArgs>(eventCopy)));
            }   
        }

        private bool ShouldTimeout()
        {
            lock (_syncLock)
            {
                return _lastEventReceivedTimestamp + LoopTimeout < DateTimeOffset.Now;
            }
        }

        private bool ShouldAggregate()
        {
            lock (_syncLock)
            {
                bool isTime = _lastEventReceivedTimestamp + AggregationDelay < DateTimeOffset.Now;
                bool areEvents = _eventQueue.Count > 0;
                return isTime & areEvents;
            }
        }
    }
}
