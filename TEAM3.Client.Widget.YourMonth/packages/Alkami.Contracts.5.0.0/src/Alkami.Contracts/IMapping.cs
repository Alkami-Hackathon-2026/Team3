namespace Alkami.Contracts
{
    public interface IMappingAndOrdering : IMapping
    {
        string Field { get; set; }
        bool Ascending { get; set; }
        bool Descending { get; set; }
    }

    public interface IMapping
    {
    }
}