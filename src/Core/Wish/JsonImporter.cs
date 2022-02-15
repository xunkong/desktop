using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Xunkong.Core.Wish
{
    public class JsonImporter
    {
        private readonly List<WishlogItem> WishlogItems;

        private readonly JsonSerializerOptions options;

        private int uid;

        public JsonImporter()
        {
            WishlogItems = new();
            options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }


        public List<WishlogItem> Deserialize(string json)
        {
            var baseNode = JsonNode.Parse(json);
            if (baseNode is JsonObject obj)
            {
                var node = obj["uid"];
                if (node is not null)
                {
                    var str = node.ToString();
                    int.TryParse(str, out uid);
                }
            }
            ParseJson(baseNode);
            foreach (var item in WishlogItems)
            {
                if (item.Uid == 0)
                {
                    item.Uid = uid;
                }
                if (item.QueryType == 0)
                {
                    item.QueryType = item.WishType switch
                    {
                        WishType.CharacterEvent_2 => WishType.CharacterEvent,
                        _ => item.WishType,
                    };
                }
            }
            return WishlogItems.Distinct().OrderBy(x => x.Id).ToList();
        }


        private void ParseJson(JsonNode? node)
        {
            if (node is JsonValue value)
            {
                return;
            }

            if (node is JsonObject obj)
            {
                if (obj.ContainsKey("gacha_type")
                    && obj.ContainsKey("time")
                    && obj.ContainsKey("name")
                    && obj.ContainsKey("item_type")
                    && obj.ContainsKey("id"))
                {
                    var data = obj.Deserialize<WishlogItem>(options);
                    if (data != null)
                    {
                        WishlogItems.Add(data);
                    }
                    return;
                }
                else
                {
                    foreach (var property in obj)
                    {
                        ParseJson(property.Value);
                    }
                }
            }

            if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    ParseJson(item);
                }
            }

        }
    }
}
