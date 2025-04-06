using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using ca.whittaker.Maui.Controls.Buttons;
using System.Windows.Input;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ca.whittaker.Maui.Controls.Forms
{
    public interface IBaseFormField
    {
        #region Events

        event EventHandler<HasChangesEventArgs>? FieldHasChanges;

        event EventHandler<ValidationDataChangesEventArgs>? FieldHasValidationChanges;

        #endregion Events

        #region Properties

        FieldAccessModeEnum FieldAccessMode { get; set; }
        ChangeStateEnum FieldChangeState { get; set; }
        ICommand FieldCommand { get; set; }
        object FieldCommandParameter { get; set; }
        string FieldLabelText { get; set; }
        double FieldLabelWidth { get; set; }
        bool FieldMandatory { get; set; }
        ValidationStateEnum FieldValidationState { get; set; }
        double FieldWidth { get; set; }
        LayoutOptions HorizontalOptions { get; set; }
        LayoutOptions VerticalOptions { get; set; }

        #endregion Properties

        #region Public Methods

        void FieldClear();

        void FieldNotifyHasChanges(bool hasChanged);

        void FieldNotifyValidationChanges(bool isValid);

        void FieldSaveAndMarkAsReadOnly();

        void FieldUnfocus();

        void FieldUpdateLabelWidth(double newWidth);

        void FieldUpdateWidth(double newWidth);

        #endregion Public Methods
    }

    /// <summary>
    /// Represents the base class for customizable data capture controls.
    /// </summary>
    public abstract class BaseFormField : ContentView, IBaseFormField
    {
        #region Fields

        private bool _fieldEvaluateToRaiseHasChangesEventing;
        private bool _previousFieldHasInvalidData;
        private ValidationStateEnum _previousFieldValidationState;

        protected const SizeEnum DefaultButtonSize = SizeEnum.XXSmall;
        protected bool _fieldDisabling;
        protected bool _fieldEnabling;
        protected bool _fieldEvaluateToRaiseValidationChangesEventing;
        protected bool _fieldIsOriginalValueSet;
        protected bool _fieldPreviousHasChangedFromOriginal;
        protected bool _fieldUndoing;
        protected bool _fieldUpdateValidationAndChangedStating;
        protected bool _fieldUpdatingUI;
        protected bool _onFieldDataSourcePropertyChanging;

        public static readonly BindableProperty FieldAccessModeProperty = BindableProperty.Create(
            propertyName: nameof(FieldAccessMode),
            returnType: typeof(FieldAccessModeEnum),
            declaringType: typeof(BaseFormField),
            defaultValue: FieldAccessModeEnum.ViewOnly,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnFieldAccessModePropertyChanged);

        public static readonly BindableProperty FieldChangeStateProperty = BindableProperty.Create(
            propertyName: nameof(FieldChangeState),
            returnType: typeof(ChangeStateEnum),
            declaringType: typeof(BaseFormField),
            defaultValue: ChangeStateEnum.NotChanged,
            BindingMode.TwoWay);

        public static readonly BindableProperty FieldCommandParameterProperty = BindableProperty.Create(
            propertyName: nameof(FieldCommandParameter),
            returnType: typeof(object),
            declaringType: typeof(Form));

        public static readonly BindableProperty FieldCommandProperty = BindableProperty.Create(
            propertyName: nameof(FieldCommand),
            returnType: typeof(ICommand),
            declaringType: typeof(Form));

        public static readonly BindableProperty FieldEnabledProperty = BindableProperty.Create(
            propertyName: nameof(FieldEnabled),
            returnType: typeof(bool),
            declaringType: typeof(BaseFormField),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnFieldEnabledPropertyChanged);

        public static readonly BindableProperty FieldReadOnlyProperty = BindableProperty.Create(
            propertyName: nameof(FieldReadOnly),
            returnType: typeof(bool),
            declaringType: typeof(BaseFormField),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

        public static readonly BindableProperty FieldLabelTextProperty = BindableProperty.Create(
                    propertyName: nameof(FieldLabelText),
            returnType: typeof(string),
            declaringType: typeof(BaseFormField),
            defaultValue: string.Empty,
            propertyChanged: OnFieldLabelTextPropertyChanged);

        public static readonly BindableProperty FieldLabelWidthProperty = BindableProperty.Create(
            propertyName: nameof(FieldLabelWidth),
            returnType: typeof(double?),
            declaringType: typeof(BaseFormField),
            defaultValue: 100d,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnFieldLabelWidthPropertyChanged);

        public static readonly BindableProperty FieldMandatoryProperty = BindableProperty.Create(
            propertyName: nameof(FieldMandatory),
            returnType: typeof(bool),
            declaringType: typeof(BaseFormField),
            defaultValue: false);

        public static readonly BindableProperty FieldValidationStateProperty = BindableProperty.Create(
            propertyName: nameof(FieldValidationState),
            returnType: typeof(ValidationStateEnum),
            declaringType: typeof(BaseFormField),
            defaultValue: ValidationStateEnum.Valid,
            defaultBindingMode: BindingMode.TwoWay);

        public static readonly BindableProperty FieldWidthProperty = BindableProperty.Create(
            propertyName: nameof(FieldWidth),
            returnType: typeof(double?),
            declaringType: typeof(BaseFormField),
            defaultValue: 100d,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnFieldWidthPropertyChanged);

        public UndoButton? ButtonUndo;
        public Label? FieldLabel;
        public Label? FieldNotification;

        #endregion Fields

        #region Public Constructors

        public BaseFormField()
        {
            FieldLabel = FieldCreateLabel();
            FieldNotification = FieldCreateNotificationLabel();
            ButtonUndo = FieldCreateUndoButton(fieldAccessMode: FieldAccessMode);
            ButtonUndo.Pressed += OnFieldButtonUndoPressed;
        }

        #endregion Public Constructors

        #region Events

        public event EventHandler<HasChangesEventArgs>? FieldHasChanges;

        public event EventHandler<ValidationDataChangesEventArgs>? FieldHasValidationChanges;

        #endregion Events

        #region Properties

        public FieldAccessModeEnum FieldAccessMode
        {
            get => (FieldAccessModeEnum)GetValue(FieldAccessModeProperty);
            set => SetValue(FieldAccessModeProperty, value);
        }

        public ChangeStateEnum FieldChangeState
        {
            get => (ChangeStateEnum)GetValue(FieldChangeStateProperty);
            set => SetValue(FieldChangeStateProperty, value);
        }

        public ICommand FieldCommand
        {
            get => (ICommand)GetValue(FieldCommandProperty);
            set => SetValue(FieldCommandProperty, value);
        }

        public object FieldCommandParameter
        {
            get => GetValue(FieldCommandParameterProperty);
            set => SetValue(FieldCommandParameterProperty, value);
        }

        public bool FieldEnabled
        {
            get => (bool)GetValue(FieldEnabledProperty);
            set => SetValue(FieldEnabledProperty, value);
        }

        public bool FieldReadOnly
        {
            get => (bool)GetValue(FieldReadOnlyProperty);
            set => SetValue(FieldReadOnlyProperty, value);
        }

        public string FieldLabelText
        {
            get => (string)GetValue(FieldLabelTextProperty);
            set => SetValue(FieldLabelTextProperty, value);
        }

        public double FieldLabelWidth
        {
            get => (double)GetValue(FieldLabelWidthProperty);
            set => SetValue(FieldLabelWidthProperty, value);
        }

        public bool FieldMandatory
        {
            get => (bool)GetValue(FieldMandatoryProperty);
            set => SetValue(FieldMandatoryProperty, value);
        }

        public ValidationStateEnum FieldValidationState
        {
            get => (ValidationStateEnum)GetValue(FieldValidationStateProperty);
            set => SetValue(FieldValidationStateProperty, value);
        }

        public double FieldWidth
        {
            get => (double)GetValue(FieldWidthProperty);
            set => SetValue(FieldWidthProperty, value);
        }

        public new LayoutOptions HorizontalOptions
        {
            get => base.HorizontalOptions;
            set
            {
                if (Children.FirstOrDefault() is Grid grid)
                    grid.HorizontalOptions = value;
                base.HorizontalOptions = value;
            }
        }

        public new LayoutOptions VerticalOptions
        {
            get => base.VerticalOptions;
            set
            {
                if (Content is Grid grid)
                    grid.VerticalOptions = value;
                base.VerticalOptions = value;
            }
        }

        #endregion Properties

        #region Private Methods

        private static void OnFieldAccessModePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseFormField element && newValue is FieldAccessModeEnum newAccessMode)
            {
                if (!oldValue.Equals(newValue))
                {
                    Debug.WriteLine($"{element.FieldLabelText} : OnFieldAccessModePropertyChanged({newValue})");
                    switch (newAccessMode)
                    {
                        case FieldAccessModeEnum.ViewOnly:
                            element.FieldConfigAccessModeViewOnly();
                            return;

                        case FieldAccessModeEnum.Editing:
                            element.FieldConfigAccessModeEditing();
                            return;

                        case FieldAccessModeEnum.Editable:
                            element.FieldConfigAccessModeEditable();
                            return;

                        case FieldAccessModeEnum.Hidden:
                            element.FieldConfigAccessModeHidden();
                            return;
                    }
                }
            }
        }

        private static void OnFieldEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseFormField element && newValue is bool newEnabledState)
            {
                if (!oldValue.Equals(newValue))
                {
                    Debug.WriteLine($"{element.FieldLabelText} : OnFieldEnabledPropertyChanged({newValue})");
                    if (newEnabledState)
                        element.FieldConfigEnabled();
                    else
                        element.FieldConfigDisabled();
                }
            }
        }

        private static void OnFieldLabelTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseFormField element && element.FieldLabel != null)
                element.FieldLabel.Text = newValue?.ToString() ?? "";
        }

        private static void OnFieldLabelWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseFormField element && element.Content is Grid)
                element.FieldUpdateLabelWidth((double)newValue);
        }

        private static void OnFieldWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseFormField element && element.Content is Grid)
                element.FieldUpdateWidth((double)newValue);
        }

        private static void RunOnMainThread(Action action)
        {
            if (MainThread.IsMainThread)
                action();
            else
                MainThread.BeginInvokeOnMainThread(action);
        }

        private void EvaluateToRaiseHasChangesEvent()
        {
            if (_fieldEvaluateToRaiseHasChangesEventing)
                return;

            _fieldEvaluateToRaiseHasChangesEventing = true;
            bool hasChangedFromOriginal = FieldHasChangedFromOriginal();
            bool hasChangedFromLast = FieldHasChangedFromLast();
            if (_fieldPreviousHasChangedFromOriginal != hasChangedFromOriginal)
            {
                Debug.WriteLine($"{FieldLabelText} : EvaluateToRaiseHasChangesEvent()");
                if (ButtonUndo != null)
                {
                    if (FieldAccessMode == FieldAccessModeEnum.Editing)
                    {
                        if (hasChangedFromOriginal)
                        {
                            Debug.WriteLine("ButtonUndo.Enabled()");
                            ButtonUndo.Enabled();
                        }
                        else
                        {
                            Debug.WriteLine("ButtonUndo.Disabled()");
                            ButtonUndo.Disabled();
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ButtonUndo.Hide()");
                        ButtonUndo.Hide();
                    }
                }
                _fieldPreviousHasChangedFromOriginal = hasChangedFromOriginal;
                FieldChangeState = hasChangedFromOriginal ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
                FieldNotifyHasChanges(hasChangedFromOriginal);
            }
            _fieldEvaluateToRaiseHasChangesEventing = false;
        }

        private void EvaluateToRaiseValidationChangesEvent(bool forceRaise = false)
        {
            if (_fieldEvaluateToRaiseValidationChangesEventing)
                return;

            _fieldEvaluateToRaiseValidationChangesEventing = true;
            bool currentFieldHasInvalidData = FieldHasValidData() == false;
            var currentFieldValidationState = currentFieldHasInvalidData ? ValidationStateEnum.FormatError : ValidationStateEnum.Valid;
            if (_previousFieldHasInvalidData != currentFieldHasInvalidData || forceRaise || _previousFieldValidationState != currentFieldValidationState)
            {
                Debug.WriteLine($"{FieldLabelText} : EvaluateToRaiseValidationChangesEvent(forceRaise: {forceRaise})");
                FieldValidationState = currentFieldValidationState;
                FieldNotifyValidationChanges(currentFieldHasInvalidData);
                FieldRefreshUI();
            }
            _previousFieldHasInvalidData = currentFieldHasInvalidData;
            _previousFieldValidationState = currentFieldValidationState;
            _fieldEvaluateToRaiseValidationChangesEventing = false;
        }

        #endregion Private Methods

        #region Protected Methods

        protected static Label FieldCreateNotificationLabel() =>
                            new Label
                            {
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                IsVisible = false,
                                TextColor = Colors.Red
                            };

        protected static UndoButton FieldCreateUndoButton(FieldAccessModeEnum fieldAccessMode) =>
            new UndoButton
            {
                Text = "",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Colors.Transparent,
                ButtonSize = DefaultButtonSize,
                WidthRequest = -1,
                ButtonState = ButtonStateEnum.Hidden,
                ButtonStyle = ButtonStyleEnum.IconOnly,
                ButtonIcon = ButtonIconEnum.Undo,
                BorderWidth = 0,
                Margin = new Thickness(0),
                Padding = new Thickness(5, 0, 0, 0),
                IsVisible = fieldAccessMode == FieldAccessModeEnum.Editing
            };

        protected void Field_Focused(object? sender, FocusEventArgs e)
        { }

        protected void Field_Unfocused(object? sender, FocusEventArgs e) =>
            FieldUpdateNotificationMessage();

        protected ValidationStateEnum FieldCalculateValidationState() =>
            FieldHasRequiredError() ? ValidationStateEnum.RequiredFieldError :
            FieldHasFormatError() ? ValidationStateEnum.FormatError : ValidationStateEnum.Valid;

        protected void FieldConfigAccessModeEditable()
        {
            FieldOriginalValue_Reset();
            FieldUpdateValidationAndChangedState();
            FieldEnabled = false;
        }

        protected void FieldConfigAccessModeEditing()
        {
            FieldUpdateValidationAndChangedState();
            FieldEnabled = true;
        }

        protected void FieldConfigAccessModeHidden() =>
            RunOnMainThread(() =>
            {
                BatchBegin();
                Content.IsVisible = false;
                BatchCommit();
            });

        protected void FieldConfigAccessModeViewOnly() =>
            RunOnMainThread(() =>
            {
                BatchBegin();
                Content.IsVisible = true;
                FieldEnabled = false;
                BatchCommit();
            });

        // Disable the field and hide the undo button
        protected void FieldConfigDisabled()
        {
            if (_fieldDisabling) return;
            _fieldDisabling = true;
            void _unfocus()
            {
                Content.Unfocus();
                foreach (var c in this.GetVisualTreeDescendants())
                {
                    if (c is Entry ce) { ce.IsEnabled = false; }
                    if (c is ContentView cc) { cc.IsEnabled = false; }
                    if (c is Label cl) { cl.IsEnabled = true; }
                }
            }

            if (FieldAccessMode == FieldAccessModeEnum.Editable)
            {
                ButtonUndo?.Hide();
            }
            else if (FieldAccessMode == FieldAccessModeEnum.Editing)
            {
                if (FieldReadOnly == true)
                    ButtonUndo?.Hide();
                else
                    ButtonUndo?.Disabled();
            }
            else
                ButtonUndo?.Hide();

            if (MainThread.IsMainThread)
                _unfocus();
            else
                MainThread.BeginInvokeOnMainThread(_unfocus);

            _fieldDisabling = false;
        }

        protected void FieldConfigEnabled()
        {
            if (_fieldEnabling)
                return;

            _fieldEnabling = true;
            void UnfocusIfNotChanged()
            {
                if (FieldChangeState == ChangeStateEnum.NotChanged)
                    Content.Unfocus();
                foreach (var c in this.GetVisualTreeDescendants())
                {
                    if (c is Entry ce) { ce.IsEnabled = true; }
                    if (c is ContentView cv) { cv.IsEnabled = true; }
                }
            }

            if (FieldAccessMode == FieldAccessModeEnum.Editable)
            {
                ButtonUndo?.Hide();
            }
            else if (FieldAccessMode == FieldAccessModeEnum.Editing)
            {
                if (FieldChangeState == ChangeStateEnum.NotChanged)
                    ButtonUndo?.Disabled();
                else
                    ButtonUndo?.Enabled();
            }
            else
                ButtonUndo?.Hide();

            RunOnMainThread(UnfocusIfNotChanged);
            _fieldEnabling = false;
        }

        protected Label FieldCreateLabel() =>
                                            new Label
                                            {
                                                Text = FieldLabelText,
                                                HorizontalOptions = LayoutOptions.Start,
                                                BackgroundColor = Colors.Transparent,
                                                VerticalOptions = LayoutOptions.Center,
                                                HeightRequest = -1
                                            };

        protected abstract Grid FieldCreateLayoutGrid();

        protected abstract string FieldGetFormatErrorMessage();

        protected abstract bool FieldHasChangedFromLast();

        protected abstract bool FieldHasChangedFromOriginal();

        protected abstract bool FieldHasFormatError();

        protected abstract bool FieldHasRequiredError();

        protected bool FieldHasValidData()
        {
            return !FieldHasFormatError() && !FieldHasRequiredError();
        }

        protected abstract void FieldOriginalValue_SetToCurrentValue();

        protected abstract void FieldOriginalValue_SetToClear();

        protected abstract void FieldOriginalValue_Reset();

        protected abstract void FieldRefreshUI();

        protected void FieldReset()
        {
            FieldOriginalValue_Reset();
            FieldUpdateValidationAndChangedState();
            FieldConfigDisabled();
        }

        protected void FieldUpdateNotificationMessage()
        {
            var validationState = FieldCalculateValidationState();
            string notificationMessage = validationState switch
            {
                ValidationStateEnum.RequiredFieldError => "Field is required.",
                ValidationStateEnum.FormatError => FieldGetFormatErrorMessage(),
                _ => ""
            };
            bool isNotificationVisible = validationState != ValidationStateEnum.Valid;
            if (FieldNotification != null)
            {
                FieldNotification.Text = notificationMessage;
                FieldNotification.IsVisible = isNotificationVisible;
            }
            FieldValidationState = validationState;
        }

        // set datasource value
        protected void FieldUpdateValidationAndChangedState()
        {
            if (_fieldUpdateValidationAndChangedStating) return;
            _fieldUpdateValidationAndChangedStating = true;
            EvaluateToRaiseHasChangesEvent();
            EvaluateToRaiseValidationChangesEvent();
            _fieldUpdateValidationAndChangedStating = false;
        }

        protected void InitializeLayout()
        {
            Content = FieldCreateLayoutGrid();
        }

        protected void OnFieldButtonUndoPressed(object? sender, EventArgs e)
        {
            Debug.WriteLine($"{FieldLabelText} : OnFieldButtonUndoPressed");
            if (_fieldUndoing)
                return;

            _fieldUndoing = true;
            if (FieldChangeState == ChangeStateEnum.Changed)
            {
                FieldOriginalValue_Reset();
                EvaluateToRaiseHasChangesEvent();
            }
            _fieldUndoing = false;
        }

        protected abstract void OnFieldDataSourcePropertyChanged(object newValue, object oldValue);

        protected void OnFieldSizeRequestChanged(double newValue)
        {
            HeightRequest = newValue;
            WidthRequest = newValue;
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            FieldRefreshUI();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.OnPropertyChanged(propertyName);
            FieldRefreshUI();
        }

        #endregion Protected Methods

        #region Public Methods

        public void FieldClear()
        {
            FieldOriginalValue_SetToClear();
            FieldUpdateValidationAndChangedState();
            FieldUnfocus();
        }

        public void FieldNotifyHasChanges(bool hasChanged) =>
            FieldHasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));

        public void FieldNotifyValidationChanges(bool isValid) =>
            FieldHasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));

        public void FieldSaveAndMarkAsReadOnly()
        {
            FieldAccessMode = FieldAccessModeEnum.ViewOnly;
            //if (FieldHasChangedFromOriginal())
            //{
                FieldOriginalValue_SetToCurrentValue();
            //}
            FieldConfigDisabled();
            FieldUpdateValidationAndChangedState();
        }

        public virtual void FieldUnfocus() => base.Unfocus();

        public void FieldUpdateLabelWidth(double newWidth)
        {
            if (Content is Grid grid && grid.ColumnDefinitions.Count > 0)
            {
                grid.ColumnDefinitions[0].Width = new GridLength(newWidth, GridUnitType.Absolute);
                FieldLabel!.WidthRequest = newWidth;
            }
        }

        public void FieldUpdateWidth(double newWidth)
        {
            if (Content is Grid grid && grid.ColumnDefinitions.Count > 1)
                grid.ColumnDefinitions[1].Width = new GridLength(newWidth, GridUnitType.Absolute);
        }

        #endregion Public Methods
    }
}