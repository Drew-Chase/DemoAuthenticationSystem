using DemoAuthenticationSystem.Lib;

namespace DemoAuthenticationSystem.CLI;

internal static class Program
{
    private static void Main(string[] args)
    {
        using UserManager manager = new();
        while (true)
        {
            Console.Write(">> ");
            string command = Console.ReadLine()?.ToLower() ?? "";
            switch (command)
            {
                case "create":
                    CreateUser(manager);
                    break;
                case "list":
                    ListUsers(manager);
                    break;
                case "delete":
                    DeleteUser(manager);
                    break;
                case "login":
                    Login(manager);
                    break;
                case "login-token":
                    LoginWithToken(manager);
                    break;
                case "search":
                    Search(manager);
                    break;
                case "exit":
                    Console.WriteLine("Exiting...");
                    return;
                case "commands":
                case "cmd":
                case "help":
                    Console.WriteLine("Commands: create, list, delete, login, login-token, search, help, exit");
                    break;
                default:
                    Console.WriteLine($"Invalid command: '{command}'");
                    break;
            }
        }
    }

    private static void CreateUser(UserManager manager)
    {
        while (true)
        {
            Console.Write("Username: ");
            string username = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Invalid username");
                continue;
            }

            Console.Write("Password: ");
            string password = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Invalid password");
                continue;
            }

            Console.Write("Email: ");
            string email = Console.ReadLine() ?? "";
            manager.CreateUser(username, password, email);
            break;
        }
    }

    private static void ListUsers(UserManager manager)
    {
        var users = manager.GetUsers();
        if (users.Length == 0)
        {
            Console.WriteLine("No users found.");
            return;
        }

        foreach (var user in users)
        {
            Console.WriteLine(user);
        }
    }

    private static void DeleteUser(UserManager manager)
    {
        Console.Write("Enter the Id of the user to delete: ");
        string id = Console.ReadLine() ?? "";
        if (manager.TryGetUserById(id, out User user))
        {
            Console.Write($"Are you sure you want to delete '{user.Username}' (y/N)? ");
            string response = Console.ReadLine()?.ToLower() ?? "";
            if (response != "y")
            {
                Console.WriteLine("Operation cancelled.");
                return;
            }

            manager.DeleteUser(id);
            Console.WriteLine($"User '{user.Username}' deleted successfully.");
        }
        else
        {
            Console.WriteLine("User not found.");
        }
    }

    private static void Login(UserManager manager)
    {
        Console.Write("Username: ");
        string username = Console.ReadLine() ?? "";
        Console.Write("Password: ");
        string password = Console.ReadLine() ?? "";
        Console.WriteLine(manager.AuthenticateUser(username, password, Environment.MachineName, out string? token, out User user) ? $"Welcome {user.Username} your token is {token}" : "Login failed.");
    }

    private static void LoginWithToken(UserManager manager)
    {
        Console.Write("Token: ");
        string token = Console.ReadLine() ?? "";
        Console.WriteLine(manager.AuthenticateUserWithToken(token, Environment.MachineName, out User user) ? $"Welcome {user.Username}" : "Login failed.");
    }

    private static void Search(UserManager manager)
    {
        Console.Write("Enter the username to search: ");
        string username = Console.ReadLine() ?? "";
        var users = manager.SearchUsers(username);
        foreach (var user in users)
        {
            Console.WriteLine(user);
        }
    }
}