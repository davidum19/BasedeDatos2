using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RecuperacionBD
{
    // Clase Utils - Métodos auxiliares para manipular JSON y logs
    public static class Utils
    {
        private static readonly string DataFile = "data.json";
        private static readonly string LogFile = "log.json";

        // Leer la base de datos desde el archivo JSON
        public static Dictionary<string, object> ReadDatabase()
        {
            if (!File.Exists(DataFile)) return new Dictionary<string, object>();
            var json = File.ReadAllText(DataFile);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }

        // Escribir en la base de datos (guardar en el archivo JSON)
        public static void WriteDatabase(Dictionary<string, object> data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(DataFile, json);
        }

        // Leer el log de transacciones
        public static List<Dictionary<string, object>> ReadLog()
        {
            if (!File.Exists(LogFile)) return new List<Dictionary<string, object>>();
            var json = File.ReadAllText(LogFile);
            return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
        }

        // Escribir en el log de transacciones
        public static void WriteLog(List<Dictionary<string, object>> log)
        {
            var json = JsonConvert.SerializeObject(log, Formatting.Indented);
            File.WriteAllText(LogFile, json);
        }
    }
}
