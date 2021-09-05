namespace Hagi.HagiGuest.Install
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Win32;

    /// <summary>
    /// Deals with the installation and restoration of registry values.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class RegistryHandler
    {
        private string storageFile = Config.GetConfigFilePath("registry-installer.json");

        public const string ConfigPath = @"SOFTWARE\hagi-guest";
        public static readonly string ConfigFullPath = $@"HKEY_CURRENT_USER\{RegistryHandler.ConfigPath}";

        public static void SetConfig(string name, string value)
        {
            Registry.SetValue(RegistryHandler.ConfigFullPath, name, value, RegistryValueKind.String);
        }

        public static void SetConfig(string name, int value)
        {
            Registry.SetValue(RegistryHandler.ConfigFullPath, name, value, RegistryValueKind.DWord);
        }

        public static void SetConfig(string name, bool value)
        {
            Registry.SetValue(RegistryHandler.ConfigFullPath, name, value ? "1" : "0", RegistryValueKind.String);
        }

        public static string? GetConfig(string name)
        {
            return Registry.GetValue(RegistryHandler.ConfigFullPath, name, null)?.ToString();
        }

        public static string GetConfig(string name, string defaultValue)
        {
            return Registry.GetValue(RegistryHandler.ConfigFullPath, name, defaultValue)?.ToString() ?? defaultValue;
        }

        public static bool GetConfig(string name, bool defaultValue)
        {
            return RegistryHandler.GetConfig(name, defaultValue ? 1 : 0) != 0;
        }

        public static int GetConfig(string name, int defaultValue)
        {
            object? obj = Registry.GetValue(RegistryHandler.ConfigFullPath, name, defaultValue);

            try
            {
                return obj is int value
                    ? value
                    : Convert.ToInt32(obj);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static void DeleteKeyRecursive(string path)
        {
            RegistryHandler.DeleteKeyRecursive(Registry.CurrentUser, path);
        }
        public static void DeleteKeyRecursive(RegistryKey key, string path)
        {
            if (!path.Contains("hagi", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"trying to delete {key.Name}\\{path}");
            }
            key.DeleteSubKeyTree(path, false);
        }

        /// <summary>Applies registry items, storing the original value.</summary>
        public void ApplyRegistryValues<TGroup>(IEnumerable<RegistryItem> registryItems)
        {
            StoredItems storedItems = StoredItems.Load<TGroup>(this.storageFile);

            foreach (RegistryItem registryItem in registryItems)
            {
                RegistryItem storedValue = registryItem.Apply();

                // Don't save the current value if it's already been taken.
                if (!storedItems.Contains(registryItem))
                {
                    storedItems.Add(storedValue);
                }
            }

            storedItems.Save();
        }

        /// <summary>Restores the registry items to their original value.</summary>
        public void RestoreRegistryValues<TGroup>()
        {
            StoredItems storedItems = StoredItems.Load<TGroup>(this.storageFile);

            List<RegistryItem> paths = new List<RegistryItem>();

            foreach (RegistryItem item in storedItems.Items)
            {
                item.Restore();
            }

            // Delete any keys which were created.
            foreach (RegistryItem registryItem in storedItems.Items.Where(item => item.NoPath))
            {
                try
                {
                    RegistryKey? key = registryItem.Root.OpenSubKey(registryItem.Path);
                    if (registryItem.Path.Contains("hagi", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Owned by this application - delete it all
                        registryItem.Root.DeleteSubKeyTree(registryItem.Path, false);
                    }
                    else if (key != null)
                    {
                        // Only delete if it's empty.
                        if (key.GetValueNames().Length == 0)
                        {
                            registryItem.Root.DeleteSubKey(registryItem.Path, false);
                        }
                    }
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            storedItems.Items.Clear();
            storedItems.Save();
        }

        /// <summary>The original values of registry items.</summary>
        private class StoredItems
        {
            [JsonIgnore]
            private string _storageFile = null!;

            [JsonIgnore]
            private string _group = string.Empty;

            [JsonPropertyName("Items")]
            public Dictionary<string, List<RegistryItem>> AllItems { get; set; } = new Dictionary<string, List<RegistryItem>>();

            [JsonIgnore]
            public List<RegistryItem> Items { get; set; } = null!;

            /// <summary>Load some stored values</summary>
            public static StoredItems Load<TGroup>(string storageFile)
            {
                string? content;
                try
                {
                    content = File.ReadAllText(storageFile);
                }
                catch (IOException)
                {
                    content = "{}";
                }

                StoredItems storedItems = JsonSerializer.Deserialize<StoredItems>(content) ??
                                            throw new ApplicationException("aa");
                storedItems._storageFile = storageFile;
                storedItems._group = typeof(TGroup).Name;

                if (storedItems.AllItems.TryGetValue(storedItems._group, out List<RegistryItem>? items))
                {
                    storedItems.Items = items;
                }
                else
                {
                    storedItems.Items = new List<RegistryItem>();
                    storedItems.AllItems.Add(storedItems._group, storedItems.Items);
                }

                return storedItems;
            }

            /// <summary>Write the values to disk.</summary>
            public void Save()
            {
                string content = JsonSerializer.Serialize(this);

                string? directoryName = Path.GetDirectoryName(this._storageFile);
                if (directoryName != null)
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllText(this._storageFile, content);
            }


            public void Add(RegistryItem value)
            {
                this.Items.Add(value);
            }

            public bool Contains(RegistryItem registryItem)
            {
                string id = registryItem.Id;
                return this.Items.Any(i => i.Id == id);
            }

            public bool Get(RegistryItem registryItem, out RegistryItem registryItem1)
            {
                throw new NotImplementedException();
            }
        }

        public class RegistryItem
        {
            private static readonly Dictionary<string, RegistryKey> Roots = new Dictionary<string, RegistryKey>()
            {
                { "HKCU", Registry.CurrentUser },
                { "HKLM", Registry.LocalMachine }
            };

            [JsonIgnore]
            public RegistryKey Root { get; private set; }

            [JsonPropertyName("root")]
            public string RootName
            {
                get => RegistryItem.Roots.First(p => p.Value == this.Root).Key;
                set => this.Root = RegistryItem.Roots[value];
            }

            [JsonPropertyName("path")]
            public string Path { get; init; }

            [JsonPropertyName("value")]
            public string ValueName { get; init; }

            [JsonPropertyName("data")]
            public string? Data { get; set; }

            [JsonPropertyName("kind")]
            public RegistryValueKind ValueKind { get; set; }

            [JsonPropertyName("noPath")]
            public bool NoPath { get; set; }

            [JsonIgnore]
            public string Id
            {
                get { return $"{this.Root.Name}:{this.Path}@{this.ValueName}"; }
            }

            /// <summary>
            /// A registry item in HKEY_CURRENT_USER
            /// </summary>
            public RegistryItem(string path, string valueName, string data, RegistryValueKind valueKind)
                : this(Registry.CurrentUser, path, valueName, data, valueKind)
            {
            }

            /// <summary>
            /// A registry item in HKEY_CURRENT_USER
            /// </summary>
            public RegistryItem(string path, string valueName, string data)
                : this(Registry.CurrentUser, path, valueName, data, RegistryValueKind.String)
            {
            }

            public RegistryItem(RegistryKey root, string path, string valueName, string data,
                RegistryValueKind valueKind)
            {
                this.Root = root;
                this.Path = path;
                this.ValueName = valueName;
                this.Data = data;
                this.ValueKind = valueKind;
            }

            [JsonConstructor]
            public RegistryItem()
            {
                this.Root = null!;
                this.Path = null!;
                this.ValueName = null!;
            }

            private RegistryItem Copy()
            {
                return new RegistryItem(this.Root, this.Path, this.ValueName, string.Empty, this.ValueKind);
            }

            private RegistryItem GetCurrentValue()
            {
                RegistryItem currentValue = this.Copy();

                RegistryKey? key = this.Root.OpenSubKey(this.Path, false);
                if (key != null)
                {
                    (string? dataString, RegistryValueKind kind) = RegistryItem.GetValue(key, this.ValueName);
                    currentValue.Data = dataString;
                    currentValue.ValueKind = kind;
                }
                else
                {
                    currentValue.Data = null;
                    currentValue.NoPath = true;
                }

                return currentValue;
            }

            private static void SetValue(RegistryKey key, string valueName, string? dataString, RegistryValueKind kind)
            {
                if (dataString == null)
                {
                    try
                    {
                        key.DeleteValue(valueName);
                    }
                    catch (ArgumentException)
                    {
                        // Probably failed because it doesn't exist.
                    }
                }
                else
                {
                    object? data;
                    switch (kind)
                    {
                        case RegistryValueKind.String:
                        case RegistryValueKind.ExpandString:
                        case RegistryValueKind.MultiString:
                            data = dataString;
                            break;
                        case RegistryValueKind.Binary:
                            data = Convert.FromBase64String(dataString);
                            break;
                        case RegistryValueKind.DWord:
                            data = Convert.ToInt32(dataString);
                            break;
                        case RegistryValueKind.QWord:
                            data = Convert.ToInt64(dataString);
                            break;
                        default:
                            data = null;
                            break;
                    }

                    if (data != null)
                    {
                        key.SetValue(valueName, data, kind);
                    }
                }

            }

            private static (string? dataString, RegistryValueKind kind) GetValue(RegistryKey key, string valueName)
            {
                object? data = key.GetValue(valueName, null);
                string? dataString;
                RegistryValueKind kind;

                if (data == null)
                {
                    dataString = null;
                    kind = RegistryValueKind.None;
                }
                else
                {
                    kind = key.GetValueKind(valueName);

                    switch (kind)
                    {
                        case RegistryValueKind.String:
                        case RegistryValueKind.ExpandString:
                        case RegistryValueKind.MultiString:
                            dataString = data as string ?? data.ToString() ?? string.Empty;
                            break;
                        case RegistryValueKind.Binary:
                            dataString = Convert.ToBase64String(data as byte[] ?? Array.Empty<byte>());
                            break;
                        case RegistryValueKind.DWord:
                        case RegistryValueKind.QWord:
                            dataString = data.ToString() ?? "0";
                            break;
                        default:
                            dataString = null;
                            break;
                    }
                }

                return (dataString, kind);
            }

            private void Set()
            {
                RegistryItem.SetValue(this.Root.CreateSubKey(this.Path, true), this.ValueName, this.Data,
                    this.ValueKind);
            }

            public RegistryItem Apply()
            {
                RegistryItem current = this.GetCurrentValue();
                this.Set();
                return current;
            }

            public void Restore()
            {
                this.Set();
            }
        }

    }
}