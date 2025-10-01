using Microsoft.EntityFrameworkCore;
using AutoMapper;




var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAutoMapper(typeof(Program));
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowHekmateSubdomains", policy =>
{
    policy
        .SetIsOriginAllowed(origin =>
        {
            return new Uri(origin).Host.EndsWith(".hekmateislami.com")
                   || new Uri(origin).Host == "hekmateislami.com";
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
});

    // Policy خاص برای یک API خاص
    options.AddPolicy("AllowOnlyVC", policy =>
    {
        policy
            .WithOrigins("https://vc.hekmateislami.com") // دقیقا همین ساب‌دامین
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddDbContext<DbContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DBConnecDBConnectiontionMohrez")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }
app.UseCors();  
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
