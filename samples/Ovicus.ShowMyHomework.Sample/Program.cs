using Ovicus.ShowMyHomework.Auth;
using System;
using System.Threading.Tasks;

namespace Ovicus.ShowMyHomework.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to ShowMyHome Commandline Utility");

            Console.WriteLine("Enter access token or press ENTER to provide credentials: ");
            var accessToken = Console.ReadLine();
            if (string.IsNullOrEmpty(accessToken))
            {
                var authService = new AuthenticationService();

                Console.Write("Enter School Id: ");
                var schoolId = Console.ReadLine();

                Console.Write("Enter Username: ");
                var username = Console.ReadLine();

                Console.Write("Enter Password: ");
                var password = Console.ReadLine();

                bool isAuthenticated = await authService.Authenticate(username, password, schoolId);

                if (isAuthenticated)
                {
                    accessToken = await authService.GetAccessToken();
                    await PrintTodos(accessToken);
                }
            }
            else
            {
                await PrintTodos(accessToken);
            }
        }

        private static async Task PrintTodos(string accessToken)
        {
            var client = new ShowMyHomeworkClient(accessToken);

            var todos = await client.GetTodos();

            foreach (var todo in todos)
            {
                Console.WriteLine("{0,-12}{1,-12}{2,-30}",
                    todo.Subject, todo.DueOn.Date.ToShortDateString(), todo.Title);
            }
        }
    }
}
