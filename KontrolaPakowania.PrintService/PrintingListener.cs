using KontrolaPakowania.PrintService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KontrolaPakowania.PrintService
{
    public static class PrintingListener
    {
        public static void Start(bool running)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:54321/print/");
            listener.Start();

            while (running)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                response.AddHeader("Access-Control-Allow-Origin", "*");

                if (request.HttpMethod == "POST")
                {
                    using (var reader = new StreamReader(request.InputStream))
                    {
                        var body = reader.ReadToEnd();

                        var job = JsonSerializer.Deserialize<PrintJob>(body,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        PrintManager.Print(job);

                        var buffer = Encoding.UTF8.GetBytes("OK");
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                }

                response.Close();
            }

            listener.Stop();
        }
    }
}
