using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KontrolaPakowania.PrintService
{
    public partial class PrintingService : ServiceBase
    {
        private Thread listenerThread;
        private bool running;
        public PrintingService()
        {
            InitializeComponent();
            ServiceName = "KontrolaPakowaniaPrintingService";
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            running = true;

            listenerThread = new Thread(StartListener)
            {
                IsBackground = true
            };

            listenerThread.Start();
        }

        protected override void OnStop()
        {
            running = false;
        }

        private void StartListener()
        {
            try
            {
                PrintingListener.Start(running);
            }
            catch (Exception ex)
            {
                // Log to Event Log or file
                EventLog.WriteEntry(
                    "PrintingAgentService",
                    ex.ToString(),
                    EventLogEntryType.Error);
            }
        }
    }
}
