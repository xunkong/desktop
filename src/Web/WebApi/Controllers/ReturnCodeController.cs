using Microsoft.AspNetCore.Mvc;
using Xunkong.Core;
using Xunkong.Core.XunkongApi;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Xunkong.Web.Api.Controllers
{

    /// <summary>
    /// 返回值
    /// </summary>
    [ApiController]
    [ApiVersion("0.1")]
    [Route("v{version:ApiVersion}/[controller]")]
    public class ReturnCodeController : ControllerBase
    {



        /// <summary>
        /// 获取返回值格式，以及状态码列表
        /// </summary>
        /// <returns>retcode</returns>
        [HttpGet]
        [ResponseCache(Duration = 86400)]
        public ResponseBaseWrapper Get()
        {
            var dic = Enum.GetValues<ErrorCode>().ToDictionary(x => (int)x, x => x.ToDescriptionOrString());
            return new ResponseBaseWrapper(ErrorCode.Ok, null, new { CodeCount = dic.Count, Dic = dic });
        }

    }
}
