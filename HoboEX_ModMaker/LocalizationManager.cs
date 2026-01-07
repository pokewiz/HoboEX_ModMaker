using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace HoboEX_ModMaker
{
    public class AppSettings
    {
        public string Language { get; set; } = "zh-CN";
        public string AiApiBase { get; set; } = "https://api.siliconflow.cn/v1";
        public string AiApiKey { get; set; } = "";
        public string AiModel { get; set; } = "deepseek-ai/DeepSeek-V3";
        public List<string> AiTargetLanguages { get; set; } = new List<string> { "zh", "en", "cs", "es", "ja", "fr", "ru", "pl", "de" };
        public List<string> FavoriteColors { get; set; } = new List<string> { "#BB786B", "#FF0000", "#00FF00", "#0000FF" };
    }

    public static class LocalizationManager
    {
        private static Dictionary<string, string> _currentStrings = new();
        public static AppSettings Settings { get; private set; } = new();
        public static string CurrentLanguage { get; private set; } = "zh-CN";

        private static string SettingsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        Settings = settings;
                        SetLanguage(settings.Language);
                    }
                }
                else
                {
                    SetLanguage("zh-CN");
                }
            }
            catch { SetLanguage("zh-CN"); }
        }

        public static void SetLanguage(string langCode)
        {
            CurrentLanguage = langCode;
            string fileName = $"lang_{langCode}.json";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            
            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null)
                    {
                        _currentStrings = dict;
                    }
                }
                catch { }
            }
            else
            {
                // Fallback or empty if not found
                if (_currentStrings == null || _currentStrings.Count == 0)
                {
                    // Minimal fallback
                    _currentStrings = new Dictionary<string, string>();
                }
            }
            SaveSettings();
        }

        public static string Get(string key)
        {
            return _currentStrings.TryGetValue(key, out var val) ? val : key;
        }

        public static void SaveSettings()
        {
            try
            {
                Settings.Language = CurrentLanguage;
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(Settings, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }
        
        /// <summary>
        /// Looks up localized strings for property names and descriptions.
        /// Key format: Prop_{ClassName}_{PropName} or Desc_{ClassName}_{PropName}
        /// Context format: Prop_{ClassName}_{Context}_{PropName} or Desc_{ClassName}_{Context}_{PropName}
        /// Fallback to Prop_Common_{PropName} or Desc_Common_{PropName}
        /// Trie: exact match, PascalCase, ID shortcut.
        /// </summary>
        public static (string displayName, string description) GetPropertyInfo(string className, string propName, string typeContext = null)
        {
            // Generate candidates: exact, PascalCase, ID-special
            var candidates = new List<string> { propName };
            if (propName.Length > 0 && char.IsLower(propName[0]))
            {
                candidates.Add(char.ToUpper(propName[0]) + propName.Substring(1));
            }
            if (string.Equals(propName, "id", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add("ID");
            }

            foreach (var suffix in candidates)
            {
                string name = null;
                string desc = null;

                // 1. Try Context Specific Class (e.g. Desc_Action_Pay_Value)
                if (!string.IsNullOrEmpty(typeContext))
                {
                    string contextNameKey = $"Prop_{className}_{typeContext}_{suffix}";
                    string contextDescKey = $"Desc_{className}_{typeContext}_{suffix}";
                    
                    if (_currentStrings.TryGetValue(contextNameKey, out var cn)) name = cn;
                    if (_currentStrings.TryGetValue(contextDescKey, out var cd)) desc = cd;
                }

                // 2. Try Specific Class (e.g. Desc_Action_Value)
                if (name == null)
                {
                    string dKey = $"Prop_{className}_{suffix}";
                    if (_currentStrings.TryGetValue(dKey, out var n)) name = n;
                }
                if (desc == null)
                {
                    string descKey = $"Desc_{className}_{suffix}";
                    if (_currentStrings.TryGetValue(descKey, out var d)) desc = d;
                }

                // 3. Try Common Fallback
                if (name == null) _currentStrings.TryGetValue($"Prop_Common_{suffix}", out name);
                if (desc == null) _currentStrings.TryGetValue($"Desc_Common_{suffix}", out desc);

                if (name != null || desc != null)
                {
                    return (name ?? propName, desc ?? "");
                }
            }
            
            return (propName, "");
        }
    }
}

