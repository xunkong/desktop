using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Models;

namespace Xunkong.Web.Api.Filters
{
    public static class FilterHelper
    {


        public static T GetRecordModel<T>(ResultExecutingContext context) where T : BaseRecordModel, new()
        {
            var requestId = context.HttpContext.Request.Headers["X-Fc-Request-Id"];
            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString.Value;
            var ua = context.HttpContext.Request.Headers["User-Agent"];
            var ip = context.HttpContext.Request.Headers["X-Forwarded-For"];
            var deviceId = context.HttpContext.Request.Headers["X-Device-Id"];
            var platform = context.HttpContext.Request.Headers["X-Platform"];
            var channel = context.HttpContext.Request.Headers["X-Channel"];
            var version = context.HttpContext.Request.Headers["X-Version"];
            var record = new T
            {
                RequestId = string.IsNullOrWhiteSpace(requestId) ? Guid.NewGuid().ToString("D") : requestId,
                DateTime = DateTimeOffset.UtcNow,
                Path = path,
                Method = method,
                StatusCode = 200,
                DeviceId = deviceId,
                UserAgent = ua,
                Ip = ip,
                Platform = platform,
                Channel = channel,
                Version = version,
            };
            if (context.Result is ObjectResult j)
            {
                if (j.Value is ResponseBaseWrapper data)
                {
                    record.ReturnCode = data.Code;
                    record.Message = data.Message;
                };
            }
            return record;
        }


    }
}
