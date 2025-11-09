using System.Text;
using Chat.Data;
using Chat.Services;
using Chat.Services.CurrentUser;
using Chat.Services.FriendService;
using Chat.Services.MessageService;
using Chat.Services.MessagService;
using Chat.Services.UserSerice;
using Chat.Services.UserService;
using Chat.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// database connection
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));

builder.Services.AddSingleton<WebSocketServices>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFriendService, FriendService>();

//JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
//pepper
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("SecuritySettings"));
// Abilita il servizio CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200") // 🔹 metti qui il tuo frontend
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // serve se usi cookie o auth headers (es. JWT)
    });
});

//configuro tutte le url di api in minuscolo per convenzione
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});


// Configura l'autenticazione JWT usando ServiceCollectionExtensions
builder.Services.AddJwtAuthentication(builder.Configuration);

//aggiungo controllers
builder.Services.AddControllers();
//DI servizi http
builder.Services.AddHttpContextAccessor();
//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Chat",
        Version = "v1"
    });

    // Definizione dello schema di sicurezza JWT Bearer
    // permette di testare API che necessitano di autorizzazione
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Inserisci il token JWT nel formato: Bearer {token}"
    });

    // Obbliga Swagger ad usare il token per tutte le chiamate
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//eseguo prima il mio middleware di gestione errore il quale poi a sua volta dirà di proseguire con gli altri middleware/pipeline se uno dei successivi fallisce risalgo tutto e gestisco nel catch
app.UseMiddleware<GlobalExceptionMiddleware>();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

app.MapRazorPages();
app.MapControllers();

//webSocketService implementato da me (tipo copia di SignalR)
var wsService = app.Services.GetRequiredService<WebSocketServices>();

app.Map("/ws", wsService.HandleConnection);


app.Run();
