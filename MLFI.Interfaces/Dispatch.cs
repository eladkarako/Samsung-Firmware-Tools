using System;
using System.Collections.Generic;
using System.Text;

namespace MLFI.Interfaces
{
    public static class Dispatch
    {
        public static event EventHandler<MessageEventArgs> Message;
        public static event EventHandler<EventArgs> MessagePump;
        public static void AddMessage(string pluginName, string message)
        {
            MessageEventArgs e = new MessageEventArgs(pluginName, message);
            if (Message != null)
                Message(null, e);
        }
        public static void AddMessage(string pluginName, string format, params object[] args)
        {
            AddMessage(pluginName, string.Format(format, args));
        }
        public static void Pump()
        {
            if (MessagePump != null)
                MessagePump(null, new EventArgs());
        }
    }
    public class MessageEventArgs : EventArgs
    {
        string _pname;
        string _message;
        public string Plugin
        {
            get { return _pname;}
        }
        public string Message
        {
            get { return _message;}
        }
        public MessageEventArgs(string pluginName, string msg)
        {
            _pname = pluginName;
            _message = msg;
        }
        public override string ToString()
        {
            return string.Format("{0}: {1}", _pname, _message);
        }
    }
}
