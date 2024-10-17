using Microsoft.AspNetCore.HttpOverrides;
using MoonEnergy.Sso;
using MoonEnergy.Sso.Pages;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLogging(loggingBuilder => { loggingBuilder.AddApplicationInsights(); });

builder.Services.AddRazorPages();

builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader())
);

builder.Services.AddIdentityServer()
    .AddInMemoryIdentityResources(Resources.Identity)
    .AddInMemoryApiScopes(Resources.ApiScopes)
    .AddInMemoryApiResources(Resources.ApiResources)
    .AddInMemoryClients(Clients.List)
    .AddTestUsers(TestUsers.Users);
//    .AddProfileService<CustomProfileService>();

builder.Services.AddAuthorization();

var app = builder.Build();

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "false")
{
    if (!app.Environment.IsDevelopment())
    {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
}

var fordwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
fordwardedHeaderOptions.KnownNetworks.Clear();
fordwardedHeaderOptions.KnownProxies.Clear();

app.UseForwardedHeaders(fordwardedHeaderOptions);

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.Run();