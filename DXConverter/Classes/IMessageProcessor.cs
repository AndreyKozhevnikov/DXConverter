using System;
using System.Linq;

namespace DXConverter {

    public interface IMessageProcessor {
        void SendMessage(string message);
        void SendMessage(string message, ConsoleColor color);
    }
    public class ConsoleMessageProcessor : IMessageProcessor {
        public ConsoleMessageProcessor() {
            tmpDT = DateTime.Now;
        }
        DateTime tmpDT;
        public void SendMessage(string message, ConsoleColor color) {
            var dt = DateTime.Now - tmpDT;
            message = string.Format("{0} {1}", message, dt.ToString(@"ss\:fff"));
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        public void SendMessage(string message) {
            Console.WriteLine(message, null);
        }
    }
}
