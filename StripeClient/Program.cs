using Stripe;

namespace StripeClient;

internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            StripeConfiguration.ApiKey = "";
            const long amount = 100;

            var options = new ChargeCreateOptions
            {
                Amount = amount,
                Currency = "USD",
                Source = "tok_visa",
                Description = "Test payment of $1"
            };

            var service = new ChargeService();
            var charge = await service.CreateAsync(options);

            Console.WriteLine(charge.StripeResponse.Content);
        }
        catch (StripeException e)
        {
            Console.WriteLine(e);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}