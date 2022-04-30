
using System;
using Telegram.Bot.Types;

namespace Code{
    class Ambush : Mob{

        new int TIME_FOR_TIMER = 300;
        public Ambush(Message message, string _link): base(message,_link)
        {
            link = _link;
            cwMessage = message;
            inlineKeyboardMarkup = CreateInlineKeyboardMarkup();
            averageMobLvl = CalculateAverangeMobLevel();
            chatsForSending.Add(message.Chat.Id);
            DateTime forwardDate = (DateTime)base.cwMessage.ForwardDate;
            base.endTime = forwardDate.AddSeconds(TIME_FOR_TIMER);
        }
    }
}