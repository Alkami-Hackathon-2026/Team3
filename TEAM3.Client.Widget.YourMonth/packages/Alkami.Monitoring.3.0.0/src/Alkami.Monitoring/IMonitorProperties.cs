using System;
using System.Collections.Generic;

namespace Alkami.Monitoring
{
    /// <summary>
    /// The ICustomProperties interface provides an abstraction for adding custom properties (key/value pair) to the monitoring 
    /// transaction context which can be used to provide added context for monitoring the health of an application.
    /// </summary>
    public interface IMonitorProperties
    {
        /// <summary>
        /// Adds a custom property (name/value pair) to the monitoring transaction context.
        /// </summary>
        /// <param name="name">The name of the name/value pair to add to the monitoring transaction context.</param>
        /// <param name="value">The value of the name/value pair to add to the monitoring transaction context.</param>
        void AddCustomProperty(string name, string value);

        /// <summary>
        /// Ignores the transaction.
        /// </summary>
        void IgnoreTransaction();

        /// <summary>
        /// Sets the name of the transaction.
        /// </summary>
        /// <param name="category">The category of this transaction, which you can use to distinguish different types of transactions.</param>
        /// <param name="name">The name of the transaction.</param>
        void SetTransactionName(string category, string name);

        /// <summary>
        /// Sets the user properties to the monitoring transaction context.
        /// </summary>
        /// <param name="userValue">Specify a name or user name to associate with this request. This value is assigned to the user key.</param>
        /// <param name="accountValue">Specify the name of a user account to associate with this request. This value is assigned to the account key.</param>
        /// <param name="productValue">Specify the name of a product to associate with this request. This value is assigned to the product key.</param>
        void SetUserProperties(string userValue, string accountValue, string productValue);
    }
}
