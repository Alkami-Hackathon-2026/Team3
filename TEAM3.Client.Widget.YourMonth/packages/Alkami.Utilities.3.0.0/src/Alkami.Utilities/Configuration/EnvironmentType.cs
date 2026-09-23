namespace Alkami.Utilities.Configuration
{
    /// <summary>
    /// The EnvironmentType enumeration defines the type of the environment such as
    /// Development, Staging and Production.
    /// </summary>
    public enum EnvironmentType
    {
        /// <summary>
        /// The environment type is either unknown or not configured.
        /// </summary>
        Unknown = 9999,
        /// <summary>
        /// The environment type is a development environment.
        /// </summary>
        Development = 10,
        /// <summary>
        /// The environment type is a team QA environment.
        /// </summary>
        TeamQA = 20,
        /// <summary>
        /// The environment type is QA environment.
        /// </summary>
        QA = 30,
        /// <summary>
        /// The environment type is an Integration envrionment.
        /// </summary>
        Integration = 100,
        /// <summary>
        /// The environment type is a build environment
        /// </summary>
        Build = 110,
        /// <summary>
        /// The Secure envrionment is a marker enumeration that separates
        /// secure environments like staging and production from less
        /// secure environments such as development and QA.
        /// </summary>
        Secure = 1000,
        /// <summary>
        /// The environment type is a staging environment.
        /// </summary>
        Staging = 2000,
        /// <summary>
        /// The environment type is a production environment.
        /// </summary>
        Production = 3000,
        /// <summary>
        /// The environment type is a load testing environment.
        /// </summary>
        LoadTest = 120,
        /// <summary>
        /// The environment type is a sandbox environment.
        /// </summary>
        Sandbox = 121,
        /// <summary>
        /// The environment type is a regression environment.
        /// </summary>
        Regression = 122,
        /// <summary>
        /// The environment type is a test environment.
        /// </summary>
        Test = 123,
        /// <summary>
        /// The environment type is a sdk environment.
        /// </summary>
        SDK = 124,
        /// <summary>
        /// The environment type is a unconfigured environment.
        /// </summary>
        Unconfigured = 500
    }
}