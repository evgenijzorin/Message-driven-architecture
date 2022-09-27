using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Seminar.Services.impl
{
    public class ConsoleMessager : IMessenger
    {
        public ConsoleMessager()
        {
            // Подключить кодирование текста UTF8
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        public void WriteMessage(string message) 
        {
            Console.Write(message);
        }
        /// <summary>
        /// Write a delayed message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="seconds">number seconds of delay </param>
        public void WriteMessage(string message, int seconds)
        {
            Thread.Sleep(1000 * seconds);
            Console.Write(message);
        }

        public async Task WriteMessageAsync (string message, int seconds)
        {
            await Task.Delay(seconds);
            Console.Write(message);
        }
    }
}
