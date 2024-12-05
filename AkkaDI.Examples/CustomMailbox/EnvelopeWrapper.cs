using Akka.Actor;

namespace AkkaDI.Examples.CustomMailbox;

public record EnvelopeWrapper(Envelope Envelope, long SequenceNumber);
