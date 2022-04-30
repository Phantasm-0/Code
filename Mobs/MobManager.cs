
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Code{
public static class MobManager{
    public static ConcurrentDictionary<string,Mob> mobs = new ConcurrentDictionary<string, Mob>();

    async public static Task AddAndStart(ITelegramBotClient botClient, Message message, string link) {
            Mob mob;
            Regex ambushRegex = new("It's an ambush!");
            if(ambushRegex.IsMatch(message.Text)){
                mob = new Ambush(message,link);
            }
            else{
                mob = new (message, link);
            }
            if(mobs.ContainsKey(link)){
                mobs.TryGetValue(link,out mob);
                if(!mob.chatsForSending.Contains(message.Chat.Id)){
                    await mob.Start(botClient,message.Chat.Id);
                }
            }else
            {
                mobs.TryAdd(link,mob);
                await mob.Start(botClient,message.Chat.Id);
                mobs.TryRemove(link,out mob);
            }
    }
    async public static void MobUpdate(ITelegramBotClient bot, CallbackQuery callbackQuery){
            Mob mob;
            mobs.TryGetValue(callbackQuery.Data, out mob);
            if(mob.UpdateHelpers(callbackQuery.From)){
                await bot.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQuery.Id,
                        text: $"You joined the fight");
            }
            else
            {
                await bot.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQuery.Id,
                        text: $"Cloning is taboo");
            }
    }

    }
}