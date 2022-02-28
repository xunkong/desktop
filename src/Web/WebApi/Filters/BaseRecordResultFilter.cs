using Microsoft.AspNetCore.Mvc.Filters;
using Xunkong.Web.Api.Models;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Filters
{
    public class BaseRecordResultFilter : IAsyncResultFilter
    {


        private readonly ILogger<WishlogResultFilter> _logger;
        private readonly XunkongDbContext _dbContext;

        public BaseRecordResultFilter(ILogger<WishlogResultFilter> logger, XunkongDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }


        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var record = FilterHelper.GetRecordModel<BaseRecordModel>(context);
            try
            {
                _dbContext.AllRecords.Add(record);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in save record of base record filter.");
            }
            await next();
        }






    }
}

