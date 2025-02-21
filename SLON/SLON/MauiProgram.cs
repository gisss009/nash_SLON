using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.SwipeCardView;
using Xe.AcrylicView;

namespace SLON
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSwipeCardView()
                .UseMauiCommunityToolkit()
                .UseAcrylicView()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialSymbolsOutlined-Regular.ttf", "MaterialSymbolsOutlined");
                });


    		builder.Logging.AddDebug();
            return builder.Build();
        }
    }
}
