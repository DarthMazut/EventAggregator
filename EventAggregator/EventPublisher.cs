using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventAggregator
{
    public class RandomNumberEventPublisher
    {
        private readonly Random _rnd = new();
        private CancellationTokenSource _tsc = new();

        public int ThreadsNumber { get; init; } = 1;

        public int MaxRandomNumberValue { get; init; } = int.MinValue;

        public int MaxPublishDelay { get; init; } = 100;

        public int MinPublishDelay { get; init; } = 0;

        public event EventHandler<RandomNumberPublishedEventArgs>? EventPublished;

        public void Start()
        {
            _tsc = new CancellationTokenSource();
            for (int i = 0; i < ThreadsNumber; i++)
            {
                _ = Task.Run(() =>
                {
                    while (true)
                    {
                        int publishDelay = _rnd.Next(MinPublishDelay, MaxPublishDelay + 1);
                        EventPublished?.Invoke(this,
                            new RandomNumberPublishedEventArgs(
                                _rnd.Next(MaxRandomNumberValue),
                                Environment.CurrentManagedThreadId,
                                publishDelay));

                        _tsc.Token.ThrowIfCancellationRequested();
                        //await Task.Delay(publishDelay);
                        Thread.Sleep(publishDelay);
                    }
                }, _tsc.Token);
            }
        }

        public void Stop()
        {
            _tsc.Cancel();
        }
    }

    public class RandomNumberPublishedEventArgs : EventArgs
    {
        public RandomNumberPublishedEventArgs(int number, int threadId, int publishDelay)
        {
            Number = number;
            ThreadId = threadId;
            PublishDelay = publishDelay;
        }

        public int Number { get; }

        public int ThreadId { get; }

        public int PublishDelay { get;}
    }
}
