using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UnifiedServiceFramework
{
    /// <summary>
    /// Provides for allowing the backend WCF service to communite with its host application
    /// </summary>
    public static class Communicator
    {
        /// <summary>
        /// Indicates whether or not communications from the service have been frozen.
        /// </summary>
        public static bool IsFrozen = false;

        private static TextWriter _writer = null;
        /// <summary>
        /// Indicates which messages should be forwarded onto the host, and which messages should be silently assassinated.
        /// </summary>
        public static List<MessagePriority> listeningPriorities = new List<MessagePriority>();

        /// <summary>
        /// Describes message priorities so that the host can choose what messages it listens to.
        /// </summary>
        public enum MessagePriority
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
            /// Critical messages alert the host to the fact that a fatal error has occurred in the service and that remediating action should be taken immediately.
            /// </summary>
            Critical
        };

        /// <summary>
        /// Gets a value indicating if the Communicator has been initialized.
        /// </summary>
        public static bool IsCommunicatorInitialized
        {
            get
            {
                return _writer != null;
            }
        }

        /// <summary>
        /// Initializes the communications object.  The text writer should be a stream to which you want messages to be posted.  The priorities indicate to which messages the caller would like to listen.
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="priorities"></param>
        public static void InitializeCommunicator(TextWriter textWriter, List<MessagePriority> priorities)
        {
            _writer = textWriter;
            listeningPriorities = priorities;
        }

        /// <summary>
        /// Initializes the communications object.  The text writer should be a stream to which you want messages to be posted.  Listens to all priorities.
        /// </summary>
        /// <param name="textWriter"></param>
        public static void InitializeCommunicator(TextWriter textWriter)
        {
            _writer = textWriter;
            listeningPriorities = new List<MessagePriority>() { MessagePriority.Critical, MessagePriority.Important, MessagePriority.Informational, MessagePriority.Warning };
        }

        /// <summary>
        /// Sends a message to the message stream if it has been set.  If it hasn't, nothing happens.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="priority"></param>
        public static void PostMessageToHost(string message, MessagePriority priority)
        {
            if (_writer != null && listeningPriorities.Contains(priority) && !IsFrozen)
            {
                _writer.WriteLine(string.Format("{0} Service Message @ {1}:\n\t{2}", priority.ToString(), DateTime.Now.ToString(), message));
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
            _writer.Dispose();
            _writer = null;
        }

    }
}
