using System.Threading.Tasks;

namespace CellConquest.Interfaces;

public interface IEventPublisher
{
    public Task PublishEvent(object @event);
}