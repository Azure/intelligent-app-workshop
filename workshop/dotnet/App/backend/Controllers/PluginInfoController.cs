using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.AspNetCore.Mvc;
using Core.Utilities.Models;
using Core.Utilities.Extensions;

namespace Controllers;

[ApiController]
[Route("sk")]
public class PluginInfoController : ControllerBase {

    private readonly Kernel _kernel;

    public PluginInfoController(Kernel kernel)
    {
        _kernel = kernel;
    }

    /// <summary>
    /// Get the metadata for all the plugins and functions.
    /// </summary>
    /// <returns></returns>
    [HttpGet("/pluginInfo/metadata")]
    public async Task<IList<PluginFunctionMetadata>> GetPluginInfoMetadata()
    {
        var functions = _kernel.Plugins.GetFunctionsMetadata().ToPluginFunctionMetadataList();
        return functions;
    }
}