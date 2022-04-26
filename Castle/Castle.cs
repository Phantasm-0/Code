using System.Text.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Telegram.Bot.Types;
using Nito.AsyncEx;
using System.Threading;
using System.Threading.Tasks;
using System;
namespace Code
{
    public static class Castle{
        static private ConcurrentDictionary<string,SemaphoreSlim> playersLocks = new ();
        static private ConcurrentDictionary<string,SemaphoreSlim> squadLocks = new();
        static private ConcurrentDictionary<string, SemaphoreSlim> guildsLocks = new();
        private static List<string> playersPathes = new();
        private static List<string> guildsPathes = new();
        private static List<string> squadesPathes = new();
        private static List<Guild> guilds = new();
        private static List<Squad> squads = new();
        private static string guildDirectoryPath = "./Castle/Guilds";
        private static string squadsDirectoryPath = "./Castle/Squads";
        private static string playersDirectoryPath = "./Castle/Players";
        private const string jsonPostfix= ".json";

        public static async Task Start(){
            /*await LaunchGuilds();
            await LaunchSquads();*/
        }

        private static async Task LaunchSquads()
        {
            DirectoryInfo info = Directory.CreateDirectory(squadsDirectoryPath);
            FileInfo[] fileInfos = info.GetFiles();
            if(fileInfos.Length > 0){
                foreach(var file in fileInfos){
                   squadesPathes.Add(file.Name);
                }
                foreach (string path in squadesPathes){
                    var semaphoreSlim = squadLocks.GetOrAdd(path, new SemaphoreSlim(1, 1));
                    await semaphoreSlim.WaitAsync();
                    try{
                        Squad squad = JsonSerializer.Deserialize<Squad>(System.IO.File.ReadAllText(path),new JsonSerializerOptions());
                        squads.Add(squad);
                    }
                    catch(Exception exception){
                        Console.WriteLine($"{exception.Message}");
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                }
            }
        }

        private static async Task  LaunchGuilds()
        {
            DirectoryInfo info = Directory.CreateDirectory(guildDirectoryPath);
            FileInfo[] fileInfos = info.GetFiles();
            if(fileInfos.Length > 0){
                foreach(var file in fileInfos){
                   guildsPathes.Add(file.Name);
                }
                foreach (string path in guildsPathes){
                    var semaphoreSlim = guildsLocks.GetOrAdd(path, new SemaphoreSlim(1, 1));
                    await semaphoreSlim.WaitAsync();
                    try{
                        Guild guild  = JsonSerializer.Deserialize<Guild>(System.IO.File.ReadAllText(path),new JsonSerializerOptions());
                        guilds.Add(guild);
                    }
                    catch(Exception exception){
                        Console.WriteLine($"{exception.Message}");
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                }
            }
        }
            
    
        public static async Task WriteMembers(Player stranger,long userId){
            var semaphoreSlim = playersLocks.GetOrAdd(userId.ToString() + jsonPostfix, new SemaphoreSlim(1, 1));
            await semaphoreSlim.WaitAsync();
            try
            {
                await System.IO.File.WriteAllTextAsync(playersDirectoryPath + "/" + userId + jsonPostfix,JsonSerializer.Serialize(stranger));
            }
            catch(Exception exception)
            {
                Console.WriteLine($"{exception.Message}");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        public static async Task<List<Player>> GetPlayers(){
            DirectoryInfo info = Directory.CreateDirectory(playersDirectoryPath);
            FileInfo[] fileInfos = info.GetFiles();
            if(fileInfos.Length > 0){
                foreach(var file in fileInfos){
                   playersPathes.Add(file.Name);
                }
                List<Player> players = new();
                foreach (string path in playersPathes){
                    var semaphoreSlim = playersLocks.GetOrAdd(path, new SemaphoreSlim(1, 1));
                    await semaphoreSlim.WaitAsync();
                    try{
                        Player player  = JsonSerializer.Deserialize<Player>(System.IO.File.ReadAllText(playersDirectoryPath + "/" + path),new JsonSerializerOptions());
                        players.Add(player);
                    }
                    catch(Exception exception){
                        Console.WriteLine($"{exception.Message}");
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                }
                return players;
            }
            return null;
        }
        public static async Task<Player> GetPlayer(long userId){
            DirectoryInfo info = Directory.CreateDirectory(playersDirectoryPath);
            FileInfo[] fileInfos = info.GetFiles();
            if(fileInfos.Length > 0){
                foreach(var file in fileInfos){
                   if(file.Name == userId +jsonPostfix)
                   {
                        var semaphoreSlim = playersLocks.GetOrAdd(file.Name, new SemaphoreSlim(1, 1));
                        await semaphoreSlim.WaitAsync();
                        try{
                            Player player  = JsonSerializer.Deserialize<Player>(System.IO.File.ReadAllText(playersDirectoryPath + "/" + file.Name),new JsonSerializerOptions());
                            return player;
                        }
                        catch(Exception exception){
                            Console.WriteLine($"{exception.Message}");
                        }
                        finally
                        {
                            semaphoreSlim.Release();
                        }
                    } 
                }
            }
            return null;
        } 
    }
}