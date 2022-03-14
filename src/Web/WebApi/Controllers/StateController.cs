using Microsoft.AspNetCore.Mvc;
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


        public StateController()
        {
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
            return new(0, "Cache cleared.");
        }






    }
}
