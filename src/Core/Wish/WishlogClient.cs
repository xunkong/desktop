using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Xunkong.Core.Hoyolab;

namespace Xunkong.Core.Wish
{
    public class WishlogClient
    {

        private const string CnUrl = "https://hk4e-api.mihoyo.com/event/gacha_info/api/getGachaLog";

        private const string SeaUrl = "https://hk4e-api-os.mihoyo.com/event/gacha_info/api/getGachaLog";

        private readonly HttpClient _httpClient;


        public event EventHandler<(WishType WishType, int Page)>? ProgressChanged;

        public delegate void GetWishlogProgressChangedHandler(WishType WishType, int Page);


        public WishlogClient(HttpClient? httpClient = null)
        {
            if (httpClient is null)
            {
                _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
                _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            }
            else
            {
                _httpClient = httpClient;
            }
        }




        private static string GetBaseAndAuthString(string wishlogUrl)
        {
            var match = Regex.Match(wishlogUrl, @"(https://webstatic.+#/log)");
            if (!match.Success)
            {
                throw new ArgumentException("Url does not meet the requirement.");
            }
            wishlogUrl = match.Groups[1].Value;
            var auth = wishlogUrl.Substring(wishlogUrl.IndexOf('?')).Replace("#/log", "");
            if (wishlogUrl.Contains("webstatic-sea"))
            {
                return SeaUrl + auth;
            }
            else
            {
                return CnUrl + auth;
            }
        }




        /// <summary>
        /// 获取一页祈愿数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns>没有数据时返回空集合</returns>
        /// <exception cref="HoyolabException">api请求返回值不为零时抛出异常</exception>
        private async Task<List<WishlogItem>> GetWishlogByParamAsync(string baseString, QueryParam param)
        {
            var url = $"{baseString}&{param}";
            await Task.Delay(Random.Shared.Next(200, 300));
            var response = await _httpClient.GetFromJsonAsync<WishlogResponseData>(url);
            if (response is null)
            {
                throw new HoyolabException(-1, "Cannot parse the return data.");
            }
            if (response.Retcode != 0)
            {
                throw new HoyolabException(response.Retcode, response.Message ?? "No return meesage.");
            }
            return response.Data?.List ?? new(0);
        }



        /// <summary>
        /// 获取一种卡池类型的祈愿数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="lastId">获取的祈愿id小于最新id即停止</param>
        /// <param name="size">每次api请求获取几条数据，不超过20，默认6</param>
        /// <returns>没有数据返回空集合</returns>
        /// <exception cref="HoyolabException">api请求返回值不为零时抛出异常</exception>
        private async Task<List<WishlogItem>> GetWishlogByTypeAsync(string baseString, WishType type, long lastId = 0, int size = 20)
        {
            var param = new QueryParam(type, 1, size);
            var result = new List<WishlogItem>();
            while (true)
            {
                ProgressChanged?.Invoke(this, (type, param.Page));
                var list = await GetWishlogByParamAsync(baseString, param);
                result.AddRange(list);
                if (list.Count == size && list.Last().Id > lastId)
                {
                    param.Page++;
                    param.EndId = list.Last().Id;
                }
                else
                {
                    break;
                }
            }
            foreach (var item in result)
            {
                item.QueryType = type;
            }
            return result;
        }



        /// <summary>
        /// 获取所有的祈愿数据，以id顺序排列
        /// </summary>
        /// <param name="lastId">获取的祈愿id小于最新id即停止</param>
        /// <param name="size">每次api请求获取几条数据，不超过20，默认20</param>
        /// <returns>没有数据返回空集合</returns>
        /// <exception cref="HoyolabException">api请求返回值不为零时抛出异常</exception>
        public async Task<List<WishlogItem>> GetAllWishlogAsync(string wishlogUrl, long lastId = 0, int size = 20)
        {
            var baseUrl = GetBaseAndAuthString(wishlogUrl);
            lastId = lastId < 0 ? 0 : lastId;
            size = Math.Clamp(size, 1, 20);
            var result = new List<WishlogItem>();
            result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.Novice, lastId, size));
            result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.Permanent, lastId, size));
            result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.CharacterEvent, lastId, size));
            result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.WeaponEvent, lastId, size));
            return result.OrderBy(x => x.Id).ToList();
        }



        /// <summary>
        /// 获取祈愿记录网址所属的Uid
        /// </summary>
        /// <returns>返回值为0代表没有祈愿数据</returns>
        /// <exception cref="HoyolabException">api请求返回值不为零时抛出异常</exception>
        /// <exception cref="ArgumentNullException">祈愿记录网址为空</exception>
        public async Task<int> GetUidAsync(string wishlogUrl)
        {
            var baseUrl = GetBaseAndAuthString(wishlogUrl);
            var param = new QueryParam(WishType.CharacterEvent, 1);
            var list = await GetWishlogByParamAsync(baseUrl, param);
            if (list.Any())
            {
                return list.First().Uid;
            }
            param.WishType = WishType.Permanent;
            list = await GetWishlogByParamAsync(baseUrl, param);
            if (list.Any())
            {
                return list.First().Uid;
            }
            param.WishType = WishType.WeaponEvent;
            list = await GetWishlogByParamAsync(baseUrl, param);
            if (list.Any())
            {
                return list.First().Uid;
            }
            param.WishType = WishType.Novice;
            list = await GetWishlogByParamAsync(baseUrl, param);
            if (list.Any())
            {
                return list.First().Uid;
            }
            return 0;
        }




    }
}
