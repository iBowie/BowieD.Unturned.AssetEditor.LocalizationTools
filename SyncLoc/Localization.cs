using System.Diagnostics.CodeAnalysis;
using YamlDotNet.RepresentationModel;

namespace SyncLoc;

public sealed class Localization
{
	public Localization()
	{
		Name = "#name";
		Authors = Array.Empty<string>();
		Version = "#version";
		CultureCode = "";
		Keys = new();
		Tips = Array.Empty<string>();
		Root = new("");
		ExternalKeys = new();
	}

	public string Name { get; set; }
	public string[] Authors { get; set; }
	public string Version { get; set; }
	public string CultureCode { get; set; }
	public Dictionary<string, string> Keys { get; set; }
	public string[] Tips { get; set; }
	public LocalizationSectionNode Root { get; set; }
	public Dictionary<string, string> ExternalKeys { get; set; }

	public string ExportToYaml()
	{
		YamlMappingNode rootNode = new();
		{
			// authors
			YamlSequenceNode authorsSequence = new(this.Authors.Select(d => new YamlScalarNode(d) { Style = YamlDotNet.Core.ScalarStyle.DoubleQuoted }));

			rootNode.Add("authors", authorsSequence);

			// version
			YamlScalarNode versionNode = new(this.Version) { Style = YamlDotNet.Core.ScalarStyle.Plain };

			rootNode.Add("version", versionNode);

			// cultureCode
			YamlScalarNode cultureCodeNode = new(this.CultureCode) { Style = YamlDotNet.Core.ScalarStyle.DoubleQuoted };

			rootNode.Add("cultureCode", cultureCodeNode);

			// keys
			YamlMappingNode keysMapping = new();

			void addFromSection(YamlMappingNode parentMap, LocalizationSectionNode section)
			{
				foreach (var child in section.Children)
				{
					if (child is LocalizationSectionNode nextSection)
					{
						YamlMappingNode nextMapping = new();

						addFromSection(nextMapping, nextSection);

						YamlScalarNode keyNode = new(nextSection.Name) { Style = YamlDotNet.Core.ScalarStyle.Any };
						
						parentMap.Add(keyNode, nextMapping);
					}
					else if (child is LocalizationValueNode nextValue)
					{
						YamlScalarNode keyNode = new(nextValue.Name) { Style = YamlDotNet.Core.ScalarStyle.Any };
						YamlScalarNode valueNode = new(nextValue.Value) { Style = YamlDotNet.Core.ScalarStyle.DoubleQuoted };
						
						parentMap.Add(keyNode, valueNode);
					}
				}
			}

			addFromSection(keysMapping, this.Root);

			rootNode.Add("keys", keysMapping);

			// external keys
			YamlMappingNode externalKeysMapping = new();

			foreach (var kv in this.ExternalKeys)
			{
				YamlScalarNode keyNode = new(kv.Key) { Style = YamlDotNet.Core.ScalarStyle.DoubleQuoted };
				YamlScalarNode valueNode = new(kv.Value) { Style = YamlDotNet.Core.ScalarStyle.DoubleQuoted };

				externalKeysMapping.Add(keyNode, valueNode);
			}

			rootNode.Add("externalKeys", externalKeysMapping);

			// tips
			YamlSequenceNode tipsSequence = new(this.Tips.Select(d => new YamlScalarNode(d) { Style = YamlDotNet.Core.ScalarStyle.DoubleQuoted }));

			rootNode.Add("tips", tipsSequence);
		}
		YamlDocument document = new(rootNode);

		using StringWriter sb = new();
		YamlStream ys = new(document);
		ys.Save(sb, false);
		return sb.ToString();
	}

	public static bool TryDeserialize(string ymlContent, [NotNullWhen(true)] out Localization? localization)
	{
		try
		{
			using StringReader sr = new(ymlContent);

			var yaml = new YamlStream();
			yaml.Load(sr);

			var root = yaml.Documents[0].RootNode;

			localization = new()
			{
				Name = "EMPTY",
				Version = "0",
				Authors = new string[2]
				{
					"Author 1",
					"Author 2",
				},
				CultureCode = "",
				Keys = new(),
				Tips = new string[2]
				{
					"Tip 1",
					"Tip 2",
				},
				Root = new(""),
			};

			YamlScalarNode authorsNode = new("authors");
			YamlScalarNode versionNode = new("version");
			YamlScalarNode cultureCodeNode = new("cultureCode");
			YamlScalarNode tipsNode = new("tips");

			localization.Authors = ((YamlSequenceNode?)root[authorsNode])!.Children.Select(d => ((YamlScalarNode)d).Value ?? "").ToArray();
			localization.Version = ((YamlScalarNode?)root[versionNode])!.Value ?? "N/A";
			localization.CultureCode = ((YamlScalarNode?)root[cultureCodeNode])!.Value ?? "en-US";
			localization.Tips = ((YamlSequenceNode?)root[tipsNode])!.Children.Select(d => ((YamlScalarNode)d).Value ?? "").ToArray();

			YamlScalarNode keysNode = new("keys");

			var keys = (YamlMappingNode)root[keysNode];

			void addFromSequence(YamlMappingNode sequence, LocalizationSectionNode section)
			{
				foreach (var kv in sequence)
				{
					var key = ((YamlScalarNode)kv.Key).Value!;
					var value = kv.Value;

					switch (value)
					{
						case YamlMappingNode mapping:
							LocalizationSectionNode nextSection = new(key);

							section.Children.Add(nextSection);

							addFromSequence(mapping, nextSection);
							break;
						case YamlScalarNode scalar:
							section.Children.Add(new LocalizationValueNode(key, scalar.Value!));
							break;
					}
				}
			}

			addFromSequence(keys, localization.Root);

			YamlScalarNode externalKeysNode = new("externalKeys");

			var externalKeys = (YamlMappingNode)root[externalKeysNode];

			foreach (var kv in externalKeys.Children)
			{
				var key = ((YamlScalarNode)kv.Key).Value!;
				var value = kv.Value;

				if (value is not YamlScalarNode valueScalar || valueScalar.Value == null)
					continue;

				localization.ExternalKeys[key] = valueScalar.Value;
			}

			return true;
		}
		catch
		{
			localization = default;
			return false;
		}
	}
}
