using System;
using System.Threading;
using System.Threading.Tasks;

public class MidiEventQueueTests
{
    [Fact]
    public async Task StressTest()
    {
        var queue = new MidiEventQueue();
        var producer = Task.Run(() =>
        {
            for (int i = 0; i < 10000; i++)
            {
                queue.Enqueue(new MidiEvent { Id = i });
            }
        });
        var consumer = Task.Run(() =>
        {
            for (int i = 0; i < 10000; i++)
            {
                var @event = queue.TryDequeue(out MidiEvent @event);
                if (@event != null)
                {
                    Console.WriteLine(@event.Id);
                }
            }
        });
        await Task.WhenAll(producer, consumer);
    }
}
