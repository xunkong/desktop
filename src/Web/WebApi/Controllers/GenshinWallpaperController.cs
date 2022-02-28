using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Filters;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Controllers
{

    [ApiController]
    [ApiVersion("0.1")]
    [Route("v{version:ApiVersion}/Genshin/Wallpaper")]
    [ServiceFilter(typeof(BaseRecordResultFilter))]
    public class GenshinWallpaperController : Controller
    {


        private readonly ILogger<GenshinWallpaperController> _logger;

        private readonly XunkongDbContext _dbContext;

        private readonly IMemoryCache _cache;


        public GenshinWallpaperController(ILogger<GenshinWallpaperController> logger, XunkongDbContext dbContext, IMemoryCache cache)
        {
            _logger = logger;
            _dbContext = dbContext;
            _cache = cache;
        }



        [HttpGet("random")]
        public async Task<ResponseBaseWrapper> GetRandomWallpaperAsJsonResultAsync(int excludeId = 0)
        {
            if (excludeId == 0)
            {
                var last = await _dbContext.WallpaperInfos.Where(x => x.Enable).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return ResponseBaseWrapper.Ok(last!);
            }
            var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync() - (excludeId == 0 ? 0 : 1);
            var index = Random.Shared.Next(count);
            var info = await _dbContext.WallpaperInfos.Where(x => x.Enable && x.Id != excludeId).Skip(index).FirstOrDefaultAsync();
            if (info == null)
            {
                info = await _dbContext.WallpaperInfos.Where(x => x.Enable).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            }
            return ResponseBaseWrapper.Ok(info!);
        }



        [HttpGet("next")]
        public async Task<ResponseBaseWrapper> GetNextWallpaperAsJsonResultAsync(int excludeId = 0)
        {
            var info = await _dbContext.WallpaperInfos.Where(x => x.Enable && x.Id > excludeId).OrderBy(x => x.Id).FirstOrDefaultAsync();
            if (info == null)
            {
                info = await _dbContext.WallpaperInfos.Where(x => x.Enable).FirstOrDefaultAsync();
            }
            return ResponseBaseWrapper.Ok(info!);
        }




    }
}
