using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Models;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Filters
{
    public class WishlogResultFilter : IAsyncResultFilter
    {


        private readonly ILogger<WishlogResultFilter> _logger;
        private readonly XunkongDbContext _dbContext;

        public WishlogResultFilter(ILogger<WishlogResultFilter> logger, XunkongDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }


        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var record = FilterHelper.GetRecordModel<WishlogRecordModel>(context);
            var operation = record.Path?.Substring(record.Path.LastIndexOf('/') + 1).ToLower();
            if (context.Result is ObjectResult j)
            {
                if (j.Value is ResponseBaseWrapper data)
                {
                    if (data.Data is WishlogCloudBackupResult result)
                    {
                        record.Uid = result.Uid;
                        record.Operation = operation;
                        record.CurrentCount = result.CurrentCount;
                        record.GetCount = result.GetCount;
                        record.PutCount = result.PutCount;
                        record.DeleteCount = result.DeleteCount;
                    }
                };
            }
            try
            {
                _dbContext.WishlogRecords.Add(record);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in save record of wishlog result filter.");
            }
            await next();
        }


    }
}

