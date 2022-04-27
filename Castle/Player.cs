using System.Text.Json.Serialization;
using Telegram.Bot.Types;

namespace Code
{
    public class Player
    {
        public long UserId{get;set;}
        public string CwName{get;set;} 
        public int Level{get;set;}
        public string GuildTag{get;set;}
        public Pings PingSettings{get;set;}
        public int Stamina{get;set;} 
        public long Experience{get;set;}
        public int HitPoints{get;set;}
        
        public int Attack {get;set;}
        public int Defend{get;set;}
        public int Gold{get;set;}
        public string CastleEmote{get;set;}

        public string Username{get;set;}



        /*[JsonConstructor]
        public Player(long userid,string cwName,int level,string profile, Pings pingSettings = Pings.OnAll,  string guildTag = null){
            UserId = userid;
            CwName = cwName;
            Level = level;
            GuildTag = guildTag;
            PingSettings = pingSettings;
            Profile = profile;

        }*/
    }
}