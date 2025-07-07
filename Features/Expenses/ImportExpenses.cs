using MediatR;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExpenseTrackerApi.Features.Expenses;

public static class ImportExpenses
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/import", async (IFormFile file, ISender sender) =>
            {
                var command = new ImportExpensesCommand(file);
                var jobId = await sender.Send(command);
                return Results.Accepted($"/api/jobs/{jobId}", new { JobId = jobId });
            })
          .WithName("ImportExpenses")
          .DisableAntiforgery(); 
        }
    }

    public record ImportExpensesCommand(IFormFile File) : IRequest<string>;
    public class Handler : IRequestHandler<ImportExpensesCommand, string>
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public Handler(IBackgroundJobClient backgroundJobClient) => _backgroundJobClient = backgroundJobClient;

        public async Task<string> Handle(ImportExpensesCommand request, CancellationToken cancellationToken)
        {
            if (request.File.Length == 0) throw new ArgumentException("ფაილი ცარიელია.");

            var tempPath = Path.GetTempFileName();
            await using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            var userId = Guid.NewGuid(); // ჩანაცვლდება რეალური მომხმარებლის ID-ით
            var jobId = _backgroundJobClient.Enqueue<IExpenseImportService>(service =>
                service.ProcessImport(userId, tempPath));

            return jobId;
        }
    }

    public interface IExpenseImportService
    {
        Task ProcessImport(Guid userId, string filePath);
    }
}