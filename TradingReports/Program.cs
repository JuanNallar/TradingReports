using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingReports;

var hostBuilder = Host.CreateDefaultBuilder(args);

string? _outputFolder;
string? _intervalInMinutes;

if (args.Length == 2)
{
    _intervalInMinutes = args[0];
    _outputFolder = args[1];
}
else 
{
    var configuration = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", optional: false).Build();

    _intervalInMinutes = configuration["IntervalInMinutes"];
    _outputFolder = configuration["OutputFolder"];
}


if(SetStaticConfigurationFromArgsOrConfigFile())
{

    hostBuilder.ConfigureServices(services =>
    {
        services.AddHostedService<Requester>();
    });
    await hostBuilder.RunConsoleAsync();
}


bool SetStaticConfigurationFromArgsOrConfigFile()
{
    if (string.IsNullOrWhiteSpace(_intervalInMinutes))
    {
        throw new ArgumentException("[IntervalinMinutes] value not provided in command line and not configured");
    }
    if (!int.TryParse(_intervalInMinutes, out int intervalInMinutes))
    {
        throw new ArgumentException($"[intervalInMinutes] (first argument in command line or configured value) must be of integer type. Current value: [{_intervalInMinutes}]");
    }

    if (!Directory.Exists(_outputFolder))
    {
        try
        {
            var dirInfo = Directory.CreateDirectory(_outputFolder);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Could not create Output Folder specified in [{_outputFolder}]");
        }
    }

    TradingReportsConfiguration.IntervalInMinutes = intervalInMinutes;
    TradingReportsConfiguration.OutputFolder = _outputFolder;
    return true;
          
}
