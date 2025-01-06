using System;

namespace AkkaDI.Examples.Commands;

public record ProcessedMessage
{
    public IScheduleMessage Message { get; }
    public TimeSpan MinTimestamp { get; }

    public ProcessedMessage(IScheduleMessage message, TimeSpan minTimestamp)
    {
        Message = message;
        MinTimestamp = minTimestamp;
    }
}
