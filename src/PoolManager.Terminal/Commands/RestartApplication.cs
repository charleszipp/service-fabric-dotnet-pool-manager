using System;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using PoolManager.Core.Commands;

namespace PoolManager.Terminal.Commands
{
    [Verb("restart-app")]
    public class RestartApplication : ICommand
    {
        public RestartApplication(string applicationType, string applicationName, string applicationVersion)
        {
            ApplicationType = applicationType;
            ApplicationName = applicationName;
            ApplicationVersion = applicationVersion;
        }

        [Option('t', "type", Default = Constants.NoOpApplicationTypeName)]
        public string ApplicationType { get; }

        [Option('n', "name", Default = Constants.NoOpApplicationName)]
        public string ApplicationName { get; }

        [Option('v', "version", Default = Constants.NoOpApplicationVersion)]
        public string ApplicationVersion { get; }

        public Uri ApplicationUri => new Uri(ApplicationName);
    }

    public class RestartApplicationHandler : IHandleCommand<RestartApplication>
    {
        private readonly FabricClient _fabricClient;

        public RestartApplicationHandler(FabricClient fabricClient = null)
        {
            _fabricClient = fabricClient ?? new FabricClient();
        }        

        public async Task ExecuteAsync(RestartApplication command, CancellationToken cancellationToken)
        {
            var types = await _fabricClient.QueryManager.GetApplicationTypeListAsync(command.ApplicationType);
            if (types?.Any() ?? false)
            {
                var applications = await _fabricClient.QueryManager.GetApplicationListAsync(command.ApplicationUri);
                if (applications?.Any() ?? false)
                {
                    DeleteApplicationDescription delete = new DeleteApplicationDescription(command.ApplicationUri);
                    await _fabricClient.ApplicationManager.DeleteApplicationAsync(delete);
                }

                ApplicationDescription applicationDescription = new ApplicationDescription(command.ApplicationUri, command.ApplicationType, command.ApplicationVersion);
                await _fabricClient.ApplicationManager.CreateApplicationAsync(applicationDescription);
            }
            else
                throw new ArgumentException($"Application type '{command.ApplicationType}' not deployed to the cluster.");
        }
    }
}
