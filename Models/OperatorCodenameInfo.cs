namespace OperatorImageViewer.Models
{
    public readonly record struct OperatorCodenameInfo : IComparable<OperatorCodenameInfo>
    {
        public OperatorCodenameInfo(string codeName, string name)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Codename = codeName ?? throw new System.ArgumentNullException(nameof(codeName));
        }

        public string Codename { get; init; }
        public string Name { get; init; }

        public int CompareTo(OperatorCodenameInfo other)
        {
            return Codename.CompareTo(other.Codename);
        }

        public override string ToString()
        {
            return Codename;
        }
    }
}
