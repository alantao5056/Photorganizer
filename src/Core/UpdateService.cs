using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace Alan.Photorganizer.App.Services;

public static class UpdateService
{
    /// <summary>
    /// Checks the Microsoft Store for mandatory app updates.
    /// Returns true if at least one mandatory update is available.
    /// Returns false if no mandatory updates, or if the Store is unavailable
    /// (e.g. running unpackaged during development).
    /// </summary>
    public static async Task<bool> HasMandatoryUpdateAsync()
    {
        try
        {
            var context = StoreContext.GetDefault();
            var updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();
            return updates.Any(u => u.Mandatory);
        }
        catch
        {
            // Store API unavailable (unpackaged, no Store, etc.) — skip gracefully
            return false;
        }
    }
}
