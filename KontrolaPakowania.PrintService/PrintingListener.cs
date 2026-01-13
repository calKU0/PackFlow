using BusinessObjects.ThirdParty.OOC.FSSL;
using KontrolaPakowania.PrintService.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using KontrolaPakowania.PrintService.Logging;
using Logger = KontrolaPakowania.PrintService.Logging.Logger;

namespace KontrolaPakowania.PrintService
{
    public static class PrintingListener
    {
        public static void Start(bool running)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:54321/print/");
            listener.Start();
            Logger.Info("Started on http://localhost:54321/print/");

            while (running)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                response.AddHeader("Access-Control-Allow-Origin", "*");

                if (request.HttpMethod == "POST")
                {
                    try
                    {
                        using (var reader = new StreamReader(request.InputStream))
                        {
                            var body = reader.ReadToEnd();

                            // Log that a request came in
                            Logger.Info($"[INFO] Received print request: {body}");

                            var job = JsonSerializer.Deserialize<PrintJob>(body,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            // Log the target printer and data type
                            Logger.Info($"[INFO] Printing to '{job.PrinterName}', type '{job.DataType}'");

                            PrintManager.Print(job); // might throw

                            // If successful
                            Logger.Info("[INFO] Print job completed successfully.");
                            response.StatusCode = 200;
                            var buffer = Encoding.UTF8.GetBytes("OK");
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[ERROR] Printing failed: {ex}");
                        response.StatusCode = 500; // Internal Server Error
                        var buffer = Encoding.UTF8.GetBytes($"Printing failed: {ex.Message}");
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                }

                response.Close();
            }

            listener.Stop();
        }
    }
}