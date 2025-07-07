namespace ExpenseTrackerApi.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendBudgetAlertAsync(string to, string categoryName, decimal budget, decimal spent)
    {
        // რეალურ აპლიკაციაში აქ იქნებოდა იმეილის გაგზავნის ლოგიკა (მაგ. SMTP კლიენტის გამოყენებით)
        _logger.LogInformation("იმეილის გაგზავნა: {To}. კატეგორია '{CategoryName}'-ს ბიუჯეტი ({Budget}) თითქმის ამოიწურა. დახარჯულია: {Spent}",
            to, categoryName, budget, spent);

        return Task.CompletedTask;
    }
}