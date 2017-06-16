using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PostSharp.MSBuild
{
    /// <summary>
    /// <b>[MSBuild Task]</b> Move or rename a file and
    /// retries the operation during a defined amount of time
    /// in case that a sharing violation exception occurs.
    /// </summary>
    public class PostSharp15MoveWithRetry : Task
    {
        private string source;
        private string destination;
        private int timeout = 10000;
        private int warningTimeout = 100;

        /// <summary>
        /// Gets or sets the source file path.
        /// </summary>
        [Required]
        public string Source
        {
            get { return source; }
            set { source = value; }
        }

        /// <summary>
        /// Gets or sets the target file path.
        /// </summary>
        [Required]
        public string Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        /// <summary>
        /// Gets or sets the timeout, in milliseconds, for the whole operation.
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        
        /// <summary>
        /// Gets or sets the timeout before a warning is emitted.
        /// </summary>
        public int WarningTimeout
        {
            get { return warningTimeout; }
            set { warningTimeout = value; }
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            DateTime maxEndTime = DateTime.Now.AddMilliseconds(timeout);
            DateTime warningTime = DateTime.Now.AddMilliseconds( warningTimeout );
            bool warningWritten = false;
            while (DateTime.Now < maxEndTime)
            {
                if ( DateTime.Now > warningTime && !warningWritten )
                {
                    this.Log.LogWarning( "File \"{0}\" is locked by another process. Waiting until it is released.",
                        this.destination);
                    warningWritten = true;
                }

                try
                {
                    if (File.Exists(this.destination))
                        File.Delete(this.destination);

                    File.Move(this.source, this.destination);
                    return true;
                }
                catch (Exception e)
                {
                    bool isLockException = false;

                    if ( e is UnauthorizedAccessException)
                    {
                        isLockException = true;
                    }
                    else
                    {
                        IOException ioException = e as IOException;
                        if (ioException != null)
                        {
                            uint hresult = (uint) (int)
                                                  typeof (IOException).GetProperty("HResult",
                                                                                   BindingFlags.NonPublic |
                                                                                   BindingFlags.Instance)
                                                      .GetValue(ioException, null);

                            if (hresult == 0x80070020) isLockException = true;
                        }
                    }

                    if ( isLockException )
                        Thread.Sleep(50);
                    else throw;
                }
            }

            this.Log.LogError("Cannot move file {0} to {1}.", this.source, this.destination);
            return false;
        }
    }
}