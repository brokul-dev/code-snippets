using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace TestWebApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IUserContext, UserContext>();
            services.AddTransient<TransactionProccessor>();
            services.AddTransient<ITransactionProccessor, UserAwareTransactionProccessor>(serviceProvider =>
            {
                var userContext = serviceProvider.GetRequiredService<IUserContext>();
                var transactionProccessor = serviceProvider.GetRequiredService<TransactionProccessor>();
                return new UserAwareTransactionProccessor(transactionProccessor, userContext);
            });
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ClientCodeMiddleware>();
        }
    }

    public class ClientCodeMiddleware
    {
        public ClientCodeMiddleware(RequestDelegate next) {}

        public async Task InvokeAsync(HttpContext context, ITransactionProccessor proccessor)
        {
            var transactionItem = new TransactionItem { Amount = 100 };
            var message = await proccessor.ProcessAsync(transactionItem);
            await context.Response.WriteAsync(message);
        }
    }

    public interface ITransactionProccessor
    {
        Task<string> ProcessAsync(TransactionItem transactionItem);
    }

    public class TransactionProccessor : ITransactionProccessor
    {
        public async Task<string> ProcessAsync(TransactionItem transactionItem)
        {
            // complex code responsible for processing...
            return "Transaction item has been processed!";
        }
    }

    public class TransactionItem
    {
        public decimal Amount { get; set; }
    }

    public enum AcceptanceLevel
    {
        Level1, Level2, Level3
    }

    public class UserAwareTransactionProccessor : ITransactionProccessor
    {
        private readonly ITransactionProccessor _transactionProccessor;
        private readonly IUserContext _userContext;

        public UserAwareTransactionProccessor(
            ITransactionProccessor transactionProccessor, 
            IUserContext userContext)
        {
            _transactionProccessor = transactionProccessor;
            _userContext = userContext;
        }

        public async Task<string> ProcessAsync(TransactionItem transactionItem)
        {
            if (_userContext.CurrentUser.AcceptanceLevel < AcceptanceLevel.Level2)
            {
                return "Current user has not sufficient privileges";
            }

            return await _transactionProccessor.ProcessAsync(transactionItem);
        }
    }

    public interface IUserContext
    {
        AppUser CurrentUser { get; }
    }

    public class UserContext : IUserContext
    {
        public AppUser CurrentUser => new() { AcceptanceLevel = AcceptanceLevel.Level1 };
    }

    public class AppUser
    {
        public AcceptanceLevel AcceptanceLevel { get; set; }
    }
}