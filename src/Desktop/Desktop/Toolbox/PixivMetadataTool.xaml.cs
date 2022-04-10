using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Xunkong.Core.XunkongApi;
using IS = SixLabors.ImageSharp;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Toolbox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Toolbox("Pixiv 元数据", "ms-appx:///Assets/Images/Pixiv.jfif", "把 Pixiv 图片的信息作为元数据写入到文件中")]
    public sealed partial class PixivMetadataTool : Page
    {

        private const string PixivToolBoxProxy = "PixivToolBoxProxy";


        private HttpClient _httpClient;


        public PixivMetadataTool()
        {
            this.InitializeComponent();
            var proxy = LocalSettingHelper.GetSetting<string>(PixivToolBoxProxy);
            BuildHttpClient(proxy);
            _TextBox_Proxy.Text = proxy;
        }


        private bool useCompactName = false;

        private bool donotOutputFile = false;


        private List<WallpaperInfo> wallpaperInfos = new();


        private void BuildHttpClient(string? proxy)
        {
            var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All, UseProxy = true };
            if (!string.IsNullOrWhiteSpace(proxy))
            {
                handler.Proxy = new WebProxy(proxy);
            }
            var client = new HttpClient(handler, false);
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN");
            client.DefaultRequestHeaders.Add("Referer", "https://www.pixiv.net/");
            _httpClient = client;
        }


        private void _Button_ConfirmProxy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var proxy = _TextBox_Proxy.Text;
                BuildHttpClient(proxy);
                LocalSettingHelper.SaveSetting(PixivToolBoxProxy, proxy);
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
            }
        }


        private void _Border_GragIn_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;
        }



        private async void _Border_GragIn_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    _PrograssRing.IsActive = true;
                    wallpaperInfos.Clear();
                    await Parallel.ForEachAsync(items.Select(x => x.Path), async (path, _) => await GetPixivMetadataAndWriteToImageAsync(path));
                    _PrograssRing.IsActive = false;
                    InfoBarHelper.Success("完成");
                }
            }
        }



        private async Task GetPixivMetadataAndWriteToImageAsync(string path)
        {
            try
            {
                GetPidFromImageFile(path, out int pid, out int p);
                var metadata = await GetMetadataFromPixivAsync(pid);
                var fileName = Path.GetFileName(path);
                var wallpaper = new WallpaperInfo
                {
                    Title = metadata.Title,
                    Author = metadata.Artist,
                    Description = metadata.Description?.Replace("'", "\\'"),
                    FileName = fileName,
                    Url = $"https://file.xunkong.cc/wallpaper/{fileName}",
                    Source = $"https://www.pixiv.net/artworks/{pid}",
                    Tags = metadata.Tags,
                };
                if (!donotOutputFile)
                {
                    using var image = await IS.Image.LoadAsync(path);
                    var ep = new ExifProfile();
                    ep.SetValue(ExifTag.XPTitle, metadata.Title);
                    ep.SetValue(ExifTag.Artist, metadata.Artist);
                    ep.SetValue(ExifTag.XPAuthor, metadata.Artist);
                    ep.SetValue(ExifTag.XPComment, metadata.Description);
                    ep.SetValue(ExifTag.DateTimeOriginal, metadata.Time.LocalDateTime.ToString());
                    ep.SetValue(ExifTag.XPKeywords, string.Join(";", metadata.Tags));
                    image.Metadata.ExifProfile = ep;
                    if (useCompactName)
                    {
                        var file = Path.Combine(Path.GetDirectoryName(path)!, $"{pid}_p{p}.jpg");
                        await image.SaveAsJpegAsync(file, new JpegEncoder());
                    }
                    else
                    {
                        var file = Path.Combine(Path.GetDirectoryName(path)!, wallpaper.FileName);
                        await image.SaveAsJpegAsync(file, new JpegEncoder());
                    }
                }
                wallpaperInfos.Add(wallpaper);
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex, $"File name: {Path.GetFileName(path)}");
            }
        }




        private void GetPidFromImageFile(string path, out int pid, out int p)
        {
            var fileName = Path.GetFileName(path);
            var match = Regex.Match(fileName, @"(\d+)_p(\d+)");
            int.TryParse(match.Groups[1].Value, out pid);
            int.TryParse(match?.Groups[2].Value, out p);
        }





        private async Task<PixivMetadata> GetMetadataFromPixivAsync(int pid)
        {
            var url = $"https://www.pixiv.net/ajax/illust/{pid}";
            var str = await _httpClient.GetStringAsync(url);
            var baseNode = JsonNode.Parse(str);
            if (baseNode?["body"] is JsonObject obj)
            {
                var title = obj["title"]?.ToString();
                var artist = obj["userName"]?.ToString();
                var description = obj["description"]?.ToString()?.Replace("<br />", "\\n");
                DateTimeOffset.TryParse(obj["createDate"]?.ToString(), out var time);
                var tags = new List<string>();
                if (obj["tags"]?["tags"] is JsonArray tagArray)
                {
                    tags.AddRange(tagArray.Select(x => x?["tag"]?.ToString() ?? ""));
                    tags.AddRange(tagArray.Select(x => x?["translation"] as JsonObject).Select(x => x?["en"]?.ToString() ?? ""));
                    tags = tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                }
                return new PixivMetadata(pid, title, artist, description, time, tags);
            }
            else
            {
                throw new Exception("Return body is null.");
            }
        }


        private void _Button_CopySql_Click(object sender, RoutedEventArgs e)
        {
            if (!wallpaperInfos.Any())
            {
                InfoBarHelper.Warning("没有图片", 3000);
                return;
            }
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("INSERT INTO wallpapers (FileName,Title, Author, Description, Tags, Url, Source) VALUES");
                foreach (var item in wallpaperInfos)
                {
                    sb.AppendLine($"('{item.FileName}','{item.Title}','{item.Author}','{item.Description}','{string.Join(';', item.Tags)}','{item.Url}','{item.Source}'),");
                }
                sb[sb.Length - 3] = ';';
                var data = new DataPackage();
                data.RequestedOperation = DataPackageOperation.Copy;
                data.SetText(sb.ToString());
                Clipboard.SetContent(data);
                InfoBarHelper.Success("已复制");
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
            }
        }



        private record PixivMetadata(int Pid, string? Title, string? Artist, string? Description, DateTimeOffset Time, List<string> Tags);



    }
}
