namespace SyncLoc;

public sealed class LocalizationValueNode : LocalizationNode
{
	public LocalizationValueNode(string name, string value) : base(name)
	{
		this.Value = value;
	}

	public override LocalizationNode? this[string key] => throw new NotImplementedException();

	public string Value { get; set; }
}
