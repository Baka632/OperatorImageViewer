namespace OperatorImageViewer.Models
{
    public readonly record struct OperatorCodenameInfo
    {
        public OperatorCodenameInfo(string codeName, string name)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Codename = codeName ?? throw new System.ArgumentNullException(nameof(codeName));
        }

        public string Codename { get; init; }
        public string Name { get; init; }

        public override string ToString()
        {
            return Codename;
        }
    }
}
