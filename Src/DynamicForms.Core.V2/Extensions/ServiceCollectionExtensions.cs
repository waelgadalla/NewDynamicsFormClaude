using DynamicForms.Core.V2.Services;
using DynamicForms.Core.V2.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicForms.Core.V2.Extensions;

/// <summary>
/// Extension methods for registering DynamicForms.Core.V2 services with dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all DynamicForms.Core.V2 services to the service collection.
    /// Registers hierarchy service, validation service, and built-in validation rules.
    /// Uses InMemoryCodeSetProvider by default (no database required).
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDynamicFormsV2(this IServiceCollection services)
    {
        // Register core services as singletons (they are stateless and thread-safe)
        services.AddSingleton<IFormHierarchyService, FormHierarchyService>();
        services.AddSingleton<IFormValidationService, FormValidationService>();

        // Register default in-memory CodeSet provider
        services.AddSingleton<ICodeSetProvider, InMemoryCodeSetProvider>();

        // Register built-in validation rules as singletons
        services.AddSingleton<IValidationRule, RequiredFieldRule>();
        services.AddSingleton<IValidationRule, LengthValidationRule>();
        services.AddSingleton<IValidationRule, PatternValidationRule>();
        services.AddSingleton<IValidationRule, EmailValidationRule>();

        return services;
    }

    /// <summary>
    /// Adds DynamicForms.Core.V2 services with the option to register a custom repository implementation.
    /// Use this overload when you want to provide your own IFormModuleRepository implementation.
    /// </summary>
    /// <typeparam name="TRepository">The repository implementation type</typeparam>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDynamicFormsV2<TRepository>(this IServiceCollection services)
        where TRepository : class, IFormModuleRepository
    {
        // Add core services
        services.AddDynamicFormsV2();

        // Register custom repository
        services.AddScoped<IFormModuleRepository, TRepository>();

        return services;
    }

    /// <summary>
    /// Adds DynamicForms.Core.V2 services with a custom CodeSet provider implementation.
    /// Use this when you want to load CodeSets from a database, file system, or external API.
    /// </summary>
    /// <typeparam name="TCodeSetProvider">The CodeSet provider implementation type</typeparam>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDynamicFormsV2WithCodeSetProvider<TCodeSetProvider>(
        this IServiceCollection services)
        where TCodeSetProvider : class, ICodeSetProvider
    {
        // Register core services (without default CodeSet provider)
        services.AddSingleton<IFormHierarchyService, FormHierarchyService>();
        services.AddSingleton<IFormValidationService, FormValidationService>();

        // Register custom CodeSet provider
        services.AddSingleton<ICodeSetProvider, TCodeSetProvider>();

        // Register built-in validation rules
        services.AddSingleton<IValidationRule, RequiredFieldRule>();
        services.AddSingleton<IValidationRule, LengthValidationRule>();
        services.AddSingleton<IValidationRule, PatternValidationRule>();
        services.AddSingleton<IValidationRule, EmailValidationRule>();

        return services;
    }

    /// <summary>
    /// Adds DynamicForms.Core.V2 services without any CodeSet provider.
    /// Use this when you don't need CodeSet support or will register a provider separately.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDynamicFormsV2WithoutCodeSets(this IServiceCollection services)
    {
        // Register core services only
        services.AddSingleton<IFormHierarchyService, FormHierarchyService>();
        services.AddSingleton<IFormValidationService, FormValidationService>();

        // Register built-in validation rules
        services.AddSingleton<IValidationRule, RequiredFieldRule>();
        services.AddSingleton<IValidationRule, LengthValidationRule>();
        services.AddSingleton<IValidationRule, PatternValidationRule>();
        services.AddSingleton<IValidationRule, EmailValidationRule>();

        return services;
    }
}
