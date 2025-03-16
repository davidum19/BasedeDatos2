using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecuperacionBD
{
    public class AuthManager
    {
        private string usersFile = "users.json";
        private string authLogFile = "auth_log.json";

        public void RegisterUser(string username)
        {
            var users = LoadUsers();
            if (users.ContainsKey(username))
            {
                Console.WriteLine("⚠️ El usuario ya existe.");
                Console.ReadLine();
                return;
            }

            Console.Write("Ingrese su contraseña: ");
            string password = ReadPassword();

            users[username] = password;
            SaveUsers(users);
            Console.WriteLine("\n Usuario registrado con éxito.");
            Console.ReadLine();
        }

        public bool Login(string username)
        {
            var users = LoadUsers();

            Console.Write("Ingrese su contraseña: ");
            string password = ReadPassword();

            if (users.ContainsKey(username) && users[username] == password)
            {
                Console.WriteLine("\n Inicio de sesión exitoso.");
                Console.ReadLine();
                return true;
            }

            Console.WriteLine("\n Usuario o contraseña incorrectos.");
            LogFailedAttempt(username);
            Console.ReadLine();
            return false;
        }

        private void LogFailedAttempt(string username)
        {
            List<Dictionary<string, string>> logEntries;

            if (File.Exists(authLogFile))
            {
                string existingData = File.ReadAllText(authLogFile);
                logEntries = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(existingData) ?? new List<Dictionary<string, string>>();
            }
            else
            {
                logEntries = new List<Dictionary<string, string>>();
            }

            var logEntry = new Dictionary<string, string>
        {
            { "Fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "Usuario", username },
            { "Mensaje", "Intento de inicio de sesión fallido" }
        };

            logEntries.Add(logEntry);
            File.WriteAllText(authLogFile, JsonConvert.SerializeObject(logEntries, Formatting.Indented));

            Console.WriteLine(" Intento fallido registrado en el log de autenticación.");
        }

        public static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(intercept: true);
                if (key.Key != ConsoleKey.Enter)
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        Console.Write("\b \b");
                        password = password[..^1];
                    }
                    else if (key.Key != ConsoleKey.Backspace)
                    {
                        Console.Write("*");
                        password += key.KeyChar;
                    }
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine(); // Nueva línea después de la contraseña
            return password;
        }

        private Dictionary<string, string> LoadUsers()
        {
            if (File.Exists(usersFile))
            {
                string json = File.ReadAllText(usersFile);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            return new Dictionary<string, string>();
        }

        private void SaveUsers(Dictionary<string, string> users)
        {
            File.WriteAllText(usersFile, JsonConvert.SerializeObject(users, Formatting.Indented));
        }
    }
}