using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Code{
    class Squad{


        Player Comander{get;set;}
        long ChatId{get;set;}
        Player ViceCommander{get;set;}
        List<Player> Players{get;set;}
        [JsonConstructor]
        public Squad(List<Player> players){
            Players = players;
        }
    }
}