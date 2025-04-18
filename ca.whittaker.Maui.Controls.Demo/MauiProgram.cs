﻿using ca.whittaker.Maui.Controls.Demo.ViewModels;
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

            builder.Services.AddSingleton<LoginLogoutViewModel>();
            builder.Services.AddSingleton<LoginLogoutPage>();
            builder.Services.AddSingleton<UserProfilePage>();
            //builder.Services.AddSingleton<SampleCheckBoxPage>();

            builder.Services.AddSingleton<MauiPage1>();
            builder.Services.AddSingleton<MauiPage2>();

            return builder.Build();
        }
    }
}
