using GoodWareMixApi.Interfaces;
using GoodWareMixApi.Intrefaces;
using GoodWareMixApi.Quartz;
using GoodWareMixApi.Repositories;
using GoodWareMixApi.Scheduler.SchedulTask;
using GoodWareMixApi.Service;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using Quartz;
using Serilog;
using Serilog.Formatting.Json;
using System.Collections.Specialized;
using System.Net.WebSockets;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console();
    config.WriteTo.File("logs.txt");
    config.WriteTo.File(new JsonFormatter(),"logJson.json");
});


builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("GoodWareMixDatabase"));
Connection.context = new MongoDBService("WebApiDatabase");//название базы 
Connection.InternalCodeAPI = "http://172.16.41.246:3005/api/";
Connection.URLPagination = "https://localhost:7105"; //Путь для клиентского приложения ( для пагинации )


ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});

builder.Services.Configure<FormOptions>(options =>
{
    // Set the limit to 256 MB
    options.MultipartBodyLengthLimit = 268435456;
});
// Add services to the container.
builder.Services.AddAuthentication(
        CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMvc().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});


builder.Services.AddTransient<ISupplierRepository, SupplierRepository>();
builder.Services.AddTransient<ILogRepository, LogRepository>();
//builder.Services.AddSingleton<IHostedService, TaskSchedul>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IDocumentRepository, DocumentRepository>();
builder.Services.AddTransient<IAttributeRepository, AttributeRepository>();
builder.Services.AddTransient<ISchedulerTaskRepository, SchedulerTaskRepository>();

builder.Services.AddCors(policy =>
{
    policy.AddPolicy("AllowBlazor", opt => opt.AllowAnyOrigin().AllowAnyHeader());
});


////builder.Services.AddTransient<JobFactory>();
builder.Services.AddScoped<JobSupplier>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options=>
{
    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddSingleton<IUriService>(o =>
{
    var accessor = o.GetRequiredService<IHttpContextAccessor>();
    var request = accessor.HttpContext.Request;
    var uri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
    return new UriService(uri);
});

var configuration = new ConfigurationBuilder().AddJsonFile(".\\appsettings.json").Build();
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
builder.Logging.AddSerilog(Log.Logger);
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowBlazor");
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();
var wsOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
};
app.UseWebSockets(wsOptions);


Log.Information("Application starting Up");

Task.Delay(3000)
   .ContinueWith(t =>
   {
       StartSheduler.Start();
   }); //запуск задач Sheduler'a при старте серверного приложения
app.Run();

