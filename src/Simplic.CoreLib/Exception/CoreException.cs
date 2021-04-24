namespace Simplic.Exception
{
    /// <summary>
    /// Simplic core exception, should be used when throwing an expected exception
    /// </summary>
    [System.Serializable]
    public class CoreException : System.Exception
    {
        /// <summary>
        /// Initialize exception instance
        /// </summary>
        public CoreException() : base()
        {
            Parameter = new string[] { };
        }

        /// <summary>
        /// Initialize exception instance
        /// </summary>
        /// <param name="code">Unique error code</param>
        /// <param name="parameter">Additional parameter for the exception message</param>
        public CoreException(string code, params string[] parameter)
        {
            Code = code;
            Parameter = parameter;
        }

        /// <summary>
        /// Gets or sets the error-code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the parameter for the error message
        /// </summary>
        public string[] Parameter { get; set; }
    }
}
