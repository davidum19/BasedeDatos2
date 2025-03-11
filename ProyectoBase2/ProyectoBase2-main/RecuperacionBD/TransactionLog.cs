using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RecuperacionBD
{
    class TransactionLog
    {
        private static string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "log.json");
        private List<Dictionary<string, object>> logEntries;
        private Stack<string> undoStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                string lastTransaction = undoStack.Pop();
                redoStack.Push(lastTransaction);
                Console.WriteLine($"Deshacer: {lastTransaction}");
            }
            else
            {
                Console.WriteLine("No hay transacciones para deshacer.");
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                string lastUndone = redoStack.Pop();
                undoStack.Push(lastUndone);
                Console.WriteLine($"Rehacer: {lastUndone}");
            }
            else
            {
                Console.WriteLine("No hay transacciones para rehacer.");
            }
        }

        public void CreateCheckpoint()
        {
            Console.WriteLine("Punto de control creado.");
        }
        public TransactionLog()
        {
            logEntries = new List<Dictionary<string, object>>();  
            LoadLogAsync().Wait();  
        }

        private async Task LoadLogAsync()
        {
            if (File.Exists(logFilePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(logFilePath);
                    logEntries = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json) ?? new List<Dictionary<string, object>>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error al cargar el log: {ex.Message}");
                    logEntries = new List<Dictionary<string, object>>(); 
                    await SaveLogAsync(); 
                }
            }
            else
            {
                logEntries = new List<Dictionary<string, object>>();
                await SaveLogAsync();  
            }
        }

        // Guardar el log en log.json de manera asincrónica
        private async Task SaveLogAsync()
        {
            try
            {
                string json = JsonConvert.SerializeObject(logEntries, Formatting.Indented);
                Console.WriteLine($"Guardando log en: {logFilePath}"); // Verifica la ruta
                await File.WriteAllTextAsync(logFilePath, json);
           
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar el log: {ex.Message}");
            }
        }


        public void LogTransaction(string tipo, string tabla, Dictionary<string, object> datos, bool commit = false)
        {
      
            string status = "Fallido";
            bool isCommit = commit;

            // Preguntar si se desea simular un fallo
            if (commit)
            {
                Console.WriteLine("¿Desea simular un fallo antes de confirmar la transacción? (S/N):");
                string falloRespuesta = Console.ReadLine()?.ToUpper();

                if (falloRespuesta == "S")
                {
               
                    status = "Fallido";
                    isCommit = false;
                    Console.WriteLine("⚠️ Simulando fallo... Transacción no realizada.");

          
                    RegisterTransactionLog(tipo, tabla, datos, status, isCommit);
                    return; 
                }
                else
                {
                    Console.WriteLine("✅ Commit confirmado. Ejecutando transacción...");
                }
            }

            try
            {
                Console.WriteLine($"🔄 Ejecutando transacción de tipo {tipo} en la tabla {tabla}...");

                if (isCommit)
                {
                    status = "Exitoso";
                }
                else
                {
                    status = "Fallido"; 
                }
            }
            catch (Exception ex)
            {
                // Si ocurre un error, aseguramos que el commit sea falso y marcamos como fallido
                status = "Fallido";
                isCommit = false;
                Console.WriteLine($"❌ Error en la transacción: {ex.Message}");
            }
            // Registrar en el log
            RegisterTransactionLog(tipo, tabla, datos, status, isCommit);
        }

        // Función para registrar en el log correctamente
        private void RegisterTransactionLog(string tipo, string tabla, Dictionary<string, object> datos, string status, bool commit)
        {
            var entry = new Dictionary<string, object>
    {
        { "TransactionID", Guid.NewGuid().ToString() },
        { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
        { "Tipo", tipo },
        { "Tabla", tabla },
        { "Datos", datos },
        { "Commit", commit },  
        { "Estado", status }  
    };

            logEntries.Add(entry);
            SaveLogAsync().Wait();
        }

        // Mostrar el log de transacciones
        public void ShowLog()
        {
            Console.WriteLine("📜 Registro de Transacciones:");
            foreach (var entry in logEntries)
            {
                // Mostrar cada transacción de manera ordenada
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"Timestamp: {entry["Timestamp"]}");
                Console.WriteLine($"Tipo: {entry["Tipo"]}");
                Console.WriteLine($"Tabla: {entry["Tabla"]}");
                Console.WriteLine("Datos:");

                if (entry["Datos"] is Dictionary<string, object> datos)
                {
                    foreach (var kvp in datos)
                    {
                        Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("  No se encontraron datos.");
                }

                Console.WriteLine($"Commit: {entry["Commit"]}");

                // Mostrar el estado de la transacción
                if (entry.TryGetValue("Estado", out var status))
                {
                    Console.WriteLine($"Estado: {status}");
                }
                else
                {
                    Console.WriteLine("Estado no disponible");
                }

                Console.WriteLine("--------------------------------------------------");
            }
        }

        public void FinalizeLog()
        {
            SaveLogAsync().Wait();
        }
    }
}


