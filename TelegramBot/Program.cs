using System;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ApiAiSDK;

namespace TelegramBot
{
    class Program
    {
        static TelegramBotClient Bot;
        static ApiAi apiAi;

        static void Main(string[] args)
        {
            Bot = new TelegramBotClient("815864992:AAEwhTd-g9pmUwEnmxKXWGYYbySevJsVP_E");
            AIConfiguration config = new AIConfiguration("e2bb41f502504e878b8eb7c75ea9f4c3", SupportedLanguage.Russian);
            apiAi = new ApiAi(config);

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallBackQueryReceived;

            var me = Bot.GetMeAsync().Result;

            Console.WriteLine(me.FirstName);

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnCallBackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string buttonText = e.CallbackQuery.Data;
            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}";

            Console.WriteLine($"{name} pressed button {buttonText}");

            try
            {
                if (buttonText == "картинка")
                {
                    await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "https://vk.com/borsch?z=photo-460389_457742583%2Falbum-460389_00%2Frev");
                }
                else if (buttonText == "видео")
                {
                    await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "https://www.youtube.com/watch?v=WNeLUngb-Xg&list=RDWNeLUngb-Xg&start_radio=1");
                }

                await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, $"You pressed button {buttonText}");

            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private static async void BotOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;

            if (message == null || message.Type != MessageType.Text)
            {
                return;
            }

            string name = $"{message.From.FirstName} {message.From.LastName}";

            Console.WriteLine($"{name} sent message: {message.Text}");

            switch (message.Text)
            {
                case "/start":
                    {
                        string text =
                           @"List of commands:
 /start - launch bot;
 /inline - show menu;
 /keyboard - show keyboard;";

                        await Bot.SendTextMessageAsync(message.From.Id, text);

                        break;
                    }
                case "/keyboard":
                    {
                        var replyKeyboard = new ReplyKeyboardMarkup(new[]
                        { 
                            new[]
                            { 
                                new KeyboardButton("Привет!"),
                                new KeyboardButton("Как дела?")
                            },                             
                            new[]
                            {
                                new KeyboardButton("Контакт") {RequestContact = true },
                                new KeyboardButton("Геолокация") {RequestLocation = true }
                            }
                        });

                        await Bot.SendTextMessageAsync(message.Chat.Id, "Message",
                            replyMarkup: replyKeyboard);

                        break;
                    }
                case "/inline":
                    {
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithUrl("VK", "https://vk.com/id11833696"),
                                InlineKeyboardButton.WithUrl("Telegram", "https://t.me/trakhimovichilya")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("картинка"),
                                InlineKeyboardButton.WithCallbackData("видео")
                            }
                        });

                        await Bot.SendTextMessageAsync(message.From.Id, "Select menu",
                            replyMarkup: inlineKeyboard);

                        break;
                    }
                default:
                    {
                        var response = apiAi.TextRequest(message.Text);
                        string answer = response.Result.Fulfillment.Speech;

                        if (answer == "")
                        {
                            answer = "Прости, я тебя не понял. Я тупой еще :(.";
                        }

                        await Bot.SendTextMessageAsync(message.From.Id, answer);

                        break;
                    }                   
            }
        }
    }
}
