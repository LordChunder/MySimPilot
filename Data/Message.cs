using System;
using MySimPilot.SimConnect;

namespace MySimPilot.Data
{
    public class Message
    {
        public Message(string body, DateTime time, MessageType type)
        {
            Body = $"({time.ToShortTimeString()}) {body}";
            Type = type;

            switch (Type)
            {
                case MessageType.Error:
                {
                    TextColor = "Red";
                    break;
                }
                case MessageType.Alert:
                {
                    TextColor = "Yellow";
                    break;
                    
                }
            }
        }

        public string Body { get; set; }
        private MessageType Type { get; set; }

        public string TextColor { get; set; }
    }
}