using Akka.Actor;

using AkkaDI.Examples.Commands;

namespace AkkaDI.Examples.Actors;

public class GeneratorTestNoConstructorActor : ReceiveActor
{
    public GeneratorTestNoConstructorActor()
    {

        Receive<FakeScheduleCommand>(msg =>
        {
            // Simulate processing delay if needed
            //System.Threading.Thread.Sleep(100);
        });
    }
}
