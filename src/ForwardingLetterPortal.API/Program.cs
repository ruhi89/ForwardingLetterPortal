using ForwardingLetterPortal.API.Infrastructure;
using ForwardingLetterPortal.DataAccess;
using ForwardingLetterPortal.DataAccess.Repositories;
using ForwardingLetterPortal.Interface.Interfaces;
using ForwardingLetterPortal.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<OfficerRepository>();
builder.Services.AddScoped<CatalogRepository>();
builder.Services.AddScoped<VerificationRepository>();
builder.Services.AddScoped<LetterRepository>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOfficerService, OfficerService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<ILetterService, LetterService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // A token is only honoured while the officer is still active and still has the role in the token,
        // so deactivating or demoting an officer takes effect immediately.
        opt.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var idClaim = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var roleClaim = ctx.Principal?.FindFirstValue(ClaimTypes.Role);

                var currentRole = int.TryParse(idClaim, out var id)
                    ? await db.Officers.Where(o => o.OfficerId == id && o.IsActive)
                        .Select(o => o.Role).FirstOrDefaultAsync()
                    : null;

                if (currentRole is null || currentRole != roleClaim)
                    ctx.Fail("Officer account is inactive or its role has changed.");
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(opt =>
    opt.AddPolicy("AllowFrontend", p =>
        p.WithOrigins("http://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the JWT token only (no 'Bearer ' prefix)."
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

// First-run bootstrap: create the initial Super Admin from the "SeedSuperAdmin" config section.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var created = await DbSeeder.SeedSuperAdminAsync(
            scope.ServiceProvider.GetRequiredService<OfficerRepository>(), app.Configuration);
        if (created) app.Logger.LogInformation("Seeded initial Super Admin account.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not seed the Super Admin (is the database created?).");
    }
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
