
var builder = WebApplication.CreateBuilder(args);

// =========================
// 1. Đăng ký Controller
// =========================
builder.Services.AddControllers();

// =========================
// 2. Swagger / OpenAPI
// =========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =========================
// 3. CORS
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
// 4. Swagger
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =========================
// 5. HTTPS
// =========================
app.UseHttpsRedirection();

// =========================
// 6. CORS
// =========================
app.UseCors("AllowAll");

// =========================
// 7. Authorization
// =========================
app.UseAuthorization();

// =========================
// 8. Controller
// =========================
app.MapControllers();

app.Run();