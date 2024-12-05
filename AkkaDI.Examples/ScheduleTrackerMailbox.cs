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
        return new SchedulePriorityMessageQueue();
    }
}
