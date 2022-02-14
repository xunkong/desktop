using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Filters;
using Xunkong.Web.Api.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Xunkong.Web.Api.Controllers
{

    /// <summary>
    /// 祈愿记录托管
    /// </summary>
    [ApiController]
    [ApiVersion("0.1")]
    [Route("v{version:ApiVersion}/[controller]")]
    [ServiceFilter(typeof(WishlogResultFilter))]
    [ServiceFilter(typeof(WishlogAuthActionFilter))]
    public class WishlogController : ControllerBase
    {
        private readonly ILogger<WishlogController> _logger;


        private readonly XunkongDbContext _dbContext;


        private readonly IMemoryCache _cache;


        public WishlogController(ILogger<WishlogController> logger, XunkongDbContext dbContext, IMemoryCache cache)
        {
            _cache = cache;
            _logger = logger;
            _dbContext = dbContext;
        }


        /// <summary>
        /// 获取数据，大于 EndId
        /// </summary>
        /// <param name="wishlog"></param>
        /// <returns></returns>
        [HttpPost("get")]
        public async Task<ResponseBaseWrapper> GetWishlogAsync([FromBody] WishlogCloudBackupRequestModel wishlog)
        {
            var uid = wishlog.Uid;
            var currentCount = await _dbContext.WishlogItems.Where(x => x.Uid == uid).CountAsync();
            if (currentCount == 0)
            {
                ResponseBaseWrapper.Ok(new WishlogCloudBackupResult(uid, 0, 0, 0, 0));
            }
            var list = await _dbContext.WishlogItems.AsNoTracking()
                                                    .Where(x => x.Uid == uid && x.Id > wishlog.LastId)
                                                    .OrderBy(x => x.Id)
                                                    .ToListAsync();
            var result = new WishlogCloudBackupResult(uid, currentCount, list.Count, 0, 0, list);
            return ResponseBaseWrapper.Ok(result);
        }


        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="wishlog"></param>
        /// <returns></returns>
        [HttpPost("put")]
        public async Task<ResponseBaseWrapper> UpdateWishlogAsync([FromBody] WishlogCloudBackupRequestModel wishlog)
        {
            var uid = wishlog.Uid;
            var list = wishlog.List?.Where(x => x.Uid == uid).ToList();
            if (list is null || !list.Any())
            {
                throw new XunkongException(ErrorCode.NoWishlogItem);
            }
            var existing = await _dbContext.WishlogItems.Where(x => x.Uid == uid).Select(x => x.Id).ToListAsync();
            var inserting = list.ExceptBy(existing, x => x.Id).ToList();
            foreach (var item in inserting)
            {
                item.QueryType = item.WishType switch
                {
                    WishType.CharacterEvent_2 => WishType.CharacterEvent,
                    _ => item.WishType,
                };
            }
            _dbContext.AddRange(inserting);
            var putCount = await _dbContext.SaveChangesAsync();
            var currentCount = await _dbContext.WishlogItems.Where(x => x.Uid == uid).CountAsync();
            var result = new WishlogCloudBackupResult(uid, currentCount, 0, putCount, 0, null);
            return ResponseBaseWrapper.Ok(result);
        }


        /// <summary>
        /// 删除指定用户的所有数据
        /// </summary>
        /// <param name="wishlog"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        public async Task<ResponseBaseWrapper> DeleteWishlogAsync([FromBody] WishlogCloudBackupRequestModel wishlog)
        {
            var uid = wishlog.Uid;
            using var t = await _dbContext.Database.BeginTransactionAsync();
            int deleteCount = 0;
            try
            {
                deleteCount = await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM wishlog_items WHERE Uid={uid};");
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception when deleting wishlog items where uid={uid} using transaction.", ex);
                await t.RollbackAsync();
                throw;
            }
            if (deleteCount == 0)
            {
                ResponseBaseWrapper.Ok(new WishlogCloudBackupResult(uid, 0, 0, 0, 0));
            }
            var result = new WishlogCloudBackupResult(uid, 0, 0, 0, deleteCount, null);
            return ResponseBaseWrapper.Ok(result);
        }



        /// <summary>
        /// 获取指定用户的祈愿统计和最后一条数据
        /// </summary>
        /// <param name="wishlog"></param>
        /// <returns></returns>
        [HttpPost("last")]
        public async Task<ResponseBaseWrapper> GetLastWishlogAsync([FromBody] WishlogCloudBackupRequestModel wishlog)
        {
            var uid = wishlog.Uid;
            var currentCount = await _dbContext.WishlogItems.Where(x => x.Uid == uid).CountAsync();
            if (currentCount == 0)
            {
                return ResponseBaseWrapper.Ok(new WishlogCloudBackupResult(uid, 0, 0, 0, 0));
            }
            var item = await _dbContext.WishlogItems.AsNoTracking().Where(x => x.Uid == uid).OrderBy(x => x.Id).LastOrDefaultAsync();
            var result = new WishlogCloudBackupResult(uid, currentCount, 1, 0, 0, new List<WishlogItem> { item! });
            return ResponseBaseWrapper.Ok(result);
        }




    }
}
