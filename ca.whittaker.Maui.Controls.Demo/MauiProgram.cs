using ca.whittaker.Maui.Controls.Demo.ViewModels;
using ca.whittaker.Maui.Controls.Demo.Views;
using Microsoft.Extensions.Logging;

namespace ca.whittaker.Maui.Controls.Demo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            //builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<TestForm1ViewModel>();
            builder.Services.AddSingleton<TestForm2ViewModel>();
            builder.Services.AddSingleton<TestForm3ViewModel>();
            builder.Services.AddSingleton<TestForm4ViewModel>();
            builder.Services.AddSingleton<TestForm5ViewModel>();

            builder.Services.AddSingleton<TestForm1>();
            builder.Services.AddSingleton<TestForm2>();
            builder.Services.AddSingleton<TestForm3>();
            builder.Services.AddSingleton<TestForm4>();
            builder.Services.AddSingleton<TestForm5>();

            builder.Services.AddSingleton<TestForm>();

            return builder.Build();
        }
    }
}
