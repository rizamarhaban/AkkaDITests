using System;

namespace AkkaDI.Examples.Commands;

public record ProcessedMessage(FakeScheduleCommand Message, DateTime ProcessedAt);
