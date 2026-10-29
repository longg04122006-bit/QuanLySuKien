var builder = WebApplication.CreateBuilder(args);

// Cho phep Frontend goi den Gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Cau hinh API Gateway bang YARP
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("AllowFrontend");

// Chuyen tiep request den Backend
app.MapReverseProxy();

app.Run();
