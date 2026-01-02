using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Taxi_API.Services
{
    public class ServersDocumentFilter : IDocumentFilter
    {
        private readonly string _prodHost = "http://zeyro.space";
        private readonly string _localHost = "http://localhost:5000";

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Add both production and local server entries so Swagger UI shows both
            swaggerDoc.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = _prodHost },
                new OpenApiServer { Url = _localHost }
            };
        }
    }
}
