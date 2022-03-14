using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [ResponseCache(Duration = 900)]
    public class DesktopController : Controller
    {

        private readonly ILogger<DesktopController> _logger;

        private readonly XunkongDbContext _dbContext;


        public DesktopController(ILogger<DesktopController> logger, XunkongDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }



        [HttpGet("CheckUpdate")]
        public async Task<ResponseBaseWrapper> CheckUpdateAsync([FromQuery] ChannelType channel)
        {
            var version = await _dbContext.DesktopUpdateVersions.AsNoTracking().Where(x => x.Channel.HasFlag(channel)).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            if (version is not null)
            {
                return ResponseBaseWrapper.Ok(version);
            }
            else
            {
                throw new XunkongException(ErrorCode.NoContentForVersion);
            }
        }


        [HttpGet("AppInstaller")]
        public async Task<ActionResult<string>> GetAppInstallerContentAsync([FromQuery] ChannelType channel = ChannelType.All, [FromQuery] string? version = null)
        {
            Version.TryParse(version, out var v);
            var vm = await _dbContext.DesktopUpdateVersions.AsNoTracking().Where(x => x.Version == v).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            if (vm is null)
            {
                vm = await _dbContext.DesktopUpdateVersions.AsNoTracking().Where(x => x.Channel.HasFlag(channel)).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            }
            if (vm is null)
            {
                return NotFound();
            }
            else
            {
                var template = await System.IO.File.ReadAllTextAsync("template.appinstaller");
                return template.Replace("{AppVersion}", vm.Version.ToString()).Replace("{PackageUrl}", vm.PackageUrl);
            }
        }



        [HttpGet("Changelog")]
        public async Task<ResponseBaseWrapper> GetChangelogAsync([FromQuery] string? version)
        {
            if (!Version.TryParse(version, out var v))
            {
                throw new XunkongException(ErrorCode.VersionIsNull);
            }
            var changelog = await _dbContext.DesktopChangelogs.AsNoTracking().Where(x => x.Version == v).FirstOrDefaultAsync();
            if (changelog is not null)
            {
                return ResponseBaseWrapper.Ok(changelog);
            }
            else
            {
                throw new XunkongException(ErrorCode.NoContentForVersion);
            }
        }




        [HttpGet("Notifications")]
        public async Task<ResponseBaseWrapper> GetNotificationsAsync([FromQuery] ChannelType channel, [FromQuery] string? version, [FromQuery] int lastId)
        {
            if (!Version.TryParse(version, out var v))
            {
                throw new XunkongException(ErrorCode.VersionIsNull);
            }
            var vmin = new Version(0, 0);
            var vmax = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
            var notifications = await _dbContext.NotificationItems.AsNoTracking()
                                                                  .Where(x => x.Platform == PlatformType.Desktop && x.Channel.HasFlag(channel) && x.Id > lastId && x.Enable)
                                                                  .ToListAsync();
            notifications = notifications.Where(x => (x.MinVersion ?? vmin) <= v && v < (x.MaxVersion ?? vmax))
                                         .OrderByDescending(x => x.Time)
                                         .ToList();
            var dto = new NotificationWrapper<NotificationServerModel>
            {
                Platform = PlatformType.Desktop,
                Channel = channel,
                Version = v,
                List = notifications,
            };
            return ResponseBaseWrapper.Ok(dto);
        }
    }


}
