using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using MoonEnergy;
using MoonEnergy.Chat;
using MoonEnergy.Chat.Base;
using MoonEnergy.Chat.ChatTools;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Services.AddBff();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookie";
    options.DefaultChallengeScheme = "oidc";
    options.DefaultSignOutScheme = "oidc";
}).AddCookie("cookie", options =>
{
    options.Cookie.Name = "__Host-bff";
    options.Cookie.SameSite = SameSiteMode.Strict; // Strict in PROD or make vuejs proxy HTTPS
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
}).AddOpenIdConnect("oidc", options =>
{
    options.Authority = builder.Configuration.GetValue<string>("Authentication:Authority");
    options.ClientId = builder.Configuration.GetValue<string>("Authentication:ClientId");
    options.ClientSecret = builder.Configuration.GetValue<string>("Authentication:ClientSecret");
    options.ResponseType = "code";
    options.ResponseMode = "query";

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.GetClaimsFromUserInfoEndpoint = true;
    options.MapInboundClaims = false;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAuthorization();

builder.Services.Configure<OpenAiConfig>(builder.Configuration.GetSection("OpenAI"));

builder.Services.AddSingleton<IChatTool, GetWeatherTool>();
builder.Services.AddSingleton<IChatTool, GetTermijnbedragTool>();
builder.Services.AddSingleton<IChatTool, SetTermijnbedragTool>();
builder.Services.AddSingleton<IChatTool, LoginTool>();
builder.Services.AddSingleton<IChatTool, GetEnergyConsumptionTool>();

builder.Services.AddSingleton<ChatService>();

builder.Services.AddSpaStaticFiles(configuration => { configuration.RootPath = "clientapp"; });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSpaStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseBff();
app.UseAuthorization();
app.MapControllers();

app.UseSpa(spa =>
{
    //spa.Options.SourcePath = "clientapp";

    //if (env.IsDevelopment())
    //{
    //    // Development requests are send through to local node server
    //    spa.UseProxyToSpaDevelopmentServer("http://localhost:8080/");
    //}
});

app.MapBffManagementEndpoints();

app.Run();