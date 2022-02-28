namespace Xunkong.Core.Metadata
{
    internal record MetadataDto<T>(int Count, IEnumerable<T>? List) where T : class { }
}
