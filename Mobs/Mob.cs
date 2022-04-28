using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Concurrent;
namespace Code{

    public class Mob {
        Message cwMessage;
        private string link;
        private int TIME_FOR_TIMER = 180;  //180
        private InlineKeyboardMarkup inlineKeyboardMarkup;

        private LinkedList<User> helpers = new();
        private int averageMobLvl;
        public List<long> chatsForSending = new();
        DateTime endTime;

        private const int allowableDifference = 10;

        public Mob(Message message, string _link){
            link = _link;
            cwMessage = message;
            inlineKeyboardMarkup = CreateInlineKeyboardMarkup();
            DateTime forwardDate = (DateTime)cwMessage.ForwardDate;
            endTime = forwardDate.AddSeconds(TIME_FOR_TIMER);
            averageMobLvl = CalculateAverangeMobLevel();
            chatsForSending.Add(message.Chat.Id);

        }

        public async Task Start(ITelegramBotClient bot,long chatId){ 
            chatsForSending.Add(chatId); 
            Message myMessage = await bot.SendTextMessageAsync(chatId: chatId, 
                                                        text:MessageText(GetTimeDelta()),
                                                        parseMode: ParseMode.Html,
                                                        replyMarkup: inlineKeyboardMarkup);
            await Task.Delay(3000 * MobManager.mobs.Count +1);
            await Updating(bot,chatId,myMessage.MessageId);
            return;
        }
        private async Task Updating(ITelegramBotClient bot,long chatId,int messageId){
            List<int> listForClear = new(){messageId};
            List<int> pingsMessages = await Ping(bot,chatId);
            if(pingsMessages != null){
                foreach(int temp in pingsMessages){
                    listForClear.Add(temp);
                }
            }
            while((GetTimeDelta())> 0){
                try
                {
                    Message myMessage = await bot.EditMessageTextAsync(chatId,messageId,
                                                            MessageText(GetTimeDelta()),
                                                            ParseMode.Html, 
                                                            replyMarkup: inlineKeyboardMarkup);
                    await Task.Delay(2000 * MobManager.mobs.Count);
                }
                catch (System.Exception exception)
                {
                    var ErrorMessage = exception switch
                    {
                        //заменить на хендлер потом
                    ApiRequestException apiRequestException
                                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _                             => exception.ToString()
                    };
                    System.Console.WriteLine(ErrorMessage);
                    await Task.Delay(3000* MobManager.mobs.Count + 1);
                }
            }
            Clear(bot,chatId,listForClear);
            return;
        }
        private void Clear(ITelegramBotClient bot,long chatId,List<int> messageIds){
            foreach(int messageId in messageIds){
                bot.DeleteMessageAsync(chatId,messageId);
            }
            return;
        }
        private string MessageText(double time){
            Regex messageWithoutRef = new Regex(".*\n");
            MatchCollection matchCollection = messageWithoutRef.Matches(cwMessage.Text);
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var str in matchCollection){
                stringBuilder.Append(str.ToString());
            }
            stringBuilder.Append(string.Format("<b>⏰: {0:d2}:{1:d2}\n\n🌪 Windwalkers:{2}</b>\n",(int)time /60,(int)time % 60,GetHelpers()));
            return stringBuilder.ToString();;
        }
        private InlineKeyboardMarkup CreateInlineKeyboardMarkup(){
            InlineKeyboardButton linkButton = new InlineKeyboardButton("⚔️ Fight");
            linkButton.Url ="https://t.me/share/url?url=" + link;
            InlineKeyboardButton helpButton = InlineKeyboardButton.WithCallbackData("🤝 Helping",link);
            LinkedList<InlineKeyboardButton> row = new LinkedList<InlineKeyboardButton>();
            row.AddLast(linkButton);
            row.AddLast(helpButton);
            return new InlineKeyboardMarkup(inlineKeyboardRow: row);
        }
        private double GetTimeDelta(){
            TimeSpan timeSpan = endTime - DateTime.UtcNow;
            return timeSpan.TotalSeconds;
        }
        private string GetHelpers(){
            StringBuilder stringBuilder = new();
            int counter = 0;
            foreach(User helper in helpers){
                counter++;
                stringBuilder.Append($"\n<b>{counter})</b>{helper?.FirstName} (@{helper?.Username})");
            };
            return stringBuilder.ToString();
        }
        public bool UpdateHelpers(User user){
            foreach (var helper in helpers)
            {
                if(helper.Id  == user.Id){
                    return false;
                }
            }
            helpers.AddLast(user);
            return true;
        }
        async Task<List<int>> Ping(ITelegramBotClient bot,long chatId){
            List<Player> players = await Castle.GetPlayers();
            List<int> messagesForDelete = new();
            if(players == null){
                return null;
            }
            int counter = 0;
            StringBuilder stringBuilder = new();
            foreach(Player player in players){
                if(Math.Abs(player.Level - averageMobLvl) < allowableDifference){
                    stringBuilder.Append($"@{player.Username ?? string.Empty}");
                }
                counter++;
                if(counter>= 5){
                    Message message = await bot.SendTextMessageAsync(chatId,stringBuilder.ToString());
                    messagesForDelete.Add(message.MessageId);
                    counter = 0;
                    stringBuilder.Clear();
                }
            }
            if(stringBuilder.Length > 0){
                Message finalMessage = await bot.SendTextMessageAsync(chatId,stringBuilder.ToString());
                messagesForDelete.Add(finalMessage.MessageId);
            }
            return messagesForDelete;
        }
        int CalculateAverangeMobLevel(){
            Regex spliter = new Regex(".*\n");
            Regex digitRegex = new Regex("\\d{1,2}");
            Regex mobsCountRegex = new Regex("\\d{1,2} x");
            Regex lvlRegex = new Regex("lvl.\\d{1,2}");
            MatchCollection stringsForCalculate = spliter.Matches(cwMessage.Text);
            int mobCount = 1;
            int mobLvl = 0;
            int counter = 0;
            int summ = 0;
            for(int i = 1; i < stringsForCalculate.Count; i++){
                string temp = stringsForCalculate[i].Value;
                if(temp == "\n"){
                    break;
                }
                if(lvlRegex.IsMatch(temp)){
                    Match mobsLvl = lvlRegex.Match(temp);
                    mobLvl = Int32.Parse(digitRegex.Match(mobsLvl.Value).Value);
                    mobCount = 1;
                }
                if(mobsCountRegex.IsMatch(temp)){
                    Match mobsCount = mobsCountRegex.Match(temp);
                    mobCount = Int32.Parse(digitRegex.Match(mobsCount.Value).Value);
                }
                summ+= mobCount * mobLvl;
                counter+=mobCount;
                mobCount = 0;
            }
            return summ / counter;
        }
    }
}