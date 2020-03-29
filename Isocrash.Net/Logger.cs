namespace Isocrash.Net
{
    public sealed class Logger
    {
        #region Public Members
        /// <summary>
        /// Method that the logger will use to log with
        /// </summary>
        /// <param name="message"></param>
        public delegate void LoggerCallback(object message);
        #endregion

        #region Internal Members
        internal LoggerCallback _infoCallback;
        internal LoggerCallback _warningCallback;
        internal LoggerCallback _errorCallback;
        internal LoggerCallback _exceptionCallback;
        #endregion
        
        #region Internal Constructors
        /// <summary>
        /// Create a new callback
        /// </summary>
        /// <param name="info">The information log method</param>
        /// <param name="warning">The warning log method</param>
        /// <param name="error">The error log method</param>
        /// <param name="exception">The exception log method</param>
        internal Logger(
            LoggerCallback info,
            LoggerCallback warning,
            LoggerCallback error,
            LoggerCallback exception)
        {
            this._infoCallback = info;
            this._warningCallback = warning;
            this._errorCallback = error;
            this._exceptionCallback = exception;
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Logs a message as information
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Log(object message)
        {
            _infoCallback?.Invoke(message);
        }
        /// <summary>
        /// Logs a message as warning
        /// </summary>
        /// <param name="message">The message to log</param>
        public void LogWarning(object message)
        {
            _warningCallback?.Invoke(message);
        }
        /// <summary>
        /// Logs a message as warning
        /// </summary>
        /// <param name="message">The message to log</param>
        public void LogError(object message)
        {
            _errorCallback?.Invoke(message);
        }
        /// <summary>
        /// Logs a message as exception
        /// </summary>
        /// <param name="message">The message to log</param>
        public void LogException(object message)
        {
            _exceptionCallback?.Invoke(message);
        }
        #endregion
    }
}