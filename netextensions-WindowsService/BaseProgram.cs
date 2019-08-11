using Microsoft.AspNetCore.Hosting;

namespace netextensions.WindowsService
{
    public class BaseProgram<T> where T : class
    {
        public static IWebHostBuilder CreateWebHostBuilder()
        {
            return new ServiceExtension<T>().CreateWebHostBuilder();
        }
    }

}
