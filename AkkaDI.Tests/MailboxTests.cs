using Akka.Actor;
using Akka.Configuration;
using Akka.DependencyInjection;
using Akka.TestKit;
using Akka.TestKit.NUnit;

using AkkaDI.Examples.Actors;
using AkkaDI.Examples.Commands;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Generic;
using System.Linq;

namespace AkkaDI.Tests;

[TestFixture]
public class MailboxTests : IDisposable
{
    private IServiceProvider? _serviceProvider;
    private ActorSystem? _actorSystem;
    private IHost _host;

    [SetUp]
    public void Setup()
    {
        _host = new HostBuilder()
            .ConfigureServices((context, services) =>
            {
                _serviceProvider = services.BuildServiceProvider();

                var bootstrap = BootstrapSetup.Create();

                // enable DI support inside this ActorSystem, if needed
                var diSetup = DependencyResolverSetup.Create(_serviceProvider);

                var akkaConfig = ConfigurationFactory.ParseString(@"
                 akka {
                     # Akka.NET settings
                 }
                 schedule-priority-mailbox {
                     mailbox-type = ""AkkaDI.Examples.ScheduleTrackerMailbox, AkkaDI.Examples""
                 }");

                // merge this setup (and any others) together into ActorSystemSetup
                var actorSystemSetup = bootstrap.WithConfig(akkaConfig).And(diSetup);

                // start ActorSystem
                _actorSystem = ActorSystem.Create(GetType().Name, actorSystemSetup);

                var testKit = new TestKit(_actorSystem);
                var testProbe = testKit?.CreateTestProbe();

                services.AddSingleton(testKit);
                services.AddSingleton(testProbe);
            })
            .Build();

        _host.StartAsync();
    }

    [TearDown]
    public void Dispose() => _host?.Dispose();

    [Test]
    public void ScheduleTrackerMailbox_Should_Prioritize_Messages_Correctly()
    {
        var testKit = _host?.Services?.GetService<TestKit>();
        var testProbe = _host?.Services?.GetService<TestProbe>();
        var actorSystem = testKit?.Sys;

        var resolver = DependencyResolver.For(actorSystem);
        var props = resolver.Props<GeneratorTestActor>([testProbe?.Ref, nameof(GeneratorTestActor)])
            .WithMailbox("schedule-priority-mailbox");
        var actor = testKit?
            .ActorOfAsTestActorRef<GeneratorTestActor>(props, nameof(GeneratorTestActor));
        actor.Should().NotBeNull();

        // Define messages with different priorities
        var messages = new List<FakeScheduleCommand>
        {
            new(TimeSpan.FromSeconds(5), "Message A", isReExecute: false),
            new(TimeSpan.FromSeconds(13), "Message B", isReExecute: false),
            new(TimeSpan.FromSeconds(17), "Message C", isReExecute: false),
            new(TimeSpan.FromSeconds(2), "Message D", isReExecute: true, isSelfMessage: true),
            new(TimeSpan.FromSeconds(11), "Message E", isReExecute: false),
            new(TimeSpan.FromSeconds(4), "Message F", isReExecute: false),
            new(TimeSpan.FromSeconds(14), "Message F", isReExecute: true),
            new(TimeSpan.FromSeconds(4), "Message G", isReExecute: true, isSelfMessage: true),
            new(TimeSpan.FromSeconds(7), "Message H", isReExecute: false),
            new(TimeSpan.FromSeconds(7), "Message I", isReExecute: true, isSelfMessage: true),
            new(TimeSpan.FromSeconds(8), "Message J", isReExecute: false),
        };

        // Send messages in a random order
        var random = new Random();
        var shuffledMessages = messages.OrderBy(x => random.Next()).ToList();

        // Write the original message sequence
        TestContext.Out.WriteLine("Original Message Sequence");
        WriteMessages(shuffledMessages);

        foreach (var msg in shuffledMessages)
        {
            actor?.Tell(msg);
        }

        // Collect processed messages
        var processedMessages = new List<ProcessedMessage>();
        for (int i = 0; i < messages.Count; i++)
        {
            var processed = testProbe.ExpectMsg<ProcessedMessage>();
            processedMessages.Add(processed);
        }

        // Assert: Expected processing order based on priority
        var expectedOrder = new List<string>
        {
            "Message D", // Timestamp = 00:00:02 (IsSelfMessage & IsReExecute)
            "Message G", // Timestamp = 00:00:04 (IsSelfMessage & IsReExecute)
            "Message I", // Timestamp = 00:00:07 (IsSelfMessage & IsReExecute)
            "Message F", // Timestamp = 00:00:14 (IsReExecute)
            "Message F", // Timestamp = 00:00:04
            "Message A", // Timestamp = 00:00:05
            "Message H", // Timestamp = 00:00:07
            "Message J", // Timestamp = 00:00:08
            "Message E", // Timestamp = 00:00:11
            "Message B", // Timestamp = 00:00:13
            "Message C", // Timestamp = 00:00:17
        };

        var actualOrder = processedMessages.Select(pm => pm.Message).ToList();

        // Write the actual message sequence
        TestContext.Out.WriteLine("\r\nReceived Message Sequence");
        WriteMessages(actualOrder);

        actualOrder.Select(p => p?.Content)
            .SequenceEqual(expectedOrder).Should().BeTrue();
    }

    [Test]
    public void ScheduleTrackerMailbox_Should_Prioritize_Messages_Correctly_No_Actor_Constructor()
    {
        TestContext.Out.WriteLine("This test just to check Breakpoint on the mailbox side. The results are not important.");

        var testKit = _host?.Services?.GetService<TestKit>();
        var actorSystem = testKit?.Sys;

        var resolver = DependencyResolver.For(actorSystem);
        var props = resolver.Props<GeneratorTestNoConstructorActor>()
            .WithMailbox("schedule-priority-mailbox");
        var actor = testKit?
            .ActorOfAsTestActorRef<GeneratorTestNoConstructorActor>(props, nameof(GeneratorTestNoConstructorActor));
        actor.Should().NotBeNull();

        // Define messages with different priorities
        var messages = new List<FakeScheduleCommand>
        {
            new(TimeSpan.FromSeconds(5), "Message A", isReExecute: false),
            new(TimeSpan.FromSeconds(13), "Message B", isReExecute: false),
            new(TimeSpan.FromSeconds(17), "Message C", isReExecute: false),
            new(TimeSpan.FromSeconds(2), "Message D", isReExecute: true, isSelfMessage: true),
            new(TimeSpan.FromSeconds(11), "Message E", isReExecute: false),
            new(TimeSpan.FromSeconds(4), "Message F", isReExecute: false),
            new(TimeSpan.FromSeconds(14), "Message F", isReExecute: true),
            new(TimeSpan.FromSeconds(4), "Message G", isReExecute: true, isSelfMessage: true),
            new(TimeSpan.FromSeconds(7), "Message H", isReExecute: false),
            new(TimeSpan.FromSeconds(7), "Message I", isReExecute: true, isSelfMessage: true),
            new(TimeSpan.FromSeconds(8), "Message J", isReExecute: false),
        };

        // Send messages in a random order
        var random = new Random();
        var shuffledMessages = messages.OrderBy(x => random.Next()).ToList();

        foreach (var msg in shuffledMessages)
        {
            actor?.Tell(msg);
        }
    }

    private void WriteMessages(List<FakeScheduleCommand?> messages)
    {
        if (messages is null)
            return;

        foreach (var item in messages)
        {
            if (item is null)
                continue;

            if (item.IsSelfMessage && item.IsReExecute)
            {
                TestContext.Out.WriteLine($"{item.Content} Timestamp = {item.Timestamp} (IsSelfMessage & IsReExecute)");
            }
            else if (item.IsSelfMessage)
            {
                TestContext.Out.WriteLine($"{item.Content} Timestamp = {item.Timestamp} (IsSelfMessage)");
            }
            else if (item.IsReExecute)
            {
                TestContext.Out.WriteLine($"{item.Content} Timestamp = {item.Timestamp} (IsReExecute)");
            }
            else
            {
                TestContext.Out.WriteLine($"{item.Content} Timestamp = {item.Timestamp}");
            }
        }
    }
}
