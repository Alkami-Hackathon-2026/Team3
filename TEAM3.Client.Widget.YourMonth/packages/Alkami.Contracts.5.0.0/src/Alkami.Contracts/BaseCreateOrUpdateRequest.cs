using System.Collections.Generic;
using System.Runtime.Serialization;
using Alkami.Data.Validations;

namespace Alkami.Contracts
{
    /// <summary>
    /// Base Create Or Update Request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Alkami.Contracts.BaseRequest" />
    /// <seealso cref="Alkami.Data.Validations.IValidate" />
    [DataContract(IsReference = true)]
    public abstract class BaseCreateOrUpdateRequest<T> : BaseRequest, IValidate where T : class, new()
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BaseCreateOrUpdateRequest{T}" />.
        /// </summary>
        public BaseCreateOrUpdateRequest()
        {
            ItemList = new List<T>();
        }

        /// <summary>
        /// Gets or sets the item list.
        /// </summary>
        /// <value>
        /// The item list.
        /// </value>
        [DataMember]
        public List<T> ItemList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [run as single unit of work].
        /// </summary>
        /// <value>
        /// <c>true</c> if [run as single unit of work]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RunAsSingleUnitOfWork { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var itemList = ItemList == null ? "[EMPTY]" : string.Format("[{0}]", string.Join(",", ItemList));
            return string.Format("{0}\r\n\tBaseCreateOrUpdateRequest<{3}> => ItemList: {1}\t RunAsSingleUnitOfWork: {2}", base.ToString(), itemList, RunAsSingleUnitOfWork, typeof(T).Name);
        }

        /// <summary>
        /// Validates the current object producing a collection of <see cref="ValidationResult"/>s.
        /// </summary>
        /// <returns>A collection of <see cref="ValidationResult"/>s.</returns>
        public List<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();

            if (ItemList == null)
            {
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Field = "ItemList",
                    Message = "ItemList cannot be null.",
                    Severity = Severity.Error,
                });
            }
            else
            {
                for (var i = 0; i < ItemList.Count; ++i)
                {
                    List<ValidationResult> innerResults;

                    ItemList[i].Validate(out innerResults);

                    foreach (var vr in innerResults)
                        vr.ItemIndex = i;

                    results.AddRange(innerResults);
                }
            }

            return results;
        }
    }
}