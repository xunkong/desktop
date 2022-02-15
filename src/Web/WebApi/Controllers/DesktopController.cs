using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Filters;
using Xunkong.Web.Api.Models;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Controllers
{

    [ApiController]
    [ApiVersion("0.1")]
    [Route("v{version:ApiVersion}/[controller]")]
    [ServiceFilter(typeof(BaseRecordResultFilter))]
    public class DesktopController : Controller
    {

        private readonly ILogger<DesktopController> _logger;

        private readonly XunkongDbContext _dbContext;

        private readonly IMemoryCache _cache;



        public DesktopController(ILogger<DesktopController> logger, XunkongDbContext dbContext, IMemoryCache cache)
        {
            _logger = logger;
            _dbContext = dbContext;
            _cache = cache;
        }



        [HttpGet("CheckUpdate")]
        public async Task<ResponseBaseWrapper> CheckUpdateAsync([FromQuery] ChannelType channel)
        {
            var key = $"desktop_checkupdate_{channel}";
            if (_cache.TryGetValue(key, out ResponseBaseWrapper result))
            {
                return result;
            }
            else
            {
                var version = await _dbContext.DesktopUpdateVersions.AsNoTracking().Where(x => x.Channel == channel).OrderByDescending(x => x.Version).FirstOrDefaultAsync();
                if (version is not null)
                {
                    result = ResponseBaseWrapper.Ok(version);
                    _cache.Set(key, result, TimeSpan.FromMinutes(15));
                    return result;
                }
                else
                {
                    throw new XunkongException(ErrorCode.NoContentForVersion);
                }
            }
        }


        [HttpGet("AppInstaller")]
        public async Task<ActionResult<string>> GetAppInstallerContentAsync([FromQuery] ChannelType channel = ChannelType.All, [FromQuery] string? version = null)
        {
            var key = $"desktop_appinstaller_{channel}_{version}";
            if (_cache.TryGetValue(key, out string result))
            {
                if (string.IsNullOrWhiteSpace(result))
                {
                    return NotFound();
                }
                else
                {
                    return result;
                }
            }
            else
            {
                Version.TryParse(version, out var v);
                var vm = await _dbContext.DesktopUpdateVersions.AsNoTracking().Where(x => channel.HasFlag(x.Channel) && x.Version == v).OrderByDescending(x => x.Version).FirstOrDefaultAsync();
                if (vm is null)
                {
                    vm = await _dbContext.DesktopUpdateVersions.AsNoTracking().Where(x => channel.HasFlag(x.Channel)).OrderByDescending(x => x.Version).FirstOrDefaultAsync();
                }
                if (vm is null)
                {
                    _cache.Set(key, "", TimeSpan.FromMinutes(15));
                    return NotFound();
                }
                else
                {
                    if (!_cache.TryGetValue("template.appinstaller", out string template))
                    {
                        template = await System.IO.File.ReadAllTextAsync("template.appinstaller");
                        _cache.Set("template.appinstaller", template);
                    }
                    result = template.Replace("{AppVersion}", vm.Version.ToString()).Replace("{PackageUrl}", vm.PackageUrl);
                    _cache.Set(key, result, TimeSpan.FromMinutes(15));
                    return result;
                }
            }
        }



        [HttpGet("Changelog")]
        public async Task<ResponseBaseWrapper> GetChangelogAsync([FromQuery] string? version)
        {
            if (!Version.TryParse(version, out var v))
            {
                throw new XunkongException(ErrorCode.VersionIsNull);
            }
            var key = $"desktop_changelog_{version}";
            if (_cache.TryGetValue(key, out ResponseBaseWrapper result))
            {
                return result;
            }
            else
            {
                var changelog = await _dbContext.DesktopChangelogs.AsNoTracking().Where(x => x.Version == v).FirstOrDefaultAsync();
                if (changelog is not null)
                {
                    result = ResponseBaseWrapper.Ok(changelog);
                    _cache.Set(key, result, TimeSpan.FromMinutes(15));
                    return result;
                }
                else
                {
                    throw new XunkongException(ErrorCode.NoContentForVersion);
                }
            }
        }




        [HttpGet("Notifications")]
        public async Task<ResponseBaseWrapper> GetNotificationsAsync([FromQuery] ChannelType channel, [FromQuery] string? version, [FromQuery] int lastId)
        {
            if (!Version.TryParse(version, out var v))
            {
                throw new XunkongException(ErrorCode.VersionIsNull);
            }
            var key = $"desktop_notification_{channel}_{version}";
            if (_cache.TryGetValue(key, out ResponseBaseWrapper result))
            {
                return result;
            }
            else
            {
                var vmin = new Version(0, 0);
                var vmax = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
                var notifications = await _dbContext.NotificationItems.AsNoTracking()
                                                                      .Where(x => x.Platform == PlatformType.Desktop && x.Channel.HasFlag(channel) && x.Id > lastId && x.Enable)
                                                                      .Where(x => (x.MinVersion ?? vmin) <= v && v < (x.MaxVersion ?? vmax))
                                                                      .OrderByDescending(x => x.Time)
                                                                      .ToListAsync();
                var dto = new NotificationWrapper<NotificationServerModel>
                {
                    Platform = PlatformType.Desktop,
                    Channel = channel,
                    Version = v,
                    List = notifications,
                };
                result = ResponseBaseWrapper.Ok(dto);
                _cache.Set(key, result, TimeSpan.FromMinutes(15));
                return result;
            }
        }
    }


}
