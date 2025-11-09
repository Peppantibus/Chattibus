namespace Chat.Utilities;

public class GlobalExceptionMiddleware
{
    //spedisce il context alla pipeline successiva
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // qui il controller VERRA' eseguito
            //tutte le pipeline successive anche (guarda program) 
            await _next(context);
        }
        catch (Exception ex)
        {
            //se nelle successive pipeline scatta un eccezione il middleware risale e torna qui per gestire l'errore
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        //in base all'exception ottenuta setto uno status code specifico
        var statusCode = ex switch
        {
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        //setto oggetto json 
        var problem = new
        {
            status = statusCode,
            title = "An error occurred while processing your request.",
            detail = ex.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        //scrivo il json nella response del contesto corrente
        return context.Response.WriteAsJsonAsync(problem);
    }
}

