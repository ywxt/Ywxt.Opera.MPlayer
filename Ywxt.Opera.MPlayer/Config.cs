using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ywxt.Opera.MPlayer
{
    public class Config
    {
        public int Volume { get; set; } = 50;

        public string CurrentFile { get; set; }

        public int CurrentPosition { get; set; }

        public static Config Load()
        {
            if (!File.Exists("config.json")) return new Config();
            var file = File.ReadAllBytes("config.json");
            return JsonSerializer.Deserialize<Config>(file);
        }

        public void Save()
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(
                this,
                new JsonSerializerOptions {IgnoreNullValues = true}
            );
            File.WriteAllBytes("config.json", json);
        }
    }
}