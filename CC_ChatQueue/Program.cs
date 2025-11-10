using Application;
using Application.Interfaces;
using Infrastructure.HostedServices;
using Infrastructure.Repositories;
using Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC Controllers
builder.Services.AddControllers();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Load mock/demo data
var teams = MockData.CreateMockTeams();

const int maxChatConcurrency = 10;
int totalCapacity = MockData.CalculateTotalCapacity(teams, maxChatConcurrency);
int maxQueueLength = MockData.CalculateQueueMaxLength(totalCapacity);

// Dependency Injection setup
builder.Services.AddSingleton(teams);
builder.Services.AddSingleton(new InMemoryAgentRepository(teams));
builder.Services.AddSingleton<IQueueService>(_ => new InMemoryQueueService(maxQueueLength));
builder.Services.AddSingleton<IAssignmentService>(sp =>
    new RoundRobinAssigner(
        sp.GetRequiredService<IQueueService>(),
        sp.GetRequiredService<InMemoryAgentRepository>(),
        maxChatConcurrency
    ));

builder.Services.AddHostedService<AssignmentHostedService>();

var app = builder.Build();

// // Enable Swagger UI and JSON in Development 
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
// else
// {
//     // Optional: enable Swagger in production too
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseSwagger();
app.UseSwaggerUI(config =>
{
    config.DocumentTitle = "Chat Queue API";
    config.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat Queue API v1");
});

app.MapControllers();
app.Run();