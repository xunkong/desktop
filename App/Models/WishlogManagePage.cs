using MiniExcelLibs.Attributes;
using System.Text.Json.Serialization;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.Models;

internal class WishlogManagePage_ImportItem
{

    [ExcelColumnName("uid")]
    [JsonPropertyName("uid"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Uid { get; set; }

    [ExcelColumnName("gacha_type")]
    [JsonPropertyName("gacha_type"), JsonConverter(typeof(WishTypeJsonConverter))]
    public WishType WishType { get; set; }

    [JsonIgnore]
    public DateTimeOffset Time => GetTime();

    [ExcelColumnName("time")]
    [JsonInclude, JsonPropertyName("time")]
    public string _TimeString { get; set; }

    [ExcelColumnName("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [ExcelColumnName("lang")]
    [JsonPropertyName("lang")]
    public string Language { get; set; }

    [ExcelColumnName("item_type")]
    [JsonPropertyName("item_type")]
    public string ItemType { get; set; }

    [ExcelColumnName("rank_type")]
    [JsonPropertyName("rank_type"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int RankType { get; set; }

    [ExcelColumnName("id")]
    [JsonPropertyName("id"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Id { get; set; }

    public WishType QueryType => WishType switch
    {
        WishType.CharacterEvent_2 => WishType.CharacterEvent,
        _ => WishType,
    };

    public DateTimeOffset GetTime()
    {
        var offset = Uid.ToString()[0] switch
        {
            '6' => -5,
            '7' => 1,
            _ => 8,
        };
        if (DateTime.TryParse(_TimeString, out var time))
        {
            return new DateTimeOffset(time, TimeSpan.FromHours(offset));
        }
        else
        {
            return default;
        }
    }

}

internal class WishlogManagePage_ExcelExport_Origin
{

    public string Count { get; set; } = "1";

    public string WishType { get; set; }

    public string Id { get; set; }

    public string ItemId { get; set; }

    public string ItemType { get; set; }

    public string Language { get; set; }

    public string Name { get; set; }

    public string RankType { get; set; }


    public DateTimeOffset Time;

    public string TimeString => Time.ToString("yyyy-MM-dd HH:mm:ss");

    public string Uid { get; set; }


    public string QueryType => WishType switch
    {
        "400" => "301",
        _ => WishType,
    };


}

internal class WishlogManagePage_ExcelExport_ByType
{
    public string WishType { get; set; }

    public string Id { get; set; }

    public string ItemType { get; set; }

    public string Name { get; set; }

    public string RankType { get; set; }

    public string Index { get; set; }

    public string GuaranteeIndex { get; set; }

    public string VersionAndName { get; set; }


    public DateTimeOffset Time;

    public string TimeString => Time.ToString("yyyy-MM-dd HH:mm:ss");


}

internal class WishlogManagePage_JsonExport
{

    [JsonPropertyName("info")]
    public WishlogManagePage_JsonExport_Info Info { get; set; }


    [JsonPropertyName("list")]
    public List<WishlogManagePage_JsonExport_Item> List { get; set; }


    public void InitializeInfo()
    {
        if (!(List?.Any() ?? false))
        {
            throw new NullReferenceException("No wishlog item in list.");
        }
        var uid = List.First().Uid;
        if (uid == 0)
        {
            throw new NullReferenceException("Uid is 0.");
        }
        if (List.Any(x => x.Uid != uid))
        {
            throw new ArgumentOutOfRangeException("More than one uid in list.");
        }
        var lang = List.FirstOrDefault()?.Language;
        if (string.IsNullOrWhiteSpace(lang))
        {
            throw new NullReferenceException("Lang is null.");
        }
        if (List.Any(x => x.Language != lang))
        {
            throw new ArgumentOutOfRangeException("More than one lang in list.");
        }
        var time = DateTimeOffset.Now;
        var version = XunkongEnvironment.AppVersion;
        var info = new WishlogManagePage_JsonExport_Info
        {
            Uid = uid,
            Language = lang,
            ExportTime = time.ToString("yyyy-MM-dd HH:mm:ss"),
            ExportTimeStamp = time.ToUnixTimeSeconds().ToString(),
            ExportAppVersion = version.ToString(4),
        };
        Info = info;
    }

}

internal class WishlogManagePage_JsonExport_Info
{

    [JsonPropertyName("uid"), JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public int Uid { get; set; }

    [JsonPropertyName("lang")]
    public string Language { get; set; }

    [JsonPropertyName("export_time")]
    public string ExportTime { get; set; }

    [JsonPropertyName("export_timestamp")]
    public string ExportTimeStamp { get; set; }

    [JsonPropertyName("export_app")]
    public string ExportApp { get; set; } = "Xunkong.Desktop";

    [JsonPropertyName("export_app_version")]
    public string ExportAppVersion { get; set; }

    [JsonPropertyName("uigf_version")]
    public string UIGFVersion { get; set; } = "v2.2";

}

internal class WishlogManagePage_JsonExport_Item : IJsonOnSerializing
{
    /// <summary>
    /// 用户Uid
    /// </summary>
    [JsonPropertyName("uid"), JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public int Uid { get; set; }

    /// <summary>
    /// 祈愿类型（卡池类型）
    /// </summary>
    [JsonPropertyName("gacha_type"), JsonConverter(typeof(WishTypeJsonConverter))]
    public WishType WishType { get; set; }

    /// <summary>
    /// 此值为空
    /// </summary>
    [JsonPropertyName("item_id"), JsonConverter(typeof(WishlogItemIdJsonConverter))]
    public int ItemId { get; set; }

    /// <summary>
    /// 物品数量（暂时都是1）
    /// </summary>
    [JsonPropertyName("count"), JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public int Count { get; private set; } = 1;

    /// <summary>
    /// 时间
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset Time { get; set; }


    /// <summary>
    /// 字符串形式的时间，时区与账号服务器所在地理位置相关
    /// </summary>
    [JsonInclude, JsonPropertyName("time")]
    public string _TimeString { get; set; }

    /// <summary>
    /// 物品名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 语言（如zh-cn）
    /// </summary>
    [JsonPropertyName("lang")]
    public string Language { get; set; }

    /// <summary>
    /// 物品类型（角色、武器）
    /// </summary>
    [JsonPropertyName("item_type")]
    public string ItemType { get; set; }

    /// <summary>
    /// 星级
    /// </summary>
    [JsonPropertyName("rank_type"), JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public int RankType { get; set; }

    /// <summary>
    /// 祈愿Id，这个值很重要，全服唯一
    /// </summary>
    [JsonPropertyName("id"), JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public long Id { get; set; }


    /// <summary>
    /// 查询类型，此值为自行添加的值
    /// </summary>
    [JsonPropertyName("uigf_gacha_type"), JsonConverter(typeof(WishTypeJsonConverter))]
    public WishType QueryType { get; set; }


    public void OnSerializing()
    {
        var offset = Uid.ToString()[0] switch
        {
            '6' => -5,
            '7' => 1,
            _ => 8,
        };
        _TimeString = Time.UtcDateTime.AddHours(offset).ToString("yyyy-MM-dd HH:mm:ss");
    }

}




internal class WishlogItemIdJsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return 0;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteStringValue("");
    }
}


