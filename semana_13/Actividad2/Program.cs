using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotNetEnv;
using Microsoft.Extensions.Configuration;

namespace MIA_AzureBlob
{
    class Program
    {
        // Nombre del contenedor en minúsculas
        private const string containerName = "mia-archivos";

        private static string GetRequiredConnectionString()
        {
            const string configKey = "AzureStorage:ConnectionString";
            const string environmentVariableName = "AZURE_STORAGE_CONNECTION_STRING";

            var valueFromEnvironment = Environment.GetEnvironmentVariable(environmentVariableName);
            if (!string.IsNullOrWhiteSpace(valueFromEnvironment))
            {
                return valueFromEnvironment;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var valueFromConfig = configuration[configKey];
            if (!string.IsNullOrWhiteSpace(valueFromConfig))
            {
                return valueFromConfig;
            }

            throw new InvalidOperationException(
                $"Falta la configuración '{configKey}'. Establécela como variable de entorno '{environmentVariableName}' " +
                "en el archivo .env o en appsettings.Development.json, y no subas esos archivos a GitHub.");
        }

        static async Task Main(string[] args)
        {
            string envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envFilePath))
            {
                Env.Load(envFilePath);
            }

            string connectionString = GetRequiredConnectionString();

            BlobServiceClient blobServiceClient;
            BlobContainerClient containerClient;

            try
            {
                blobServiceClient = new BlobServiceClient(connectionString);
                containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Crear el contenedor si no existe de forma privada
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Error de conexión con Azure]: {ex.Message}");
                Console.WriteLine("Verifica que la Connection String sea válida y corresponda a tu cuenta de Azure Storage.");
                return;
            }

            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("=================================");
                Console.WriteLine("    MIA - AZURE BLOB STORAGE     ");
                Console.WriteLine("=================================");
                Console.WriteLine("1. Subir archivo");
                Console.WriteLine("2. Listar archivos");
                Console.WriteLine("3. Descargar archivo");
                Console.WriteLine("4. Eliminar archivo");
                Console.WriteLine("5. Salir");
                Console.WriteLine("=================================");
                Console.Write("Seleccione una opción: ");

                string? opcion = Console.ReadLine();

                try
                {
                    switch (opcion)
                    {
                        case "1":
                            await SubirArchivo(containerClient);
                            break;
                        case "2":
                            await ListarArchivos(containerClient);
                            break;
                        case "3":
                            await DescargarArchivo(containerClient);
                            break;
                        case "4":
                            await EliminarArchivo(containerClient);
                            break;
                        case "5":
                            salir = true;
                            Console.WriteLine("¡Hasta luego!");
                            break;
                        default:
                            Console.WriteLine("Opción no válida.");
                            Pausar();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[Error de operación]: {ex.Message}");
                    Pausar();
                }
            }
        }

        private static async Task SubirArchivo(BlobContainerClient containerClient)
        {
            Console.WriteLine("\n--- SUBIR ARCHIVO ---");
            Console.Write("Ingrese la ruta local del archivo: ");
            string? entradaRuta = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(entradaRuta))
            {
                Console.WriteLine("Error: La ruta no puede estar vacía.");
                Pausar();
                return;
            }

            string rutaLocal = entradaRuta.Trim('"');

            if (!File.Exists(rutaLocal))
            {
                Console.WriteLine("Error: El archivo local no existe.");
                Pausar();
                return;
            }

            string nombreArchivo = Path.GetFileName(rutaLocal);
            BlobClient blobClient = containerClient.GetBlobClient(nombreArchivo);

            Console.WriteLine("Subiendo archivo a Azure Storage...");
            using FileStream stream = File.OpenRead(rutaLocal);
            await blobClient.UploadAsync(stream, overwrite: true);

            Console.WriteLine($"✓ Archivo '{nombreArchivo}' subido con éxito.");
            Pausar();
        }

        private static async Task ListarArchivos(BlobContainerClient containerClient)
        {
            Console.WriteLine("\n--- ARCHIVOS EN AZURE BLOB STORAGE ---");
            Console.WriteLine("{0,-30} {1,15}", "Nombre", "Tamaño");
            Console.WriteLine(new string('-', 48));

            int contador = 0;
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                string tamanoStr = blobItem.Properties.ContentLength.HasValue 
                    ? $"{blobItem.Properties.ContentLength.Value} bytes" 
                    : "N/A";

                Console.WriteLine("{0,-30} {1,15}", blobItem.Name, tamanoStr);
                contador++;
            }

            if (contador == 0)
            {
                Console.WriteLine("El contenedor está vacío.");
            }

            Pausar();
        }

        private static async Task DescargarArchivo(BlobContainerClient containerClient)
        {
            Console.WriteLine("\n--- DESCARGAR ARCHIVO ---");
            Console.Write("Ingrese el nombre del blob en Azure: ");
            string? nombreBlob = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombreBlob))
            {
                Console.WriteLine("Error: El nombre del blob no puede estar vacío.");
                Pausar();
                return;
            }

            BlobClient blobClient = containerClient.GetBlobClient(nombreBlob);

            if (!await blobClient.ExistsAsync())
            {
                Console.WriteLine("Error: El blob especificado no existe en el contenedor.");
                Pausar();
                return;
            }

            Console.Write("Ingrese la carpeta de destino local: ");
            string? entradaCarpeta = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(entradaCarpeta))
            {
                Console.WriteLine("Error: La carpeta de destino no puede estar vacía.");
                Pausar();
                return;
            }

            string carpetaDestino = entradaCarpeta.Trim('"');

            if (!Directory.Exists(carpetaDestino))
            {
                Directory.CreateDirectory(carpetaDestino);
            }

            string rutaDestinoCompleta = Path.Combine(carpetaDestino, nombreBlob);

            Console.WriteLine("Descargando...");
            await blobClient.DownloadToAsync(rutaDestinoCompleta);

            Console.WriteLine($"✓ Archivo guardado exitosamente en: {rutaDestinoCompleta}");
            Pausar();
        }

        private static async Task EliminarArchivo(BlobContainerClient containerClient)
        {
            Console.WriteLine("\n--- ELIMINAR ARCHIVO ---");
            Console.Write("Ingrese el nombre del blob a eliminar: ");
            string? nombreBlob = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombreBlob))
            {
                Console.WriteLine("Error: El nombre del blob no puede estar vacío.");
                Pausar();
                return;
            }

            BlobClient blobClient = containerClient.GetBlobClient(nombreBlob);

            if (!await blobClient.ExistsAsync())
            {
                Console.WriteLine("Error: El blob especificado no existe.");
                Pausar();
                return;
            }

            Console.Write($"¿Está seguro de que desea eliminar '{nombreBlob}'? (s/n): ");
            string? confirmacion = Console.ReadLine();

            if (confirmacion?.ToLower() == "s")
            {
                await blobClient.DeleteIfExistsAsync();
                Console.WriteLine($"✓ El blob '{nombreBlob}' ha sido eliminado.");
            }
            else
            {
                Console.WriteLine("Operación cancelada.");
            }

            Pausar();
        }

        private static void Pausar()
        {
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}