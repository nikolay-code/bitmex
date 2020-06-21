using System;

namespace Bitmex.NET
{
    public class BitmexWebSocketLimitReachedException : Exception
    {
        public BitmexWebSocketLimitReachedException() : base("remining connections count is 0")
        {

        }
    }

    public class BitmexWebSocketOpenConnectionException : Exception
    {
        public BitmexWebSocketOpenConnectionException() : base("Open connection timeout. Welcome message is not received")
        {

        }
    }
}
