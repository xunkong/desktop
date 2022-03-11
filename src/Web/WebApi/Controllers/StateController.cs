using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Web.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("0")]
    [ApiVersionNeutral]
    public class StateController : ControllerBase
    {


        public static DateTime StartTime;

        private static DateTime lastUpdateTime = new FileInfo(typeof(StateController).Assembly.Location).CreationTime;


        private readonly IMemoryCache _cache;

        public StateController(IMemoryCache cache)
        {
            _cache = cache;
        }


        /// <summary>
        /// 返回函数实例的状态，并保持函数实例长时间运行
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(NoStore = true)]
        public ResponseBaseWrapper GetState()
        {
            var data = new ResponseBaseWrapper(ErrorCode.Ok, "Api instance has started.", new { LastUpdateTime = lastUpdateTime, RunningTime = DateTime.Now - StartTime });
            return data;
        }


        [HttpPost("ClearCache")]
        public ResponseBaseWrapper ClearCache()
        {
            (_cache as MemoryCache)?.Compact(1);
            return new(0, "Cache cleared.");
        }






    }
}
