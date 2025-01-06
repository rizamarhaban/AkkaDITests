using Akka.Actor;

using AkkaDI.Examples.Commands;
using AkkaDI.Examples.CustomMailbox;

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

            var registry = EventCommandMailboxRegistry.For(Context.System);
            var mailbox = registry.GetMailbox(Self);
            var minTimestamp = mailbox?.GetMinTimestamp()!;

            // Notify the test probe
            if (minTimestamp.HasValue)
                _testProbe.Tell(new ProcessedMessage(msg, minTimestamp.Value));

        });
    }
}
