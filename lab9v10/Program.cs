using System;
using System.Collections.Generic;

namespace Lab9
{
    abstract class MessageSender
    {
        public string ChatId { get; set; } = "";
        public string Message { get; set; } = "";

        public abstract void SendMessage(string chatId, string message);
    }

    class TelegramSender : MessageSender
    {
        public bool IsChannel { get; set; }

        public override void SendMessage(string chatId, string message)
        {
            if (string.IsNullOrEmpty(chatId) || !chatId.StartsWith("@"))
            {
                throw new Exception("ChatID для Telegram повинен починатися з '@'");
            }

            Console.WriteLine($"[Telegram] До {chatId}: {message}");
        }
    }

    class WhatsAppSender : MessageSender
    {
        public bool IsBusiness { get; set; }

        public override void SendMessage(string chatId, string message)
        {
            if (string.IsNullOrEmpty(chatId) || !chatId.StartsWith("+"))
            {
                throw new Exception("Номер для WhatsApp повинен починатися з '+'");
            }

            Console.WriteLine($"[WhatsApp] До {chatId}: {message}");
        }
    }

    class SlackSender : MessageSender
    {
        public string Workspace { get; set; } = "MainWorkspace";

        public override void SendMessage(string chatId, string message)
        {
            if (string.IsNullOrEmpty(chatId) || (!chatId.StartsWith("#") && !chatId.StartsWith("@")))
            {
                throw new Exception("ChatID для Slack повинен починатися з '#' або '@'");
            }

            Console.WriteLine($"[Slack] До {chatId}: {message}");
        }
    }

    class NotificationService
    {
        public void SendAll(List<MessageSender> items)
        {
            Console.WriteLine("Початок відправки повідомлень:\n");

            foreach (var item in items)
            {
                try
                {
                    item.SendMessage(item.ChatId, item.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка ({item.GetType().Name}): {ex.Message}");
                }
            }

            Console.WriteLine("\nВідправку завершено.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<MessageSender> senders = new List<MessageSender>
            {
                new TelegramSender { ChatId = "@student_group", Message = "Привіт! Лабораторна готова." },
                new WhatsAppSender { ChatId = "+380971234567", Message = "Код підтвердження: 5544" },
                new SlackSender { ChatId = "general-chat", Message = "Повідомлення без знака #" }, // Викличе помилку
                new SlackSender { ChatId = "#general", Message = "Всім привіт у Slack!" },
                new TelegramSender { ChatId = "wrong_chat_id", Message = "Ще одна помилка" } // Викличе помилку
            };

            NotificationService service = new NotificationService();
            service.SendAll(senders);
        }
    }
}