using System;

namespace AkkaDI.Examples.Commands;

public class FakeScheduleCommand
{
    public TimeSpan Timestamp { get; }
    public string Content { get; }
    public bool IsReExecute { get; }
    public bool IsSelfMessage { get; }

    public FakeScheduleCommand(TimeSpan timestamp, string content, bool isReExecute, bool isSelfMessage = false)
    {
        Timestamp = timestamp;
        Content = content;
        IsReExecute = isReExecute;
        IsSelfMessage = isSelfMessage;
    }

    public override string ToString()
    {
        return $"Command(Content={Content}, Timestamp={Timestamp}, IsReexecute={IsReExecute}, IsSelfMessage={IsSelfMessage})";
    }
}
