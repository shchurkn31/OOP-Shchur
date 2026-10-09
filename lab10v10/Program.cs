using System;
using System.Collections.Generic;

namespace Lab10
{
    interface IStorage
    {
        void Save(string key, string value);
        string Load(string key);
    }

    class LocalStorage : IStorage
    {
        private Dictionary<string, string> data = new Dictionary<string, string>();

        public void Save(string key, string value)
        {
            data[key] = value;
            Console.WriteLine($"[LocalStorage] Збережено: {key} = {value}");
        }

        public string Load(string key)
        {
            if (data.TryGetValue(key, out string val))
            {
                return val;
            }
            return "Не знайдено";
        }
    }

    class SessionStorage : IStorage
    {
        private Dictionary<string, string> sessionData = new Dictionary<string, string>();

        public void Save(string key, string value)
        {
            sessionData[key] = value;
            Console.WriteLine($"[SessionStorage] Збережено: {key} = {value}");
        }

        public string Load(string key)
        {
            if (sessionData.TryGetValue(key, out string val))
            {
                return val;
            }
            return "Сесія порожня";
        }
    }

    abstract class MessageSender
    {
        public string Target { get; set; } = "";

        public abstract void Connect();
        public abstract void Send(string message);

        public void Disconnect()
        {
            Console.WriteLine($"Відключено від {Target}\n");
        }
    }

    class EmailSender : MessageSender
    {
        public string Email { get; set; } = "";

        public override void Connect()
        {
            Target = Email;
            Console.WriteLine($"[Email] Підключення до {Email}...");
        }

        public override void Send(string message)
        {
            Console.WriteLine($"[Email] Лист на {Email}: \"{message}\"");
        }
    }

    class SmsSender : MessageSender
    {
        public string Phone { get; set; } = "";

        public override void Connect()
        {
            Target = Phone;
            Console.WriteLine($"[SMS] Підключення до {Phone}...");
        }

        public override void Send(string message)
        {
            Console.WriteLine($"[SMS] Повідомлення на {Phone}: \"{message}\"");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<IStorage> storages = new List<IStorage>
            {
                new LocalStorage(),
                new SessionStorage()
            };

            foreach (var storage in storages)
            {
                storage.Save("token", "12345");
                Console.WriteLine($"Зчитано: {storage.Load("token")}");
            }

            Console.WriteLine();

            List<MessageSender> senders = new List<MessageSender>
            {
                new EmailSender { Email = "user@mail.com" },
                new SmsSender { Phone = "+380501112233" }
            };

            foreach (var sender in senders)
            {
                sender.Connect();
                sender.Send("Тестове повідомлення");
                sender.Disconnect();
            }
        }
    }
}