using RecuperacionBD;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        AuthManager auth = new AuthManager();
        Database db = new Database();
        TransactionLog log = new TransactionLog();

        if (!AuthenticateUser(auth))
        {
            Console.WriteLine("⛔ Demasiados intentos fallidos. Cerrando programa...");
            return;
        }

        while (true)
        {
            ShowMenu();

            if (!int.TryParse(Console.ReadLine(), out int tablaIndex) || tablaIndex < 0 || tablaIndex > 2)
            {
                Console.WriteLine(" Opción inválida. Intente de nuevo.");
                continue;
            }

            if (tablaIndex == 0)
            {
                Console.WriteLine(" Saliendo del programa...");
                break;
            }

            List<string> tablas = new List<string> { "Empleados", "Departamentos" };
            string tablaSeleccionada = tablas[tablaIndex - 1];
            Console.WriteLine($" Tabla seleccionada: {tablaSeleccionada}");
            db.ShowTable(tablaSeleccionada);

            Console.WriteLine("\n¿Qué desea hacer?");
            Console.WriteLine("1️ Insertar un nuevo registro");
            Console.WriteLine("2️ Actualizar un registro existente");
            Console.WriteLine("3️ Eliminar un registro");
            Console.WriteLine("4️ Recuperar Base de Datos");
            Console.WriteLine("5️ Deshacer última transacción (Undo)");
            Console.WriteLine("6️ Rehacer última transacción deshecha (Redo)");
            Console.WriteLine("7️ Crear un punto de control (Checkpoint)");
            Console.WriteLine("8️ Volver al menú principal");
            Console.Write("🔹 Ingrese una opción: ");

            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    InsertarRegistro(db, log, tablaSeleccionada);
                    break;
                case "2":
                    ActualizarRegistro(db, log, tablaSeleccionada);
                    break;
                case "3":
                    EliminarRegistro(db, log, tablaSeleccionada);
                    break;
                case "4":
                    RecuperarBaseDeDatos(db, log);
                    break;
                case "5":
                    log.Undo();
                    break;
                case "6":
                    log.Redo();
                    break;
                case "7":
                    log.CreateCheckpoint();
                    break;
                case "8":
                    continue;
                default:
                    Console.WriteLine("❌ Opción inválida.");
                    break;
            }

            Console.WriteLine($"\n Estado actualizado de la tabla {tablaSeleccionada}:");
            db.ShowTable(tablaSeleccionada);

            Console.WriteLine("\n Registro de transacciones:");
            log.ShowLog();

            Console.WriteLine("\nPresione ENTER para continuar...");
            Console.ReadLine();
        }
    }
    static bool AuthenticateUser(AuthManager auth)
    {
        int intentos = 0;
        while (intentos < 3)
        {
            Console.Clear();
            Console.WriteLine("🔐 Sistema de Autenticación");
            Console.WriteLine("1️⃣ Iniciar sesión");
            Console.WriteLine("2️⃣ Registrar nuevo usuario");
            Console.Write("Seleccione una opción: ");
            string opcion = Console.ReadLine();

            if (opcion == "2") // Opción de registro
            {
                Console.Write("Ingrese un nombre de usuario: ");
                string newUsername = Console.ReadLine();
                auth.RegisterUser(newUsername); // El método RegisterUser ya maneja la contraseña
                Console.WriteLine("✅ Usuario registrado correctamente.");
                // No se pide contraseña nuevamente después del registro
            }
            else if (opcion == "1") // Opción de inicio de sesión
            {
                Console.Write("Usuario: ");
                string username = Console.ReadLine();
                if (auth.Login(username)) // Aquí se verifica el login correctamente
                {
                    return true;
                }

                intentos++;
                Console.WriteLine($"❌ Intento {intentos}/3 fallido.");
            }
            else
            {
                Console.WriteLine("⚠️ Opción inválida.");
            }
        }
        return false;
    }




    static void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine(" Base de Datos - Menú Principal");
        Console.WriteLine("Seleccione una tabla para trabajar:(opción 0 para salir)");

        List<string> tablas = new List<string> { "Empleados", "Departamentos" };
        for (int i = 0; i < tablas.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {tablas[i]}");
        }
        Console.WriteLine("0.  Salir");
        Console.Write(" Ingrese el número de la tabla: ");
    }

    static void InsertarRegistro(Database db, TransactionLog log, string tabla)
    {
        Dictionary<string, object> nuevoRegistro = new Dictionary<string, object>();

        Console.Write("Ingrese ID (número): ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine(" ID inválido.");
            return;
        }

        if (db.ExistsInTable(tabla, id))
        {
            Console.WriteLine("❌ Error: El ID ingresado ya existe.");
            return;
        }

        nuevoRegistro["ID"] = id;

        Console.Write("Ingrese Nombre: ");
        nuevoRegistro["Nombre"] = Console.ReadLine();

        Console.Write("Ingrese Departamento: ");
        nuevoRegistro["Departamento"] = Console.ReadLine();

        db.Insert(tabla, nuevoRegistro);
        log.LogTransaction("INSERT", tabla, nuevoRegistro, commit: true);
        db.LoadDatabase();
    }

    static void ActualizarRegistro(Database db, TransactionLog log, string tabla)
    {
        Console.Write("Ingrese el ID del registro a actualizar: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine(" ID inválido.");
            return;
        }

        Dictionary<string, object> cambios = new Dictionary<string, object>();

        Console.Write("Ingrese el nuevo Nombre: ");
        cambios["Nombre"] = Console.ReadLine();

        Console.Write("Ingrese el nuevo Departamento: ");
        cambios["Departamento"] = Console.ReadLine();

        db.Update(tabla, id, cambios);
        log.LogTransaction("UPDATE", tabla, cambios, commit: true);
        db.LoadDatabase();
    }

    static void EliminarRegistro(Database db, TransactionLog log, string tabla)
    {
        Console.Write("Ingrese el ID del registro a eliminar: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine(" ID inválido.");
            return;
        }

        db.Delete(tabla, id);
        log.LogTransaction("DELETE", tabla, new Dictionary<string, object> { { "ID", id } }, commit: true);
        db.LoadDatabase();
    }

    static void RecuperarBaseDeDatos(Database db, TransactionLog log)
    {
        Console.WriteLine("Iniciando recuperación de base de datos...");
        log.ShowLog();
        Console.WriteLine("Recuperación completada.");
        db.LoadDatabase();
    }
}