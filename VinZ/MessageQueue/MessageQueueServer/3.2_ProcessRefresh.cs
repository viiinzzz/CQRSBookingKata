namespace VinZ.MessageQueue;

public partial class MqServer
{
    private int _refresh = config.BusRefreshMinMilliseconds;

    private void LogRefresh()
    {
        if (_isTrace) log.LogInformation($"--> Throttling refresh rate @{_refresh}ms");
    }

    private void RefreshFastest()
    {
        if (_refresh <= config.BusRefreshMinMilliseconds)
        {
            return;
        }

        _refresh = config.BusRefreshMinMilliseconds;
        LogRefresh();
    }

    private void RefreshFaster()
    {
        if (_refresh / 2 >= config.BusRefreshMinMilliseconds)
        {
            _refresh /= 2;
            LogRefresh();
        }
    }

    private void RefreshSlower()
    {
        if (_refresh * 2 <= config.BusRefreshMaxMilliseconds)
        {
            _refresh *= 2;
            LogRefresh();
        }
    }
}