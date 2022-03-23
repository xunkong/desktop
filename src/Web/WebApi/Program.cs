using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Serilog;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Controllers;
using Xunkong.Web.Api.Filters;
using Xunkong.Web.Api.Services;

// net6限定：避免因为启用剪裁，在efcore的使用中出现「找不到System.DateOnly相关方法」的异常
DateOnly.FromDayNumber(1).AddYears(1).AddMonths(1).AddDays(1);
StateController.StartTime = DateTime.Now;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ControllerExceptionFilter>();
});
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v0.1", new()
    {
        Title = "Xunkong Web Api",
        Version = "v0.1",
    });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Xunkong.Web.Api.xml"), true);
});

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});



builder.Services.AddResponseCompression();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<WishlogAuthActionFilter>();
builder.Services.AddScoped<BaseRecordResultFilter>();
builder.Services.AddScoped<WishlogResultFilter>();
builder.Services.AddDbContextPool<XunkongDbContext>(options =>
{
#if DEBUG
    options.UseMySql(builder.Configuration.GetConnectionString("constr_xunkong"), new MySqlServerVersion("8.0.27"));
#else
    options.UseMySql(Environment.GetEnvironmentVariable("CONSTR"), new MySqlServerVersion("8.0.25"));
#endif
});

#if DEBUG
builder.Services.AddSingleton(new DbConnectionFactory(builder.Configuration.GetConnectionString("constr_xunkong")));
#else
builder.Services.AddSingleton(new DbConnectionFactory(Environment.GetEnvironmentVariable("CONSTR")));
#endif

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
});

//builder.Services.AddLogging(builder => builder.AddSimpleConsole(c => { c.ColorBehavior = LoggerColorBehavior.Enabled; c.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff zzz\n"; }));

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = (context) => throw new XunkongException(ErrorCode.InvalidModelException));

builder.Services.AddHttpLogging(options => options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders);

builder.Services.AddHttpClient<WishlogClient>()
                .ConfigureHttpClient(client => client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br"))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });

builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));

builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

builder.Services.AddInMemoryRateLimiting();

builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();


var app = builder.Build();

// 异常捕获
app.UseExceptionHandler(c => c.Run(async context =>
{
    var feature = context.Features.Get<IExceptionHandlerPathFeature>();
    var ex = feature?.Error;
    app.Services.GetService<Microsoft.Extensions.Logging.ILogger>()?.LogError(ex, "Exception middleware");
    var result = new ResponseBaseWrapper(ErrorCode.InternalException, ex?.Message, new ExceptionResult(ex?.GetType()?.ToString(), ex?.Message, ex?.StackTrace));
    context.Response.StatusCode = 500;
    await context.Response.WriteAsJsonAsync(result);
}));

// 请求速率限制
app.UseIpRateLimiting();

// 阻止http请求
app.Use(async (context, next) =>
{
    if (context.Request.Headers["X-Forwarded-Proto"] == "http")
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Please use https protocol.");
        return;
    }
    await next.Invoke();
});


app.UseResponseCompression();

app.UseHttpLogging();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v0.1/swagger.json", "Xunkong Web Api v0.1");
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();

