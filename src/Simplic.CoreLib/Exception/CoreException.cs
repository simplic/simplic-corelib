using System;
using System.Collections.Generic;

namespace Simplic
{
    /// <summary>
    /// Simplic core exception, should be used when throwing an expected/unexpected exception
    /// </summary>
    [System.Serializable]
    public class CoreException : System.Exception
    {
        /// <summary>
        /// Initialize exception instance
        /// </summary>
        public CoreException() : base()
        {
            Parameter = new List<string>();
        }

        /// <summary>
        /// Initialize exception instance
        /// </summary>
        /// <param name="code">Unique error code</param>
        /// <param name="id">Unique exception id</param>
        /// <param name="type">Exception type</param>
        /// <param name="parameter">Additional parameter for the exception message. Parameters are handled as function,
        /// to minimize the possibility of passing parameters that causes an exception too.</param>
        public CoreException(string code, string id, ExceptionType type, params Func<string>[] parameter) : this(code, id, type, null, parameter)
        {

        }

        /// <summary>
        /// Initialize exception instance
        /// </summary>
        /// <param name="code">Unique error code</param>
        /// <param name="id">Unique exception id</param>
        /// <param name="type">Exception type</param>
        /// <param name="innerException">Inner exception id</param>
        /// <param name="parameter">Additional parameter for the exception message. Parameters are handled as function,
        /// to minimize the possibility of passing parameters that causes an exception too.</param>
        public CoreException(string code, string id, ExceptionType type, Exception innerException, params Func<string>[] parameter) : base($"{code}/{id}", innerException)
        {
            Code = code;
            Parameter = new List<string>();
            Id = id;
            Type = type;

            if (parameter != null)
            {
                foreach (var param in parameter)
                {
                    if (param == null)
                        continue;

                    try
                    {
                        Parameter.Add(param() ?? "");
                    }
                    catch (Exception ex)
                    {
                        Parameter.Add($"Invalid parameter ({ex})");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the error-code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the exception id. By default this should be guid
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the parameter for the error message
        /// </summary>
        public IList<string> Parameter { get; set; }

        /// <summary>
        /// Gets or sets the actual exception type
        /// </summary>
        public ExceptionType Type { get; set; }
    }
}
