using System;

namespace AkkaDI.Examples.Commands;

internal sealed class ProcessFailed
{
    public ProcessFailed(IScheduleMessage original, Exception ex)
    {
        Original = original;
        Exception = ex;
    }

    public IScheduleMessage Original { get; }
    public Exception Exception { get; }
}
