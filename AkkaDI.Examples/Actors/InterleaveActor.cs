using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;

using AkkaDI.Examples.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AkkaDI.Examples.Actors;
public class InterleaveActor : ReceiveActor
{
    private readonly IActorRef _testProbe;
    private readonly ILoggingAdapter _logger = Context.GetLogger();
    private readonly IServiceProvider _serviceProvider;

    private readonly SortedList<TimeSpan, FakeScheduleCommand> _events = [];
    private bool _isProcessing;

    public InterleaveActor(IServiceProvider serviceProvider, IActorRef testProbe, string id)
    {
        _logger.Info($"[{id}] InterleaveActor created");

        _serviceProvider = serviceProvider;
        _testProbe = testProbe;

        Receive<FakeScheduleCommand>(msg =>
        {
            _events.Add(msg.Timestamp, msg);
            _logger.Info($"[{msg.Timestamp}] Event added");

            // If not already processing, kick off StartProcessing
            if (!_isProcessing)
                Self.Tell(StartProcessing.Instance);
        });

        Receive<StartProcessing>(_ =>
        {
            // If we are already in the middle of a process or have no events at all, do nothing
            if (_isProcessing || _events.Count == 0)
                return;

            // Find the earliest (lowest Timestamp) that is not yet processed
            // Using LINQ's FirstOrDefault => no explicit while/for loop
            var nextUnprocessed = _events
                .Values
                .FirstOrDefault(e => !e.IsProcessed);

            // If every item is processed (or no unprocessed), do nothing
            if (nextUnprocessed == null)
                return;

            // Mark that we are processing
            _isProcessing = true;

            // from within an actor
            var mailboxCount = Context.AsInstanceOf<ActorCell>().Mailbox.MessageQueue.Count;
            _logger.Info($"[{mailboxCount}] Message in Mailbox");

            // Wrap it in a message for asynchronous processing
            var processMessage = new ProcessScheduleCommand(nextUnprocessed);

            // Process asynchronously, pipe the result (success/failure) back to Self
            Task
                .Run(async () =>
                {
                    var result = await DoProcessAsync(processMessage);
                    _isProcessing = false;

                    _logger.Info($"[{nextUnprocessed.Timestamp}] Processed: {result}");

                    // If there is still any unprocessed event, send StartProcessing again
                    if (_events.Values.Any(e => !e.IsProcessed))
                        Self.Tell(StartProcessing.Instance);
                })
                .PipeTo(Self, Sender);
        });

        //
        // 3) If processing succeeds
        //
        Receive<ProcessDone>(done =>
        {
            // Mark the original event as processed
            done.Original.IsProcessed = true;

            // We are no longer busy
            _isProcessing = false;

            // If there is still any unprocessed event, send StartProcessing again
            if (_events.Values.Any(e => !e.IsProcessed))
                Self.Tell(StartProcessing.Instance);
        });

        //
        // 4) If processing fails
        //
        Receive<ProcessFailed>(failed =>
        {
            // Mark as processed or you might leave it unprocessed
            // depending on your failure-handling requirements.
            // For example, let's assume we still mark it as processed 
            // so that we don’t retry it infinitely:
            failed.Original.IsProcessed = true;

            // We are no longer busy
            _isProcessing = false;

            // If there is still any unprocessed event, send StartProcessing again
            if (_events.Values.Any(e => !e.IsProcessed))
                Self.Tell(StartProcessing.Instance);
        });
    }

    /// <summary>
    /// Example asynchronous processing. 
    /// Replace with your real logic (I/O, CPU-bound tasks, etc.).
    /// </summary>
    private async Task<string> DoProcessAsync(ProcessScheduleCommand msg)
    {
        await Task.Delay(500); // simulate some asynchronous work
        msg.OriginalMessage.IsProcessed = true;
        return $"Processed[{msg.OriginalMessage.Content}]";
    }
}
