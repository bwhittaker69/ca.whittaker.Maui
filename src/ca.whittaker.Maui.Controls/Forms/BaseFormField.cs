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

        void Field_Clear();

        void Field_NotifyHasChanges(bool hasChanged);

        void Field_NotifyValidationChanges(bool isValid);

        void Field_SaveAndMarkAsReadOnly();

        void Field_Unfocus();

        void Field_UpdateLabelWidth(double newWidth);

        void Field_UpdateWidth(double newWidth);

        #endregion Public Methods
    }

    /// <summary>
    /// Represents the base class for customizable data capture controls.
    /// </summary>
    public abstract class BaseFormField : ContentView, IBaseFormField
    {
        #region Fields

        private bool _fieldEvaluateToRaiseHasChangesEventing = false;
        private bool _fieldUndoing = false;
        private bool _fieldUpdating = false;
        private bool _previousFieldHasInvalidData = false;
        private ValidationStateEnum _previousFieldValidationState;

        protected const SizeEnum DefaultButtonSize = SizeEnum.XXSmall;
        protected bool _fieldDisabling = false;
        protected bool _fieldEnabling = false;
        protected bool _fieldEvaluateToRaiseValidationChangesEventing = false;
        protected bool _fieldIsOriginalValueSet = false;
        protected bool _fieldPreviousHasChangedFromOriginal = false;
        protected bool _fieldUpdateValidationAndChangedStating = false;
        protected bool _fieldUpdatingUI = false;
        protected bool _onFieldDataSourcePropertyChanging = false;

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

        public static readonly BindableProperty FieldLabelTextProperty = BindableProperty.Create(
                    propertyName: nameof(FieldLabelText),
            returnType: typeof(string),
            declaringType: typeof(BaseFormField),
            defaultValue: string.Empty,
            propertyChanged: OnFieldLabelTextPropertyChanged);

        public static readonly BindableProperty FieldUndoButtonVisibleProperty = BindableProperty.Create(
            propertyName: nameof(FieldUndoButtonVisible),
            returnType: typeof(bool),
            declaringType: typeof(BaseFormField),
            defaultValue: true,
            propertyChanged: OnFieldUndoButtonVisiblePropertyChanged);

        public static readonly BindableProperty FieldLabelVisibleProperty = BindableProperty.Create(
                    propertyName: nameof(FieldLabelVisible),
            returnType: typeof(bool),
            declaringType: typeof(BaseFormField),
            defaultValue: true,
            propertyChanged: OnFieldLabelVisiblePropertyChanged);

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

        public static readonly BindableProperty FieldReadOnlyProperty = BindableProperty.Create(
                                            propertyName: nameof(FieldReadOnly),
            returnType: typeof(bool),
            declaringType: typeof(BaseFormField),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

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
            FieldLabel = Field_CreateLabel(fieldLabelVisible: FieldLabelVisible);
            FieldNotification = Field_CreateNotificationLabel();
            ButtonUndo = Field_CreateUndoButton(fieldHasUndo: FieldUndoButtonVisible, fieldAccessMode: FieldAccessMode);
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

        public bool FieldReadOnly
        {
            get => (bool)GetValue(FieldReadOnlyProperty);
            set => SetValue(FieldReadOnlyProperty, value);
        }

        public bool FieldUndoButtonVisible
        {
            get => (bool)GetValue(FieldUndoButtonVisibleProperty);
            set => SetValue(FieldUndoButtonVisibleProperty, value);
        } 

        public bool FieldLabelVisible
        {
            get => (bool)GetValue(FieldLabelVisibleProperty);
            set => SetValue(FieldLabelVisibleProperty, value);
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
                            element.Field_ConfigAccessModeViewOnly();
                            return;

                        case FieldAccessModeEnum.Editing:
                            element.Field_ConfigAccessModeEditing();
                            return;

                        case FieldAccessModeEnum.Editable:
                            element.Field_ConfigAccessModeEditable();
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
                    Debug.WriteLine($"{element.FieldLabelText} : private static void OnFieldEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)\r\n({newValue})");
                    if (newEnabledState)
                        element.Field_ConfigEnabled();
                    else
                        element.Field_ConfigDisabled();
                }
            }
        }

        private static void OnFieldUndoButtonVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseFormField element && newValue is bool fieldUndoButtonVisible)
            {
                if (!oldValue.Equals(newValue))
                {
                    Debug.WriteLine($"OnFieldUndoButtonVisiblePropertyChanged({newValue})");
                    if (fieldUndoButtonVisible)
                        element.ButtonUndo!.IsVisible = true;
                    else
                        element.ButtonUndo!.IsVisible = false;
                }
            }
        }
        private static void OnFieldLabelVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseFormField element && newValue is bool fieldLabelVisible)
            {
                if (!oldValue.Equals(newValue))
                {
                    Debug.WriteLine($"OnFieldLabelVisiblePropertyChanged({newValue})");
                    if (fieldLabelVisible)
                        element.FieldLabel!.IsVisible = true;
                    else
                        element.FieldLabel!.IsVisible = false;
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
                element.Field_UpdateLabelWidth((double)newValue);
        }

        private static void OnFieldWidthPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseFormField element && element.Content is Grid)
                element.Field_UpdateWidth((double)newValue);
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
            bool hasChangedFromOriginal = Field_HasChangedFromOriginal();
            bool hasChangedFromLast = Field_HasChangedFromLast();
            if (_fieldPreviousHasChangedFromOriginal != hasChangedFromOriginal)
            {
                Debug.WriteLine($"{FieldLabelText} : EvaluateToRaiseHasChangesEvent()");
                if (ButtonUndo != null)
                {
                    if (FieldUndoButtonVisible)
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
                }
                _fieldPreviousHasChangedFromOriginal = hasChangedFromOriginal;
                FieldChangeState = hasChangedFromOriginal ? ChangeStateEnum.Changed : ChangeStateEnum.NotChanged;
                Field_NotifyHasChanges(hasChangedFromOriginal);
            }
            _fieldEvaluateToRaiseHasChangesEventing = false;
        }

        private void EvaluateToRaiseValidationChangesEvent(bool forceRaise = false)
        {
            if (_fieldEvaluateToRaiseValidationChangesEventing)
                return;

            _fieldEvaluateToRaiseValidationChangesEventing = true;
            bool currentFieldHasInvalidData = Field_HasValidData() == false;
            var currentFieldValidationState = currentFieldHasInvalidData ? ValidationStateEnum.FormatError : ValidationStateEnum.Valid;
            if (_previousFieldHasInvalidData != currentFieldHasInvalidData || forceRaise || _previousFieldValidationState != currentFieldValidationState)
            {
                Debug.WriteLine($"{FieldLabelText} : EvaluateToRaiseValidationChangesEvent(forceRaise: {forceRaise})");
                FieldValidationState = currentFieldValidationState;
                Field_NotifyValidationChanges(currentFieldHasInvalidData);
                //FieldRefreshUI();
            }
            _previousFieldHasInvalidData = currentFieldHasInvalidData;
            _previousFieldValidationState = currentFieldValidationState;
            _fieldEvaluateToRaiseValidationChangesEventing = false;
        }

        #endregion Private Methods

        #region Protected Methods

        protected static Label Field_CreateNotificationLabel() =>
                            new Label
                            {
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                IsVisible = false,
                                TextColor = Colors.Red
                            };

        protected static UndoButton Field_CreateUndoButton(bool fieldHasUndo, FieldAccessModeEnum fieldAccessMode) =>
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
                IsVisible = (fieldAccessMode == FieldAccessModeEnum.Editing && fieldHasUndo)
            };

        protected void Field_Focused(object? sender, FocusEventArgs e)
        { }

        protected void Field_Unfocused(object? sender, FocusEventArgs e) =>
            Field_UpdateNotificationMessage();

        protected ValidationStateEnum FieldCalculateValidationState() =>
            Field_HasRequiredError() ? ValidationStateEnum.RequiredFieldError :
            Field_HasFormatError() ? ValidationStateEnum.FormatError : ValidationStateEnum.Valid;

        protected void Field_ConfigAccessModeEditable()
        {
            Field_OriginalValue_Reset();
            Field_UpdateValidationAndChangedState();
            FieldEnabled = false;
        }

        protected void Field_ConfigAccessModeEditing()
        {
            Field_UpdateValidationAndChangedState();
            FieldEnabled = true;
        }

        protected void FieldConfigAccessModeHidden() =>
            RunOnMainThread(() =>
            {
                BatchBegin();
                Content.IsVisible = false;
                BatchCommit();
            });

        protected void Field_ConfigAccessModeViewOnly() =>
            RunOnMainThread(() =>
            {
                BatchBegin();
                Content.IsVisible = true;
                FieldEnabled = false;
                BatchCommit();
            });

        protected abstract void UpdateRow0Layout();

        // Update the _editorBox layout in row 0 based on the visibility of FieldLabel and ButtonUndo.
        protected void UpdateRow1Layout()
        {
        }
        private void SetupVisibilityHandlers()
        {
            FieldLabel!.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FieldLabel.IsVisible))
                {
                    Debug.WriteLine($"FieldLabel.IsVisible={FieldLabel.IsVisible}");
                    UpdateRow0Layout();
                }
            };

            ButtonUndo!.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ButtonUndo.IsVisible))
                {
                    Debug.WriteLine($"ButtonUndo.IsVisible={ButtonUndo.IsVisible}");
                    UpdateRow0Layout();
                }
            };

            FieldNotification!.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FieldNotification.IsVisible))
                {
                    Debug.WriteLine($"FieldNotification.IsVisible={FieldNotification.IsVisible}");
                    UpdateRow1Layout();
                }
            };

        }


        // Disable the field and hide the undo button
        protected void Field_ConfigDisabled()
        {
            if (_fieldDisabling) return;
            _fieldDisabling = true;
            void _hideUndoButton()
            {
                ButtonUndo!.IsVisible = false;
            }
            void _showUndoButton()
            {
                ButtonUndo!.IsVisible = true;
            }
            void _unfocus()
            {
                BatchBegin();
                Content.Unfocus();

                ControlVisualHelper.DisableDescendantControls(this, FieldLabelVisible);

                //foreach (var c in this.GetVisualTreeDescendants())
                //{
                //    // ****************************************
                //    //  one for every implimented control type
                //    // ****************************************
                //    if (c is Entry e) e.IsEnabled = false;
                //    if (c is Picker p) p.IsEnabled = false;
                //    if (c is Editor ed) ed.IsEnabled = false;
                //    if (c is ContentView cv) cv.IsEnabled = false;

                //    // if label is visible, label always remains enabled
                //    if (c is Label l && FieldLabelVisible) l.IsEnabled = true;
                //}
                BatchCommit();
            }

            if (FieldUndoButtonVisible)
            {
                if (FieldAccessMode == FieldAccessModeEnum.Editable)
                {
                    RunOnMainThread(_hideUndoButton);
                }
                else if (FieldAccessMode == FieldAccessModeEnum.Editing)
                {
                    if (FieldReadOnly == true)
                    {
                        RunOnMainThread(_showUndoButton);
                    }
                    else
                    {
                        RunOnMainThread(_showUndoButton);
                        ButtonUndo?.Disabled();
                    }
                }
                else
                {
                    RunOnMainThread(_hideUndoButton);
                }
            }
            else
                RunOnMainThread(_hideUndoButton);


            UpdateRow0Layout();

            if (MainThread.IsMainThread)
                _unfocus();
            else
                MainThread.BeginInvokeOnMainThread(_unfocus);

            _fieldDisabling = false;
        }

        protected void Field_ConfigEnabled()
        {
            if (_fieldEnabling)
                return;

            _fieldEnabling = true;
            void _hideUndoButton()
            {
                ButtonUndo!.IsVisible = false; 
            }
            void _showUndoButton()
            {
                ButtonUndo!.IsVisible = true;
            }

            void UnfocusIfNotChanged()
            {
                if (FieldChangeState == ChangeStateEnum.NotChanged)
                    Content.Unfocus();

                ControlVisualHelper.EnableDescendantControls(this, FieldLabelVisible);

                //foreach (var c in this.GetVisualTreeDescendants())
                //{
                //    // ****************************************
                //    //  one for every implimented control type
                //    // ****************************************
                //    if (c is Entry e) e.IsEnabled = true;
                //    if (c is Editor ed) ed.IsEnabled = true;
                //    if (c is ContentView cv) cv.IsEnabled = true;

                //    // if label is visible, label always remains enabled
                //    if (c is Label l && FieldLabelVisible) l.IsEnabled = true;
                //}
            }
            if (FieldUndoButtonVisible)
            {
                if (FieldAccessMode == FieldAccessModeEnum.Editable)
                {
                    RunOnMainThread(_hideUndoButton);
                }
                else if (FieldAccessMode == FieldAccessModeEnum.Editing)
                {
                    if (FieldChangeState == ChangeStateEnum.NotChanged)
                    {
                        RunOnMainThread(_showUndoButton);
                        if (FieldUndoButtonVisible) ButtonUndo?.Disabled();
                    }
                    else
                    {
                        RunOnMainThread(_showUndoButton);
                        if (FieldUndoButtonVisible) ButtonUndo?.Enabled();
                    }
                }
                else
                {
                    RunOnMainThread(_hideUndoButton);
                }
            }
            else
                RunOnMainThread(_hideUndoButton);

            UpdateRow0Layout();

            RunOnMainThread(UnfocusIfNotChanged);
            _fieldEnabling = false;
        }

        protected Label Field_CreateLabel(bool fieldLabelVisible) =>
                                            new Label
                                            {
                                                Text = FieldLabelText,
                                                HorizontalOptions = LayoutOptions.Start,
                                                BackgroundColor = Colors.Transparent,
                                                VerticalOptions = LayoutOptions.Center,
                                                HeightRequest = -1,
                                                IsVisible = fieldLabelVisible
                                            };

        protected abstract Grid Field_CreateLayoutGrid();

        protected abstract string Field_GetFormatErrorMessage();

        protected abstract bool Field_HasChangedFromLast();

        protected abstract bool Field_HasChangedFromOriginal();

        protected abstract bool Field_HasFormatError();

        protected abstract bool Field_HasRequiredError();

        protected bool Field_HasValidData()
        {
            return !Field_HasFormatError() && !Field_HasRequiredError();
        }

        protected abstract void Field_OriginalValue_Reset();

        protected abstract void Field_OriginalValue_SetToClear();

        protected abstract void Field_OriginalValue_SetToCurrentValue();


        protected void Field_Reset()
        {
            Field_OriginalValue_Reset();
            Field_UpdateValidationAndChangedState();
            Field_ConfigDisabled();
        }

        protected void Field_UpdateNotificationMessage()
        {
            var validationState = FieldCalculateValidationState();
            string notificationMessage = validationState switch
            {
                ValidationStateEnum.RequiredFieldError => "Field is required.",
                ValidationStateEnum.FormatError => Field_GetFormatErrorMessage(),
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
        protected void Field_UpdateValidationAndChangedState()
        {
            if (_fieldUpdateValidationAndChangedStating) return;
            _fieldUpdateValidationAndChangedStating = true;
            EvaluateToRaiseHasChangesEvent();
            EvaluateToRaiseValidationChangesEvent();
            _fieldUpdateValidationAndChangedStating = false;
        }

        protected void InitializeLayout()
        {
            Content = Field_CreateLayoutGrid();
            SetupVisibilityHandlers();
            UpdateRow0Layout();
        }

        protected abstract void OnDataSourcePropertyChanged(object newValue, object oldValue);

        protected void OnFieldButtonUndoPressed(object? sender, EventArgs e)
        {
            Debug.WriteLine($"{FieldLabelText} : OnFieldButtonUndoPressed");
            if (_fieldUndoing)
                return;

            _fieldUndoing = true;
            if (FieldChangeState == ChangeStateEnum.Changed)
            {
                Field_OriginalValue_Reset();
                EvaluateToRaiseHasChangesEvent();
            }
            _fieldUndoing = false;
        }

        protected void OnFieldSizeRequestChanged(double newValue)
        {
            HeightRequest = newValue;
            WidthRequest = newValue;
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.OnPropertyChanged(propertyName);
        }

        #endregion Protected Methods

        #region Public Methods

        public void Field_Clear()
        {
            Field_OriginalValue_SetToClear();
            Field_UpdateValidationAndChangedState();
            Field_Unfocus();
        }

        public void Field_NotifyHasChanges(bool hasChanged) =>
            FieldHasChanges?.Invoke(this, new HasChangesEventArgs(hasChanged));

        public void Field_NotifyValidationChanges(bool isValid) =>
            FieldHasValidationChanges?.Invoke(this, new ValidationDataChangesEventArgs(!isValid));

        public void Field_SaveAndMarkAsReadOnly()
        {
            FieldAccessMode = FieldAccessModeEnum.ViewOnly;
            Field_OriginalValue_SetToCurrentValue();
            Field_ConfigDisabled();
            Field_UpdateValidationAndChangedState();
        }

        public virtual void Field_Unfocus() => base.Unfocus();

        public void Field_UpdateLabelWidth(double newWidth)
        {
            if (Content is Grid grid && grid.ColumnDefinitions.Count > 0)
            {
                grid.ColumnDefinitions[0].Width = new GridLength(newWidth, GridUnitType.Absolute);
                FieldLabel!.WidthRequest = newWidth;
            }
        }

        public void Field_UpdateWidth(double newWidth)
        {
            if (Content is Grid grid && grid.ColumnDefinitions.Count > 1)
                grid.ColumnDefinitions[1].Width = new GridLength(newWidth, GridUnitType.Absolute);
        }

        #endregion Public Methods
    }
}