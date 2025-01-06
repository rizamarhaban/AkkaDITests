using System;

namespace AkkaDI.Examples.Commands;

public interface IScheduleMessage
{
    TimeSpan Timestamp { get; }
    string Content { get; }
    bool IsReExecute { get; }
    bool IsSelfMessage { get; }
    bool IsProcessed { get; set; }
}