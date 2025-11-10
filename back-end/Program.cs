using Chat.Services;
using Chat.Utilities;


var builder = WebApplication.CreateBuilder(args);
//aggiungo dipendenze di progetto da serviceCollectionExtension
builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

//eseguo prima il mio middleware di gestione errore il quale poi a sua volta dirà di proseguire con gli altri middleware/pipeline se uno dei successivi fallisce risalgo tutto e gestisco nel catch
app.UseMiddleware<GlobalExceptionMiddleware>();
//middleware per sicurezza degli headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// uso le dipendenze costruite prima sempre passando da servicecollection
app.UseDependencies();

app.MapControllers();

//webSocketService implementato da me (tipo copia di SignalR)
var wsService = app.Services.GetRequiredService<WebSocketServices>();

app.Map("/ws", wsService.HandleConnection);


app.Run();
