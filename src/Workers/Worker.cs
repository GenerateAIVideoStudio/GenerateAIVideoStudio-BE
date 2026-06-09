namespace Workers;

public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // To be implemented
        await Task.Delay(-1, stoppingToken);
    }
}
