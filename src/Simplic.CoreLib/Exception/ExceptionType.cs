namespace Simplic
{
    /// <summary>
    /// Defines the exception type. An exception type determines the root/cause/issuer of the exception
    /// </summary>
    public enum ExceptionType
    {
        /// <summary>
        /// The exception is caused by the system
        /// </summary>
        Unexpected = 0,

        /// <summary>
        /// The exception is caused due to some wrong user input/data
        /// </summary>
        Expected = 1
    }
}