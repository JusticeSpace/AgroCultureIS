using System.Windows;
using System.Windows.Media;
using AgroCulture.Services;

namespace AgroCulture
{
    public partial class App : Application
    {
        public static DatabaseService Database { get; private set; }
        public static Users CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ✅ ГЛОБАЛЬНЫЕ НАСТРОЙКИ КАЧЕСТВА РЕНДЕРИНГА
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;

            // Инициализация БД
            Database = new DatabaseService();
        }
    }
}