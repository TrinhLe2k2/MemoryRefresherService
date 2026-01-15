using Location.Infrastructures;
using Location.Infrastructures.Redis;
using Location.Repositories.Implements;
using Location.Repositories.Interfaces;
using Location.Services.Implements;
using Location.Services.Interfaces;
using Microsoft.AspNetCore.Connections;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connection_string = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Missing DefaultConnection");

builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connection_string));


builder.Services.AddDistributedMemoryCache(); // In-Memory cache, restart app → mất cache
//builder.Services.AddStackExchangeRedisCache(options => // Redis cache Distributed ON CLOUD
//{
//    options.Configuration = builder.Configuration.GetConnectionString("Redis");
//});
builder.Services.AddSingleton<IRedisCacheUsingDistributed, RedisCacheUsingDistributed>();


builder.Services.AddSingleton<IRedisConnectionFactory>(_ =>
    new RedisConnectionFactory(builder.Configuration.GetConnectionString("Redis")!) // Redis cache CLOUD
);
builder.Services.AddSingleton<IRedisCacheUsingMultiplexer, RedisCacheUsingMultiplexer>(); 


builder.Services.AddScoped<IProvinceRepository, ProvinceRepository>();
builder.Services.AddScoped<IProvinceService, ProvinceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
