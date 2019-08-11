using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace netextensions.WindowsService
{
    public class WebHostServiceExtension : WebHostService
    {
        public WebHostServiceExtension(IWebHost webHost) : base(webHost)
        {

        }
    }

}
