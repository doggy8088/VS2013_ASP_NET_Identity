using System;
using System.Collections.Generic;
using System.Configuration;
using MVC_OrganizationalAccounts.Utils;

namespace MVC_OrganizationalAccounts
{
    // 如需 ASP.NET Identity 的詳細資訊，請瀏覽 http://go.microsoft.com/fwlink/?LinkId=301863
    public static class IdentityConfig 
    {
        public static void ConfigureIdentity() 
        {
            RefreshValidationSettings();
        }

        public static void RefreshValidationSettings()
        {
            string metadataLocation = ConfigurationManager.AppSettings["ida:FederationMetadataLocation"];
            SingleTenantIssuerNameRegistry.RefreshKeys(metadataLocation);
        }
    }
}