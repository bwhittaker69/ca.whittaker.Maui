using ca.whittaker.Maui.Controls;
using ca.whittaker.Maui.Controls.Forms;
using Microsoft.Maui.Controls;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ca.whittaker.UnitTesting.Maui.Controls
{
    [TestFixture]
    public class DropdownFieldTests
    {
        private DropdownField _sut;

        [SetUp]
        public void Setup()
        {
            // make all RunOnMainThread calls synchronous
            UiThreadHelper.SetRunOnMainThreadHandler(a => a());

            // satisfy MAUI’s dispatcher check
            Application.Current = new Application();

            _sut = new DropdownField();
            // simulate MAUI attaching the control
            InvokePrivate(_sut, "OnParentSet");
        }

        [TearDown]
        public void TearDown()
        {
            UiThreadHelper.SetRunOnMainThreadHandler(null);
        }

        // Helpers
        private static T GetPrivateField<T>(object target, string fieldName)
            => (T)target.GetType()
                        .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(target)!;

        private static object InvokePrivate(object target, string methodName, params object?[] args)
        {
            var mi = target.GetType()
                           .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;
            return mi.Invoke(target, args)!;
        }

        [Test]
        public void Defaults_AreAsExpected()
        {
            var placeholder = GetPrivateField<Entry>(_sut, "_pickerControlPlaceholder");
            var picker = GetPrivateField<Picker>(_sut, "_pickerControl");

            Assert.Multiple(() =>
            {
                Assert.That(_sut.DropdownPlaceholder, Is.Empty);
                Assert.That(placeholder.Placeholder, Is.Empty);
                Assert.That(placeholder.IsEnabled, Is.False);
                Assert.That(placeholder.InputTransparent, Is.True);

                Assert.That(picker.IsEnabled, Is.False);
                Assert.That(picker.Items.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void Setting_Placeholder_Updates_PlaceholderEntry()
        {
            var placeholder = GetPrivateField<Entry>(_sut, "_pickerControlPlaceholder");

            _sut.DropdownPlaceholder = "Select...";
            InvokePrivate(_sut, "OnDropdownPlaceholderPropertyChanged", _sut.DropdownPlaceholder);

            Assert.That(placeholder.Text, Is.EqualTo("Select..."));
        }

        [Test]
        public void ItemsSource_DelimitedString_NotMandatory_IncludesBlank()
        {
            const string data = "A,B,C";
            var picker = GetPrivateField<Picker>(_sut, "_pickerControl");

            // directly invoke the private change‐handler
            InvokePrivate(_sut, "OnDropdownItemsSourceChanged", data);

            // blank + 3 items
            Assert.That(picker.Items.Count, Is.EqualTo(4));
            Assert.That(picker.Items[0], Is.EqualTo(string.Empty));
            CollectionAssert.AreEqual(new[] { "A", "B", "C" }, picker.Items.Skip(1));
        }

        [Test]
        public void ItemsSource_DelimitedString_Mandatory_NoBlank()
        {
            const string data = "X;Y";
            var picker = GetPrivateField<Picker>(_sut, "_pickerControl");

            _sut.FieldMandatory = true;
            InvokePrivate(_sut, "OnDropdownItemsSourceChanged", data);

            Assert.That(picker.Items.Count, Is.EqualTo(2));
            CollectionAssert.AreEqual(new[] { "X", "Y" }, picker.Items);
        }

        [Test]
        public void SelectionChange_DelimitedString_Updates_FieldDataSource()
        {
            const string data = "One,Two";
            var picker = GetPrivateField<Picker>(_sut, "_pickerControl");

            _sut.FieldAccessMode = FieldAccessModeEnum.Editing;
            InvokePrivate(_sut, "OnDropdownItemsSourceChanged", data);

            // select second item ("Two")
            picker.SelectedIndex = 2;
            InvokePrivate(_sut, "OnDropdownSelectedIndexChanged", picker, EventArgs.Empty);

            Assert.That(_sut.FieldDataSource, Is.EqualTo("Two"));
        }

        class TestItem { public string Id { get; set; } = ""; public string Name { get; set; } = ""; }

        [Test]
        public void ItemsSource_ComplexObject_NotMandatory_IncludesNull()
        {
            var items = new List<TestItem>
            {
                new() { Id="1", Name="One" },
                new() { Id="2", Name="Two" }
            };
            var picker = GetPrivateField<Picker>(_sut, "_pickerControl");

            _sut.DropdownItemsSourcePrimaryKey = "Id";
            _sut.DropdownItemsSourceDisplayPath = "Name";
            InvokePrivate(_sut, "OnDropdownItemsSourceChanged", items);

            // ItemsSource is IEnumerable<object?>
            var src = (IList<object?>)picker.ItemsSource!;
            Assert.That(src.Count, Is.EqualTo(1 + items.Count));
            Assert.That(src[0], Is.Null);
            Assert.That(src[1], Is.SameAs(items[0]));
            Assert.That(src[2], Is.SameAs(items[1]));
        }

        [Test]
        public void SelectionChange_ComplexObject_Updates_FieldDataSource()
        {
            var items = new List<TestItem>
            {
                new() { Id="10", Name="Ten" },
                new() { Id="20", Name="Twenty" }
            };
            var picker = GetPrivateField<Picker>(_sut, "_pickerControl");

            _sut.FieldAccessMode = FieldAccessModeEnum.Editing;
            _sut.DropdownItemsSourcePrimaryKey = "Id";
            _sut.DropdownItemsSourceDisplayPath = "Name";
            InvokePrivate(_sut, "OnDropdownItemsSourceChanged", items);

            // select the second object
            picker.SelectedItem = items[1];
            InvokePrivate(_sut, "OnDropdownSelectedIndexChanged", picker, EventArgs.Empty);

            Assert.That(_sut.FieldDataSource, Is.EqualTo("20"));
        }

        [Test]
        public void RequiredValidation_Works()
        {
            _sut.FieldMandatory = true;

            // no selection => error
            var error1 = (bool)InvokePrivate(_sut, "Field_HasRequiredError");
            Assert.That(error1, Is.True);

            // set a value on the *data source*, not just the UI
            _sut.FieldDataSource = "foo";   // ← this drives the VM
            var error2 = (bool)InvokePrivate(_sut, "Field_HasRequiredError");
            Assert.That(error2, Is.False);
        }

        [Test]
        public void GetListFromDelimitedString_SplitsCorrectly()
        {
            var list1 = DropdownField.GetListFromDelimitedString("a,b,c");
            CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, list1);

            var list2 = DropdownField.GetListFromDelimitedString("x;y;z");
            CollectionAssert.AreEquivalent(new[] { "x", "y", "z" }, list2);

            // equal counts => comma chosen
            var list3 = DropdownField.GetListFromDelimitedString("a,b;c");
            CollectionAssert.AreEqual(new[] { "a", "b;c" }, list3);
        }
        [Test]
        public void UndoButton_DefaultViewOnly_HiddenThenEnabledOnSelections()
        {
            // ARRANGE
            var items = new List<TestItem>
            {
                new() { Id = "1", Name = "One" },
                new() { Id = "2", Name = "Two" },
                new() { Id = "3", Name = "Three" }
            };
            var picker = GetPrivateField<Picker>(_sut, "_pickerControl");
            _sut.DropdownItemsSourcePrimaryKey = "Id";
            _sut.DropdownItemsSourceDisplayPath = "Name";
            InvokePrivate(_sut, "OnDropdownItemsSourceChanged", items);

            var undoBtn = _sut.FieldButtonUndo;
            var results = new List<string>();

            // VERIFY #0: default access mode is ViewOnly
            bool defaultIsViewOnly = _sut.FieldAccessMode == FieldAccessModeEnum.ViewOnly;
            results.Add($"> {(defaultIsViewOnly ? "PASS" : "FAIL")}: Default FieldAccessMode = {_sut.FieldAccessMode}");

            // VERIFY #1: undo starts Hidden
            bool initialHidden = undoBtn.ButtonState == ButtonStateEnum.Hidden;
            results.Add($"> {(initialHidden ? "PASS" : "FAIL")}: Initially, ButtonState = {undoBtn.ButtonState}");

            // ACT: switch into Editing
            _sut.FieldAccessMode = FieldAccessModeEnum.Editing;
            bool nowEditing = _sut.FieldAccessMode == FieldAccessModeEnum.Editing;
            results.Add($"> {(nowEditing ? "PASS" : "FAIL")}: After setting, FieldAccessMode = {_sut.FieldAccessMode}");

            // ACT #2: first selection
            picker.SelectedItem = items[0];
            InvokePrivate(_sut, "OnDropdownSelectedIndexChanged", picker, EventArgs.Empty);
            bool enabledAfterFirst = undoBtn.ButtonState == ButtonStateEnum.Enabled;
            results.Add($"> {(enabledAfterFirst ? "PASS" : "FAIL")}: After first selection, ButtonState = {undoBtn.ButtonState}");

            // ACT #3: second selection
            picker.SelectedItem = items[1];
            InvokePrivate(_sut, "OnDropdownSelectedIndexChanged", picker, EventArgs.Empty);
            bool enabledAfterSecond = undoBtn.ButtonState == ButtonStateEnum.Enabled;
            results.Add($"> {(enabledAfterSecond ? "PASS" : "FAIL")}: After second selection, ButtonState = {undoBtn.ButtonState}");

            // ASSERT
            try
            {
                Assert.That(defaultIsViewOnly, Is.True, "Default access mode must be ViewOnly");
                Assert.That(initialHidden, Is.True, "Undo must start Hidden");
                Assert.That(nowEditing, Is.True, "FieldAccessMode must switch to Editing");
                Assert.That(enabledAfterFirst, Is.True, "Undo must enable on first selection");
                Assert.That(enabledAfterSecond, Is.True, "Undo must remain enabled on second selection");
                Assert.Pass($"Test passed: UndoButton state diagnostics\n{string.Join("\n", results)}");
            }
            catch (AssertionException)
            {
                Assert.Fail($"Test failed: UndoButton state diagnostics\n{string.Join("\n", results)}");
            }
        }



    }
}
