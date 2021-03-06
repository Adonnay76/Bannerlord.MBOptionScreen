﻿using MBOptionScreen.Attributes;
using MBOptionScreen.Settings;

using System;
using System.Linq;

using TaleWorlds.Library;

namespace MBOptionScreen.GUI.v1a.ViewModels
{
    public class SettingPropertyGroup : ViewModel, IComparable<SettingPropertyGroup>
    {
        private bool _isExpanded = true;
        protected ModSettingsScreenVM MainView => ModSettingsView.MainView;
        protected ModSettingsVM ModSettingsView { get; }
        public UndoRedoStack URS => ModSettingsView.URS;

        public SettingPropertyGroupDefinition SettingPropertyGroupDefinition { get; }
        public string GroupName => SettingPropertyGroupDefinition.GroupName;
        public SettingPropertyGroupAttribute Attribute
        {
            get => SettingPropertyGroupDefinition.Attribute;
            set => SettingPropertyGroupDefinition.Attribute = value;
        }
        public SettingPropertyGroup ParentGroup { get; set; } = null;
        public SettingProperty GroupToggleSettingProperty { get; private set; } = null;
        public string HintText
        {
            get
            {
                if (GroupToggleSettingProperty != null && !string.IsNullOrWhiteSpace(GroupToggleSettingProperty.HintText))
                {
                    return $"{GroupToggleSettingProperty.HintText}";
                }
                return "";
            }
        }
        public bool SatisfiesSearch
        {
            get
            {
                if (MainView == null || string.IsNullOrEmpty(MainView.SearchText))
                    return true;
                return GroupName.IndexOf(MainView.SearchText, StringComparison.OrdinalIgnoreCase) >= 0 || AnyChildSettingSatisfiesSearch;
            }
        }
        public bool AnyChildSettingSatisfiesSearch
        {
            get
            {
                return SettingProperties.Any(x => x.SatisfiesSearch) || SettingPropertyGroups.Any(x => x.SatisfiesSearch);
            }
        }

        [DataSourceProperty]
        public string GroupNameDisplay
        {
            get
            {
                var addition = GroupToggle ? "" : "(Disabled)";
                return $"{GroupName} {addition}";
            }
        }
        [DataSourceProperty]
        public MBBindingList<SettingProperty> SettingProperties { get; } = new MBBindingList<SettingProperty>();
        [DataSourceProperty]
        public MBBindingList<SettingPropertyGroup> SettingPropertyGroups { get; } = new MBBindingList<SettingPropertyGroup>();
        [DataSourceProperty]
        public bool GroupToggle
        {
            get
            {
                if (GroupToggleSettingProperty != null)
                    return GroupToggleSettingProperty.BoolValue;
                else
                    return true;
            }
            set
            {
                if (GroupToggleSettingProperty != null && GroupToggleSettingProperty.BoolValue != value)
                {
                    GroupToggleSettingProperty.BoolValue = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsExpanded));
                    OnGroupClick();
                    OnGroupClick();
                    OnPropertyChanged(nameof(GroupNameDisplay));
                    foreach (var propSetting in SettingProperties)
                    {
                        propSetting.OnPropertyChanged(nameof(SettingProperty.IsEnabled));
                        propSetting.OnPropertyChanged(nameof(SettingProperty.IsSettingVisible));
                    }
                    foreach (var subGroup in SettingPropertyGroups)
                    {
                        subGroup.OnPropertyChanged(nameof(SettingPropertyGroup.IsGroupVisible));
                        subGroup.OnPropertyChanged(nameof(SettingPropertyGroup.IsExpanded));
                    }
                }
            }
        }
        [DataSourceProperty]
        public bool IsGroupVisible
        {
            get
            {
                if (!SatisfiesSearch && !AnyChildSettingSatisfiesSearch)
                    return false;
                else if (ParentGroup != null)
                    return ParentGroup.IsExpanded && ParentGroup.GroupToggle;
                else
                    return true;
            }
        }
        [DataSourceProperty]
        public bool HasSettingsProperties => SettingProperties.Count > 0;
        [DataSourceProperty]
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                    OnPropertyChanged(nameof(IsGroupVisible));
                    foreach (var subGroup in SettingPropertyGroups)
                    {
                        subGroup.OnPropertyChanged(nameof(SettingPropertyGroup.IsGroupVisible));
                        subGroup.OnPropertyChanged(nameof(SettingPropertyGroup.IsExpanded));
                    }
                    foreach (var settingProp in SettingProperties)
                        settingProp.OnPropertyChanged(nameof(SettingProperty.IsSettingVisible));
                }
            }
        }
        [DataSourceProperty]
        public bool HasGroupToggle => GroupToggleSettingProperty != null;

        public SettingPropertyGroup(SettingPropertyGroupDefinition definition, ModSettingsVM modSettingsView)
        {
            ModSettingsView = modSettingsView;
            SettingPropertyGroupDefinition = definition;
            foreach (var settingPropertyDefinition in SettingPropertyGroupDefinition.SettingProperties)
                Add(settingPropertyDefinition);
            foreach (var settingPropertyDefinition in SettingPropertyGroupDefinition.SubGroups)
                SettingPropertyGroups.Add(new SettingPropertyGroup(settingPropertyDefinition, ModSettingsView));

            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            foreach (var setting in SettingProperties)
                setting.RefreshValues();
            foreach (var setting in SettingPropertyGroups)
                setting.RefreshValues();

            OnPropertyChanged(nameof(GroupNameDisplay));
        }

        private void Add(SettingPropertyDefinition definition)
        {
            var sp = new SettingProperty(definition, ModSettingsView);
            SettingProperties.Add(sp);
            sp.Group = this;

            if (sp.GroupAttribute.IsMainToggle)
            {
                if (HasGroupToggle)
                    throw new Exception($"Tried to add a group toggle to Setting Property Group {GroupName} but it already has a group toggle: {GroupToggleSettingProperty.Name}. A Setting Property Group can only have one group toggle.");

                Attribute = sp.GroupAttribute;
                GroupToggleSettingProperty = sp;
            }
        }

        public void NotifySearchChanged()
        {
            if (SettingPropertyGroups.Count > 0)
            {
                foreach (var group in SettingPropertyGroups)
                    group.NotifySearchChanged();
            }
            if (SettingProperties.Count > 0)
            {
                foreach (var prop in SettingProperties)
                    prop.OnPropertyChanged(nameof(SettingProperty.IsSettingVisible));
            }
            OnPropertyChanged(nameof(IsGroupVisible));
        }

        private void OnHover()
        {
            if (MainView != null && !string.IsNullOrWhiteSpace(HintText))
                MainView.HintText = HintText;
        }

        private void OnHoverEnd()
        {
            if (MainView != null)
                MainView.HintText = "";
        }

        private void OnGroupClick()
        {
            IsExpanded = !IsExpanded;
        }

        public override string ToString() => GroupName;
        public override int GetHashCode() => GroupName.GetHashCode();

        public int CompareTo(SettingPropertyGroup other) => string.Compare(other.GroupName, GroupName, StringComparison.InvariantCulture);
    }
}
