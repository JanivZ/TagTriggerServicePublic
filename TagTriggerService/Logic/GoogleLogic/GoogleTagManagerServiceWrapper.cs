using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.TagManager.v2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Contrib.WaitAndRetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

namespace TagTriggerService.Logic.GoogleLogic
{
    public class GoogleTagManagerServiceWrapper : IGoogleTagManagerServiceWrapper
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private TagManagerService _service;

        public GoogleTagManagerServiceWrapper(ILogger<GoogleTagManagerServiceWrapper> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        private void InitializeService()
        {
            _logger.LogInformation("InitializeService -> execution started !");

            if (_service != null)
            {
                return;
            }
            var serviceAccountEmail = _config["GoogleTagManagerHandler:serviceAccountEmail"];

            var certificate = new X509Certificate2(@"tagmanagerproject-318917-50da48f19d67.p12",
                "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail)
               {
                   Scopes = new[] {
                       TagManagerService.Scope.TagmanagerManageAccounts,
                       TagManagerService.Scope.TagmanagerEditContainers,
                       TagManagerService.Scope.TagmanagerDeleteContainers,
                       TagManagerService.Scope.TagmanagerManageUsers,
                       TagManagerService.Scope.TagmanagerPublish,
                       TagManagerService.Scope.TagmanagerEditContainerversions

                   }
               }
            .FromCertificate(certificate));
            _service = new TagManagerService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "tagmanagerproject-318917",
            });
        }

        public TagManagerService Service { 
            get 
            {
                if (_service == null)
                {
                    InitializeService();
                }
                return _service;
            } 
        }

        public string AccountPath => _config["GoogleTagManagerHandler:accountPath"]; // such as: 'accounts/555555';

        public string ContainerPath => _config["GoogleTagManagerHandler:containerPath"];

        public string AccountAndContainerPath => AccountPath + "/" + ContainerPath;
    }
}
