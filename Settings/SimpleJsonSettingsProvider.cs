﻿using MBOptionScreen.Attributes;
using MBOptionScreen.Interfaces;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using TaleWorlds.Engine;

using Path = System.IO.Path;

namespace MBOptionScreen.Settings
{
    [SettingsStorageVersion("e1.0.0",  2)]
    [SettingsStorageVersion("e1.0.1",  2)]
    [SettingsStorageVersion("e1.0.2",  2)]
    [SettingsStorageVersion("e1.0.3",  2)]
    [SettingsStorageVersion("e1.0.4",  2)]
    [SettingsStorageVersion("e1.0.5",  2)]
    [SettingsStorageVersion("e1.0.6",  2)]
    [SettingsStorageVersion("e1.0.7",  2)]
    [SettingsStorageVersion("e1.0.8",  2)]
    [SettingsStorageVersion("e1.0.9",  2)]
    [SettingsStorageVersion("e1.0.10", 2)]
    [SettingsStorageVersion("e1.0.11", 2)]
    [SettingsStorageVersion("e1.1.0",  2)]
    internal class SimpleJsonSettingsProvider : ISettingsProvider
    {
        private readonly string _defaultRootFolder = Path.Combine(Utilities.GetConfigsPath(), "ModSettings");
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new IgnorePropertiesResolver(new string[]
            {
                "ID", "Id",
                "ModuleFolderName",
                "ModName",
                "SubFolder",
                "SubGroupDelimiter",
            })

        };
        private Dictionary<string, SettingsBase> LoadedSettings { get; } = new Dictionary<string, SettingsBase>();

        public List<ModSettingsDefinition> CreateModSettingsDefinitions => LoadedSettings.Keys
            .Select(id => new ModSettingsDefinition(id))
            .OrderByDescending(a => a.ModName)
            .ToList();

        public bool RegisterSettings(SettingsBase settingsInstance)
        {
            if (settingsInstance == null || LoadedSettings.ContainsKey(settingsInstance.Id))
                return false;

            LoadedSettings.Add(settingsInstance.Id, settingsInstance);

            var path = Path.Combine(_defaultRootFolder, settingsInstance.ModuleFolderName, settingsInstance.SubFolder ?? "", $"{settingsInstance.Id}.json");
            var file = new FileInfo(path);
            if (file.Exists)
            {
                using var reader = file.OpenText();
                var content = reader.ReadToEnd();
                JsonConvert.PopulateObject(content, settingsInstance, _jsonSerializerSettings);
            }
            else
            {
                var content = JsonConvert.SerializeObject(settingsInstance, _jsonSerializerSettings);
                file.Directory?.Create();
                using var writer = file.CreateText();
                writer.Write(content);
            }

            return true;
        }

        public SettingsBase? GetSettings(string id) => LoadedSettings.TryGetValue(id, out var result)
            ? result
            : null;

        public void SaveSettings(SettingsBase settingsInstance)
        {
            if (settingsInstance == null || !LoadedSettings.ContainsKey(settingsInstance.Id))
                return;

            var path = Path.Combine(_defaultRootFolder, settingsInstance.ModuleFolderName, settingsInstance.SubFolder ?? "", $"{settingsInstance.Id}.json");
            var file = new FileInfo(path);

            var content = JsonConvert.SerializeObject(settingsInstance, _jsonSerializerSettings);
            file.Directory?.Create();
            using var writer = file.CreateText();
            writer.Write(content);
        }

        public bool OverrideSettings(SettingsBase newSettingsInstance)
        {
            if (newSettingsInstance == null || !LoadedSettings.ContainsKey(newSettingsInstance.Id))
                return false;

            LoadedSettings[newSettingsInstance.Id] = newSettingsInstance;
            SaveSettings(newSettingsInstance);
            return true;
        }

        public SettingsBase ResetSettings(string id)
        {
            if (!LoadedSettings.ContainsKey(id))
                return null;

            var defaultSettingsInstance = (SettingsBase) Activator.CreateInstance(LoadedSettings[id].GetType());
            LoadedSettings[id] = defaultSettingsInstance;
            SaveSettings(defaultSettingsInstance);
            return defaultSettingsInstance;
        }

        public class IgnorePropertiesResolver : DefaultContractResolver
        {
            private readonly IEnumerable<string> _propsToIgnore;
            public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
            {
                _propsToIgnore = propNamesToIgnore;
            }
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                property.ShouldSerialize = x => !_propsToIgnore.Contains(property.PropertyName);
                return property;
            }
        }
    }
}