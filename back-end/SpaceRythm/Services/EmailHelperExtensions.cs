using SpaceRythm.Models;

namespace SpaceRythm.Services;

public static class EmailHelperExtensions
{
    public static void AddEmailHelper(this IServiceCollection services, EmailHelperOptions options)
    {
        services.AddTransient((service) => new EmailHelper(options));
    }
}
