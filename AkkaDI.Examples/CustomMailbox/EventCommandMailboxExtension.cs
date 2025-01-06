using Akka.Actor;

using System.Collections.Concurrent;

namespace AkkaDI.Examples.CustomMailbox;

// Registry to track custom mailboxes
public class EventCommandMailboxRegistry : IExtension
{
    private readonly ConcurrentDictionary<IActorRef, SchedulePriorityMessageQueue> _mailboxRegistry = new();

    public static EventCommandMailboxRegistry For(ActorSystem system)
    {
        return system.WithExtension<EventCommandMailboxRegistry, EventCommandMailboxRegistryProvider>();
    }

    public void RegisterMailbox(IActorRef actor, SchedulePriorityMessageQueue messageQueue)
    {
        _mailboxRegistry[actor] = messageQueue;
    }

    public SchedulePriorityMessageQueue? GetMailbox(IActorRef actor)
    {
        _mailboxRegistry.TryGetValue(actor, out var mailbox);
        return mailbox;
    }
}

public class EventCommandMailboxRegistryProvider : ExtensionIdProvider<EventCommandMailboxRegistry>
{
    public override EventCommandMailboxRegistry CreateExtension(ExtendedActorSystem system)
    {
        return new EventCommandMailboxRegistry();
    }
}
