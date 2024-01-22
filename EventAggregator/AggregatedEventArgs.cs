namespace EventAggregator
{
    public class AggregatedEventArgs<TEventArgs> : EventArgs
    {
        public AggregatedEventArgs(IReadOnlyList<TEventArgs> aggregatedEvents)
        {
            AggregatedEvents = aggregatedEvents;
        }

        public IReadOnlyList<TEventArgs> AggregatedEvents { get; }
    }
}