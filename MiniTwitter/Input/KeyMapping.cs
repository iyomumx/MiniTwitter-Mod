using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using MiniTwitter.Input;

namespace MiniTwitter.Input
{
    [Serializable]
    public class KeyMapping : IEquatable<KeyMapping>
    {
        public string Name { get; set; }

        public KeyBinding[] KeyBindings { get; set; }

        private static readonly Dictionary<string, KeyMapping> keyMappings = new Dictionary<string, KeyMapping>();

        public static Dictionary<string, KeyMapping> KeyMappings
        {
            get { return KeyMapping.keyMappings; }
        }

        public static KeyMapping GetKeyMapping(int index)
        {
            return KeyMappings.ElementAt(index).Value;
        }

        public static void LoadFrom(string directory)
        {
            foreach (var file in Directory.GetFiles(directory, "*.xml"))
            {
                using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
                {
                    var keyMapping = Serializer<KeyMapping>.Deserialize(stream);
                    keyMappings.Add(keyMapping.Name, keyMapping);
                }
            }
        }

        public bool Equals(KeyMapping other)
        {
            return this.Name == other.Name;
        }
    }
}
