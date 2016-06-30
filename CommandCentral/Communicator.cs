using System;
using System.Collections.Generic;
using System.IO;

namespace CommandCentral
{
    /// <summary>
    /// Provides for allowing the backend WCF service to communicate with its host application
    /// </summary>
    public static class Communicator
    {
        /// <summary>
        /// Indicates whether or not communications from the service have been frozen.
        /// </summary>
        public static bool IsFrozen;

        internal static TextWriter TextWriter;
        /// <summary>
        /// Indicates which messages should be forwarded onto the host, and which messages should be silently assassinated.
        /// </summary>
        public static List<MessageTypes> ListeningTypes = new List<MessageTypes>();

        /// <summary>
        /// Describes message types so that the host can choose what messages it listens to.
        /// </summary>
        public enum MessageTypes
        {
            /// <summary>
            /// Informational messages alert the host of routine operations.
            /// </summary>
            Informational,
            /// <summary>
            /// Important messages alert the host to information that could affect the operation of the service.
            /// </summary>
            Important,
            /// <summary>
            /// Warnings alert the host to information that, left unchecked, will affect smooth operations of the service.
            /// </summary>
            Warning,
            /// <summary>
            /// Critical messages alert the host to the fact that a fatal error has occurred in the service and that re-mediating action should be taken immediately.
            /// </summary>
            Critical,
            /// <summary>
            /// A cron operation message is a message from the Cron Operations Manager.
            /// </summary>
            CronOperation
        }

        /// <summary>
        /// Gets a value indicating if the Communicator has been initialized.
        /// </summary>
        public static bool IsCommunicatorInitialized
        {
            get
            {
                return TextWriter != null;
            }
        }

        /// <summary>
        /// Initializes the communications object.  The text writer should be a stream to which you want messages to be posted.  The priorities indicate to which messages the caller would like to listen.
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="priorities"></param>
        public static void InitializeCommunicator(TextWriter textWriter, List<MessageTypes> priorities)
        {
            TextWriter = textWriter;
            ListeningTypes = priorities;
        }

        /// <summary>
        /// Initializes the communications object.  The text writer should be a stream to which you want messages to be posted.  Listens to all priorities.
        /// </summary>
        /// <param name="textWriter"></param>
        public static void InitializeCommunicator(TextWriter textWriter)
        {
            TextWriter = textWriter;
            ListeningTypes = new List<MessageTypes> { MessageTypes.Critical, MessageTypes.Important, MessageTypes.Informational, MessageTypes.Warning, MessageTypes.CronOperation };
        }

        /// <summary>
        /// Sends a message to the message stream if it has been set.  If it hasn't, nothing happens.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        public static void PostMessageToHost(string message, MessageTypes messageType)
        {
            if (TextWriter != null && ListeningTypes.Contains(messageType) && !IsFrozen)
            {
                TextWriter.WriteLine("{0} Service Message @ {1}:\n\t{2}", messageType, DateTime.Now, message);
                TextWriter.WriteLine();
            }
        }

        /// <summary>
        /// Freezes the communicator, stopping all communication from the service; however, the service will continue to run.
        /// </summary>
        public static void Freeze()
        {
            IsFrozen = true;
        }

        /// <summary>
        /// Resumes communications from the service.
        /// </summary>
        public static void Unfreeze()
        {
            IsFrozen = false;
        }

        /// <summary>
        /// Releases the communicator by disposing of the writer object and setting it to null
        /// </summary>
        public static void ReleaseCommunicator()
        {
            TextWriter.Dispose();
            TextWriter = null;
        }

    }
}
