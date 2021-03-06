﻿using MBOptionScreen.GUI.v1a.ViewModels;
using MBOptionScreen.Interfaces;

namespace MBOptionScreen.Actions
{
    public class SetIntSettingProperty : IAction
    {
        private readonly int _originalValue;

        public Ref Context { get; } = null;
        public object Value { get; }
        private SettingProperty SettingProperty { get; }


        public SetIntSettingProperty(SettingProperty settingProperty, int value)
        {
            Value = value;
            SettingProperty = settingProperty;
            _originalValue = SettingProperty.IntValue;
        }

        public void DoAction() => SettingProperty.IntValue = (int)Value;
        public void UndoAction() => SettingProperty.IntValue = _originalValue;
    }
}
