using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using ca.whittaker.Maui.Controls.Buttons;

namespace ca.whittaker.Maui.Controls.Forms
{
    /// <summary>
    /// Represents a form control within a MAUI application, providing integrated state management,
    /// validation, and control wiring for a composite user input interface.
    /// </summary>
    public class Form : ContentView
    {
        #region Fields

        private SaveButton? _formButtonCancel;
        private EditButton? _formButtonEdit;
        private CancelButton? _formButtonSave;
        private Label? _formLabel;
        private Label? _formLabelNotification;
        private bool _formStatusEvaluating = false;
        private SizeEnum DefaultButtonSize = SizeEnum.XXSmall;

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
            nameof(CommandParameter),
            typeof(object),
            typeof(Form));

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(Form));

        public static readonly BindableProperty FormAccessModeProperty = BindableProperty.Create(
            nameof(FormAccessMode),
            typeof(FormAccessModeEnum),
            typeof(Form),
            defaultValue: FormAccessModeEnum.Editable,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnFormAccessModeChanged);

        public static readonly BindableProperty FormCancelButtonTextProperty = BindableProperty.Create(
            nameof(FormCancelButtonText),
            typeof(string),
            typeof(Form),
            defaultValue: "cancel",
            propertyChanged: OnFormCancelButtonTextChanged);

        public static readonly BindableProperty FormEditButtonTextProperty = BindableProperty.Create(
            nameof(FormEditButtonText),
            typeof(string),
            typeof(Form),
            defaultValue: "edit",
            propertyChanged: OnFormEditButtonTextChanged);

        public static readonly BindableProperty FormHasChangesProperty = BindableProperty.Create(
            nameof(FormHasChanges),
            typeof(bool),
            typeof(Form),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWayToSource);

        public static readonly BindableProperty FormHasErrorsProperty = BindableProperty.Create(
            nameof(FormHasErrors),
            typeof(bool),
            typeof(Form),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWayToSource);

        public static readonly BindableProperty FormNameProperty = BindableProperty.Create(
            nameof(FormName),
            typeof(string),
            typeof(Form),
            defaultValue: "",
            propertyChanged: OnFormNameChanged);

        public static readonly BindableProperty FormSaveButtonTextProperty = BindableProperty.Create(
            nameof(FormSaveButtonText),
            typeof(string),
            typeof(Form),
            defaultValue: "save",
            propertyChanged: OnFormSaveButtonTextChanged,
            defaultBindingMode: BindingMode.TwoWay);

        #endregion Fields

        #region Properties

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public FormAccessModeEnum FormAccessMode
        {
            get => (FormAccessModeEnum)GetValue(FormAccessModeProperty);
            set => SetValue(FormAccessModeProperty, value);
        }

        public string FormCancelButtonText
        {
            get => (string)GetValue(FormCancelButtonTextProperty);
            set => SetValue(FormCancelButtonTextProperty, value);
        }

        public string FormEditButtonText
        {
            get => (string)GetValue(FormEditButtonTextProperty);
            set => SetValue(FormEditButtonTextProperty, value);
        }

        public bool FormHasChanges
        {
            get => (bool)GetValue(FormHasChangesProperty);
            set => SetValue(FormHasChangesProperty, value);
        }

        public bool FormHasErrors
        {
            get => (bool)GetValue(FormHasErrorsProperty);
            set => SetValue(FormHasErrorsProperty, value);
        }

        public string FormName
        {
            get => (string)GetValue(FormNameProperty);
            set => SetValue(FormNameProperty, value);
        }

        public string FormSaveButtonText
        {
            get => (string)GetValue(FormSaveButtonTextProperty);
            set => SetValue(FormSaveButtonTextProperty, value);
        }

        public new LayoutOptions HorizontalOptions
        {
            get => base.HorizontalOptions;
            set => base.HorizontalOptions = value;
        }

        public new LayoutOptions VerticalOptions
        {
            get => base.VerticalOptions;
            set => base.VerticalOptions = value;
        }

        #endregion Properties

        #region Public Events

        // Declare the event to notify subscribers when the form is saved.
        public event EventHandler<FormSavedEventArgs>? FormSaved;

        // Protected virtual method to raise the event.
        protected virtual void OnFormSaved(FormSavedEventArgs e)
        {
            FormSaved?.Invoke(this, e);
        }

        #endregion Public Events


        #region Private Methods

        private static void OnFormAccessModeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Form form && newValue is FormAccessModeEnum newAccessMode)
            {
                RunOnMainThread(() =>
                {
                    if (!oldValue.Equals(newValue))
                    {
                        Debug.WriteLine($"OnFormAccessModeChanged({newAccessMode.ToString()})");
                        switch (newAccessMode)
                        {
                            case FormAccessModeEnum.Editable:
                                form.FormFieldsConfigAccessEditable();
                                break;

                            case FormAccessModeEnum.Editing:
                                form.FormFieldsConfigAccessEditing();
                                break;

                            case FormAccessModeEnum.ViewOnly:
                                form.FormFieldsConfigViewOnlyMode();
                                break;

                            case FormAccessModeEnum.Hidden:
                                form.FormFieldsConfigAccessHidden();
                                break;
                        }
                        form.FormConfigButtonStates();
                    }
                });
            }
        }

        private static void OnFormCancelButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Form form && newValue is string newText)
            {
                RunOnMainThread(() =>
                {
                    if (form._formButtonCancel != null)
                        form._formButtonCancel.Text = newText;
                });
            }
        }

        private static void OnFormEditButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Form form && newValue is string newText)
            {
                RunOnMainThread(() =>
                {
                    if (form._formButtonEdit != null)
                        form._formButtonEdit.Text = newText;
                });
            }
        }

        private static void OnFormNameChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Form form && newValue is string newName)
            {
                RunOnMainThread(() =>
                {
                    if (form._formLabel != null)
                    {
                        form._formLabel.Text = newName;
                        form._formLabel.IsVisible = !string.IsNullOrEmpty(newName);
                    }
                });
            }
        }

        private static void OnFormSaveButtonTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Form form && newValue is string newText)
            {
                RunOnMainThread(() =>
                {
                    if (form._formButtonSave != null)
                        form._formButtonSave.Text = newText;
                });
            }
        }

        private static void RunOnMainThread(Action action)
        {
            if (MainThread.IsMainThread)
                action();
            else
                MainThread.BeginInvokeOnMainThread(action);
        }

        private void FormClear()
        {
            foreach (BaseFormField field in this.GetVisualTreeDescendants().OfType<BaseFormField>())
            {
                if (field is TextBoxField textBox)
                    textBox.Field_Clear();
                if (field is CheckBoxField checkBox)
                    checkBox.Field_Clear();
                field.Field_Unfocus();
            }
        }

        private void FormConfigButtonStates()
        {
            if (FormAccessMode == FormAccessModeEnum.Editable)
            {
                // Editable: show only the edit button
                _formButtonSave!.ButtonState = ButtonStateEnum.Hidden;
                _formButtonCancel!.ButtonState = ButtonStateEnum.Hidden;
                _formButtonEdit!.ButtonState = ButtonStateEnum.Enabled;
            }
            else if (FormAccessMode == FormAccessModeEnum.Editing)
            {
                // Editing: show save and cancel buttons
                _formButtonSave!.ButtonState = FormHasChanges
                    ? (FormHasErrors ? ButtonStateEnum.Disabled : ButtonStateEnum.Enabled)
                    : ButtonStateEnum.Disabled;
                _formButtonCancel!.ButtonState = ButtonStateEnum.Enabled;
                _formButtonEdit!.ButtonState = ButtonStateEnum.Hidden;
            }
            else if (FormAccessMode == FormAccessModeEnum.ViewOnly || FormAccessMode == FormAccessModeEnum.Hidden)
            {
                // ViewOnly/Hidden: hide all buttons
                _formButtonSave!.ButtonState = ButtonStateEnum.Hidden;
                _formButtonCancel!.ButtonState = ButtonStateEnum.Hidden;
                _formButtonEdit!.ButtonState = ButtonStateEnum.Hidden;
            }
        }

        private void FormEvaluateStatus()
        {
            if (_formStatusEvaluating)
                return;

            _formStatusEvaluating = true;
            Debug.WriteLine("FormEvaluateStatus()");
            FormHasErrors = !FormFieldsCheckAreValid();
            FormHasChanges = !FormFieldsCheckArePristine();
            FormConfigButtonStates();
            _formStatusEvaluating = false;
        }

        private bool FormFieldsCheckArePristine()
        {
            foreach (var field in this.GetVisualTreeDescendants().OfType<BaseFormField>())
            {
                if (field.FieldChangeState == ChangeStateEnum.Changed)
                {
                    return false;
                }
            }
            return true;
            // => return this.GetVisualTreeDescendants().OfType<BaseFormField>().All(field => field.FieldChangeState != ChangeStateEnum.Changed);
        }

        private bool FormFieldsCheckAreValid() =>
                    this.GetVisualTreeDescendants().OfType<BaseFormField>().All(field => field.FieldValidationState == ValidationStateEnum.Valid);

        /// <summary>
        /// puts form into "read" mode with edit button visible
        /// </summary>
        private void FormFieldsConfigAccessEditable()
        {
            foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
                t.FieldAccessMode = FieldAccessModeEnum.Editable; 
        }

        /// <summary>
        /// puts form into "read/write" mode with save and cancel buttons visible
        /// </summary>
        private void FormFieldsConfigAccessEditing()
        {
            foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
                t.FieldAccessMode = FieldAccessModeEnum.Editing; 
        }

        /// <summary>
        /// puts form into "hidden" mode with everything hidden
        /// </summary>
        private void FormFieldsConfigAccessHidden()
        {
            foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
                t.FieldAccessMode = FieldAccessModeEnum.Hidden;
        }

        /// <summary>
        /// puts form into "view only" mode with no buttons visible
        /// </summary>
        private void FormFieldsConfigViewOnlyMode()
        {
            foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
                t.FieldAccessMode = FieldAccessModeEnum.ViewOnly; 
        }

        /// <summary>
        /// updates original value with current value, and sets form to readonly
        /// </summary>
        private void FormFieldsMarkAsSaved()
        {
            foreach (BaseFormField t in this.GetVisualTreeDescendants().OfType<BaseFormField>())
                t.Field_SaveAndMarkAsReadOnly(); 
        }

        private void FormFieldsWireUp()
        {
            var formElements = this.GetVisualTreeDescendants().OfType<BaseFormField>();
            if (!formElements.Any())
                throw new InvalidOperationException("Form missing controls");

            foreach (var element in formElements)
            {
                element.FieldHasChanges += OnFieldHasChanges;
                element.FieldHasValidationChanges += OnFieldHasValidationChanges;
            }
        }

        private void FormInitialize()
        {
            void InitializeUI()
            {
                _formButtonEdit = new EditButton
                {
                    Text = FormEditButtonText,
                    ButtonSize = DefaultButtonSize,
                    BackgroundColor = Colors.Transparent,
                    BorderWidth = 0,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    ButtonState = ButtonStateEnum.Enabled,
                    ButtonIcon = ButtonIconEnum.Edit,
                    IsVisible = FormAccessMode == FormAccessModeEnum.Editable,
                };
                _formButtonEdit.Clicked += OnFormEditButtonClicked;
                _formButtonEdit.UpdateUI();

                _formButtonSave = new CancelButton
                {
                    Text = FormSaveButtonText,
                    ButtonSize = DefaultButtonSize,
                    BackgroundColor = Colors.Transparent,
                    BorderWidth = 0,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    ButtonState = ButtonStateEnum.Hidden,
                    ButtonIcon = ButtonIconEnum.Save,
                    IsVisible = FormAccessMode == FormAccessModeEnum.Editable,
                };
                _formButtonSave.Clicked += OnFormSaveButtonClicked;
                _formButtonSave.UpdateUI();

                _formButtonCancel = new SaveButton
                {
                    Text = FormCancelButtonText,
                    ButtonSize = DefaultButtonSize,
                    BackgroundColor = Colors.Transparent,
                    BorderWidth = 0,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    ButtonState = ButtonStateEnum.Hidden,
                    ButtonIcon = ButtonIconEnum.Cancel,
                    IsVisible = FormAccessMode == FormAccessModeEnum.Editable,
                };
                _formButtonCancel.Clicked += OnFormCancelButtonClicked;
                _formButtonCancel.UpdateUI();

                _formLabelNotification = new Label
                {
                    Text = "",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    IsVisible = false,
                    TextColor = Colors.Red,
                };

                _formLabel = new Label
                {
                    Text = FormName,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    IsVisible = !string.IsNullOrEmpty(FormName)
                };

                var gridLayout = new Grid
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(5),
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto }
                    },
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star }
                    }
                };

                gridLayout.Add(_formLabel, 0, 0);
                gridLayout.Add(_formButtonSave, 0, 1);
                gridLayout.Add(_formButtonCancel, 1, 1);
                gridLayout.Add(_formButtonEdit, 0, 2);
                gridLayout.Add(_formLabelNotification, 0, 3);
                gridLayout.SetColumnSpan(_formLabel, 2);
                gridLayout.SetColumnSpan(_formLabelNotification, 2);
                gridLayout.SetColumnSpan(_formButtonEdit, 2);

                if (Content is Layout existingLayout)
                {
                    if (existingLayout.Children.Any())
                        existingLayout.Children.Insert(0, gridLayout);
                    else
                        existingLayout.Children.Add(gridLayout);
                }
                else if (Content is View existingElement)
                {
                    var containerLayout = new StackLayout();
                    containerLayout.Children.Add(existingElement);
                    containerLayout.Children.Add(gridLayout);
                    Content = containerLayout;
                }
                else
                {
                    Content = gridLayout;
                }
            }

            RunOnMainThread(InitializeUI);
            FormFieldsWireUp();
        }

        private void OnFieldHasChanges(object? sender, HasChangesEventArgs e) => FormEvaluateStatus();

        private void OnFieldHasValidationChanges(object? sender, ValidationDataChangesEventArgs e) => FormEvaluateStatus();

        private void OnFormCancelButtonClicked(object? sender, EventArgs e)
        {
            if (FormAccessMode != FormAccessModeEnum.Editable)
                FormAccessMode = FormAccessModeEnum.Editable;
        }

        private void OnFormEditButtonClicked(object? sender, EventArgs e)
        {
            if (FormAccessMode != FormAccessModeEnum.Editing)
                FormAccessMode = FormAccessModeEnum.Editing;
        }

        private void OnFormSaveButtonClicked(object? sender, EventArgs e)
        {
            if (Command?.CanExecute(CommandParameter) == true)
                Command.Execute(CommandParameter);


            bool hasChanges = !FormFieldsCheckArePristine();

            //
            // loop over each field, set original value to datasource value
            // 
            if (hasChanges)
                FormFieldsMarkAsSaved();

            //
            // set form state to "editable"
            //
            FormAccessMode = FormAccessModeEnum.Editable;

            // Raise the FormSaved event.
            OnFormSaved(new FormSavedEventArgs(hasChanges));

        }

        #endregion Private Methods

        #region Protected Methods

        protected override void OnParentSet()
        {
            base.OnParentSet();
            FormInitialize();
            FormFieldsWireUp();
            FormEvaluateStatus();
        }

        #endregion Protected Methods
    }
}