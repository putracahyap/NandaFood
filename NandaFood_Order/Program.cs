using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NandaFood_Auth.Data;
using NandaFood_Auth.Middlewares;
using NandaFood_Auth.Services;
using NandaFood_Menu.Data;
using NandaFood_Order.Data;

var builder = WebApplication.CreateBuilder(args);

// register DbContext
var connStringAuth = builder.Configuration.GetConnectionString("AuthConnection");
builder.Services.AddDbContext<NandaFoodAuthContext>(options => options.UseSqlServer(connStringAuth), ServiceLifetime.Transient);
var connStringMenu = builder.Configuration.GetConnectionString("MenuConnection");
builder.Services.AddDbContext<NandaFoodMenuContext>(options => options.UseSqlServer(connStringMenu), ServiceLifetime.Transient);
var connStringOrder = builder.Configuration.GetConnectionString("OrderConnection");
builder.Services.AddDbContext<NandaFoodOrderContext>(options => options.UseSqlServer(connStringOrder), ServiceLifetime.Transient);

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<TokenRevocationMiddleware>();

var tokenValidationParameters = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:Secret"] ?? string.Empty)),
    ValidateIssuer = true,
    ValidIssuer = builder.Configuration["JWT:Issuer"],
    ValidateAudience = true,
    ValidAudience = builder.Configuration["JWT:Audience"],
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
};

builder.Services.AddSingleton(tokenValidationParameters);

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = tokenValidationParameters;
});

// Add Authorization
builder.Services.AddAuthorization();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<TokenRevocationMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();