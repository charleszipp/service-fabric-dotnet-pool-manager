using MongoDB.Bson;

namespace PoolManager.Terminal
{
    public interface ITerminal
    {
        void Write(string message);
    }
}