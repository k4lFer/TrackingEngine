namespace WebApi.Scalar;

public static class ScalarConfiguration
{
    public static IServiceCollection AddScalarConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Version = "1.0.0";
                document.Info.Title = "API";
                document.Info.Description = "Documentación de la API con autenticación JWT";
                document.Info.Contact = new()
                {
                    Name = "Soporte técnico",
                    Email = ""
                };
                document.Info.License = new()
                {
                    Name = "Proprietary"
                };
                
                return Task.CompletedTask;
            });
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<BearerSecurityRequirementTransformer>(); 
        });
        
        return services;
    }
}