using Microsoft.EntityFrameworkCore;
using SpaceRythm.Data;
using SpaceRythm.Util;
using Microsoft.Extensions.Options;
using SpaceRythm.Interfaces;
using SpaceRythm.Services;
using SpaceRythm.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using SpaceRythm.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using SpaceRythm.Pages;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;



var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders(); 
builder.Logging.AddConsole(); 

// Configure services
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var dbInitializer = services.GetRequiredService<DbInitializer>();
        await dbInitializer.Initialize();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

app.Use(async (context, next) =>
{
    var sessionStartTime = DateTime.UtcNow;

    await next.Invoke();

    var sessionEndTime = DateTime.UtcNow;
});

ConfigureMiddleware(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettingsSection = configuration.GetSection("JwtSettings");
    var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

    services.Configure<JwtSettings>(jwtSettingsSection);

    var connectionString = configuration.GetConnectionString("DefaultConnection");
   
    services.AddDbContext<MyDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    services.AddHttpContextAccessor();

    services.AddDbContextFactory<MyDbContext>(options =>
     options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
     ServiceLifetime.Scoped);

    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
    });

    //services.AddHttpClient();
    services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp",
            builder => builder.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    });
    var emailOptions = builder.Configuration.GetSection("EmailSender").Get<EmailHelperOptions>() ?? throw new InvalidOperationException("Email Sender options not found.");

    var requireEmailConfirmed = configuration.GetValue<bool>("RequireConfirmedEmail");

    services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

    services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),

            NameClaimType = ClaimTypes.NameIdentifier
        };
    })
    .AddCookie()
    .AddGoogle("Google", options =>
    {
        options.ClientId = configuration["Google:ClientId"];
        options.ClientSecret = configuration["Google:ClientSecret"];
        options.CallbackPath = "/users/google-response";

        options.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                var state = context.Properties.Items["state"];
                context.HttpContext.Session.SetString("oauth_state", state);
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnCreatingTicket = context =>
            {
                var expectedState = context.HttpContext.Session.GetString("oauth_state");
                if (context.Properties.Items["state"] != expectedState)
                {
                    context.Fail("Invalid state.");
                }
                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                return Task.CompletedTask;
            }
        };
    })
    .AddFacebook(options =>
    {
        options.AppId = configuration["Facebook:AppId"];
        options.AppSecret = configuration["Facebook:AppSecret"];
        options.CallbackPath = "/users/facebook-response";

        options.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                var state = context.Properties.Items["state"];
                context.HttpContext.Session.SetString("oauth_state", state);
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnCreatingTicket = context =>
            {
                var expectedState = context.HttpContext.Session.GetString("oauth_state");
                if (context.Properties.Items["state"] != expectedState)
                {
                    context.Fail("Invalid state.");
                }
                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                return Task.CompletedTask;
            }
        };
    });

    services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminPolicy", policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy("UserPolicy", policy =>
            policy.RequireRole("User", "Admin"));
    });

    services.AddEmailHelper(emailOptions);

    services.Configure<PasswordOptions>(options =>
    {
        options.RequireDigit = true;
        options.RequireNonAlphanumeric = true;
        options.RequiredLength = 10;
    });

    services.AddScoped<IUserService, UserService>();
    services.AddScoped<TokenService>();
    services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    services.AddScoped<IUserStatisticsService, UserStatisticsService>();
    services.AddScoped<ITrackService, TrackService>();
    services.AddScoped<IArtistService, ArtistService>();
    services.AddScoped<ITrackCategoryService, TrackCategoryService>();
    services.AddScoped<IFileService, FileService>();
    services.AddScoped<DbInitializer>();
    services.AddScoped<IPlaylistService, PlaylistService>();
    services.AddScoped<IListeningService, ListeningService>();
    services.AddScoped<ILikeService, LikeService>();
    services.AddScoped<ICommentService, CommentService>();
    services.AddScoped<IFollowerService, FollowerService>();
    services.AddScoped<IListeningHistoryService, ListeningHistoryService>();
    services.AddScoped<ICommentLikeService, CommentLikeService>();
    services.AddScoped<RssService>();

    services.AddHttpClient<TestUsersModel>();

    services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
    services.AddRazorPages();
}

void ConfigureMiddleware(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseCors("AllowReactApp");
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();

    app.Use(async (context, next) =>
    {
        await next.Invoke();
    });

    app.UseSession();

    app.UseCookiePolicy(new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.Lax,
        Secure = CookieSecurePolicy.Always,
    });

    app.UseAuthentication();

    app.UseAuthorization();
    //app.UseMiddleware<JwtMiddleware>();

    app.MapRazorPages();
    app.MapControllers();
}



