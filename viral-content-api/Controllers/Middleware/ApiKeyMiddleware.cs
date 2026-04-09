namespace ViralContentApi.Middleware
{
    public class ApiKeyMiddleware
    {
        private const string ApiKeyHeaderName = "X-API-Key";
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            var isContentEndpoint = path.StartsWith("/api/Content", StringComparison.OrdinalIgnoreCase);

            if (!isContentEndpoint)
            {
                await _next(context);
                return;
            }

            if (HttpMethods.IsGet(method))
            {
                await _next(context);
                return;
            }

            if (!HttpMethods.IsPost(method) &&
                !HttpMethods.IsPut(method) &&
                !HttpMethods.IsDelete(method))
            {
                await _next(context);
                return;
            }

            var configuredApiKey = _configuration["ApiKey"];

            if (string.IsNullOrWhiteSpace(configuredApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "API key is not configured."
                });
                return;
            }

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "API key is missing."
                });
                return;
            }

            if (!string.Equals(providedApiKey.ToString(), configuredApiKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Invalid API key."
                });
                return;
            }

            await _next(context);
        }
    }
}