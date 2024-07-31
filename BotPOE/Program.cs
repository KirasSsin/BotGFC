using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using BotLogic;

namespace BotPOE
{
    internal class Program
    {
        private static System.Timers.Timer? myTimer;

        static async Task Main(string[] args)
        {
            myTimer = new System.Timers.Timer(600000);

            Console.WriteLine("Запущен бот " + BasicLogic.bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();

            BasicLogic.bot.StartReceiving(
                updateHandler: BasicLogic.HandleUpdateAsync,
                pollingErrorHandler: BasicLogic.HandlePollingErrorAsync,
                receiverOptions: new ReceiverOptions
                {
                    AllowedUpdates = { }, // receive all update types
                },
                cancellationToken: cts.Token);

            Console.ReadLine();
            cts.Cancel();
        }
    }
}
