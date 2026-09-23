using System.Collections.Generic;
using System.Runtime.Serialization;
using Alkami.Data.Validations;

namespace Alkami.Contracts
{
    [DataContract(IsReference = true)]
    public abstract class BaseGetRequest<TFilter, TMapper, TSorter> : BaseRequest, IValidate
        where TFilter : IFilter, new()
        where TMapper : IMapping, new()
        where TSorter : ISortOrder, new()
    {
        protected BaseGetRequest()
        {
            this.Filter = new TFilter();
            this.Mapping = new TMapper();
            this.Sorter = new TSorter();
        }

        [DataMember]
        public TFilter Filter { get; set; }

        [DataMember]
        public TMapper Mapping { get; set; }

        [DataMember]
        public TSorter Sorter { get; set; }

        public override string ToString()
        {
            return string.Format("{0}\r\n\tBaseGetRequest<{1},{2},{3}> => Filter: {4}\t Mapping: {5}, Sorter: {6}", base.ToString(), typeof(TFilter).Name, typeof(TMapper).Name, typeof(TSorter).Name, Filter, Mapping, Sorter);
        }

        /// <summary>
        /// Validates the current object producing a collection of <see cref="ValidationResult"/>s.
        /// </summary>
        /// <returns>A collection of <see cref="ValidationResult"/>s.</returns>
        public List<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();

            if ((this.Filter != null) && (this.Filter.GetType() != typeof(EmptyFilter)))
            {
                List<ValidationResult> innerResults;

                this.Filter.Validate(out innerResults);

                foreach (var vr in innerResults)
                    vr.Message = "Filter - " + vr.Message;

                results.AddRange(innerResults);
            }

            if ((this.Mapping != null) && (this.Mapping.GetType() != typeof(EmptyMapper)))
            {
                List<ValidationResult> innerResults;

                this.Mapping.Validate(out innerResults);

                foreach (var vr in innerResults)
                    vr.Message = "Mapping - " + vr.Message;

                results.AddRange(innerResults);
            }

            if (this.Sorter == null)
            {
                results.AddValidationWarning("Sorter", "Sorter was null, so a new Sorter was constructed.", SubCode.ValueUnsupported);

                this.Sorter = new TSorter();
            }
            else if ((this.Sorter != null) && (this.Sorter.GetType() != typeof(DefaultOrderer)))
            {
                List<ValidationResult> innerResults;

                this.Sorter.Validate(out innerResults);

                foreach (var vr in innerResults)
                    vr.Message = "Sorter - " + vr.Message;

                results.AddRange(innerResults);
            }

            return results;
        }
    }
}