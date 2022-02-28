using System.ComponentModel;

namespace Xunkong.Core.Hoyolab
{
    public enum RegionType
    {
        [Description("")]
        None = 0,

        [Description("cn_gf01")]
        GF = 1,

        [Description("cn_qd01")]
        QD = 5,

        [Description("os_usa")]
        US = 6,

        [Description("os_euro")]
        EU = 7,

        [Description("os_asia")]
        ASIA = 8,

        [Description("os_cht")]
        CHT = 9,

    }



    internal class RegionTypeJsonConverter : JsonConverter<RegionType>
    {
        public override RegionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() switch
            {
                "cn_gf01" => RegionType.GF,
                "cn_qd01" => RegionType.QD,
                "os_usa" => RegionType.US,
                "os_euro" => RegionType.EU,
                "os_asia" => RegionType.ASIA,
                "os_cht" => RegionType.CHT,
                _ => RegionType.None,
            };
        }

        public override void Write(Utf8JsonWriter writer, RegionType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToDescriptionOrString());
        }
    }

}
