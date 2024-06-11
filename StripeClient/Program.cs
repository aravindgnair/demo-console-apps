using Stripe;

namespace StripeApiClient;

internal class Program
{
    private const string ApiKey = "sk_test_51PFHFuSEKxiNGr7ukue9H13dzIuN1Sl8kmBcqdJ39kk7utau5FmnFJtiJMSWf64kyafqKJ0XYPpcpX8D67KoPKcR00DU1YCy9C";
    private const string CustomerEmail = "alice@tailspin.com";

    static async Task Main(string[] args)
    {
        try
        {
            StripeConfiguration.ApiKey = ApiKey;
            await CreateCustomerAsync(CustomerEmail);
            //await MakePaymentAsync();
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

    private static async Task MakePaymentAsync()
    {
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

    private static async Task CreateCustomerAsync(string email)
    {
        
        if (await IsCustomerExists(email))
        {
            throw new Exception("Customer already exists");
        }

        await CreateCustomer(email);
    }

    private static async Task<bool> IsCustomerExists(string email)
    {
        var service = new CustomerService();
        await foreach (var customer in service.ListAutoPagingAsync())
        {
            if (customer.Email == email)
                return true;
        }

        return false;
    }

    private static async Task CreateCustomer(string email)
    {
        var options = new CustomerCreateOptions
        {
            Email = email
        };
        var service = new CustomerService();
        var customer = await service.CreateAsync(options);
        Console.WriteLine("Customer created successfully");
    }
}