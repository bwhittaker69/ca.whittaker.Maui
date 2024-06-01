using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;

using ca.whittaker.Maui.Controls.Buttons;
using System.Text.RegularExpressions;
using Entry = Microsoft.Maui.Controls.Entry;
using Label = Microsoft.Maui.Controls.Label;

namespace ca.whittaker.Maui.Controls.Forms
{
    /// <summary>
    /// Represents a customizable text box control with various properties for text manipulation and validation.
    /// </summary>
    public class CheckBoxElement : BaseFormElement
    {
        private const SizeEnum cUndoButtonSize = SizeEnum.XXSmall;
        private Microsoft.Maui.Controls.CheckBox _checkBox;
        private bool _isOriginalValueSet = false;
        private bool _originalValue = false;
        private bool _previousHasChangedState = false;
        private bool _previousInvalidDataState = false;

        public static readonly BindableProperty MandatoryProperty =
            BindableProperty.Create(nameof(Mandatory), typeof(bool), typeof(CheckBoxElement), false);

        public static readonly BindableProperty CheckBoxSourceProperty =
            BindableProperty.Create(nameof(CheckBoxSource), typeof(bool), typeof(CheckBoxElement), false, BindingMode.TwoWay, propertyChanged: OnCheckBoxSourcePropertyChanged);

        public CheckBoxElement()
        {
            InitializeUI();
        }

        public bool Mandatory
        {
            get => (bool)GetValue(MandatoryProperty);
            set => SetValue(MandatoryProperty, value);
        }

        public bool CheckBoxSource
        {
            get => (bool)GetValue(CheckBoxSourceProperty);
            set
            {
                SetValue(CheckBoxSourceProperty, value);
                SetOriginalValue(value);
            }
        }

        private static void OnCheckBoxSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CheckBoxElement element)
            {
                element._checkBox.IsChecked = (bool)newValue;
            }
        }
        public override void Unfocus()
        {
            base.Unfocus();
            _checkBox.Unfocus();
        }
        private void InitializeUI()
        {
            _checkBox = CreateCheckBox();
            FieldLabel = CreateLabel();
            FieldNotification = CreateNotificationLabel();
            ButtonUndo = CreateUndoButton();
            Content = CreateLayoutGrid();
            ButtonUndo.Pressed += (s, e) => Undo();
        }

        public void Clear()
        {
            SetOriginalValue(false);
            UpdateValidationState();
            _checkBox.Unfocus();
        }

        public void Saved()
        {
            SetOriginalValue(_checkBox.IsChecked);
            UpdateValidationState();
            _checkBox.Unfocus();
        }

        public void InitField()
        {
            SetOriginalValue(CheckBoxSource);
            UpdateValidationState();
            _checkBox.Unfocus();
        }

        public void SetOriginalValue(bool originalValue)
        {
            _originalValue = originalValue;
            _checkBox.IsChecked = originalValue;
        }

        public void Undo()
        {
            _checkBox.IsChecked = _originalValue;
            UpdateValidationState();
            _checkBox.Unfocus();
        }

        private Microsoft.Maui.Controls.CheckBox CreateCheckBox()
        {
            var box = new Microsoft.Maui.Controls.CheckBox
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };
            box.CheckedChanged += CheckBox_ValueChanged;
            return box;
        }

        private Grid CreateLayoutGrid()
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(LabelWidth, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = new GridLength(DeviceHelper.GetImageSizeForDevice(cUndoButtonSize) * 2, GridUnitType.Absolute) },
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            grid.Add(FieldLabel, 0, 0);
            grid.Add(_checkBox, 1, 0);
            grid.Add(ButtonUndo, 2, 0);
            grid.Add(FieldNotification, 0, 1);
            Grid.SetColumnSpan(FieldNotification, 3);

            return grid;
        }

        private void CheckBox_ValueChanged(object? sender, CheckedChangedEventArgs e)
        {
            ProcessAndSetValue(e.Value);
            UpdateValidationState();
        }

        private void EvaluateToRaiseHasChangesEvent()
        {
            bool hasChanged = _originalValue != _checkBox.IsChecked;
            if (_previousHasChangedState != hasChanged)
            {
                UpdateUI(hasChanged);
            }
        }

        private void UpdateUI(bool hasChanged)
        {
            // update the binded data source
            CheckBoxSource = _checkBox.IsChecked;

            using (var resourceHelper = new ResourceHelper())
            {
                ButtonUndo.ImageSource = resourceHelper.GetImageSource(hasChanged ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled, ImageResourceEnum.Undo, cUndoButtonSize);
            }
            _previousHasChangedState = hasChanged;
            ChangeState = hasChanged ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
            RaiseHasChanges(hasChanged);
        }

        private void ProcessAndSetValue(bool value)
        {
            _checkBox.IsChecked = value;
        }

        private void UpdateValidationState()
        {
            EvaluateToRaiseHasChangesEvent();
        }
    }
}
