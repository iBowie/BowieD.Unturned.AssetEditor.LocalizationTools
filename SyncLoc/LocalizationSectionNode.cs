namespace SyncLoc;

public sealed class LocalizationSectionNode : LocalizationNode
{
	public static readonly LocalizationSectionNode EMPTY = new("");

	public LocalizationSectionNode(string name) : base(name)
	{
		this.Children = new List<LocalizationNode>();
	}
	public LocalizationSectionNode(string name, IEnumerable<LocalizationNode> children) : base(name)
	{
		this.Children = new List<LocalizationNode>(children);
	}

	public override LocalizationNode? this[string key] => Children.FirstOrDefault(d => d.Name == key);

	public IList<LocalizationNode> Children { get; }
}
