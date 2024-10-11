// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Load user secrets
builder.Configuration.AddUserSecrets<Program>();
//builder.Configuration.ConfigureAzureKeyVault();

// See: https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOutputCache();
//builder.Services.AddCrossOriginResourceSharing();
//builder.Services.AddAzureServices();
builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-TOKEN-HEADER"; options.FormFieldName = "X-CSRF-TOKEN-FORM"; });
builder.Services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseOutputCache();
app.UseRouting();
app.UseCors();
app.UseAntiforgery();
app.MapControllers();

app.Use(next => context =>
{
    var antiforgery = app.Services.GetRequiredService<IAntiforgery>();
    var tokens = antiforgery.GetAndStoreTokens(context);
    context.Response.Cookies.Append("XSRF-TOKEN", tokens?.RequestToken ?? string.Empty, new CookieOptions() { HttpOnly = false });
    return next(context);
});

app.Map("/", () => Results.Redirect("/swagger"));

// app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=ChatController}");

app.Run();