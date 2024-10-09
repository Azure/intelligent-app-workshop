using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Plugins;

public class TimeInformationPlugin
{
    [KernelFunction] 
    [Description("Retrieves the current time in UTC.")]
    public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
}