using MediatR;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExpenseTrackerApi.Features.Reports;

public interface IReportGenerationService
{
    Task GenerateMonthlyExcelReport(Guid userId, int year, int month);
}

public static class GenerateExcelReport
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/excel", async (GenerateExcelReportCommand command, ISender sender) =>
            {
                var jobId = await sender.Send(command);
                return Results.Accepted($"/api/jobs/{jobId}", new { JobId = jobId });
            })
        .WithName("GenerateExcelReport");
        }
    }

    public record GenerateExcelReportCommand(Guid UserId, int Year, int Month) : IRequest<string>;

    public class Handler : IRequestHandler<GenerateExcelReportCommand, string>
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public Handler(IBackgroundJobClient backgroundJobClient) => _backgroundJobClient = backgroundJobClient;

        public Task<string> Handle(GenerateExcelReportCommand request, CancellationToken cancellationToken)
        {
            var jobId = _backgroundJobClient.Enqueue<IReportGenerationService>(service =>
                service.GenerateMonthlyExcelReport(request.UserId, request.Year, request.Month));

            return Task.FromResult(jobId);
        }
    }
}