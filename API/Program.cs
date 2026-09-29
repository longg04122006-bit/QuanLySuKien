using BLL;
using DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// =========================
// 1. Đăng ký Controller
// =========================
builder.Services.AddControllers();

// =========================
// 2. Đăng ký JWT Authentication
// =========================
builder.Services.AddAuthentication(options =>
{
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// =========================
// 2. Đăng ký DAL
// =========================
builder.Services.AddScoped<EventDAL>();
builder.Services.AddScoped<EventBLL>();
builder.Services.AddScoped<UserDAL>();
builder.Services.AddScoped<DAL.Helper.DatabaseHelper>();

// =========================
// 3. Swagger
// =========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =========================
// 4. CORS
// =========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// =========================
// 5. Swagger
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =========================
// 6. HTTPS
// =========================
app.UseHttpsRedirection();

// =========================
// 7. CORS
// =========================
app.UseCors("AllowAll");

// =========================
// 8. Authorization
// =========================
app.UseAuthorization();

// =========================
// 9. Controller
// =========================
app.MapControllers();

app.Run();