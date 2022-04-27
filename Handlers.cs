
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
namespace Code{
    static class Handlers{
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
             var handler = update.Type switch
        {
            UpdateType.Unknown             => HandleErrorAsync(botClient,new Exception("Unknown"), cancellationToken),
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message              => HandleMessage(botClient, update.Message!,cancellationToken),
            //UpdateType.EditedMessage      => BotOnMessageReceived(botClient, update.EditedMessage!),
            UpdateType.CallbackQuery        => HandleCallback(botClient, update.CallbackQuery!),
            //UpdateType.InlineQuery        => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
            //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
            _                               => HandleErrorAsync(botClient,new Exception("Unknown"), cancellationToken),
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cancellationToken);
        }
    }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                System.Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
}

        static async Task HandleCallback(ITelegramBotClient bot, CallbackQuery callbackQuery){
            MobManager.MobUpdate(bot,callbackQuery);
        }


        static async Task HandleMessage(ITelegramBotClient bot, Message message, CancellationToken cancellationToken){

            var handler = message.Type switch
            {
            MessageType.Text     => HandleTextMessage(bot,message),
            MessageType.Unknown  => HandleErrorAsync(bot, new Exception("MessageType Unknown"),cancellationToken),
            _                    => HandleErrorAsync(bot, new Exception("MessageType Unknown"),cancellationToken)
            };
            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(bot, exception, cancellationToken);
            }
        }
        static async Task HandleTextMessage(ITelegramBotClient botClient, Message message)
        {
            if(message?.Text[0] =='/'){
                await HandleCommand(botClient,message);
                return;
            }
            Regex fight = new Regex("/fight.*");
            Regex metext = new("^🏅Me$");
            Regex proof = new Regex("🏛Class info");
            if(fight.IsMatch(message?.Text)){
                MatchCollection matchCollection = fight.Matches(message?.Text);
                if(matchCollection.Count > 0 && (message.ForwardFrom.Username == "ChatWarsBot" || message.ForwardFrom.Username == "ChatWarsEliteBot")){
                    HandleMob(botClient,message, matchCollection[0].ToString().Trim());
                }
            }
            if(proof.IsMatch(message.Text) && (message.ForwardFrom.Username == "ChatWarsBot" || message.ForwardFrom.Username == "ChatWarsEliteBot") && message.Chat.Type == ChatType.Private){
                await HandleProfile(message);
                }
            if(metext.IsMatch(message.Text) && message.Chat.Type == ChatType.Private){
                await SendProfile(botClient,message.Chat.Id,message.From.Id);
            }
        }
        private static async Task HandleCommand(ITelegramBotClient botClient, Message message)
        {
            string text = message.Text;
            User userBot = await botClient.GetMeAsync();
            Regex meCommand = new Regex("^/me$");
            Regex meCommandForChats = new($"^/me@{userBot.Username}");
            Regex metext = new("^🏅Me$");
            Regex start = new("^/start$");

            if((meCommand.IsMatch(text) || meCommandForChats.IsMatch(text))){
                await SendProfile(botClient,message.Chat.Id,message.From.Id);
            }
            if(start.IsMatch(text) && message.Chat.Type == ChatType.Private){
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                        text: "Hello there. How did you get through the storm?🌩\n\n🏅Send me your /hero",
                                                        replyMarkup: ReturnBotsButtons());
            }
        }

        private static ReplyKeyboardMarkup ReturnBotsButtons(){
            KeyboardButton button = new KeyboardButton("🏅Me");
            ReplyKeyboardMarkup markup = new(button);
            markup.ResizeKeyboard = true;
            return markup;
        }

        private static async Task SendProfile(ITelegramBotClient botClient,long chatId,long userId)
        {
            Player player = await Castle.GetPlayer(userId);
            if(player == null){
                await botClient.SendTextMessageAsync(chatId,"I dont know u");
                return;
            }
            string answer = $"{player.CastleEmote}{player.CwName} of {GetCastleName(player.CastleEmote)}\n🏅Level: {player.Level}\n⚔️Atk: {player.Attack} 🛡Def: {player.Defend}\n🔥Exp: {player.Experience}\n🔋Stamina: {player.Stamina}\n";
            await botClient.SendTextMessageAsync(chatId,answer);
            return;
        }

        private static string GetCastleName(string castleEmote){
            return castleEmote switch
            {
                "🌩" =>"Ventus" ,
                "🪨"   => "Solo" ,
                "❄️" =>"Glacies",
                "🔥" => "Ignis",
                "💧"=>"Aqua",
                _ =>"?"
            };
        }
        static async Task HandleProfile(Message message)
        {
            string text = message.Text;
            Regex digit = new Regex("\\d{1,3}");
            Regex lastDigit = new Regex("\\d{1,3}",RegexOptions.RightToLeft);

            Regex nameAndCastleRegex= new(".*");
            string nameAndCastleSubString = nameAndCastleRegex.Match(text).Value;

            Regex nameRegexWithEndStroke = new("\\w{1,50}$");
            Regex nameRegex = new("\\w{1,50}");
            string temp = nameRegexWithEndStroke.Match(nameAndCastleSubString).Value;
            string name = nameRegex.Match(nameRegexWithEndStroke.Match(nameAndCastleSubString).Value).Value;

            Regex castleRegex = new("^.\\S");
            string castle = castleRegex.Match(nameAndCastleSubString).Value;

            Regex levelRegex = new(".Level.*");
            string levelSubString = levelRegex.Match(text).Value;
            int level = Int32.Parse(digit.Match(levelSubString).Value);

            Regex statsRergex = new(".Atk.*");
            string statsSubString = statsRergex.Match(text).Value;

            Regex attackRegex = new Regex(".Atk. \\d{1,3}");
            string attackSubstring = attackRegex.Match(statsSubString).Value;
            int attackPoints = int.Parse(digit.Match(attackSubstring).Value);

            Regex defendkRegex = new Regex(".Def. \\d{1,3}");
            string defSubString = defendkRegex.Match(statsSubString).Value;
            int defendPoints = int.Parse(digit.Match(defSubString).Value);

            Regex experienceRegex  = new Regex(".Exp. \\d{1,50}");
            string experienceSubString = experienceRegex.Match(text).Value;
            long experiencePoints = long.Parse(digit.Match(experienceSubString).Value);

            Regex staminaRegex  = new Regex(".Stamina.*");
            string staminaSubString = staminaRegex.Match(text).Value;
            int staminaePoints = int.Parse(lastDigit.Match(staminaSubString).Value);

            
            Regex guildRegex = new("\\[\\w{1,3}\\]");
            string guildTag = null;
            if(guildRegex.IsMatch(text)){
                Regex textRegex = new Regex("\\w{1,3}");
                string guildSubString = guildRegex.Match(text).Value;
                guildTag = textRegex.Match(guildSubString).Value;
            }

            Player user = new Player{UserId = message.From.Id,
                                    CwName = name,
                                    CastleEmote = castle,
                                    Level = level,
                                    Attack = attackPoints,
                                    Defend = defendPoints,
                                    Experience = experiencePoints,
                                    GuildTag = guildTag,
                                    PingSettings = Pings.OnAll,
                                    Username = message.From.Username,
                                    Stamina = staminaePoints                             
                                    };
            await Castle.WriteMembers(user,message.From.Id);
        }
        static async Task HandleMob(ITelegramBotClient botClient, Message message, string link){
            await MobManager.AddAndStart(botClient,message,link);
        }
    }
}