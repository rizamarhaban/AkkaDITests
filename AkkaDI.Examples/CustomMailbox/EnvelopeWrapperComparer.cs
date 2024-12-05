using AkkaDI.Examples.Commands;

using System.Collections.Generic;

namespace AkkaDI.Examples.CustomMailbox;

public class EnvelopeWrapperComparer : IComparer<EnvelopeWrapper>
{
    public EnvelopeWrapperComparer() { }

    public int Compare(EnvelopeWrapper? x, EnvelopeWrapper? y)
    {
        var messageX = x.Envelope.Message;
        var messageY = y.Envelope.Message;

        bool isXScheduleCommand = messageX is FakeScheduleCommand;
        bool isYScheduleCommand = messageY is FakeScheduleCommand;

        if (isXScheduleCommand && isYScheduleCommand)
        {
            var scx = (FakeScheduleCommand)messageX;
            var scy = (FakeScheduleCommand)messageY;

            // Both are FakeScheduleCommand; compare based on priority and Timestamp
            int priorityX = GetPriority(scx);
            int priorityY = GetPriority(scy);

            if (priorityX != priorityY)
            {
                // Lower priority number has higher priority
                return priorityX.CompareTo(priorityY);
            }
            else
            {
                // Same priority, order by Timestamp
                int timestampComparison = scx.Timestamp.CompareTo(scy.Timestamp);
                if (timestampComparison != 0)
                {
                    return timestampComparison;
                }
                else
                {
                    // If Timestamps are equal, maintain FIFO
                    return x.SequenceNumber.CompareTo(y.SequenceNumber);
                }
            }
        }
        else if (isXScheduleCommand)
        {
            return -1; // FakeScheduleCommand messages have higher priority
        }
        else if (isYScheduleCommand)
        {
            return 1; // Non-FakeScheduleCommand messages have lower priority
        }
        else
        {
            // Both are other message types; maintain FIFO
            return x.SequenceNumber.CompareTo(y.SequenceNumber);
        }
    }

    private static int GetPriority(FakeScheduleCommand sc)
    {
        if (sc.IsSelfMessage && sc.IsReExecute)
        {
            return 1; // Priority 1
        }

        if (sc.IsReExecute)
        {
            return 2; // Priority 2
        }

        return 3; // Priority 3 (non-FakeScheduleCommand messages)
    }
}
