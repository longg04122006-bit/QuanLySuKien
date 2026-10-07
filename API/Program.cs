using BLL;
using DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controller
builder.Services.AddControllers();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("Chưa cấu hình Jwt:Key trong appsettings.json");
}

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

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});

// DAL + BLL
builder.Services.AddScoped<EventDAL>();
builder.Services.AddScoped<EventBLL>();

builder.Services.AddScoped<TicketTypeDAL>();
builder.Services.AddScoped<TicketTypeBLL>();

builder.Services.AddScoped<UserDAL>();

builder.Services.AddScoped<TicketDAL>();
builder.Services.AddScoped<TicketBLL>();
builder.Services.AddScoped<AttendeeDAL>();
builder.Services.AddScoped<AttendeeBLL>();
builder.Services.AddScoped<BookingDetailDAL>();
builder.Services.AddScoped<BookingDetailBLL>();
builder.Services.AddScoped<BookingDAL>();
builder.Services.AddScoped<BookingBLL>();
builder.Services.AddScoped<EventCategoryDAL>();
builder.Services.AddScoped<EventCategoryBLL>();
builder.Services.AddScoped<PaymentDAL>();
builder.Services.AddScoped<PaymentBLL>();
builder.Services.AddScoped<InvoiceDAL>();
builder.Services.AddScoped<InvoiceBLL>();
builder.Services.AddScoped<CheckInDAL>();
builder.Services.AddScoped<CheckInBLL>();
builder.Services.AddScoped<NotificationDAL>();
builder.Services.AddScoped<NotificationBLL>();
builder.Services.AddScoped<VenueDAL>();
builder.Services.AddScoped<VenueBLL>();
builder.Services.AddScoped<RoleDAL>();
builder.Services.AddScoped<RoleBLL>();
builder.Services.AddScoped<DAL.Helper.DatabaseHelper>();

builder.Services.AddScoped<AuditLogDAL>();
builder.Services.AddScoped<AuditLogBLL>();
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();