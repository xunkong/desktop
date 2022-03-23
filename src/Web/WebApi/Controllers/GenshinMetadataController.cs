using Dapper;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunkong.Core.Metadata;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Filters;
using Xunkong.Web.Api.Models;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.1")]
    [Route("v{version:ApiVersion}/Genshin/Metadata")]
    [ServiceFilter(typeof(BaseRecordResultFilter))]
    [ResponseCache(Duration = 3600)]
    public class GenshinMetadataController : ControllerBase
    {
        private readonly ILogger<GenshinMetadataController> _logger;
        private readonly XunkongDbContext _dbContext;
        private readonly DbConnectionFactory _dbFactory;


        public GenshinMetadataController(ILogger<GenshinMetadataController> logger, XunkongDbContext dbContext, DbConnectionFactory dbFactory)
        {
            _logger = logger;
            _dbContext = dbContext;
            _dbFactory = dbFactory;
        }


        private string ParseLanguage(string? language)
        {
            return language?.ToLower() switch
            {
                "de-de" or "de" => "de-de",
                "en-us" or "en" => "en-us",
                "es-es" or "es" => "es-es",
                "fr-fr" or "fr" => "fr-fr",
                "id-id" or "id" => "id-id",
                "ja-jp" or "ja" => "ja-jp",
                "ko-kr" or "ko" => "ko-kr",
                "pt-pt" or "pt" => "pt-pt",
                "ru-ru" or "ru" => "ru-ru",
                "th-th" or "th" => "th-th",
                "vi-vn" or "vi" => "vi-vn",
                "zh-tw" or "cht" => "zh-tw",
                _ => "zh-cn",
            };
        }


        [HttpGet("character")]
        public async Task<ResponseBaseWrapper> GetCharacterInfos([FromQuery] string? language = null)
        {
            var lang = ParseLanguage(language);
            var characters = await _dbContext.CharacterInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Talents).Include(x => x.Constellations).ToListAsync();
            if (lang != "zh-cn")
            {
                await ConverterToAnotherLanguage(characters, lang);
            }
            var list = characters.Adapt<List<CharacterInfo>>();
            return ResponseBaseWrapper.Ok(new { Language = lang, Count = list.Count, List = list });
        }


        [HttpGet("weapon")]
        public async Task<ResponseBaseWrapper> GetWeaponInfos([FromQuery] string? language = null)
        {
            var lang = ParseLanguage(language);
            var weapons = await _dbContext.WeaponInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Skills).ToListAsync();
            if (lang != "zh-cn")
            {
                await ConverterToAnotherLanguage(weapons, lang);
            }
            var list = weapons.Adapt<List<WeaponInfo>>();
            return ResponseBaseWrapper.Ok(new { Language = lang, Count = list.Count, List = list });
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
            //var list = await _dbContext.I18nModels.AsNoTracking().Where(x => !string.IsNullOrWhiteSpace(x.zh_cn) && !string.IsNullOrWhiteSpace(x.en_us)).ToListAsync();
            //return ResponseBaseWrapper.Ok(new { Count = list.Count, List = list });
            return await Task.FromResult(ResponseBaseWrapper.Ok(new { Count = 0, List = new List<I18nModel>() }));
        }


        private async Task ConverterToAnotherLanguage(IEnumerable<CharacterInfoModel> characterInfoModels, string lang)
        {
            using var dapper = _dbFactory.CreateDbConnection();
            var dy = await dapper.QueryAsync<(long Id, string? Text)>($"SELECT Id,{lang.Replace('-', '_')} FROM textmaps WHERE Type & 0x0F;");
            var dic = dy.ToDictionary(x => x.Id, x => x.Text);
            foreach (var item in characterInfoModels)
            {
                item.Name = dic[item.NameTextMapHash];
                item.Title = dic[item.TitleTextMapHash];
                item.Description = dic[item.DescTextMapHash];
                item.Affiliation = dic[item.AffiliationTextMapHash];
                item.ConstllationName = dic[item.ConstllationTextMapHash];
                item.CvChinese = dic[item.CvChineseTextMapHash];
                item.CvJapanese = dic[item.CvJapaneseTextMapHash];
                item.CvEnglish = dic[item.CvEnglishTextMapHash];
                item.CvKorean = dic[item.CvKoreanTextMapHash];
            }
            foreach (var item in characterInfoModels.SelectMany(x => x.Talents ?? new()))
            {
                item.Name = dic[item.NameTextMapHash];
                item.Description = dic[item.DescTextMapHash];
            }
            foreach (var item in characterInfoModels.SelectMany(x => x.Constellations ?? new()))
            {
                item.Name = dic[item.NameTextMapHash];
                item.Description = dic[item.DescTextMapHash];
            }
        }


        private async Task ConverterToAnotherLanguage(IEnumerable<WeaponInfoModel> weaponInfoModels, string lang)
        {
            using var dapper = _dbFactory.CreateDbConnection();
            lang = lang.Replace('-', '_');
            var dy = await dapper.QueryAsync<(long Id, string? Text)>($"SELECT Id,{lang} FROM textmaps WHERE Type & 0xF0;");
            var dic = dy.ToDictionary(x => x.Id, x => x.Text);
            foreach (var item in weaponInfoModels)
            {
                item.Name = dic[item.NameTextMapHash];
                item.Description = dic[item.DescTextMapHash];
            }
            foreach (var item in weaponInfoModels.SelectMany(x => x.Skills ?? new()))
            {
                item.Name = dic[item.NameTextMapHash];
                item.Description = dic[item.DescTextMapHash];
            }
            var dy2 = await dapper.QueryAsync<(int Id, string? Text)>($"select r.Id,t.{lang} from readables r left join readabletextmaps t on r.ContentId=t.Id;");
            var dic2 = dy2.ToDictionary(x => x.Id, x => x.Text);
            foreach (var item in weaponInfoModels)
            {
                item.Story = dic2[item.StoryId];
            }
        }



        [HttpGet("raw/character")]
        public async Task<ResponseBaseWrapper> GetRawCharacterAsync()
        {
            var list = await _dbContext.CharacterInfos.AsNoTracking().Where(x => x.Enable).Include(x => x.Talents).Include(x => x.Constellations).ToListAsync();
            return ResponseBaseWrapper.Ok(new { Language = "zh-cn", Count = list.Count, List = list });
        }




        [HttpGet("raw/i18n")]
        public async Task<ResponseBaseWrapper> GetRawI18nModelAsync()
        {
            var list = await _dbContext.I18nModels.AsNoTracking().ToListAsync();
            return ResponseBaseWrapper.Ok(new { Count = list.Count, List = list });
        }




    }
}
