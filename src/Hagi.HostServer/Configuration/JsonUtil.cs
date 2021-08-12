namespace Hagi.HostServer.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class JsonUtil
    {
        public static string ToJson(this object obj)
        {
            using StringWriter writer = new StringWriter();
            JsonSerializer.CreateDefault().Serialize(writer, obj);
            return writer.ToString();
        }

        /// <summary>
        /// Deserialise some json into a flat dictionary, with dot-seperated keys.
        /// </summary>
        public static Dictionary<string, object?> JsonToDict(string json, Dictionary<string, object?>? existingDictionary = null, string? prefix = null)
        {
            Dictionary<string, object?> dict = existingDictionary ?? new Dictionary<string, object?>();

            string[] currentPath = prefix == null
                ? Array.Empty<string>()
                : new[] { prefix };

            JToken token = JToken.Parse(json);
            JsonUtil.JTokenToDict(dict, currentPath, token);

            return dict;
        }

        /// <summary>
        /// Add a token to a dictionary. Arrays and objects will add many items.
        /// </summary>
        private static void JTokenToDict(IDictionary<string, object?> dict, string[] currentPath, JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    int index = 0;
                    foreach (JToken child in token.Children())
                    {
                        JsonUtil.JTokenToDict(dict, currentPath.Append(index++.ToString()).ToArray(), child);
                    }

                    break;

                case JTokenType.Object:
                    foreach (JProperty property in token.Children<JProperty>())
                    {
                        JsonUtil.JTokenToDict(dict, currentPath.Append(property.Name).ToArray(), property.Value);
                    }

                    break;

                default:
                    string key = string.Join('.', currentPath);
                    dict[key] = ((JValue)token).Value;
                    break;
            }
        }
    }
}