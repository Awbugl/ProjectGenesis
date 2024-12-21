using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ProjectGenesis
{
    internal static class Packer
    {
        private const string SolutionPath = @"D:\Git\ProjectGenesis";

        private static readonly bool IsDebugBuild = true;

        public const string ManifestDebugVersion = "0.3.11101";

        internal static void Main()
        {
            string releasePath = Path.Combine(SolutionPath, "release");

            foreach (string path in Directory.GetFiles(releasePath, "*.zip").Where(File.Exists)) File.Delete(path);

            var manifestObject = IsDebugBuild ? ManifestObject.DebugObject() : ManifestObject.ReleaseObject();

            File.WriteAllText(Path.Combine(releasePath, "manifest.json"), JsonConvert.SerializeObject(manifestObject, Formatting.Indented));

            var zipName = new StringBuilder(80).Append(manifestObject.Name).Append("-v").Append(ProjectGenesis.VERSION)
               .Append(IsDebugBuild ? ProjectGenesis.DEBUGVERSION : "").Append(".zip").ToString();

            string archive = Path.Combine(SolutionPath, zipName);

            if (File.Exists(archive)) File.Delete(archive);

            ZipFile.CreateFromDirectory(releasePath, archive);

            File.Move(archive, Path.Combine(releasePath, zipName));

            Process.Start("explorer", releasePath);
        }
    }

    public class ManifestObject
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("version_number")] public string VersionNumber { get; set; }

        [JsonProperty("website_url")] public string WebsiteURL { get; set; } = "https://github.com/Awbugl/ProjectGenesis";

        [JsonProperty("description")]
        public string Description { get; set; } =
            "构建真实宇宙，撰写创世之书。新矿物，新材料，新配方，新科技，新机制。Construct Real Universe. Then leave a GenesisBook. New vein, new material, new recipe, new technology, new structure.";

        [JsonProperty("dependencies")]
        public string[] Dependencies { get; set; } = { "CommonAPI-CommonAPI-1.6.5", "nebula-NebulaMultiplayerModApi-2.0.0", };

        internal static ManifestObject DebugObject()
        {
            return new ManifestObject
            {
                Name = "GenesisBook_Experimental", VersionNumber = Packer.ManifestDebugVersion,
            };
        }

        internal static ManifestObject ReleaseObject()
        {
            return new ManifestObject
            {
                Name = "GenesisBook", VersionNumber = ProjectGenesis.VERSION,
            };
        }
    }
}
