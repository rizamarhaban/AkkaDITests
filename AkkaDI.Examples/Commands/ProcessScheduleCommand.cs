namespace AkkaDI.Examples.Commands;

public class ProcessScheduleCommand
{
    public IScheduleMessage OriginalMessage { get; }

    public ProcessScheduleCommand(IScheduleMessage originalMessage)
    {
        OriginalMessage = originalMessage;
    }
}