﻿using MBOptionScreen.Attributes;
using MBOptionScreen.Settings;

namespace MBOptionScreen
{
    internal class TestSettings : AttributeSettings<TestSettings>
    {
        public const string SettingsInstanceID = "Testing";

        public override string Id { get; set; } = SettingsInstanceID;
        public override string ModName => "Testing";
        public override string ModuleFolderName => "Testing";


        [SettingProperty("Enable Crash Error Reporting", false, "When enabled, shows a message box showing the cause of a crash.")]
        [SettingPropertyGroup("Debugging")]
        public bool DebugMode { get; set; } = true;

        [SettingProperty("Test Property 1", false, "")]
        [SettingPropertyGroup("Debugging/Test Group", true)]
        public bool TestProperty1 { get; set; } = false;

        [SettingProperty("Test Property 5", false, "")]
        [SettingPropertyGroup("Debugging/Test Group")]
        public bool TestProperty5 { get; set; } = false;

        [SettingProperty("Test Property 2", false, "")]
        [SettingPropertyGroup("Debugging/Test Group/Test Group 2", true)]
        public bool TestProperty2 { get; set; } = false;

        [SettingProperty("Test Property 4", 0f, 0.5f, 0f, 100f, false, "")]
        [SettingPropertyGroup("Debugging/Test Group/Test Group 2")]
        public float TestProperty4 { get; set; } = 0.2f;

        [SettingProperty("Test Property 3", 0, 10, true, "")]
        [SettingPropertyGroup("Debugging/Test Group/Test Group 3")]
        public int TestProperty3 { get; set; } = 2;

        [SettingProperty("Test Property 6", true, "")]
        [SettingPropertyGroup("Debugging/Test Group/Test Group 3")]
        public string TestProperty6 { get; set; } = "";
    }
}
