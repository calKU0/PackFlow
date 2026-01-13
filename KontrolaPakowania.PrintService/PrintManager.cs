using CrystalDecisions.CrystalReports.Engine;
using KontrolaPakowania.PrintService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing.Printing;
using System.Threading.Tasks;
using KontrolaPakowania.PrintService.Logging;
using BusinessObjects.ThirdParty.OOC.FSSL;
using Logger = KontrolaPakowania.PrintService.Logging.Logger;

namespace KontrolaPakowania.PrintService
{
    public static class PrintManager
    {
        public static void Print(PrintJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            if (job.DataType == "ZPL" || job.DataType == "EPL")
            {
                var bytes = Convert.FromBase64String(job.Content);

                if (job.PrinterName.Contains(":"))
                    NetworkPrint(job.PrinterName, bytes);
                else
                    PrintRaw(job.PrinterName, bytes);
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
                throw new Exception($"Unsupported DataType: {job.DataType}");
            }
        }

        private static void PrintRaw(string printerName, byte[] label)
        {
            var printerSettings = new SharpZebra.Printing.PrinterSettings
            {
                PrinterName = printerName
            };

            var printer = new SharpZebra.Printing.SpoolPrinter(printerSettings);
            if (!printer.Print(label).GetValueOrDefault())
                throw new Exception($"Failed to print to {printerName}");
        }

        private static void NetworkPrint(string printerName, byte[] label)
        {
            var parts = printerName.Split(':');

            var settings = new SharpZebra.Printing.PrinterSettings
            {
                PrinterName = parts[0],
                PrinterPort = parts.Length > 1 ? int.Parse(parts[1]) : 9100
            };

            var printer = new SharpZebra.Printing.NetworkPrinter(settings);
            if (!printer.Print(label).GetValueOrDefault())
                throw new Exception($"Failed to print to network printer {printerName}");
        }

        // Copy your existing methods here unchanged
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
                printDoc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
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
                Logger.Info($"[INFO] Printed PDF to {printerName}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[ERROR] PDF printing failed: {ex.Message}");
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
                Logger.Error("Error printing report: " + ex);
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
            string resourceName = $"KontrolaPakowania.PrintService.native.win_{architecture}.pdfium.dll";

            // Temporary path to extract the DLL
            string tempPath = Path.Combine(Path.GetTempPath(), "pdfium.dll");

            foreach (var res in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                Logger.Info(res);
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
}