using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace HoboEX_ModMaker
{
    public class AppSettings
    {
        public string Language { get; set; } = "zh-CN";
    }

    public static class LocalizationManager
    {
        private static Dictionary<string, string> _currentStrings = new();
        public static string CurrentLanguage { get; private set; } = "zh-CN";

        private static readonly Dictionary<string, Dictionary<string, string>> _languages = new()
        {
            ["zh-CN"] = new()
            {
                ["MenuFile"] = "文件",
                ["MenuNew"] = "新建",
                ["MenuOpen"] = "打开",
                ["MenuSave"] = "保存",
                ["MenuSaveAs"] = "另存为",
                ["MenuLanguage"] = "语言",
                ["RootNode"] = "对话根节点",
                ["Ready"] = "就绪",
                ["NewFileCreated"] = "已创建新对话文件",
                ["FileOpened"] = "已打开文件: {0}",
                ["FileSaved"] = "已保存文件: {0}",
                ["ContextAddNPC"] = "添加 NPC 对话",
                ["ContextAddOption"] = "添加选项",
                ["ContextAddReaction"] = "添加 NPC 回复",
                ["ContextAddAction"] = "添加动作",
                ["ContextAddCondition"] = "添加条件",
                ["ContextDelete"] = "删除",
                ["NodeActions"] = "执行动作",
                ["NodeConditions"] = "触发条件",
                ["NodeL10n"] = "本地化文本",
                ["ContextAddL10n"] = "添加语言文本",
                ["Error"] = "错误",
                ["LoadError"] = "加载文件失败: {0}",
                ["SaveError"] = "保存文件失败: {0}",
                ["CategoryBasic"] = "基础",
                ["CategoryIdentification"] = "身份识别",
                ["CategoryDisplay"] = "显示",
                ["CategoryBehavior"] = "行为"
            },
            ["en-US"] = new()
            {
                ["MenuFile"] = "File",
                ["MenuNew"] = "New",
                ["MenuOpen"] = "Open",
                ["MenuSave"] = "Save",
                ["MenuSaveAs"] = "Save As",
                ["MenuLanguage"] = "Language",
                ["RootNode"] = "Dialogues Root",
                ["Ready"] = "Ready",
                ["NewFileCreated"] = "New Dialogue File Created",
                ["FileOpened"] = "Opened: {0}",
                ["FileSaved"] = "Saved: {0}",
                ["ContextAddNPC"] = "Add NPC Dialogue",
                ["ContextAddOption"] = "Add Option",
                ["ContextAddReaction"] = "Add Reaction",
                ["ContextAddAction"] = "Add Action",
                ["ContextAddCondition"] = "Add Condition",
                ["ContextDelete"] = "Delete",
                ["NodeActions"] = "Actions",
                ["NodeConditions"] = "Conditions",
                ["NodeL10n"] = "Localization",
                ["ContextAddL10n"] = "Add Language Text",
                ["Error"] = "Error",
                ["LoadError"] = "Failed to load: {0}",
                ["SaveError"] = "Failed to save: {0}",
                ["CategoryBasic"] = "Basic",
                ["CategoryIdentification"] = "Identification",
                ["CategoryDisplay"] = "Display",
                ["CategoryBehavior"] = "Behavior"
            }
        };

        public static void SetLanguage(string langCode)
        {
            if (_languages.ContainsKey(langCode))
            {
                CurrentLanguage = langCode;
                _currentStrings = _languages[langCode];
                SaveSettings();
            }
        }

        public static string Get(string key)
        {
            return _currentStrings.TryGetValue(key, out var val) ? val : key;
        }

        private static string SettingsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null) SetLanguage(settings.Language);
                }
                else
                {
                    SetLanguage("zh-CN");
                }
            }
            catch { SetLanguage("zh-CN"); }
        }

        private static void SaveSettings()
        {
            try
            {
                var settings = new AppSettings { Language = CurrentLanguage };
                var json = JsonSerializer.Serialize(settings);
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }
    }
}
