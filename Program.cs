using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Args;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Requests;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Code
{
    
    class Program
    {       static async Task  Main(string[] args){
        
            string token = "1125612607:AAG4o5Myw3TB8ZnYfBbnRMZ0AdW_YG1EVMQ";
            string worktoken = "5323850623:AAGmIQ03d9otOuQdP6z9mpsoU2ocu5yHuOc";
            var bot = new TelegramBotClient(worktoken);
            var cancellationTokenSource = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions(){
                AllowedUpdates = {}
            };         
            bot.StartReceiving(Handlers.HandleUpdateAsync, Handlers.HandleErrorAsync, receiverOptions,cancellationTokenSource.Token);
            Console.ReadLine();
            cancellationTokenSource.Cancel();
        }
            

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
    }
}

