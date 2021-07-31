using System;

namespace TestApp
{
    public class Tenant
    {
        public string Id { get; set; }
        public Uri BaseUrl { get; set; }
        public string Secret { get; set; }
        public string QaCloudId { get; set; }
    }
    
    public class TenantBuilder<TBuilder> where TBuilder : TenantBuilder<TBuilder>
    {
        protected readonly Tenant Tenant;

        public TenantBuilder()
        {
            Tenant = new Tenant();
        }

        public TBuilder WithId(string id)
        {
            Tenant.Id = id;
            return (TBuilder)this;
        }

        public TBuilder WithBaseUrl(string baseUri)
        {
            Tenant.BaseUrl = new Uri(baseUri);
            return (TBuilder)this;
        }

        public Tenant Build()
        {
            return Tenant;
        }
    }

    public class ConfidentialTenantBuilder<TBuilder> 
        : TenantBuilder<TBuilder> where TBuilder : ConfidentialTenantBuilder<TBuilder>
    {
        public TBuilder WithSecret(string secret)
        {
            Tenant.Secret = secret;
            return (TBuilder)this;
        }
    }

    public class QaTenantBuilder<TBuilder>
        : ConfidentialTenantBuilder<TBuilder> where TBuilder : QaTenantBuilder<TBuilder>
    {
        public TBuilder WithQaCloudId(string qaCloudId)
        {
            Tenant.QaCloudId = qaCloudId;
            return (TBuilder)this;
        }
    }

    public class Program
    {
        public class TestTenantBuilder : QaTenantBuilder<TestTenantBuilder>
        {
            public static implicit operator Tenant(TestTenantBuilder builder)
            {
                return builder.Build();
            }
        }

        public static void Main()
        {
            Tenant tenant = new TestTenantBuilder()
                .WithSecret("secret")
                .WithQaCloudId("cloud01")
                .WithId("Test")
                .WithBaseUrl("https://tenant1.local");
        }
    }
}