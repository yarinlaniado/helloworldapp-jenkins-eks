var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World! this is a test to see if a rolling deployment is starting!");

app.Run();
