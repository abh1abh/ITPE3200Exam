

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

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

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


builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlite(builder.Configuration["ConnectionStrings:AppDbContextConnection"]); });

builder.Services.AddDbContext<AuthDbContext>(options => {
    options.UseSqlite(builder.Configuration["ConnectionStrings:AuthDbContextConnection"]);});

builder.Services.AddIdentity<AuthUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();


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

builder.Services.AddScoped<IHealthcareWorkerService, HealthcareWorkerService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IAvailableSlotService, AvailableSlotService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();



builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IHealthcareWorkerRepository, HealthcareWorkerRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentTaskRepository, AppointmentTaskRepository>();
builder.Services.AddScoped<IAvailableSlotRepository, AvailableSlotRepository>();
builder.Services.AddScoped<IChangeLogRepository, ChangeLogRepository>();


builder.Services.AddAuthorization(options =>
{
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

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information() // levels: Trace< Information < Warning < Erorr < Fatal
    .WriteTo.File($"../APILogs/app_{DateTime.Now:yyyyMMdd_HHmmss}.log")
    .Filter.ByExcluding(e => e.Properties.TryGetValue("SourceContext", out var value) &&
                            e.Level == LogEventLevel.Information &&
                            e.MessageTemplate.Text.Contains("Executed DbCommand"));
var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var reset = builder.Configuration.GetValue<bool>("ResetDatabasesOnStartup");

    var authDb = sp.GetRequiredService<AuthDbContext>();
    var appDb  = sp.GetRequiredService<AppDbContext>();

    if (reset) // If we need to reset, set flag in appsettings.Development.ResetDatabasesOnStartup = true
    {
        
        await authDb.Database.EnsureDeletedAsync();
        await appDb.Database.EnsureDeletedAsync();
    }

    // Always migrate (safe in prod/dev)
    await authDb.Database.MigrateAsync();
    await appDb.Database.MigrateAsync();

    // Seed identity first (roles/users), then domain data
    SeedResult seedResult =  await AuthDbInit.SeedAsync(sp);
    await DBInit.SeedAsync(sp, seedResult);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // await DBInit.SeedAsync(app);
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseStaticFiles();
app.UseRouting();

app.UseCors("CorsPolicy");

// Correct order:
// app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
