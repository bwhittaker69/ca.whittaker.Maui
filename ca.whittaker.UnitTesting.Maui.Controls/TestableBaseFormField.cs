using System.ComponentModel;
using System.Reflection;
using ca.whittaker.Maui.Controls;
using ca.whittaker.Maui.Controls.Forms;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace ca.whittaker.UnitTesting.Maui.Controls;

/// <summary>
/// Concrete Test‐only implementation of BaseFormField<string>
/// that stores its “control value” in a private field.
/// </summary>
public class TestableFieldBase : BaseFormField<string>
{
    private string? _testValue;

    // No real UI element needed for control‐view wiring
    protected override List<View> Field_ControlView()
        => new List<View>();

    // Base will call this to push a value into your “control”
    protected override void Field_SetValue(string? value)
    {
        _testValue = value;
    }

    // Base will call this to pull the current “UI” value back out
    protected override string? Field_GetCurrentValue()
        => _testValue;

    // Required‐error: only if marked mandatory and no value
    protected override bool Field_HasRequiredError()
        => FieldMandatory && string.IsNullOrEmpty(_testValue);

    // No format‐validation in this test stub
    protected override bool Field_HasFormatError()
        => false;

    // Unused by stub
    protected override string Field_GetFormatErrorMessage()
        => string.Empty;

    // Change‐detection against FieldLastValue
    protected override bool Field_HasChangedFromLast()
        => !Equals(FieldLastValue, _testValue);

    // Change‐detection against FieldOriginalValue
    protected override bool Field_HasChangedFromOriginal()
        => !Equals(FieldOriginalValue, _testValue);

    // Layout‐adjust stub (no‐op)
    protected override void UpdateRow0Layout()
    { }
}



[TestFixture]
public class TestableFieldBaseTests
{

    private TestableFieldBase _sut;


    [SetUp]
    public void Setup()
    {
        UiThreadHelper.SetRunOnMainThreadHandler(a => a());
        _sut = new TestableFieldBase();
        // simulate MAUI attaching the control
        InvokeProtected(_sut, "OnParentSet");
    }

    [TearDown]
    public void TearDown()
    {
        UiThreadHelper.SetRunOnMainThreadHandler(null);
    }

    // Helper to invoke protected/private methods via reflection, walking up the inheritance chain
    private static object InvokeProtected(object target, string methodName, params object?[]? args)
    {
        // find the MethodInfo on the target type or any base type
        Type? type = target.GetType();
        MethodInfo? mi = null;
        while (type != null)
        {
            mi = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            if (mi != null) break;
            type = type.BaseType;
        }
        if (mi == null)
            throw new InvalidOperationException($"No such method '{methodName}'");

        // match the method's declared parameter count
        var needed = mi.GetParameters().Length;
        object?[]? parameters;
        if (args == null)
        {
            // user passed null => create an array of nulls if needed, or null for zero-arg
            parameters = needed == 0 ? null : new object?[needed];
        }
        else if (args.Length == 0)
        {
            // empty array => interpret as zero args if the method takes none, otherwise supply nulls
            parameters = needed == 0 ? null : new object?[needed];
        }
        else
        {
            // args.Length > 0 => pass them directly
            parameters = args;
        }

        return mi.Invoke(target, parameters)!;
    }


    [Test]
    public void Field_SetValueAndGetCurrentValue_ReturnsSameString()
    {
        const string expected = "hello world";

        InvokeProtected(_sut, "Field_SetValue", expected);
        var actual = (string?)InvokeProtected(_sut, "Field_GetCurrentValue");

        Assert.That(actual, Is.EqualTo(expected),
            "Field_GetCurrentValue() should return exactly the value passed into Field_SetValue()");
    }

    [Test]
    public void Field_HasRequiredError_WhenMandatoryAndEmpty_ReturnsTrue()
    {
        _sut.FieldMandatory = true;
        InvokeProtected(_sut, "Field_SetValue", null);

        var result = (bool)InvokeProtected(_sut, "Field_HasRequiredError");

        Assert.That(result, Is.True);
    }

    [Test]
    public void Field_HasRequiredError_WhenNotMandatory_ReturnsFalse()
    {
        _sut.FieldMandatory = false;
        InvokeProtected(_sut, "Field_SetValue", null);

        var result = (bool)InvokeProtected(_sut, "Field_HasRequiredError");

        Assert.That(result, Is.False);
    }

    [Test]
    public void Field_HasFormatError_Always_ReturnsFalse()
    {
        var result = (bool)InvokeProtected(_sut, "Field_HasFormatError");

        Assert.That(result, Is.False);
    }

    [Test]
    public void Field_HasChangedFromLastAndOriginal_WhenValueChanged_ReturnsTrue()
    {
        _sut.FieldOriginalValue = "abc";
        _sut.FieldLastValue = "abc";

        InvokeProtected(_sut, "Field_SetValue", "def");
        var changedFromLast = (bool)InvokeProtected(_sut, "Field_HasChangedFromLast");
        var changedFromOriginal = (bool)InvokeProtected(_sut, "Field_HasChangedFromOriginal");

        Assert.Multiple(() =>
        {
            Assert.That(changedFromLast, Is.True, "Should detect change from last value");
            Assert.That(changedFromOriginal, Is.True, "Should detect change from original value");
        });
    }

    [Test]
    public void Field_HasChanged_WhenValueSame_ReturnsFalse()
    {
        _sut.FieldOriginalValue = "xyz";
        _sut.FieldLastValue = "xyz";

        InvokeProtected(_sut, "Field_SetValue", "xyz");
        var changedFromLast = (bool)InvokeProtected(_sut, "Field_HasChangedFromLast");
        var changedFromOriginal = (bool)InvokeProtected(_sut, "Field_HasChangedFromOriginal");

        Assert.Multiple(() =>
        {
            Assert.That(changedFromLast, Is.False, "No change from last");
            Assert.That(changedFromOriginal, Is.False, "No change from original");
        });
    }

    [Test]
    public void Field_Clear_ResetsOriginalAndCurrentValueToNull()
    {
        InvokeProtected(_sut, "Field_SetValue", "temp");
        _sut.Field_Clear();

        var current = (string?)InvokeProtected(_sut, "Field_GetCurrentValue");
        Assert.That(current, Is.Null);
        Assert.That(_sut.FieldOriginalValue, Is.Null);
    }

    [Test]
    public void Field_UndoValue_RestoresOriginalValue()
    {
        _sut.FieldOriginalValue = "orig";
        InvokeProtected(_sut, "Field_SetValue", "new");

        _sut.Field_UndoValue();

        var current = (string?)InvokeProtected(_sut, "Field_GetCurrentValue");
        Assert.That(current, Is.EqualTo("orig"));
    }

    [Test]
    public void Field_SaveAndMarkAsReadOnly_SetsOriginalAndViewOnly()
    {
        InvokeProtected(_sut, "Field_SetValue", "saved");
        _sut.Field_SaveAndMarkAsReadOnly();

        Assert.Multiple(() =>
        {
            Assert.That(_sut.FieldOriginalValue, Is.EqualTo("saved"));
            Assert.That(_sut.FieldAccessMode, Is.EqualTo(FieldAccessModeEnum.ViewOnly));
            Assert.That(_sut.FieldChangeState, Is.EqualTo(ChangeStateEnum.NotChanged));
        });
    }

    [Test]
    public void PressingUndoButton_RevertsToOriginalValue_AfterOnParentSet()
    {
        _sut.FieldOriginalValue = "initial";
        // Set initial both in data source and UI
        InvokeProtected(_sut, "Field_SetValue", "initial");
        _sut.FieldAccessMode = FieldAccessModeEnum.Editing;

        InvokeProtected(_sut, "Field_SetValue", "modified");
        Assert.That((string?)InvokeProtected(_sut, "Field_GetCurrentValue"), Is.EqualTo("modified"),
            "Sanity: UI should hold the modified value before undo.");

        // invoke the private undo-button handler
        InvokeProtected(_sut, "OnBaseFieldButtonUndoPressed", _sut.FieldButtonUndo, EventArgs.Empty);

        var reverted = (string?)InvokeProtected(_sut, "Field_GetCurrentValue");
        Assert.That(reverted, Is.EqualTo("initial"),
            "After undo, the field's UI value must revert to the original.");
    }

    [Test]
    public void OnParentSet_InitializesContentToGrid()
    {
        // Content should be set to the Grid returned by Field_CreateLayoutGrid()
        Assert.That(_sut.Content, Is.InstanceOf<Grid>());
    }

    [Test]
    public void OnParentSet_PushesDesignTimeDataSourceIntoUI()
    {
        // simulate setting a design-time binding before parent-set
        _sut.SetValue(BaseFormField<string>.FieldDataSourceProperty, "design");
        // reattach
        InvokeProtected(_sut, "OnParentSet");

        var uiValue = (string?)InvokeProtected(_sut, "Field_GetCurrentValue");
        Assert.That(uiValue, Is.EqualTo("design"));
    }

    [Test]
    public void NotificationLabel_ShowsRequired_WhenEditingAndMandatory()
    {
        _sut.FieldAccessMode = FieldAccessModeEnum.Editing;
        _sut.FieldMandatory = true;
        InvokeProtected(_sut, "Field_SetValue", null);
        InvokeProtected(_sut, "Field_UpdateNotificationMessage");

        // grab the protected FieldNotification Label
        var lbl = _sut.FieldNotification!;

        Assert.Multiple(() =>
        {
            Assert.That(lbl.Text, Is.EqualTo("Required"));
            Assert.That(lbl.IsVisible, Is.True);
        });
    }

    [Test]
    public void FieldHasChangesEvent_Fires_WhenValueDiffersFromOriginal()
    {
        bool? sawChange = null;
        _sut.FieldHasChanges += (_, e) => sawChange = e.HasChanged;

        // original = null; now change it
        InvokeProtected(_sut, "Field_SetValue", "new");
        // trigger validation/changed logic
        InvokeProtected(_sut, "Field_UpdateValidationAndChangedState");

        Assert.That(sawChange, Is.True);
    }

    [Test]
    public void FieldHasValidationChangesEvent_Fires_WhenRequiredErrorAppears()
    {
        bool? sawValid = null;
        _sut.FieldHasValidationChanges += (_, e) => sawValid = e.Invalid == false;

        _sut.FieldMandatory = true;
        InvokeProtected(_sut, "Field_SetValue", null);
        // force a validation‐state check
        InvokeProtected(_sut, "Field_UpdateValidationAndChangedState", true);

        Assert.That(sawValid, Is.False,
            "Required‐empty should flip IsValid to false");
    }

    [Test]
    public void FieldLabelText_UpdatesFieldLabelText()
    {
        _sut.FieldLabelText = "My Label";
        Assert.That(_sut.FieldLabel.Text, Is.EqualTo("My Label"));
    }

    [Test]
    public void FieldLabelVisible_TogglesFieldLabelVisibility()
    {
        _sut.FieldLabelVisible = false;
        Assert.That(_sut.FieldLabel.IsVisible, Is.False);

        _sut.FieldLabelVisible = true;
        Assert.That(_sut.FieldLabel.IsVisible, Is.True);
    }

    [Test]
    public void FieldUndoButton_TogglesUndoButtonVisibility()
    {
        _sut.FieldUndoButton = false;
        Assert.That(_sut.FieldButtonUndo.IsVisible, Is.False);

        _sut.FieldUndoButton = true;
        Assert.That(_sut.FieldButtonUndo.IsVisible, Is.True);
    }

    [Test]
    public void Field_UpdateNotificationMessage_ShowsRequired_WhenEditingAndMandatory()
    {
        _sut.FieldAccessMode = FieldAccessModeEnum.Editing;
        _sut.FieldMandatory = true;
        InvokeProtected(_sut, "Field_SetValue", null);
        InvokeProtected(_sut, "Field_UpdateNotificationMessage");

        Assert.Multiple(() =>
        {
            Assert.That(_sut.FieldNotification.Text, Is.EqualTo("Required"));   // :contentReference[oaicite:0]{index=0}&#8203;:contentReference[oaicite:1]{index=1}
            Assert.That(_sut.FieldNotification.IsVisible, Is.True);
        });
    }

    [Test]
    public void Field_UpdateLabelWidth_AdjustsGridAndLabelWidth()
    {
        _sut.Field_UpdateLabelWidth(42);
        var grid = (Grid)_sut.Content;
        Assert.That(grid.ColumnDefinitions[0].Width.Value, Is.EqualTo(42));          // :contentReference[oaicite:2]{index=2}&#8203;:contentReference[oaicite:3]{index=3}
        Assert.That(_sut.FieldLabel.WidthRequest, Is.EqualTo(42));
    }

    [Test]
    public void Field_UpdateWidth_AdjustsControlColumnWidth()
    {
        _sut.Field_UpdateWidth(77);
        var grid = (Grid)_sut.Content;
        Assert.That(grid.ColumnDefinitions[1].Width.Value, Is.EqualTo(77));          // :contentReference[oaicite:4]{index=4}&#8203;:contentReference[oaicite:5]{index=5}
    }

    [Test]
    public void FieldDataSource_SyncsOriginalAndUI_WhenBindingContextPresent()
    {
        _sut.BindingContext = new object();                                          // :contentReference[oaicite:6]{index=6}&#8203;:contentReference[oaicite:7]{index=7}
        _sut.FieldDataSource = "design-time";
        Assert.Multiple(() =>
        {
            Assert.That(_sut.FieldOriginalValue, Is.EqualTo("design-time"));
            Assert.That((string?)InvokeProtected(_sut, "Field_GetCurrentValue"), Is.EqualTo("design-time"));
        });
    }

}
