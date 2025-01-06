namespace AkkaDI.Examples.Commands;

internal sealed class ProcessDone
{
    public ProcessDone(IScheduleMessage original, string result)
    {
        Original = original;
        Result = result;
    }

    public IScheduleMessage Original { get; }
    public string Result { get; }
}
