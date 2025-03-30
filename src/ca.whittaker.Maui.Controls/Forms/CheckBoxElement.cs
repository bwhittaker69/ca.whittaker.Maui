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
    /// Represents a customizable checkbox control that combines several UI elements:
    /// 
    /// - A Grid layout that organizes:
    /// 
    ///   • A FieldLabel (Label) for displaying the field's title.
    ///   • A CheckBox control for boolean input.
    ///   • A ButtonUndo for reverting changes.
    ///   • A FieldNotification label for showing validation or error messages.
    ///   
    /// Grid Layout Overview:
    /// +-------------------+-------------------+----------------------------------+
    /// | FieldLabel        | _checkBox         | ButtonUndo                       |
    /// +-------------------+-------------------+----------------------------------+
    /// | FieldNotification (spans all three columns)                              |
    /// +--------------------------------------------------------------------------+
    /// 
    /// This composite control supports boolean value capture, change tracking, and validation.
    /// 
    /// <para>
    /// Differences:
    /// - Uses a CheckBox control instead of an Entry for data input.
    /// - Exposes its value via a dedicated two-way bindable property named <c>CheckBoxSource</c>.
    /// - Contains logic to track changes against an original value and update an undo button accordingly.
    /// </para>
    /// </summary>
    public class CheckBoxElement : BaseFormElement
    {
        private const SizeEnum cUndoButtonSize = SizeEnum.XXSmall;
        private Microsoft.Maui.Controls.CheckBox? _checkBox;
        //private bool _isOriginalValueSet = false;
        private bool _originalValue = false;
        private bool _previousHasChangedState = false;
        //private bool _previousInvalidDataState = false;

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
                if (element._checkBox != null)
                    element._checkBox.IsChecked = (bool)newValue;
            }
        }
        public override void Unfocus()
        {
            base.Unfocus();
            _checkBox?.Unfocus();
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
            _checkBox?.Unfocus();
        }

        public void Saved()
        {
            if (_checkBox != null)
            {
                SetOriginalValue(_checkBox.IsChecked);
                UpdateValidationState();
                _checkBox.Unfocus();
            }
        }

        public void InitField()
        {
            SetOriginalValue(CheckBoxSource);
            UpdateValidationState();
            if (_checkBox != null)
            {
                _checkBox.Unfocus();
            }
        }

        public void SetOriginalValue(bool originalValue)
        {
            _originalValue = originalValue;
            if (_checkBox != null)
                _checkBox.IsChecked = originalValue;
        }

        public void Undo()
        {
            if (_checkBox != null)
                _checkBox.IsChecked = _originalValue;
            UpdateValidationState();
            if (_checkBox != null)
                _checkBox.Unfocus();
        }

        private bool _disable = false;
        public void Disable()
        {
            if (_disable || _checkBox != null) return;
            _disable = true;
            _checkBox!.IsChecked = _originalValue;
            ButtonUndo?.Hide();
            _checkBox!.IsEnabled = false;
            _checkBox!.Unfocus();
            _disable = false;
        }
        private bool _enable = false;
        public void Enable()
        {
            if (_enable || _checkBox != null) return;
            _enable = true;
            _checkBox!.IsChecked = _originalValue;
            ButtonUndo?.Disabled();
            _checkBox!.IsEnabled = false;
            _checkBox!.Unfocus();
            _enable = false;
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
            bool hasChanged = _originalValue != _checkBox?.IsChecked;
            if (_previousHasChangedState != hasChanged)
                UpdateUI(hasChanged);
        }

        private void UpdateUI(bool hasChanged)
        {
            // update the binded data source
            CheckBoxSource = _checkBox?.IsChecked == true;

            using (var resourceHelper = new ResourceHelper())
            {
                if (ButtonUndo != null)
                    ButtonUndo.ImageSource = resourceHelper.GetImageSource(hasChanged ? ButtonStateEnum.Enabled : ButtonStateEnum.Disabled, ImageResourceEnum.Undo, cUndoButtonSize);
            }
            _previousHasChangedState = hasChanged;
            ChangeState = hasChanged ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
            RaiseHasChanges(hasChanged);
        }

        private void ProcessAndSetValue(bool value)
        {
            if (_checkBox != null)
                _checkBox.IsChecked = value;
        }

        private void UpdateValidationState()
        {
            EvaluateToRaiseHasChangesEvent();
        }
    }
}
