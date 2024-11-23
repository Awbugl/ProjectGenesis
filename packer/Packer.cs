using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;

namespace ProjectGenesis
{
    internal static class Packer
    {
        private const string SolutionPath = @"D:\Git\ProjectGenesis";

        internal static void Main()
        {
            string releasePath = Path.Combine(SolutionPath, "release");

            foreach (string path in Directory.GetFiles(releasePath, "*.zip").Where(File.Exists)) File.Delete(path);

            File.WriteAllText(Path.Combine(releasePath, "manifest.json"),
                JsonConvert.SerializeObject(new ManifestObject(), Formatting.Indented));

            var zipName = $"GenesisBook-v{ProjectGenesis.VERSION}{ProjectGenesis.DEBUGVERSION}.zip";

            string archive = Path.Combine(SolutionPath, zipName);

            if (File.Exists(archive)) File.Delete(archive);

            ZipFile.CreateFromDirectory(releasePath, archive);

            File.Move(archive, Path.Combine(releasePath, zipName));

            Process.Start("explorer", releasePath);
        }
    }

    public class ManifestObject
    {
        [JsonProperty("name")] public string Name { get; set; } = "GenesisBook";

        [JsonProperty("version_number")] public string VersionNumber { get; set; } = ProjectGenesis.VERSION;

        [JsonProperty("website_url")] public string WebsiteURL { get; set; } = "https://github.com/Awbugl/ProjectGenesis";

        [JsonProperty("description")]
        public string Description { get; set; } = "构建真实宇宙，撰写创世之书。新矿物，新材料，新配方，新科技，新机制。Construct Real Universe. Then leave a GenesisBook. New vein, new material, new recipe, new technology, new structure.";

        [JsonProperty("dependencies")]
        public string[] Dependencies { get; set; } = { "CommonAPI-CommonAPI-1.6.5", "nebula-NebulaMultiplayerModApi-2.0.0", };
    }
}
