using Akka.Actor;
using Akka.Dispatch;
using Akka.Dispatch.MessageQueues;

using System.Collections.Generic;
using System.Threading;

namespace AkkaDI.Examples.CustomMailbox;

public class SchedulePriorityMessageQueue : IMessageQueue, IUnboundedMessageQueueSemantics
{
    private readonly object _syncRoot = new object();
    private readonly LinkedList<EnvelopeWrapper> _queue = [];
    private readonly EnvelopeWrapperComparer _comparer = new();
    private long _sequenceCounter = 0;

    public SchedulePriorityMessageQueue() { }

    public void Enqueue(IActorRef receiver, Envelope envelope)
    {
        lock (_syncRoot)
        {
            // Wrap the envelope with a sequence number for FIFO tie-breaking
            var wrappedEnvelope = new EnvelopeWrapper(envelope, Interlocked.Increment(ref _sequenceCounter));

            // Insert the envelope into the queue based on the comparer
            if (_queue.Count == 0)
            {
                _queue.AddFirst(wrappedEnvelope);
            }
            else
            {
                var node = _queue.First;
                while (node != null && _comparer.Compare(node.Value, wrappedEnvelope) <= 0)
                {
                    node = node.Next;
                }

                if (node == null)
                {
                    _queue.AddLast(wrappedEnvelope);
                }
                else
                {
                    _queue.AddBefore(node, wrappedEnvelope);
                }
            }
        }
    }

    public bool HasMessages
    {
        get
        {
            lock (_syncRoot)
            {
                return _queue.Count > 0;
            }
        }
    }

    public int Count
    {
        get
        {
            lock (_syncRoot)
            {
                return _queue.Count;
            }
        }
    }

    public bool TryDequeue(out Envelope envelope)
    {
        lock (_syncRoot)
        {
            if (_queue.Count > 0)
            {
                var wrappedEnvelope = _queue.First!.Value;
                _queue.RemoveFirst();
                envelope = wrappedEnvelope.Envelope;
                return true;
            }

            envelope = default;
            return false;
        }
    }

    public void CleanUp(IActorRef owner, IMessageQueue deadletters)
    {
        lock (_syncRoot)
        {
            while (_queue.Count > 0)
            {
                var wrappedEnvelope = _queue.First!.Value;
                _queue.RemoveFirst();
                deadletters.Enqueue(owner, wrappedEnvelope.Envelope);
            }
        }
    }
}
