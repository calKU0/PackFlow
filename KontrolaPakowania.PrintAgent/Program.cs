using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Microsoft.Win32;
using SharpZebra.Printing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using PrinterSettings = SharpZebra.Printing.PrinterSettings;

internal class Program
{
    private static void Main()
    {
        EnsureAutostart();

        Console.WriteLine("[INFO] Printing Agent started...");
        StartListener();
    }

    private static void EnsureAutostart()
    {
        try
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appName = "PrintingAgent";

            RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);

            if (key.GetValue(appName) == null)
            {
                key.SetValue(appName, $"\"{exePath}\"");
                Console.WriteLine("[INFO] Added PrintingAgent to Windows startup.");
            }
            else
            {
                Console.WriteLine("[INFO] PrintingAgent already in startup.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] Failed to set autostart: {ex.Message}");
        }
    }

    private static void StartListener()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:54321/print/");

        try
        {
            listener.Start();
            Console.WriteLine("[INFO] HTTP listener started on http://localhost:54321/print/");
        }
        catch (HttpListenerException ex)
        {
            Console.Error.WriteLine($"[ERROR] Failed to start HTTP listener: {ex.Message}");
            return;
        }

        while (true)
        {
            try
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    continue;
                }

                if (request.HttpMethod == "POST")
                {
                    var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                    var body = reader.ReadToEnd();
                    Console.WriteLine($"[INFO] Received print request");

                    var printJob = JsonSerializer.Deserialize<PrintJob>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (printJob != null)
                    {
                        try
                        {
                            PrintToPrinter(printJob);
                            Console.WriteLine($"[INFO] Printed successfully to {printJob.PrinterName} ({printJob.DataType})");
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"[ERROR] Failed to print: {ex.Message}");
                        }
                    }

                    var buffer = Encoding.UTF8.GetBytes("Printed");
                    response.StatusCode = 200;
                    response.ContentType = "text/plain; charset=utf-8";
                    response.ContentLength64 = buffer.Length;
                    using (var output = response.OutputStream)
                    {
                        output.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] Listener exception: {ex.Message}");
            }
        }
    }

    private static void PrintToPrinter(PrintJob job)
    {
        if (job.DataType == "ZPL" || job.DataType == "EPL")
        {
            // Convert string to ASCII bytes
            var bytes = Convert.FromBase64String(job.Content);

            // Determine if printerName contains colon (network printer)
            if (job.PrinterName.Contains(":"))
            {
                NetworkPrint(job.PrinterName, bytes);
            }
            else
            {
                Print(job.PrinterName, bytes);
            }
        }
        else if (job.DataType == "PDF")
        {
            var bytes = Convert.FromBase64String(job.Content);
            PrintPdf(bytes, job.PrinterName);
        }
        else if (job.DataType == "CRYSTAL")
        {
            PrintCrystalReport(job);
        }
        else
        {
            Console.Error.WriteLine($"[ERROR] Unsupported label type: {job.DataType}");
        }
    }

    private static void Print(string printerName, byte[] label)
    {
        var printerSettings = new PrinterSettings { PrinterName = printerName };
        var printer = new SpoolPrinter(printerSettings);
        bool? result = printer.Print(label);

        if (!result.GetValueOrDefault())
        {
            throw new Exception($"Failed to print to {printerName}");
        }
    }

    private static void NetworkPrint(string printerName, byte[] label)
    {
        // Format: IP:PORT, e.g., "192.168.0.50:9100"
        var segments = printerName.Split(':');
        var printerSettings = new PrinterSettings
        {
            PrinterName = segments[0]
        };

        if (segments.Length > 1)
        {
            printerSettings.PrinterPort = int.Parse(segments[1]);
        }

        var printer = new NetworkPrinter(printerSettings);
        bool? result = printer.Print(label);

        if (!result.GetValueOrDefault())
        {
            throw new Exception($"Failed to print to network printer {printerName}");
        }
    }

    private static void PrintPdf(byte[] pdfBytes, string printerName)
    {
        // Load Pdfium first
        LoadPdfiumNative();

        string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
        File.WriteAllBytes(tempFile, pdfBytes);

        PdfiumViewer.PdfDocument doc = null;

        try
        {
            doc = PdfiumViewer.PdfDocument.Load(tempFile);
            var printDoc = new System.Drawing.Printing.PrintDocument();
            printDoc.PrinterSettings.PrinterName = printerName;

            int pageIndex = 0;
            printDoc.PrintPage += (s, e) =>
            {
                if (pageIndex < doc.PageCount)
                {
                    var image = doc.Render(pageIndex, e.MarginBounds.Width, e.MarginBounds.Height, true);
                    e.Graphics.DrawImage(image, 0, 0);
                    pageIndex++;
                    e.HasMorePages = pageIndex < doc.PageCount;
                }
            };

            printDoc.Print();
            Console.WriteLine($"[INFO] Printed PDF to {printerName}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] PDF printing failed: {ex.Message}");
        }
        finally
        {
            doc?.Dispose();
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private static void PrintCrystalReport(PrintJob job)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));

        var report = new ReportDocument();
        try
        {
            // Load the report
            report.Load(job.Content);

            // Apply database logon for each table
            if (job.Parameters != null &&
                job.Parameters.TryGetValue("DbUser", out var dbUser) &&
                job.Parameters.TryGetValue("DbPassword", out var dbPassword) &&
                job.Parameters.TryGetValue("DbServer", out var dbServer) &&
                job.Parameters.TryGetValue("DbName", out var dbName))
            {
                report.SetDatabaseLogon(dbUser, dbPassword, dbServer, dbName);
            }

            // Apply parameters and selection formula
            if (job.Parameters != null)
            {
                foreach (var kv in job.Parameters)
                {
                    if (kv.Key.Equals("DbUser", StringComparison.OrdinalIgnoreCase) ||
                        kv.Key.Equals("DbPassword", StringComparison.OrdinalIgnoreCase) ||
                        kv.Key.Equals("DbServer", StringComparison.OrdinalIgnoreCase) ||
                        kv.Key.Equals("DbName", StringComparison.OrdinalIgnoreCase))
                        continue;

                    report.SetParameterValue(kv.Key, kv.Value);
                }
            }
            // Set printer and print
            report.PrintOptions.PrinterName = string.IsNullOrEmpty(job.PrinterName) ? string.Empty : job.PrinterName;
            //report.ExportToDisk(ExportFormatType.PortableDocFormat, @"D:\a\Test.pdf");
            report.PrintToPrinter(1, true, 0, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error printing report: " + ex);
        }
        finally
        {
            // Safe cleanup
            try { report.Close(); } catch { }
            try { report.Dispose(); } catch { }
        }
    }

    private static void LoadPdfiumNative()
    {
        string architecture = Environment.Is64BitProcess ? "x64" : "x86";
        string resourceName = $"KontrolaPakowania.PrintAgent.native.win_{architecture}.pdfium.dll";

        // Temporary path to extract the DLL
        string tempPath = Path.Combine(Path.GetTempPath(), "pdfium.dll");

        foreach (var res in Assembly.GetExecutingAssembly().GetManifestResourceNames())
        {
            Console.WriteLine(res);
        }

        if (!File.Exists(tempPath))
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new Exception($"Cannot find embedded resource: {resourceName}");

                using (var file = File.Create(tempPath))
                {
                    stream.CopyTo(file);
                }
            }
        }

        // Load the DLL manually
        IntPtr handle = NativeMethods.LoadLibrary(tempPath);
        if (handle == IntPtr.Zero)
            throw new Exception("Failed to load pdfium.dll");
    }

    private static class NativeMethods
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);
    }
}

public class PrintJob
{
    public string PrinterName { get; set; }
    public string DataType { get; set; } // "ZPL", "EPL", "PDF", "CRYSTAL"
    public string Content { get; set; }
    public Dictionary<string, string> Parameters { get; set; }
}