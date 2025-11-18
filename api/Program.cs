

using System.Security.Claims;
using System.Text;
using api;
using api.DAL;
using api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add NewtonsoftJson to handle reference loops
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Homecare Appointment Management API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Authorization: Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{ new OpenApiSecurityScheme
        { Reference = new OpenApiReference
            { Type = ReferenceType.SecurityScheme,
                Id = "Bearer"}},
        new string[] { }
        }});
});

// Configure DbContexts for Application and Identity
builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlite(builder.Configuration["ConnectionStrings:AppDbContextConnection"]); });

builder.Services.AddDbContext<AuthDbContext>(options => {
    options.UseSqlite(builder.Configuration["ConnectionStrings:AuthDbContextConnection"]);});

// Configure Identity with AuthUser and IdentityRole
builder.Services.AddIdentity<AuthUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register application services
builder.Services.AddScoped<IHealthcareWorkerService, HealthcareWorkerService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IAvailableSlotService, AvailableSlotService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register repositories
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IHealthcareWorkerRepository, HealthcareWorkerRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentTaskRepository, AppointmentTaskRepository>();
builder.Services.AddScoped<IAvailableSlotRepository, AvailableSlotRepository>();
builder.Services.AddScoped<IChangeLogRepository, ChangeLogRepository>();

// Configure Authentication and Authorization with JWT 
builder.Services.AddAuthorization(options =>
{
    // Define policies for roles. Helped by AI to add these policies.
    options.AddPolicy("IsAdmin", p => p.RequireRole("Admin"));
    options.AddPolicy("IsHealthcareWorker", p => p.RequireRole("HealthcareWorker"));
    options.AddPolicy("IsClient", p => p.RequireRole("Client"));
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT Key not found")
        )),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };
});

// Configure Serilog for logging 
var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information() 
    .WriteTo.File($"../APILogs/app_{DateTime.Now:yyyyMMdd_HHmmss}.log")
    .Filter.ByExcluding(e => e.Properties.TryGetValue("SourceContext", out var value) &&
                            e.Level == LogEventLevel.Information &&
                            e.MessageTemplate.Text.Contains("Executed DbCommand"));
var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);

// Build the application
var app = builder.Build();

// Initialize and seed databases 
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider; // Get the service provider
    // Check if we need to reset databases by reading dev settings
    var reset = builder.Configuration.GetValue<bool>("ResetDatabasesOnStartup"); 

    // Get the database contexts
    var authDb = sp.GetRequiredService<AuthDbContext>();
    var appDb = sp.GetRequiredService<AppDbContext>();

    // If appsettings.Development.ResetDatabasesOnStartup = true we reset databases
    if (reset) 
    {
        await authDb.Database.EnsureDeletedAsync();
        await appDb.Database.EnsureDeletedAsync();
    }

    // Always migrate 
    await authDb.Database.MigrateAsync();
    await appDb.Database.MigrateAsync();

    // Seed identity first (roles/users), returns SeedResult, then domain data
    SeedResult seedResult = await AuthDbInit.SeedAsync(sp);
    await DBInit.SeedAsync(sp, seedResult);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable serving static files
app.UseStaticFiles();

// Enable Routing
app.UseRouting();

// Enable CORS 
app.UseCors("CorsPolicy");

// Enable Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Run the application
app.Run();
