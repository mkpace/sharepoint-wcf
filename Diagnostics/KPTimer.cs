using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kingpin.WCF2.Classes.Diagnostics;

namespace Amazon.Kingpin.WCF2.Diagnostics
{
    /// <summary>
    /// Thin wrapper on Stopwatch timer for measuring elapsed time.
    /// This class can be used to both measure elasped tiem and output the metrics
    /// to a log or diagnostics output.
    /// </summary>
    public class KPTimer
    {
        private bool isDebugOutput;
        private Stopwatch timer;

        /// <summary>
        /// Default Ctor enables debug by default - for now.
        /// </summary>
        public KPTimer() : this(true) { }

        /// <summary>
        /// Ctor overloaded - set debug switch
        /// </summary>
        /// <param name="debugOutput"></param>
        public KPTimer(bool debugOutput)
        {
            this.timer = new Stopwatch();
            isDebugOutput = debugOutput;
        }

        /// <summary>
        /// Default Start method - uses standard message
        /// </summary>
        public void Start()
        {
            this.Start(string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Start(string message)
        {
            WriteLog("Started timer: {0} ({1})", DateTime.Now.ToString("hh:mm:ss"), message);
            if (this.timer.IsRunning) { this.timer.Stop(); }
            this.timer.Reset();
            this.timer.Start();
        }

        public long Stop()
        {
            return this.Stop(string.Empty);
        }

        public long Stop(string message)
        {
            if (this.timer.IsRunning)
            {
                WriteLog("Stopped timer: {0} ({1})", DateTime.Now.ToString("hh:mm:ss"), message);
                this.timer.Stop();
            }
            return this.ElapsedMilliseconds();
        }

        public void Reset()
        {
            this.timer.Reset();
        }

        public long ElapsedSeconds()
        {
            return this.timer.ElapsedMilliseconds / 1000;
        }

        public long ElapsedMilliseconds()
        {
            WriteLog("Elapsed: {0}", this.timer.ElapsedMilliseconds);
            return this.timer.ElapsedMilliseconds;
        }

        public void Restart()
        {
            if (!this.timer.IsRunning)
                throw new Exception("Timer is not running");

            WriteLog("Stopped restarted");
            this.timer.Restart();
        }

        private void WriteLog(string message)
        {
            if (isDebugOutput)
                EventLogger.WriteLine(message);
        }

        private void WriteLog(string message, params object[] args)
        {
            if (isDebugOutput)
                EventLogger.WriteLine(message, args);
        }
    }
}
