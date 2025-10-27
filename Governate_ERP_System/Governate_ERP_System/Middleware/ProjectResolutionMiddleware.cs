using Governate_ERP_System.Application.Interfaces;

namespace Governate_ERP_System.Middleware
{
    public class ProjectResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantCatalogService _catalog;
        private readonly ICurrentProjectAccessor _current;

        public ProjectResolutionMiddleware(RequestDelegate next, ITenantCatalogService catalog, ICurrentProjectAccessor current)
        {
            _next = next; _catalog = catalog; _current = current;
        }

        public async Task Invoke(HttpContext ctx)
        {
            if (ctx.Request.Query.TryGetValue("p", out var code))
            {
                var proj = await _catalog.GetByCodeAsync(code!);
                if (proj != null) _current.Set(proj);
            }
            await _next(ctx);
        }
    }
}
