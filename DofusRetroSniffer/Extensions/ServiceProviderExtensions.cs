using Microsoft.Extensions.DependencyInjection;

namespace DofusRetroSniffer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IServiceProvider"/> interface.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Starts the sniffer by retrieving the <see cref="ISniffer"/> service and initiating packet capture.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <returns>The service provider.</returns>
    public static IServiceProvider StartSniffer(this IServiceProvider provider)
    {
        var sniffer = provider.GetRequiredService<ISniffer>();

        sniffer.StartCapture();

        return provider;
    }

    /// <summary>
    /// Stops the sniffer by retrieving the <see cref="ISniffer"/> service and halting packet capture.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <returns>The service provider.</returns>
    public static IServiceProvider StopSniffer(this IServiceProvider provider)
    {
        var sniffer = provider.GetRequiredService<ISniffer>();

        sniffer.StopCapture();

        return provider;
    }
}
