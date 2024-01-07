using SyncLoc;

var ymls = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.yml", SearchOption.TopDirectoryOnly);

if (ymls.Length == 0)
{
	Console.WriteLine("Couldn't find any localization files here.");
	return;
}

string baseYmlPath;
string targetYmlPath;

Console.WriteLine("Select source localization");

int index = 0;
foreach (var yml in ymls)
{
	Console.WriteLine($"{++index}. {Path.GetFileNameWithoutExtension(yml)}");
}

while (true)
{
	Console.Write("> ");
	string? input = Console.ReadLine();

	if (!int.TryParse(input, out var inputIndex))
	{
		Console.WriteLine("Input is not a number.");
		continue;
	}

	if (inputIndex < 1)
	{
		Console.WriteLine("Input number is too small.");
		continue;
	}

	if (inputIndex > ymls.Length)
	{
		Console.WriteLine("Input number is too large.");
		continue;
	}

	baseYmlPath = ymls[inputIndex - 1];
	break;
}

Console.WriteLine("Select target localization");

index = 0;
foreach (var yml in ymls)
{
	Console.WriteLine($"{++index}. {Path.GetFileNameWithoutExtension(yml)}");
}

while (true)
{
	Console.Write("> ");
	string? input = Console.ReadLine();

	if (!int.TryParse(input, out var inputIndex))
	{
		Console.WriteLine("Input is not a number.");
		continue;
	}

	if (inputIndex < 1)
	{
		Console.WriteLine("Input number is too small.");
		continue;
	}

	if (inputIndex > ymls.Length)
	{
		Console.WriteLine("Input number is too large.");
		continue;
	}

	if (ymls[inputIndex - 1] == baseYmlPath)
	{
		Console.WriteLine("Target and source cannot be the same.");
		continue;
	}

	targetYmlPath = ymls[inputIndex - 1];
	break;
}

var baseYmlContent = File.ReadAllText(baseYmlPath);

if (!Localization.TryDeserialize(baseYmlContent, out var baseLocalization))
{
	Console.WriteLine(baseYmlPath + " contains errors.");
	return;
}

var targetYmlContent = File.ReadAllText(targetYmlPath);

if (!Localization.TryDeserialize(targetYmlContent, out var targetLocalization))
{
	Console.WriteLine(targetYmlPath + " contains errors.");
	return;
}

void process(LocalizationSectionNode src, LocalizationSectionNode target)
{
	int srcIndex = 0;
	foreach (var srcChild in src.Children)
	{
		var oppositeNode = target[srcChild.Name];

		if (srcChild is LocalizationSectionNode sectionNode)
		{
			if (oppositeNode is LocalizationSectionNode oppositeSection)
			{
				process(sectionNode, oppositeSection);
			}
			else if (oppositeNode is not null)
			{
				target.Children.Remove(oppositeNode);
				try
				{
					target.Children.Insert(srcIndex, sectionNode);
				}
				catch
				{
					target.Children.Add(sectionNode);
				}
			}
			else
			{
				try
				{
					target.Children.Insert(srcIndex, sectionNode);
				}
				catch
				{
					target.Children.Add(sectionNode);
				}
			}
		}
		else if (srcChild is LocalizationValueNode valueNode)
		{
			if (oppositeNode is not null && oppositeNode is not LocalizationValueNode)
			{
				target.Children.Remove(oppositeNode);
				try
				{
					target.Children.Insert(srcIndex, valueNode);
				}
				catch
				{
					target.Children.Add(valueNode);
				}
			}
			else if (oppositeNode is null)
			{
				try
				{
					target.Children.Insert(srcIndex, valueNode);
				}
				catch
				{
					target.Children.Add(valueNode);
				}
			}
		}

		srcIndex++;
	}
}

process(baseLocalization.Root, targetLocalization.Root);

Dictionary<string, string> newExternalKeys = new();

foreach (var key in baseLocalization.ExternalKeys.Keys)
{
	if (targetLocalization.ExternalKeys.TryGetValue(key, out var targetValue))
	{
		newExternalKeys[key] = targetValue;
	}
	else
	{
		newExternalKeys[key] = baseLocalization.ExternalKeys[key];
	}
}

targetLocalization.ExternalKeys = newExternalKeys;

if (baseLocalization.Version != targetLocalization.Version)
{
	var oldClr = Console.ForegroundColor;
	Console.ForegroundColor = ConsoleColor.Yellow;
	Console.WriteLine("Version is not the same. Please, check version.");
	Console.ForegroundColor = oldClr;
}

if (baseLocalization.Tips.Length != targetLocalization.Tips.Length)
{
	var oldClr = Console.ForegroundColor;
	Console.ForegroundColor = ConsoleColor.Yellow;
	Console.WriteLine("Tips count is not the same. Please, check tips.");
	Console.ForegroundColor = oldClr;
}

targetYmlContent = targetLocalization.ExportToYaml();
File.WriteAllText(targetYmlPath, targetYmlContent);

var oldClr1 = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.DarkGreen;
Console.WriteLine("Sync complete. Press any key to exit.");
Console.ForegroundColor = oldClr1;
Console.ReadKey(true);