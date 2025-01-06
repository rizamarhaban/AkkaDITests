using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;
using Akka.Dispatch.MessageQueues;

using AkkaDI.Examples.CustomMailbox;

namespace AkkaDI.Examples;

public class ScheduleTrackerMailbox : MailboxType, IProducesMessageQueue<SchedulePriorityMessageQueue>
{
    public ScheduleTrackerMailbox(Settings settings, Config config) : base(settings, config) { }

    public override IMessageQueue Create(IActorRef owner, ActorSystem system)
    {
        // Register the custom mailbox with the extension
        var messageQueue = new SchedulePriorityMessageQueue();
        EventCommandMailboxRegistry.For(system).RegisterMailbox(owner, messageQueue);
        return messageQueue;
    }
}
