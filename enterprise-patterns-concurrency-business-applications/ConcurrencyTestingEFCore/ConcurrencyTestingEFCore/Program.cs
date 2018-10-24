using Microsoft.Extensions.DependencyInjection;

namespace ConcurrencyTestingEFCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            App _app = new App();
            _app.Run();
       }
    }
}
