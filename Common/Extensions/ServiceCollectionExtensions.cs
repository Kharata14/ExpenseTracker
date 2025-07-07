using ExpenseTrackerApi.Features.Expenses;
using ExpenseTrackerApi.Features.Reports;
using ExpenseTrackerApi.Infrastructure.BackgroundJobs;
using ExpenseTrackerApi.Infrastructure.Database;
using ExpenseTrackerApi.Infrastructure.Repositories;
using ExpenseTrackerApi.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System.Reflection;
using static ExpenseTrackerApi.Features.Expenses.ImportExpenses;

namespace ExpenseTrackerApi.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        RecurringJob.AddOrUpdate<BudgetAlertService>(
            "budget-alert-job",
            service => service.CheckBudgetsAndSendAlerts(),
            Cron.Hourly); 

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 1024; // 1 MB
        });

        services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(configuration.GetConnectionString("Database"))));

        services.AddHangfireServer();

        services.AddScoped<IReportGenerationService, ReportGenerationService>();
        services.AddScoped<IExpenseImportService, ExpenseImportService>();
        services.AddScoped<BudgetAlertService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }
}