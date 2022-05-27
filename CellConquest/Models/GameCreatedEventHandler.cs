using CellConquest.Message;
using Paramore.Brighter;

namespace CellConquest.Models;

public class GameCreatedEventHandler : RequestHandler<GameCreatedEvent>
{
    private readonly IAmACommandProcessor _commandProcessor;

    public GameCreatedEventHandler(IAmACommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    public override GameCreatedEvent Handle(GameCreatedEvent @event)
    {
        
        return base.Handle(@event);
    }
}