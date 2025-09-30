using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowSpecificIP", policy =>
//     {
//         policy.WithOrigins("http://2.179.167.151","https://2.179.167.151") // آی‌پی مورد نظر
//               .AllowAnyHeader()
//               .AllowAnyMethod();
//     });
// });

builder.Services.AddControllers();
builder.Services.AddDbContext<DbContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DBConnecDBConnectiontionMohrez")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowSpecificIP");
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
