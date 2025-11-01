using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ProjectGenesis.Utils
{
    internal static class TextureHelper
    {
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        internal static Texture2D GetTexture(string name, string type = "texture")
        {
            if (Cache.TryGetValue(name, out var cached)) return cached;

            using (var stream = Assembly.GetManifestResourceStream($"ProjectGenesis.assets.{type}.{name}.png"))
            {
                if (stream == null) return null;

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    var bytes = memoryStream.ToArray();

                    var texture = new Texture2D(2, 2);
                    if (!texture.LoadImage(bytes)) return null;

                    texture.name = name;
                    Cache[name] = texture;
                    return texture;
                }
            }
        }

        internal static Sprite GetSprite(string name, int? width = null, int? height = null)
        {
            if (!Cache.TryGetValue(name, out var texture)) texture = GetTexture(name, "sprite");

            return Sprite.Create(texture, new Rect(0, 0, width ?? texture.width, height ?? texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
