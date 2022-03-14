using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Filters;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.1")]
    [Route("v{version:ApiVersion}/Genshin/Metadata")]
    [ServiceFilter(typeof(BaseRecordResultFilter))]
    [ResponseCache(Duration = 900)]
    public class GenshinMetadataController : ControllerBase
    {
        private readonly ILogger<GenshinMetadataController> _logger;
        private readonly XunkongDbContext _dbContext;

        public GenshinMetadataController(ILogger<GenshinMetadataController> logger, XunkongDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }


        [HttpGet("character")]
        public async Task<ResponseBaseWrapper> GetCharacterInfos()
        {
            var list = await _dbContext.CharacterInfos.AsNoTracking().Where(x => x.Enable).ToListAsync();
            return ResponseBaseWrapper.Ok(new { Count = list.Count, List = list });
        }


        [HttpGet("weapon")]
        public async Task<ResponseBaseWrapper> GetWeaponInfos()
        {
            var list = await _dbContext.WeaponInfos.AsNoTracking().Where(x => x.Enable).ToListAsync();
            return ResponseBaseWrapper.Ok(new { Count = list.Count, List = list });
        }


        [HttpGet("wishevent")]
        public async Task<ResponseBaseWrapper> GetWishEventInfos()
        {
            var list = await _dbContext.WishEventInfos.AsNoTracking().ToListAsync();
            return ResponseBaseWrapper.Ok(new { Count = list.Count, List = list });
        }


        [HttpGet("i18n")]
        public async Task<ResponseBaseWrapper> GetI18nModelsAsync()
        {
            var list = await _dbContext.I18nModels.AsNoTracking().Where(x => !string.IsNullOrWhiteSpace(x.zh_cn) && !string.IsNullOrWhiteSpace(x.en_us)).ToListAsync();
            return ResponseBaseWrapper.Ok(new { Count = list.Count, List = list });
        }

    }
}
