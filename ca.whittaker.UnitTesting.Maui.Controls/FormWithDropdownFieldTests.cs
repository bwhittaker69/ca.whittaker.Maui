using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ca.whittaker.Maui.Controls;
using ca.whittaker.Maui.Controls.Buttons;
using ca.whittaker.Maui.Controls.Forms;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace ca.whittaker.UnitTesting.Maui.Controls
{
    [TestFixture]
    public class FormWithDropdownFieldTests
    {
        private Form _form;
        private DropdownField _dropdown;
        private Picker _picker;
        private CancelButton _cancelBtn;
        private EditButton _editBtn;

        [SetUp]
        public void Setup()
        {
            // sync UI threads
            UiThreadHelper.SetRunOnMainThreadHandler(a => a());
            Application.Current = new Application();

            // create dropdown and parent‐set it
            _dropdown = new DropdownField()
            {
                FieldUndoButton = true
            };
            InvokePrivate(_dropdown, "OnParentSet");

            // embed into form
            _form = new Form();
            _form.Content = _dropdown;
            InvokePrivate(_form, "OnParentSet");

            // grab private controls
            _picker = GetPrivateField<Picker>(_dropdown, "_pickerControl");
            _cancelBtn = GetPrivateField<CancelButton>(_form, "_formButtonCancelAction");
            _editBtn = GetPrivateField<EditButton>(_form, "_formButtonEditAction");
        }

        [TearDown]
        public void TearDown()
            => UiThreadHelper.SetRunOnMainThreadHandler(null);

        [Test]
        public void DropdownInForm_Load_NoDataSource_InitialState()
        {
            // ARRANGE
            var results = new List<string>();

            // VERIFY initial form + dropdown state
            bool formEditable = _form.FormAccessMode == FormAccessModeEnum.Editable;
            bool changesNone = !_form.FormHasChanges;
            bool cancelHidden = _cancelBtn.ButtonState == ButtonStateEnum.Hidden;
            bool editEnabled = _editBtn.ButtonState == ButtonStateEnum.Enabled;
            bool undoHidden = _dropdown.FieldButtonUndo.ButtonState == ButtonStateEnum.Hidden;
            bool dataNull = _dropdown.FieldDataSource == null;

            results.Add($"> FormAccessMode = {_form.FormAccessMode}");
            results.Add($"> FormHasChanges = {_form.FormHasChanges}");
            results.Add($"> CancelButton = {_cancelBtn.ButtonState}");
            results.Add($"> EditButton = {_editBtn.ButtonState}");
            results.Add($"> Dropdown.Undo = {_dropdown.FieldButtonUndo.ButtonState}");
            results.Add($"> Dropdown.DataSrc = {_dropdown.FieldDataSource?.ToString() ?? "null"}");

            // ASSERT
            try
            {
                Assert.That(formEditable, Is.True, "Form default must be Editable");
                Assert.That(changesNone, Is.True, "FormHasChanges must start false");
                Assert.That(cancelHidden, Is.True, "Cancel must start hidden");
                Assert.That(editEnabled, Is.True, "Edit must start enabled");
                Assert.That(undoHidden, Is.True, "Undo must start hidden");
                Assert.That(dataNull, Is.True, "FieldDataSource must start null");

                Assert.Pass($"Initial load with no data source passed:\n{string.Join("\n", results)}");
            }
            catch (AssertionException)
            {
                Assert.Fail($"Initial load with no data source failed:\n{string.Join("\n", results)}");
            }
        }

        [Test]
        public void DropdownInForm_Load_WithItemDataSource_InitialState()
        {
            // ARRANGE
            var items = new List<(string Id, string Name)> { ("1", "One"), ("2", "Two") };
            InvokePrivate(_dropdown, "OnDropdownItemsSourceChanged", items);

            var results = new List<string>();

            // VERIFY state after loading items, no selection
            int itemCount = _picker.Items.Count;
            bool dataNull = _dropdown.FieldDataSource == null;
            bool undoHidden = _dropdown.FieldButtonUndo.ButtonState == ButtonStateEnum.Hidden;

            results.Add($"> Items loaded count = {itemCount}");
            results.Add($"> Dropdown.DataSrc = {_dropdown.FieldDataSource?.ToString() ?? "null"}");
            results.Add($"> Dropdown.Undo = {_dropdown.FieldButtonUndo.ButtonState}");

            // ASSERT
            try
            {
                Assert.That(itemCount, Is.EqualTo(items.Count + 1), "Picker should include blank + all items");
                Assert.That(dataNull, Is.True, "No selection means DataSource still null");
                Assert.That(undoHidden, Is.True, "Undo must remain hidden");

                Assert.Pass($"Initial load with item data source passed:\n{string.Join("\n", results)}");
            }
            catch (AssertionException)
            {
                Assert.Fail($"Initial load with item data source failed:\n{string.Join("\n", results)}");
            }
        }

        [Test]
        public void DropdownInForm_Load_WithDelimitedString_InitialState()
        {
            // ARRANGE
            const string data = "A,B,C";
            InvokePrivate(_dropdown, "OnDropdownItemsSourceChanged", data);

            var results = new List<string>();

            // VERIFY state after loading delimited string, no selection
            var items = data.Split(',');
            int itemCount = _picker.Items.Count;
            bool dataNull = _dropdown.FieldDataSource == null;
            bool undoHidden = _dropdown.FieldButtonUndo.ButtonState == ButtonStateEnum.Hidden;

            results.Add($"> Delimited items count = {itemCount}");
            results.Add($"> Dropdown.DataSrc = {_dropdown.FieldDataSource?.ToString() ?? "null"}");
            results.Add($"> Dropdown.Undo = {_dropdown.FieldButtonUndo.ButtonState}");

            // ASSERT
            try
            {
                Assert.That(itemCount, Is.EqualTo(items.Length + 1), "Picker should include blank + all delimited entries");
                Assert.That(dataNull, Is.True, "No selection means DataSource still null");
                Assert.That(undoHidden, Is.True, "Undo must remain hidden");

                Assert.Pass($"Initial load with delimited string passed:\n{string.Join("\n", results)}");
            }
            catch (AssertionException)
            {
                Assert.Fail($"Initial load with delimited string failed:\n{string.Join("\n", results)}");
            }
        }







        [Test]
        public void DropdownInForm_Edit_Select_CancelFlow()
        {
            // ARRANGE
            var items = new List<(string Id, string Name)> { ("1", "One"), ("2", "Two") };
            InvokePrivate(_dropdown, "OnDropdownItemsSourceChanged", items);
            // Switch into Editing (not Editable)
            _form.SetValue(Form.FormAccessModeProperty, FormAccessModeEnum.Editing);
            InvokePrivate(_form, "OnFormAccessModeChanged",
                          _form,
                          (object)FormAccessModeEnum.Editable,
                          (object)FormAccessModeEnum.Editing);

            // PHASE 1: “just switched to editing”


            bool editHiddenAfterEdit = _editBtn.ButtonState == ButtonStateEnum.Hidden;
            bool cancelDisabledAfterEdit = _cancelBtn.ButtonState == ButtonStateEnum.Disabled;

            // PHASE 2: “after selection”
            _picker.SelectedItem = items[1];
            InvokePrivate(_dropdown, "OnDropdownSelectedIndexChanged", _picker, EventArgs.Empty);
            bool undoEnabledAfterSelect = _dropdown.FieldButtonUndo.ButtonState == ButtonStateEnum.Enabled;
            bool cancelEnabledAfterSelect = _cancelBtn.ButtonState == ButtonStateEnum.Enabled;

            // PHASE 3: “after cancel”
            InvokePrivate(_form, "OnFormCancelButtonClicked", _cancelBtn, EventArgs.Empty);
            bool editEnabledAfterCancel = _editBtn.ButtonState == ButtonStateEnum.Enabled;
            bool undoHiddenAfterCancel = _dropdown.FieldButtonUndo.ButtonState == ButtonStateEnum.Hidden;

            // ASSERT
            try
            {
                // phase 1
                Assert.That(editHiddenAfterEdit, Is.True, "Edit must hide immediately on Editing");
                Assert.That(cancelDisabledAfterEdit, Is.True, "Cancel must start Disabled in Editing");

                // phase 2
                Assert.That(undoEnabledAfterSelect, Is.True, "Undo must enable on selection");
                Assert.That(cancelEnabledAfterSelect, Is.True, "Cancel must enable on selection");

                // phase 3
                Assert.That(editEnabledAfterCancel, Is.True, "Edit must re-enable after Cancel");
                Assert.That(undoHiddenAfterCancel, Is.True, "Undo must hide after Cancel");

                Assert.Pass("Passed multi‐step Dropdown-in-Form flow.");
            }
            catch (AssertionException ex)
            {
                Assert.Fail($"Test failed:\n{ex.Message}");
            }
        }
        // your TestItem helper
        public class TestItem
        {
            public string Id { get; }
            public string Name { get; }

            public TestItem(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public override string ToString() => $"({Id},{Name})";
        }


        // reflection helpers
        private static T GetPrivateField<T>(object target, string name)
            => (T)target.GetType()
                       .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
                       .GetValue(target)!;

        private static object InvokePrivate(object target, string method, params object[] args)
        {
            // look for instance OR static, non-public
            var mi = target.GetType().GetMethod(
                method,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic
            );
            if (mi == null)
                throw new InvalidOperationException($"No such method '{method}'");
            // if static, pass null; otherwise, pass the target instance
            var invokeTarget = mi.IsStatic ? null : target;
            return mi.Invoke(invokeTarget, args)!;
        }

    }
}
