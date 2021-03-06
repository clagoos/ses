using System.Threading.Tasks;

namespace Ses.Abstracts.Subscriptions
{
    public interface IHandle
    {
    }

    public interface IHandleAsync<in T> : IHandle where T : class, IEvent
    {
        Task Handle(T e, EventEnvelope envelope);
    }

    public interface IHandle<in T> : IHandle where T : class, IEvent
    {
        void Handle(T e, EventEnvelope envelope);
    }
}