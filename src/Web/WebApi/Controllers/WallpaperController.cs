using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Filters;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Controllers
{

    [ApiController]
    [ApiVersion("0.1")]
    [Route("v{version:ApiVersion}/[controller]")]
    [ServiceFilter(typeof(BaseRecordResultFilter))]
    public class WallpaperController : Controller
    {


        private readonly ILogger<GenshinWallpaperController> _logger;

        private readonly XunkongDbContext _dbContext;



        public WallpaperController(ILogger<GenshinWallpaperController> logger, XunkongDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }




        [HttpGet("{id}")]
        [ResponseCache(Duration = 2592000)]
        public async Task<ResponseBaseWrapper> GetWallpaperInfoByIdAsync([FromRoute] int id)
        {
            var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();
            if (info is not null)
            {
                return ResponseBaseWrapper.Ok(info);
            }
            else
            {
                return new ResponseBaseWrapper(ErrorCode.InternalException, "Wallpaper not found.");
            }
        }



        [HttpGet("{id}/redirect")]
        [ResponseCache(Duration = 2592000)]
        public async Task<IActionResult> RedirectToWallpaperImageByIdAsync([FromRoute] int id)
        {
            var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();
            if (info is not null)
            {
                return RedirectToImage(info.FileName!);
            }
            else
            {
                return NotFound("Wallpaper not found.");
            }
        }



        [HttpGet("recommend")]
        [ResponseCache(Duration = 3600)]
        public async Task<ResponseBaseWrapper> GetRecommendWallpaperInfoAsync()
        {
            var count = await _dbContext.WallpaperInfos.Where(x => x.Recommend).CountAsync();
            var skip = Random.Shared.Next(count);
            var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Recommend).Skip(skip).FirstOrDefaultAsync();
            if (info is null)
            {
                count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
                skip = Random.Shared.Next(count);
                info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
            }
            return ResponseBaseWrapper.Ok(info!);
        }



        [HttpGet("recommend/redirect")]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> RedirectToRecommendWallpaperImageAsync()
        {
            var count = await _dbContext.WallpaperInfos.Where(x => x.Recommend).CountAsync();
            var skip = Random.Shared.Next(count);
            var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Recommend).Skip(skip).FirstOrDefaultAsync();
            if (info is null)
            {
                count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
                skip = Random.Shared.Next(count);
                info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
            }
            return RedirectToImage(info!.FileName!);
        }



        [HttpGet("random")]
        [ResponseCache(NoStore = true)]
        public async Task<ResponseBaseWrapper> GetRandomWallpaperInfoAsync()
        {
            var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
            var skip = Random.Shared.Next(count);
            var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
            return ResponseBaseWrapper.Ok(info!);
        }



        [HttpGet("random/redirect")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> RedirectToRandomWallpaperImageAsync()
        {
            var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
            var skip = Random.Shared.Next(count);
            var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(skip).FirstOrDefaultAsync();
            return RedirectToImage(info!.FileName!);
        }



        [HttpGet("next")]
        [ResponseCache(Duration = 86400)]
        public async Task<ResponseBaseWrapper> GetNextWallpaperInfoAsync([FromQuery] int lastId)
        {
            var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Id > lastId).FirstOrDefaultAsync();
            if (info == null)
            {
                info = await _dbContext.WallpaperInfos.AsNoTracking().FirstOrDefaultAsync();
            }
            return ResponseBaseWrapper.Ok(info!);
        }



        [HttpGet("list")]
        [ResponseCache(Duration = 86400)]
        public async Task<ResponseBaseWrapper> GetWallpaperInfosAsync([FromQuery] int page = 1)
        {
            var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
            var totalPage = count / 20 + 1;
            page = Math.Clamp(page, 1, totalPage);
            var infos = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Enable).Skip(20 * page - 20).Take(20).ToListAsync();
            return ResponseBaseWrapper.Ok(new WallpaperInfoList(page, totalPage, infos.Count, infos));
        }



        [HttpPost("ChangeRecommend")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> ChangeRecommendWallpaperAsync([FromQuery] int[] id)
        {
            if (HttpContext.Request.Headers["X-Secret"] != Environment.GetEnvironmentVariable("XSECRET"))
            {
                return BadRequest();
            }
            using var t = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                int rows = 0;
                await _dbContext.Database.ExecuteSqlRawAsync("UPDATE wallpapers SET Recommend=0 WHERE Recommend=1;");
                if (id?.Any() ?? false)
                {
                    if (id[0] != 0)
                    {
                        rows = await _dbContext.Database.ExecuteSqlRawAsync($"UPDATE wallpapers SET Recommend=1 WHERE Id IN ({string.Join(',', id)});");
                    }
                }
                else
                {
                    var count = await _dbContext.WallpaperInfos.Where(x => x.Enable).CountAsync();
                    var skip = Random.Shared.Next(count);
                    var info = await _dbContext.WallpaperInfos.AsNoTracking().Skip(skip).FirstOrDefaultAsync();
                    rows = await _dbContext.Database.ExecuteSqlRawAsync($"UPDATE wallpapers SET Recommend=1 WHERE Id = {info?.Id ?? 0};");
                }
                await t.CommitAsync();
                return Ok($"Affected rows: {rows}");
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }




        private IActionResult RedirectToImage(string fileName)
        {
            var url = $"https://file.xunkong.cc/wallpaper/{Uri.EscapeDataString(fileName)}";
            return Redirect(url);
        }


    }
}
