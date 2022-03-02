using System.Data;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using Xunkong.Core.SpiralAbyss;
using Xunkong.Core.TravelRecord;

namespace Xunkong.Core.Hoyolab
{
    public class HoyolabClient
    {



        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _options;


        #region Constant

        // http header key
        private const string Accept = "Accept";
        private const string Cookie = "Cookie";
        private const string UserAgent = "User-Agent";
        private const string X_Reuqest_With = "X-Requested-With";
        private const string DS = "DS";
        private const string Referer = "Referer";
        private const string Application_Json = "application/json";
        private const string com_mihoyo_hyperion = "com.mihoyo.hyperion";
        private const string x_rpc_app_version = "x-rpc-app_version";
        private const string x_rpc_device_id = "x-rpc-device_id";
        private const string x_rpc_client_type = "x-rpc-client_type";

        // http header value
        private const string UA2101 = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) miHoYoBBS/2.10.1";
        private const string UA2111 = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) miHoYoBBS/2.11.1";
        private const string UA2161 = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) miHoYoBBS/2.16.1";
        private const string BBS_Referer = "https://bbs.mihoyo.com/";
        private const string Record_Referer = "https://webstatic.mihoyo.com/app/community-game-records/index.html?v=6";
        private const string TravelRecord_Referer = "https://webstatic.mihoyo.com/bbs/event/e20200709ysjournal/index.html?bbs_presentation_style=fullscreen&bbs_auth_required=true&utm_source=bbs&utm_medium=mys&utm_campaign=icon";
        private const string TravelRecord_Query = "bbs_presentation_style=fullscreen&bbs_auth_required=true&utm_source=bbs&utm_medium=mys&utm_campaign=icon";
        private const string SignIn_Referer = $"https://webstatic.mihoyo.com/bbs/event/signin-ys/index.html?bbs_auth_required=true&act_id={SignIn_ActivityId}&utm_source=bbs&utm_medium=mys&utm_campaign=icon";
        private static readonly string DeviceId = Guid.NewGuid().ToString("D");

        // base url
        private const string SignIn_ActivityId = "e202009291139501";
        private const string ApiTakumi = "https://api-takumi.mihoyo.com";
        private const string BbsApi = "https://bbs-api.mihoyo.com/user/wapi";
        private const string ApiTakumiRecord = "https://api-takumi-record.mihoyo.com/game_record/app/genshin/api";
        private const string Hk4eApi = "https://hk4e-api.mihoyo.com/event/ys_ledger";


        #endregion


        #region Api url

        /// <summary>
        /// 米游社用户信息
        /// </summary>
        private static readonly string UserInfoUrl = $"{BbsApi}/getUserFullInfo?gids=2";

        /// <summary>
        /// 游戏角色
        /// </summary>
        private static readonly string UserGameRoleUrl = $"{ApiTakumi}/binding/api/getUserGameRolesByCookie?game_biz=hk4e_cn";

        /// <summary>
        /// 签到
        /// </summary>
        private static readonly string SignInUrl = $"{ApiTakumi}/event/bbs_sign_reward/sign";

        /// <summary>
        /// 角色详情
        /// </summary>
        private static readonly string CharacterDetailsUrl = $"{ApiTakumiRecord}/character";

        /// <summary>
        /// 签到信息
        /// </summary>
        private static string SignInInfoUrl(UserGameRoleInfo role) => $"{ApiTakumi}/event/bbs_sign_reward/info?act_id={SignIn_ActivityId}&region={role.Region.ToDescriptionOrString()}&uid={role.Uid}";

        /// <summary>
        /// 深镜螺旋
        /// </summary>
        /// <param name="schedule">1本期，2上期</param>
        private static string RecordSpiralAbyssUrl(UserGameRoleInfo role, int schedule) => $"{ApiTakumiRecord}/spiralAbyss?schedule_type={schedule}&server={role.Region.ToDescriptionOrString()}&role_id={role.Uid}";

        /// <summary>
        /// 世界探索统计
        /// </summary>
        private static string PalyerSummaryInfoUrl(UserGameRoleInfo role) => $"{ApiTakumiRecord}/index?server={role.Region.ToDescriptionOrString()}&role_id={role.Uid}";


        /// <summary>
        /// 玩家活动记录
        /// </summary>
        private static string ActivityUrl(UserGameRoleInfo role) => $"{ApiTakumiRecord}/activities?server={role.Region.ToDescriptionOrString()}&role_id={role.Uid}";

        /// <summary>
        /// 实时便笺
        /// </summary>
        private static string DailyNoteUrl(UserGameRoleInfo role) => $"{ApiTakumiRecord}/dailyNote?server={role.Region.ToDescriptionOrString()}&role_id={role.Uid}";

        /// <summary>
        /// 旅行者札记总览
        /// </summary>
        /// <param name="month">月份</param>
        private static string TravelRecordSummaryUrl(UserGameRoleInfo role, int month) => $"{Hk4eApi}/monthInfo?month={month}&bind_uid={role.Uid}&bind_region={role.Region.ToDescriptionOrString()}&{TravelRecord_Query}";

        /// <summary>
        /// 旅行者札记详细
        /// </summary>
        /// <param name="type">1原石，2摩拉</param>
        /// <param name="month">月份</param>
        /// <param name="page">第几页</param>
        /// <param name="limit">每页限制几项，最多100</param>
        private static string TravelRecordDetailUrl(UserGameRoleInfo role, int month, TravelRecordAwardType type, int page, int limit = 10) => $"{Hk4eApi}/monthDetail?type={(int)type}&month={month}&page={page}&limit={limit}&bind_uid={role.Uid}&bind_region={role.Region.ToDescriptionOrString()}&{TravelRecord_Query}";

        #endregion




        public HoyolabClient(HttpClient? httpClient = null, JsonSerializerOptions? options = null)
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
            if (options is null)
            {
                _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            }
            else
            {
                _options = options;
            }
        }




        private async Task<T> CommonSendAsync<T>(HttpRequestMessage request) where T : class
        {
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<HoyolabBaseWrapper<T>>(content, _options);
            if (responseData is null)
            {
                throw new HoyolabException(-1, "Can not parse the response body.");
            }
            if (responseData.ReturnCode != 0)
            {
                throw new HoyolabException(responseData.ReturnCode, responseData.Message);
            }
            // warning 不确定是否应该判断响应data为null的情况
            if (responseData.Data is null)
            {
                throw new HoyolabException(-1, "Response data is null.");
            }
            return responseData.Data;
        }




        /// <summary>
        /// 米游社用户信息
        /// </summary>
        /// <returns></returns>
        public async Task<UserInfo> GetUserInfoAsync(string cookie)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentNullException(nameof(cookie));
            }
            var request = new HttpRequestMessage(HttpMethod.Get, UserInfoUrl);
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2101);
            request.Headers.Add(Cookie, cookie);
            request.Headers.Add(Referer, BBS_Referer);
            request.Headers.Add(DS, DynamicSecret.CreateSecret());
            request.Headers.Add(x_rpc_app_version, DynamicSecret.AppVersion_2101);
            request.Headers.Add(x_rpc_device_id, DeviceId);
            request.Headers.Add(x_rpc_client_type, "4");
            var data = await CommonSendAsync<UserInfoWrapper>(request);
            data.UserInfo.Cookie = cookie;
            return data.UserInfo;
        }



        /// <summary>
        /// 玩家信息（服务器，uid，昵称，等级）
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserGameRoleInfo>> GetUserGameRoleInfosAsync(string cookie)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentNullException(nameof(cookie));
            }
            var request = new HttpRequestMessage(HttpMethod.Get, UserGameRoleUrl);
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2101);
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            request.Headers.Add(Cookie, cookie);
            var data = await CommonSendAsync<UserGameRoleWrapper>(request);
            data.List?.ForEach(x => x.Cookie = cookie);
            return data.List ?? new List<UserGameRoleInfo>();
        }




        /// <summary>
        /// 签到信息
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<SignInInfo> GetSignInInfoAsync(UserGameRoleInfo role)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, SignInInfoUrl(role));
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2101);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(x_rpc_device_id, DeviceId);
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            request.Headers.Add(Referer, SignIn_Referer);
            return await CommonSendAsync<SignInInfo>(request);
        }



        /// <summary>
        /// 签到
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<SignInResult> SignInAsync(UserGameRoleInfo role)
        {
            var obj = new { act_id = SignIn_ActivityId, region = role.Region.ToDescriptionOrString(), uid = role.Uid.ToString() };
            var request = new HttpRequestMessage(HttpMethod.Post, SignInUrl);
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2101);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(Referer, SignIn_Referer);
            request.Headers.Add(DS, DynamicSecret.CreateSecret());
            request.Headers.Add(x_rpc_app_version, DynamicSecret.AppVersion_2101);
            request.Headers.Add(x_rpc_device_id, DeviceId);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            request.Content = JsonContent.Create(obj);
            return await CommonSendAsync<SignInResult>(request);
        }



        /// <summary>
        /// 深境螺旋信息
        /// </summary>
        /// <param name="role"></param>
        /// <param name="schedule">1当期，2上期</param>
        /// <returns></returns>
        public async Task<SpiralAbyssInfo> GetSpiralAbyssInfoAsync(UserGameRoleInfo role, int schedule = 1)
        {
            var url = RecordSpiralAbyssUrl(role, schedule);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2161);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecretV2(url));
            request.Headers.Add(Referer, Record_Referer);
            request.Headers.Add(x_rpc_app_version, DynamicSecret.AppVersion_2161);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            var data = await CommonSendAsync<SpiralAbyssInfo>(request);
            data.Uid = role.Uid;
            return data;
        }


        /// <summary>
        /// 玩家基础信息
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public async Task<PlayerSummaryInfo> GetPlayerSummaryInfoAsync(UserGameRoleInfo role)
        {
            var url = PalyerSummaryInfoUrl(role);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2161);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecretV2(url));
            request.Headers.Add(Referer, Record_Referer);
            request.Headers.Add(x_rpc_app_version, DynamicSecret.AppVersion_2161);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            return await CommonSendAsync<PlayerSummaryInfo>(request);
        }


        /// <summary>
        /// 角色详细信息
        /// </summary>
        /// <param name="role"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public async Task<List<AvatarDetail>> GetAvatarDetailsAsync(UserGameRoleInfo role, PlayerSummaryInfo player)
        {
            var obj = new
            {
                character_ids = player.AvatarInfos.Select(x => x.Id),
                role_id = role.Uid,
                server = role.Region.ToDescriptionOrString(),
            };
            var request = new HttpRequestMessage(HttpMethod.Post, CharacterDetailsUrl);
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2161);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecretV2(CharacterDetailsUrl, obj));
            request.Headers.Add(Referer, Record_Referer);
            request.Headers.Add(x_rpc_app_version, DynamicSecret.AppVersion_2161);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            request.Content = JsonContent.Create(obj);
            var data = await CommonSendAsync<AvatarDetailWrapper>(request);
            return data.Avatars;
        }



        /// <summary>
        /// 活动记录
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<dynamic> GetActivitiesAsync(UserGameRoleInfo role)
        {
            var url = ActivityUrl(role);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2161);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecretV2(url));
            request.Headers.Add(Referer, Record_Referer);
            request.Headers.Add(x_rpc_app_version, DynamicSecret.AppVersion_2161);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            return await CommonSendAsync<dynamic>(request);
        }


        /// <summary>
        /// 实时便笺
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<DailyNoteInfo> GetDailyNoteInfoAsync(UserGameRoleInfo role)
        {
            var url = DailyNoteUrl(role);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2161);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(DS, DynamicSecret.CreateSecretV2(url));
            request.Headers.Add(Referer, Record_Referer);
            request.Headers.Add(x_rpc_app_version, DynamicSecret.AppVersion_2161);
            request.Headers.Add(x_rpc_client_type, "5");
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            var data = await CommonSendAsync<DailyNoteInfo>(request);
            data.Uid = role.Uid;
            data.Nickname = role.Nickname;
            var now = DateTimeOffset.Now;
            data.Time = now;
            if (data.Expeditions is not null)
            {
                foreach (var ex in data.Expeditions)
                {
                    ex.FinishedTime = now + ex.RemainedTime;
                    ex.FinishedDayString = ex.FinishedTime.Date > now.Date ? "明日" : "今日";
                }
            }
            return data;
        }


        /// <summary>
        /// 旅行记录每月统计
        /// </summary>
        /// <param name="role"></param>
        /// <param name="month">月份</param>
        /// <returns></returns>
        public async Task<TravelRecordSummary> GetTravelRecordSummaryAsync(UserGameRoleInfo role, int month)
        {
            var url = TravelRecordSummaryUrl(role, month);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2101);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(Referer, TravelRecord_Referer);
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            return await CommonSendAsync<TravelRecordSummary>(request);
        }


        /// <summary>
        /// 旅行记录原石摩拉详情
        /// </summary>
        /// <param name="role"></param>
        /// <param name="month">月份</param>
        /// <param name="type">1原石，2摩拉</param>
        /// <param name="page">第几页</param>
        /// <param name="limit">每页几条，最多100</param>
        /// <returns></returns>
        public async Task<TravelRecordDetail> GetTravelRecordDetailByPageAsync(UserGameRoleInfo role, int month, TravelRecordAwardType type, int page, int limit = 100)
        {
            var url = TravelRecordDetailUrl(role, month, type, page, limit);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add(Accept, Application_Json);
            request.Headers.Add(UserAgent, UA2101);
            request.Headers.Add(Cookie, role.Cookie);
            request.Headers.Add(Referer, TravelRecord_Referer);
            request.Headers.Add(X_Reuqest_With, com_mihoyo_hyperion);
            var data = await CommonSendAsync<TravelRecordDetail>(request);
            foreach (var item in data.List)
            {
                item.Type = type;
            }
            return data;
        }


        /// <summary>
        /// 旅行记录原石摩拉详情
        /// </summary>
        /// <param name="role"></param>
        /// <param name="month">月份</param>
        /// <param name="type">1原石，2摩拉</param>
        /// <param name="limit">每页几条，最多100</param>
        /// <returns></returns>
        public async Task<TravelRecordDetail> GetTravelRecordDetailAsync(UserGameRoleInfo role, int month, TravelRecordAwardType type, int limit = 100)
        {
            var data = await GetTravelRecordDetailByPageAsync(role, month, type, 1, limit);
            if (data.List.Count < limit)
            {
                return data;
            }
            for (int i = 2; ; i++)
            {
                var addData = await GetTravelRecordDetailByPageAsync(role, month, type, i, limit);
                data.List.AddRange(addData.List);
                if (addData.List.Count < limit)
                {
                    break;
                }
            }
            return data;
        }





    }
}
