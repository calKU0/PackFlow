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

                try
                {
                    // ---- CORS HEADERS ----
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    response.AddHeader("Access-Control-Allow-Methods", "POST, OPTIONS");
                    response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

                    // ---- PREFLIGHT ----
                    if (request.HttpMethod == "OPTIONS")
                    {
                        response.StatusCode = 200;
                        var buffer = Encoding.UTF8.GetBytes("OK");
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        continue;
                    }

                    // ---- POST REQUEST ----
                    if (request.HttpMethod == "POST")
                    {
                        using (var reader = new StreamReader(request.InputStream))
                        {
                            var body = reader.ReadToEnd();

                            var job = JsonSerializer.Deserialize<PrintJob>(
                                body,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            // ---- HEALTH CHECK ----
                            if (job?.DataType == "PING")
                            {
                                response.StatusCode = 200;
                                var buffer = Encoding.UTF8.GetBytes("PONG");
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                continue;
                            }

                            // ---- REAL PRINTING ----
                            PrintManager.Print(job);

                            response.StatusCode = 200;
                            var okBuffer = Encoding.UTF8.GetBytes("OK");
                            response.OutputStream.Write(okBuffer, 0, okBuffer.Length);
                        }
                    }
                    else
                    {
                        // Method not allowed
                        response.StatusCode = 405;
                        var buffer = Encoding.UTF8.GetBytes("Method Not Allowed");
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"[ERROR] Printing failed: {ex}");
                    response.StatusCode = 500;
                    var buffer = Encoding.UTF8.GetBytes(ex.Message);
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                finally
                {
                    // Close the response always
                    response.OutputStream.Close();
                    response.Close();
                }
            }

            listener.Stop();
        }
    }
}