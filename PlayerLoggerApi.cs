using Oxide.Core.Libraries.Covalence;
using Oxide.Core;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("PlayerLoggerAPI", "Ponlponl123", "1.2.0")]
    [Description("Logs player login info via API")]
    public class PlayerLoggerAPI : CovalencePlugin
    {
        private Configuration config;

        private class Configuration
        {
            public string ApiUrl = "http://your-api:3000/v1/internal/player/traffic";
            public string ApiKey = "your-super-secret-key";
        }

        protected override void LoadDefaultConfig()
        {
            config = new Configuration();
            SaveConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            config = Config.ReadObject<Configuration>();

            if (string.IsNullOrEmpty(config.ApiUrl) || string.IsNullOrEmpty(config.ApiKey))
            {
                PrintError("Config values missing. Regenerating default config.");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config, true);
        }

        private void OnUserConnected(IPlayer player)
        {
            if (player == null || !player.IsConnected) return;

            var payload = new Dictionary<string, string>
            {
                ["steamId"] = player.Id,
                ["playerName"] = player.Name,
                ["loginTime"] = System.DateTime.UtcNow.ToString("o")
            };

            var headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["X-API-Key"] = config.ApiKey
            };

            webrequest.Enqueue(
                config.ApiUrl,
                Newtonsoft.Json.JsonConvert.SerializeObject(payload),
                (code, response) =>
                {
                    if (code != 200 || string.IsNullOrEmpty(response))
                    {
                        PrintError($"Failed to send player log: {code} - {response}");
                        return;
                    }
                    Puts($"Logged player {player.Name} successfully.");
                },
                this,
                RequestMethod.POST,
                headers
            );
        }
    }
}