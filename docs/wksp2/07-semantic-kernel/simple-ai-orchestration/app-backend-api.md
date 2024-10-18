# Creating the Backend API

These changes are already available in the repository. These instructions walk
you through the process followed for create the backend API from the Console application:

1. Start by creating a new directory:

    ```bash
    mkdir -p workshop/dotnet/App/backend
    cd workshop/dotnet/App/backend
    ```

1. Next create a new SDK .NET project:

    ```bash
    dotnet new webapi -n MinimialApi
    ```

1. Add the following nuget packages:

    ```bash
    dotnet add package Microsoft.AspNetCore.Mvc
    dotnet add package Swashbuckle.AspNetCore
    ```

1. Create a new `Program.cs` in the project directory. This file initializes and loads
   the required services and configuration for the API, namely configuring CORS protection,
   enabling controllers for the API and exposing Swagger document:

   ```csharp
    using Microsoft.AspNetCore.Antiforgery;
    using Extensions;

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    // See: https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddOutputCache();
    builder.Services.AddAntiforgery(options => { 
        options.HeaderName = "X-CSRF-TOKEN-HEADER"; 
        options.FormFieldName = "X-CSRF-TOKEN-FORM"; });
    builder.Services.AddHttpClient();
    builder.Services.AddDistributedMemoryCache();
    // Add Semantic Kernel services
    builder.Services.AddSkServices();

    // Load user secrets
    builder.Configuration.AddUserSecrets<Program>();

    var app = builder.Build();
    app.UseSwagger();
    app.UseSwaggerUI();
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

    app.MapControllerRoute(
        "default",
        "{controller=ChatController}");

    app.Run();
   ```
