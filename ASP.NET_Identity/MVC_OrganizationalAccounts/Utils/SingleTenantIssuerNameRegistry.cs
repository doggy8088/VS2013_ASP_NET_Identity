using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using MVC_OrganizationalAccounts.Models;

namespace MVC_OrganizationalAccounts.Utils
{
    public class SingleTenantIssuerNameRegistry : ValidatingIssuerNameRegistry
    {
        public static bool ContainsKey(string thumbprint)
        {
            using (TenantDbContext context = new TenantDbContext())
            {
                return context.IssuingAuthorityKeys
                    .Where(key => key.Id == thumbprint)
                    .Any();
            }
        }

        public static void RefreshKeys(string metadataLocation)
        {
            IssuingAuthority issuingAuthority = ValidatingIssuerNameRegistry.GetIssuingAuthority(metadataLocation);

            bool newKeys = false;
            foreach (string thumbprint in issuingAuthority.Thumbprints)
            {
                if (!ContainsKey(thumbprint))
                {
                    newKeys = true;
                    break;
                }
            }

            if (newKeys)
            {
                using (TenantDbContext context = new TenantDbContext())
                {
                    context.IssuingAuthorityKeys.RemoveRange(context.IssuingAuthorityKeys);
                    foreach (string thumbprint in issuingAuthority.Thumbprints)
                    {
                        context.IssuingAuthorityKeys.Add(new IssuingAuthorityKey { Id = thumbprint });
                    }
                    context.SaveChanges();
                }
            }
        }

        protected override bool IsThumbprintValid(string thumbprint, string issuer)
        {
            return ContainsKey(thumbprint);
        }
    }
}
