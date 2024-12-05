using Akka.Actor;

using AkkaDI.Examples.Commands;

using System;
using System.Reflection;

namespace AkkaDI.Examples.Actors;

public class GeneratorTestActor : ReceiveActor
{
    private readonly IActorRef _testProbe;

    public GeneratorTestActor(IServiceProvider sp, IActorRef testProbe, string id)
    {
        _testProbe = testProbe;

        var mailboxType = GetMailboxType();
        System.Diagnostics.Debug.WriteLine($"[{id}] MailboxType: {mailboxType}");

        Receive<FakeScheduleCommand>(msg =>
        {
            // Simulate processing delay if needed
            //System.Threading.Thread.Sleep(100);

            // Notify the test probe
            _testProbe.Tell(new ProcessedMessage(msg, DateTime.UtcNow));
        });
    }

    private string GetMailboxType()
    {
        // Reflection code to access the mailbox
        var context = Context;
        var actorCellField = context.GetType().GetField("_cell", BindingFlags.NonPublic | BindingFlags.Instance);
        var actorCell = actorCellField?.GetValue(context);

        var mailboxField = actorCell?.GetType().GetField("_mailbox", BindingFlags.NonPublic | BindingFlags.Instance);
        var mailbox = mailboxField?.GetValue(actorCell);

        return mailbox?.GetType().FullName ?? "Unknown";
    }
}
