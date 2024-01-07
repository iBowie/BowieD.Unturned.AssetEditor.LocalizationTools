namespace SyncLoc;

public abstract class LocalizationNode
{
	public LocalizationNode(string name)
	{
		this.Name = name;
	}

	public string Name { get; }

	public abstract LocalizationNode? this[string key] { get; }
}
