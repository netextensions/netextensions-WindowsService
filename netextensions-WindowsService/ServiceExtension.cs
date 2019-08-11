using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using LanguageExt;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace netextensions.WindowsService
{
    public class ServiceExtension<T> where T : class
    {
        private Option<ILogger> _logger;

        public ServiceExtension()
        {
            _logger = Option<ILogger>.None;
        }

        public ServiceExtension(ILogger logger)
        {
            _logger  = logger == null ? Option<ILogger>.None : Option<ILogger>.Some(logger);
        }
        public void Start(string[] args)
        {
            _ = (Debugger.IsAttached || args.Contains("--console")) ? StartConsole() : StartService(args);
        }

        private Unit StartConsole()
        {
            _logger.IfSome(x => x.LogInformation("ServiceExtension: Console is starting"));
            CreateWebHostBuilder().Build().Run();
            _logger.IfSome(x => x.LogInformation("ServiceExtension: Console is running"));
            return Unit.Default;
        }

        private Unit StartService(IEnumerable<string> args)
        {
            _logger.IfSome(x => x.LogInformation("ServiceExtension: Service is starting"));
            ServiceBase.Run(GetWebHostServiceExtension(args));
            _logger.IfSome(x => x.LogInformation("ServiceExtension: service is running"));
            return Unit.Default;
        }

        private ServiceBase GetWebHostServiceExtension(IEnumerable<string> args)
        {
            return new WebHostServiceExtension(CreateWebHost(args));
        }

        private IWebHost CreateWebHost(IEnumerable<string> args)
        {
            return WebHost.CreateDefaultBuilder(GetWebHostArgs(args))
                .UseUrls(GetServerUrls())
                .UseContentRoot(PathToContentRoot())
                .UseStartup<T>()
                .Build();
        }

        private string[] GetWebHostArgs(IEnumerable<string> args)
        {
            return args == null ? new string[0] : args.Where(arg => arg != "--console").ToArray();
        }


        private string[] GetServerUrls()
        {
            var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText($"{PathToContentRoot()}/appsettings.json"));
            if (jObject == null || !jObject.TryGetValue("server.urls", out var token) || !token.HasValues)
                return new string[0];

            return token.Select(t=> t.ToString()).ToArray();
        }

        private string PathToContentRoot()
        {
            return Path.GetDirectoryName(PathToExecutable());
        }

        private string PathToExecutable()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
                return processModule.FileName;

            _logger.IfSome(x => x.LogCritical("ServiceExtension: processModule is not found"));
            throw new ApplicationException("ServiceExtension: processModule is not found");
        }


        public IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((context, builder) => builder.SetBasePath(context.HostingEnvironment.ContentRootPath)
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build())
                .UseStartup<T>();
        }
    }

}
