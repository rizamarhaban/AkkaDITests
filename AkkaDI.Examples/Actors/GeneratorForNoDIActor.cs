using Akka.Actor;

using AkkaDI.Examples.Commands;

using System;

namespace AkkaDI.Examples.Actors;

public class GeneratorForNoDIActor : ReceiveActor
{
    private readonly IActorRef _testProbe;

    public GeneratorForNoDIActor(IActorRef testProbe)
    {
        _testProbe = testProbe;

        Receive<FakeScheduleCommand>(msg =>
        {
            // Simulate processing delay if needed
            //System.Threading.Thread.Sleep(100);

            // Notify the test probe
            _testProbe.Tell(new ProcessedMessage(msg, DateTime.UtcNow));
        });
    }
}
