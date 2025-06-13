using Repository.WebApi;
using Repository.WebApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRepository<Item>, InMemoryRepository<Item>>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program { }