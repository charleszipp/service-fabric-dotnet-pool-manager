using MongoDB.Bson;
using System;
using System.Globalization;

namespace PoolManager.Terminal
{
    public class Terminal : ITerminal
    {
        public void Write(string message)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}, {message}");
        }
    }
}