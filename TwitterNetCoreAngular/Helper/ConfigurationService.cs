public class ConfigurationService : BackgroundService
{
    private readonly ServiceHub _serviceHub;

    public ConfigurationService(ServiceHub serviceHub)
    {
        _serviceHub = serviceHub;
    }

    public ConfigurationService()
    {
    }

    override
    protected async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
    {
        await _serviceHub.setting_Up_Rules();

        while (!stoppingToken.IsCancellationRequested)
        {
            _serviceHub.streamNotification();
        }
        return Task.CompletedTask;
    }
}
