// See https://aka.ms/new-console-template for more information
using EventAggregator;

Console.WriteLine("Hello, World!");
Console.WriteLine("PRESS ANY KEY TO START...");
_ = Console.ReadKey(false);

RandomNumberEventPublisher publisher = new()
{
    ThreadsNumber = 10,
    MaxRandomNumberValue = 1000,
    MaxPublishDelay = 500
};

EventAggregator<RandomNumberPublishedEventArgs> aggregator = new()
{
    
};

aggregator.Aggregated += (s, e) =>
{
    Console.WriteLine(
        $"Aggregated events: {e.AggregatedEvents.Count}, " +
        $"avr: {e.AggregatedEvents.Average(e => e.Number)}, " +
        $"first: {e.AggregatedEvents.First().Number}, " +
        $"last: {e.AggregatedEvents.Last().Number}");
};

publisher.EventPublished += (s, e) =>
{
    Console.WriteLine($"Event published: {e.Number}, Thread={e.ThreadId}, Delay={e.PublishDelay}");
    aggregator.Consume(e);
};
publisher.Start();

Console.WriteLine("PRESS ANY KEY TO STOP...");
_ = Console.ReadKey(false);

publisher.Stop();

Console.WriteLine("PRESS ANY KEY TO EXIT...");
_ = Console.ReadKey(false);