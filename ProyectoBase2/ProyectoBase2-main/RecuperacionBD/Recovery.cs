using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RecuperacionBD
{
    class Recovery
    {
        public static void Checkpoint()
        {
            Console.WriteLine("[Checkpoint] Guardando estado seguro de la base de datos...");
            var log = Utils.ReadLog();

            // Filtrar solo las transacciones confirmadas (COMMIT)
            var committedTransactions = log.FindAll(t => t["State"].ToString() == "COMMIT");

            var database = Utils.ReadDatabase();

            foreach (var transaction in committedTransactions)
            {
                ApplyTransaction(database, transaction);
            }

            // Guardar el estado seguro en data.json y limpiar el log
            Utils.WriteDatabase(database);
            Utils.WriteLog(new List<Dictionary<string, object>>());

            Console.WriteLine("[Checkpoint] Estado seguro guardado exitosamente.");
        }

        public static void Undo()
        {
            Console.WriteLine("[UNDO] Revirtiendo transacciones no confirmadas...");
            var log = Utils.ReadLog();

            var uncommitted = log.FindAll(t => t["State"].ToString() != "COMMIT");

            foreach (var transaction in uncommitted)
            {
                Console.WriteLine($"Deshaciendo transacción: {transaction["Id"]}");
            }

            // Eliminar transacciones no confirmadas del log
            log.RemoveAll(t => t["State"].ToString() != "COMMIT");
            Utils.WriteLog(log);

            Console.WriteLine("[UNDO] Transacciones no confirmadas revertidas.");
        }

        public static void Redo()
        {
            Console.WriteLine("[REDO] Reaplicando transacciones confirmadas...");
            var log = Utils.ReadLog();
            var database = Utils.ReadDatabase();

            // Filtrar solo las transacciones confirmadas
            var committed = log.FindAll(t => t["State"].ToString() == "COMMIT");

            foreach (var transaction in committed)
            {
                Console.WriteLine($"Reaplicando transacción: {transaction["Id"]}");
                ApplyTransaction(database, transaction);
            }

            Utils.WriteDatabase(database);
            Console.WriteLine("[REDO] Transacciones confirmadas reaplicadas.");
        }

        private static void ApplyTransaction(Dictionary<string, object> database, Dictionary<string, object> transaction)
        {
            var operations = (List<Dictionary<string, object>>)transaction["Operations"];

            foreach (var operation in operations)
            {
                switch (operation["Type"].ToString())
                {
                    case "INSERT":
                        if (!database.ContainsKey(operation["Table"].ToString()))
                        {
                            database[operation["Table"].ToString()] = operation["Data"];
                        }
                        break;
                    case "UPDATE":
                        if (database.ContainsKey(operation["Table"].ToString()))
                        {
                            database[operation["Table"].ToString()] = operation["Data"];
                        }
                        break;
                    case "DELETE":
                        if (database.ContainsKey(operation["Table"].ToString()))
                        {
                            database.Remove(operation["Table"].ToString());
                        }
                        break;
                }
            }
        }

        public static void VerifyRecordExists(string table, int id)
        {
            var database = Utils.ReadDatabase();
            if (!database.ContainsKey(table) || !(database[table] as Dictionary<int, object>).ContainsKey(id))
            {
                Console.WriteLine($"❌ El registro con ID {id} no existe en la tabla {table}.");
            }
        }
    }
}
