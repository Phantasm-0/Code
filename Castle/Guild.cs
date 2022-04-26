using System.Text.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Telegram.Bot.Types;
using Nito.AsyncEx;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Code{
    public class Guild{
        public List<Player> Players{get;set;}
        
        public string GuildTag{init;get;}

        public Guild (List<Player> players, string guildTag){
            Players = players;
            GuildTag = guildTag;
        }
    }


}