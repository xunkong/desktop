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
    [Route("v{version:ApiVersion}/Genshin/Metadata")]
    [ServiceFilter(typeof(BaseRecordResultFilter))]
    public class GenshinMetadataController : ControllerBase
    {
        private readonly ILogger<GenshinMetadataController> _logger;
        private readonly XunkongDbContext _dbContext;
        private readonly IMemoryCache _cache;

        public GenshinMetadataController(ILogger<GenshinMetadataController> logger, XunkongDbContext dbContext, IMemoryCache cache)
        {
            _logger = logger;
            _dbContext = dbContext;
            _cache = cache;
        }


        [HttpGet("character")]
        public async Task<ResponseBaseWrapper> GetCharacterInfos()
        {
            if (_cache.TryGetValue("characterinfos", out ResponseBaseWrapper result))
            {
                return result;
            }
            var list = await _dbContext.CharacterInfos.AsNoTracking().Where(x => x.Enable).ToListAsync();
            result = ResponseBaseWrapper.Ok(new { Count = list.Count, List = list });
            _cache.Set("characterinfos", result);
            return result;
        }


        [HttpGet("weapon")]
        public async Task<ResponseBaseWrapper> GetWeaponInfos()
        {
            if (_cache.TryGetValue("weaponinfos", out ResponseBaseWrapper result))
            {
                return result;
            }
            var list = await _dbContext.WeaponInfos.AsNoTracking().Where(x => x.Enable).ToListAsync();
            result = ResponseBaseWrapper.Ok(new { Count = list.Count, List = list });
            _cache.Set("weaponinfos", result);
            return result;
        }


        [HttpGet("wishevent")]
        public async Task<ResponseBaseWrapper> GetWishEventInfos()
        {
            if (_cache.TryGetValue("wisheventinfos", out ResponseBaseWrapper result))
            {
                return result;
            }
            var list = await _dbContext.WishEventInfos.AsNoTracking().ToListAsync();
            result = ResponseBaseWrapper.Ok(new { Count = list.Count, List = list });
            _cache.Set("wisheventinfos", result);
            return result;
        }


        [HttpGet("i18n")]
        public async Task<ResponseBaseWrapper> GetI18nModelsAsync()
        {
            if (_cache.TryGetValue("i18n", out ResponseBaseWrapper result))
            {
                return result;
            }
            var list = await _dbContext.I18nModels.AsNoTracking().Where(x => !string.IsNullOrWhiteSpace(x.zh_cn) && !string.IsNullOrWhiteSpace(x.en_us)).ToListAsync();
            result = ResponseBaseWrapper.Ok(new { Count = list.Count, List = list });
            _cache.Set("i18n", result);
            return result;
        }

    }
}
