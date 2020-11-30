namespace Portfolio.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ISessionService sessionService)
        {
            try
            {
                await _next(context);
            }
            catch (WebApiException ex) when (ex.StatusCode == 401)
            {
                Log.Information("api reported an expired token");

                sessionService.TerminateSession();

                var redirectUrl = $"/Login?returnUrl={HttpUtility.UrlEncode(context.Request.Path + context.Request.QueryString)}";

                context.Response.Redirect(redirectUrl);
            }
            catch (WebApiException ex) when (ex.StatusCode == 403)
            {
                Log.Information("api reported unauthorized attempt at usage");
                context.Response.Redirect("/Unauthorized");
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
