using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using System.IO;

namespace RecuperacionBD
{
    class Database
    {
        private static string filePath = "data.json";

        // Estructura de la base de datos (diccionario con listas de registros)
        private Dictionary<string, List<Dictionary<string, object>>> data;

        public Database()
        {
            LoadDatabase();
        }

        // 🔹 Cargar la base de datos desde data.json
        public void LoadDatabase()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                data = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(json);
            }
            else
            {
                data = new Dictionary<string, List<Dictionary<string, object>>>
                {
                    { "Empleados", new List<Dictionary<string, object>>() },
                    { "Departamentos", new List<Dictionary<string, object>>() }
                };
                SaveDatabase();
            }
        }

        // 🔹 Guardar la base de datos en data.json
        private void SaveDatabase()
        {
            string json = JsonConvert.SerializeObject(data, (Newtonsoft.Json.Formatting)System.Xml.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // 🔹 Verificar si el ID ya existe en la tabla
        public bool ExistsInTable(string tableName, int id)
        {
            if (data.ContainsKey(tableName))
            {
                var table = data[tableName];
                var record = table.Find(r => r.ContainsKey("ID") && Convert.ToInt32(r["ID"]) == id);
                return record != null; // Retorna true si el ID existe, false si no
            }
            return false; // Retorna false si la tabla no existe
        }

        // 🔹 INSERTAR un nuevo registro en una tabla
        public void Insert(string tableName, Dictionary<string, object> newRecord)
        {
            if (data.ContainsKey(tableName))
            {
                int id = Convert.ToInt32(newRecord["ID"]);

                // Verificar si el ID ya existe antes de insertar
                if (ExistsInTable(tableName, id))
                {
                    Console.WriteLine($"❌ Error: El ID {id} ya existe en la tabla {tableName}. No se puede insertar un registro con un ID duplicado.");
                    return;
                }

                data[tableName].Add(newRecord);
                SaveDatabase();
                Console.WriteLine($"✅ Registro insertado en {tableName}");
            }
            else
            {
                Console.WriteLine($"❌ La tabla {tableName} no existe.");
            }
        }

        // 🔹 ACTUALIZAR un registro en una tabla
        public void Update(string tableName, int id, Dictionary<string, object> updatedValues)
        {
            if (data.ContainsKey(tableName))
            {
                var table = data[tableName];
                var record = table.Find(r => r.ContainsKey("ID") && Convert.ToInt32(r["ID"]) == id);

                if (record != null)
                {
                    foreach (var key in updatedValues.Keys)
                    {
                        if (record.ContainsKey(key))
                            record[key] = updatedValues[key];
                    }
                    SaveDatabase();
                    Console.WriteLine($"✅ Registro con ID {id} actualizado en {tableName}");
                }
                else
                {
                    Console.WriteLine($"❌ No se encontró un registro con ID {id} en {tableName}");
                }
            }
            else
            {
                Console.WriteLine($"❌ La tabla {tableName} no existe.");
            }
        }

        // 🔹 ELIMINAR un registro de una tabla
        public void Delete(string tableName, int id)
        {
            if (data.ContainsKey(tableName))
            {
                var table = data[tableName];
                var record = table.Find(r => r.ContainsKey("ID") && Convert.ToInt32(r["ID"]) == id);

                if (record != null)
                {
                    table.Remove(record);
                    SaveDatabase();
                    Console.WriteLine($"✅ Registro con ID {id} eliminado de {tableName}");
                }
                else
                {
                    Console.WriteLine($"❌ No se encontró un registro con ID {id} en {tableName}");
                }
            }
            else
            {
                Console.WriteLine($"❌ La tabla {tableName} no existe.");
            }
        }

        // 🔹 MOSTRAR los datos de una tabla
        public void ShowTable(string tableName)
        {
            if (data.ContainsKey(tableName))
            {
                Console.WriteLine($"📋 Tabla: {tableName}");
                foreach (var record in data[tableName])
                {
                    Console.WriteLine(JsonConvert.SerializeObject(record, (Newtonsoft.Json.Formatting)System.Xml.Formatting.Indented));
                }
            }
            else
            {
                Console.WriteLine($"❌ La tabla {tableName} no existe.");
            }
        }
    }
}
