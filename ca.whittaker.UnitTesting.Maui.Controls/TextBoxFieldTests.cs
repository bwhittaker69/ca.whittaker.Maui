using System.ComponentModel;
using System.Reflection;
using ca.whittaker.Maui.Controls.Forms;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace ca.whittaker.UnitTesting.Maui.Controls;


[TestFixture]
public partial class TextBoxFieldTests
{
    static Entry GetInnerEntry(TextBoxField fld)
    {
        var fi = typeof(TextBoxField)
                 .GetField("_textBox", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (Entry)fi.GetValue(fld)!;
    }

    static object InvokePrivate(object target, string name, params object?[] args)
    {
        var mi = target.GetType()
                       .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return mi.Invoke(target, args)!;
    }

    [Test]
    public void Defaults_Are_AsExpected()
    {
        // ARRANGE
        var fld = new TextBoxField();
        var results = new List<string>();

        // ACT
        var allLower = fld.TextBoxAllLowerCase;
        var allowWs = fld.TextBoxAllowWhiteSpace;
        var maxLen = fld.TextBoxMaxLength;
        var placeholder = fld.TextBoxPlaceholder;
        var entry = GetInnerEntry(fld);

        // ASSERT
        try
        {
            Assert.That(allLower, Is.False, "TextBoxAllLowerCase should default to false");
            results.Add("> PASS: TextBoxAllLowerCase is false");

            Assert.That(allowWs, Is.True, "TextBoxAllowWhiteSpace should default to true");
            results.Add("> PASS: TextBoxAllowWhiteSpace is true");

            Assert.That(maxLen, Is.EqualTo(255), "TextBoxMaxLength should default to 255");
            results.Add("> PASS: TextBoxMaxLength is 255");

            Assert.That(placeholder, Is.EqualTo(string.Empty), "TextBoxPlaceholder should default to empty");
            results.Add("> PASS: TextBoxPlaceholder is empty");

            Assert.That(entry.IsEnabled, Is.False, "Entry.IsEnabled should default to false");
            results.Add("> PASS: Entry.IsEnabled is false");

            Assert.That(entry.Placeholder, Is.EqualTo(string.Empty), "Entry.Placeholder should default to empty");
            results.Add("> PASS: Entry.Placeholder is empty");

            Assert.That(entry.MaxLength, Is.EqualTo(255), "Entry.MaxLength should default to 255");
            results.Add("> PASS: Entry.MaxLength is 255");

            Assert.Pass($"Test passed: Defaults_Are_AsExpected\n{string.Join("\n", results)}");
        }
        catch (AssertionException ex)
        {
            results.Add($"> FAIL: {ex.Message}");
            Assert.Fail($"Test failed: Defaults_Are_AsExpected\n{string.Join("\n", results)}");
        }
    }

    [Test]
    public void Setting_Placeholder_Updates_Entry()
    {
        // ARRANGE
        var fld = new TextBoxField();
        var results = new List<string>();

        // ACT
        fld.TextBoxPlaceholder = "Enter name";
        InvokePrivate(fld, "OnTextBoxPlaceholderPropertyChanged", fld.TextBoxPlaceholder);
        var entry = GetInnerEntry(fld);

        // ASSERT
        try
        {
            Assert.That(entry.Placeholder, Is.EqualTo("Enter name"), "Placeholder should update on property change");
            results.Add("> PASS: Placeholder updated to 'Enter name'");

            Assert.Pass($"Test passed: Setting_Placeholder_Updates_Entry\n{string.Join("\n", results)}");
        }
        catch (AssertionException ex)
        {
            results.Add($"> FAIL: {ex.Message}");
            Assert.Fail($"Test failed: Setting_Placeholder_Updates_Entry\n{string.Join("\n", results)}");
        }
    }

    [Test]
    public void Setting_MaxLength_Updates_Entry()
    {
        // ARRANGE
        var fld = new TextBoxField();
        var results = new List<string>();

        // ACT
        fld.TextBoxMaxLength = 10;
        InvokePrivate(fld, "OnTextBoxMaxLengthPropertyChanged", fld.TextBoxMaxLength);
        var entry = GetInnerEntry(fld);

        // ASSERT
        try
        {
            Assert.That(entry.MaxLength, Is.EqualTo(10), "MaxLength should update on property change");
            results.Add("> PASS: MaxLength updated to 10");

            Assert.Pass($"Test passed: Setting_MaxLength_Updates_Entry\n{string.Join("\n", results)}");
        }
        catch (AssertionException ex)
        {
            results.Add($"> FAIL: {ex.Message}");
            Assert.Fail($"Test failed: Setting_MaxLength_Updates_Entry\n{string.Join("\n", results)}");
        }
    }

    [Test]
    public void DataTypeProperty_Sets_Correct_Keyboard()
    {
        // ARRANGE
        var fld = new TextBoxField();
        var results = new List<string>();

        // ACT & ASSERT
        try
        {
            fld.TextBoxDataType = ca.whittaker.Maui.Controls.TextBoxDataTypeEnum.Email;
            InvokePrivate(fld, "OnTextBoxDataTypeChanged", ca.whittaker.Maui.Controls.TextBoxDataTypeEnum.Email);
            Assert.That(GetInnerEntry(fld).Keyboard, Is.EqualTo(Keyboard.Email), "Email type sets Email keyboard");
            results.Add("> PASS: Email keyboard set");

            fld.TextBoxDataType = ca.whittaker.Maui.Controls.TextBoxDataTypeEnum.Url;
            InvokePrivate(fld, "OnTextBoxDataTypeChanged", ca.whittaker.Maui.Controls.TextBoxDataTypeEnum.Url);
            Assert.That(GetInnerEntry(fld).Keyboard, Is.EqualTo(Keyboard.Url), "URL type sets Url keyboard");
            results.Add("> PASS: Url keyboard set");

            Assert.Pass($"Test passed: DataTypeProperty_Sets_Correct_Keyboard\n{string.Join("\n", results)}");
        }
        catch (AssertionException ex)
        {
            results.Add($"> FAIL: {ex.Message}");
            Assert.Fail($"Test failed: DataTypeProperty_Sets_Correct_Keyboard\n{string.Join("\n", results)}");
        }
    }

    [Test]
    public void ProcessAndSetText_Filters_Correctly()
    {
        // ARRANGE
        var fld = new TextBoxField
        {
            TextBoxAllLowerCase = true,
            TextBoxAllowWhiteSpace = true,
            TextBoxDataType = ca.whittaker.Maui.Controls.TextBoxDataTypeEnum.Plaintext
        };
        var results = new List<string>();

        // ACT
        InvokePrivate(fld, "TextBox_ProcessAndSetText", "Ab C!1");
        var current = (string)InvokePrivate(fld, "Field_GetCurrentValue")!;

        // ASSERT
        try
        {
            Assert.That(current, Is.EqualTo("abc!1"), "Processed text should be 'abc!1'");
            results.Add("> PASS: Processed text is 'abc!1'");

            Assert.Pass($"Test passed: ProcessAndSetText_Filters_Correctly\n{string.Join("\n", results)}");
        }
        catch (AssertionException ex)
        {
            results.Add($"> FAIL: {ex.Message}");
            Assert.Fail($"Test failed: ProcessAndSetText_Filters_Correctly\n{string.Join("\n", results)}");
        }
    }

    [Test]
    public void ReturnCommand_Executes_FieldCommand()
    {
        // ARRANGE
        var fld = new TextBoxField();
        bool invoked = false;
        fld.FieldCommand = new Command(_ => invoked = true);

        // ACT
        // invoke the private method via reflection, passing a null object parameter
        var mi = fld.GetType()
                    .GetMethod("TextBox_ReturnPressedCommand",
                               BindingFlags.Instance | BindingFlags.NonPublic)!;
        mi.Invoke(fld, new object[] { null! });

        // ASSERT
        Assert.That(invoked, Is.True, "ReturnCommand should invoke FieldCommand");
    }
    

    [Test]
    public void RequiredValidation_Works()
    {
        // ARRANGE
        var fld = new TextBoxField { FieldMandatory = true };
        var results = new List<string>();

        // ACT & ASSERT
        try
        {
            var hasReq = (bool)InvokePrivate(fld, "Field_HasRequiredError")!;
            Assert.That(hasReq, Is.True, "Mandatory field with no text should error");
            results.Add("> PASS: Required error on empty");

            InvokePrivate(fld, "Field_SetValue", "X");
            var noReq = (bool)InvokePrivate(fld, "Field_HasRequiredError")!;
            Assert.That(noReq, Is.False, "After setting value, no required error");
            results.Add("> PASS: No required error after value set");

            Assert.Pass($"Test passed: RequiredValidation_Works\n{string.Join("\n", results)}");
        }
        catch (AssertionException ex)
        {
            results.Add($"> FAIL: {ex.Message}");
            Assert.Fail($"Test failed: RequiredValidation_Works\n{string.Join("\n", results)}");
        }
    }

    [Test]
    public void FormatError_EmailValidation()
    {
        // ARRANGE
        var fld = new TextBoxField { TextBoxDataType = ca.whittaker.Maui.Controls.TextBoxDataTypeEnum.Email };
        var results = new List<string>();

        // ACT & ASSERT
        try
        {
            InvokePrivate(fld, "Field_SetValue", "not-an-email");
            var bad = (bool)InvokePrivate(fld, "Field_HasFormatError")!;
            Assert.That(bad, Is.True, "Invalid email should error");
            results.Add("> PASS: Invalid email detected");

            InvokePrivate(fld, "Field_SetValue", "me@example.com");
            var good = (bool)InvokePrivate(fld, "Field_HasFormatError")!;
            Assert.That(good, Is.False, "Valid email should not error");
            results.Add("> PASS: Valid email accepted");

            Assert.Pass($"Test passed: FormatError_EmailValidation\n{string.Join("\n", results)}");
        }
        catch (AssertionException ex)
        {
            results.Add($"> FAIL: {ex.Message}");
            Assert.Fail($"Test failed: FormatError_EmailValidation\n{string.Join("\n", results)}");
        }
    }
}
