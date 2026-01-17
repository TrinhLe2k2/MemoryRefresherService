using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Location.Infrastructures;
using Location.Infrastructures.Elasticsearch;
using Location.Infrastructures.Redis;
using Location.Repositories.Implements;
using Location.Repositories.Interfaces;
using Location.Services.Implements;
using Location.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var esUrl = builder.Configuration["Elasticsearch:Url"]!;
var username = builder.Configuration["Elasticsearch:Username"]!;
var password = builder.Configuration["Elasticsearch:Password"]!;
var fingerprint = "6f9b17ee6ce321b0608b95d0b5fd64ccbb40fcc4ac56bc2260ba7d4fae513e29";

var esSettings = new ElasticsearchClientSettings(new Uri(esUrl))
    .Authentication(new BasicAuthentication(username, password))
    .CertificateFingerprint(fingerprint)
    .DisableDirectStreaming(); // để debug

var esClient = new ElasticsearchClient(esSettings);
builder.Services.AddSingleton(esClient);
builder.Services.AddScoped(typeof(IElasticsearchService<>), typeof(ElasticsearchService<>));


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
