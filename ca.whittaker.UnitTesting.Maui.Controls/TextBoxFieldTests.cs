using NUnit.Framework;
using System;
using System.Collections.Generic;
using ca.whittaker.Maui.Controls.Buttons;
using Microsoft.Maui.Controls;
using ca.whittaker.Maui.Controls;

namespace ca.whittaker.UnitTesting.Maui.Controls;

[TestFixture]
public class TextBoxFieldTests
{

    [Test]
    public void Test_TextBoxFieldTests()
    {
        // ARRANGE
        var buttonClasses = ca.whittaker.Maui.Controls.Buttons.TypeHelper.GetNonAbstractClasses();
        var assertions = new List<string>();

        // ACT
        foreach (var buttonClass in buttonClasses)
        {
            var button = (ButtonBase?)Activator.CreateInstance(buttonClass);
            button!.Parent = new ContentView();
            Assert.That(button, Is.Not.Null, $"Button class {buttonClass.Name} should not be null.");
            Assert.That(button.ButtonIcon, Is.Not.Null, $"Button class {buttonClass.Name} should have a ButtonIcon.");
            Assert.That(button.ButtonSize, Is.Not.Null, $"Button class {buttonClass.Name} should have a ButtonSize.");
            Assert.That(button.ButtonState, Is.Not.Null, $"Button class {buttonClass.Name} should have a ButtonState.");
            assertions.Add($"> PASS: {buttonClass.Name} ImageSource {nameof(buttonClass)}.");
        }
        Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
    }


    [Test]
    public void Test_Buttons_Disabled()
    {
        // ARRANGE
        var buttonClasses = ca.whittaker.Maui.Controls.Buttons.TypeHelper.GetNonAbstractClasses();
        var assertions = new List<string>();

        // ACT
        foreach (var buttonClass in buttonClasses)
        {
            var button = (ButtonBase?)Activator.CreateInstance(buttonClass);
            button!.Parent = new ContentView();
            button.ButtonState = ButtonStateEnum.Disabled;
            var disabledImageSource = button.ImageSource.IsEmpty;

            Assert.That(button, Is.Not.Null, $"Button class {buttonClass.Name} should not be null.");
            Assert.That(disabledImageSource, Is.False, $"Button class {buttonClass.Name} should have a Disabled ImageSource.");
            Assert.That(button.ButtonIcon, Is.Not.Null, $"Button class {buttonClass.Name} should have a ButtonIcon.");
            Assert.That(button.ButtonSize, Is.Not.Null, $"Button class {buttonClass.Name} should have a ButtonSize.");
            Assert.That(button.ButtonState, Is.Not.Null, $"Button class {buttonClass.Name} should have a ButtonState.");
            assertions.Add($"> PASS: {buttonClass.Name} ImageSource {nameof(buttonClass)}.");
        }
        Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
    }


    [Test]
    public void Test_Button_ImageSource()
    {
        // ARRANGE
        var assertions = new List<string>();

        // ACT
        var cancelButton = new CancelButton();
        cancelButton.Parent = new ContentView();

        // ASSERT
        Assert.That(cancelButton.ImageSource, Is.Not.Null, "ImageSource should have a value");
        Assert.That(cancelButton.ImageSource.IsEmpty, Is.False, "ImageSource should not be empty");
        Assert.That(cancelButton.ButtonIcon, Is.EqualTo(ButtonIconEnum.Cancel), "CancelButton should set ButtonIcon to Cancel.");
        assertions.Add($"> PASS: CancelButton ImageSource {cancelButton.ButtonIcon}.");
        Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
    }



    #region Constructor Tests
    [Test]
    public void Test_Button_Constructor_Sets_ButtonIconToCancel()
    {
        // ARRANGE
        var assertions = new List<string>();

        // ACT
        var cancelButton = new CancelButton();

        // ASSERT
        Assert.That(cancelButton.ButtonIcon, Is.EqualTo(ButtonIconEnum.Cancel), "CancelButton should set ButtonIcon to Cancel.");
        assertions.Add($"> PASS: CancelButton constructor sets ButtonIcon to {cancelButton.ButtonIcon}.");
        Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
    }
    #endregion

    #region State Methods Tests
    [Test]
    public void Test_Enabled_Method_Sets_StateAndUIProperties()
    {
        // ARRANGE
        var assertions = new List<string>();
        var button = new CancelButton
        {
            // Assume a valid size and style for testing UI logic.
            ButtonSize = SizeEnum.Normal,
            ButtonStyle = ButtonStyleEnum.IconAndText,
            Text = "Cancel"
        };

        // ACT
        // First, set to a different state.
        button.Disabled();
        button.Enabled();

        // ASSERT
        Assert.That(button.ButtonState, Is.EqualTo(ButtonStateEnum.Enabled), "Enabled() should set ButtonState to Enabled.");
        assertions.Add("> PASS: Enabled() set ButtonState to Enabled.");
        Assert.That(button.IsEnabled, Is.True, "Enabled() should set IsEnabled to true.");
        assertions.Add("> PASS: Enabled() set IsEnabled to true.");
        Assert.That(button.IsVisible, Is.True, "Enabled() should set IsVisible to true.");
        assertions.Add("> PASS: Enabled() set IsVisible to true.");

        Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
    }

    [Test]
    public void Test_Disabled_Method_Sets_StateAndUIProperties()
    {
        // ARRANGE
        var assertions = new List<string>();
        var button = new CancelButton
        {
            ButtonSize = SizeEnum.Normal,
            ButtonStyle = ButtonStyleEnum.IconAndText,
            Text = "Cancel"
        };

        // ACT
        button.Disabled();

        // ASSERT
        Assert.That(button.ButtonState, Is.EqualTo(ButtonStateEnum.Disabled), "Disabled() should set ButtonState to Disabled.");
        assertions.Add("> PASS: Disabled() set ButtonState to Disabled.");
        Assert.That(button.IsEnabled, Is.False, "Disabled() should set IsEnabled to false.");
        assertions.Add("> PASS: Disabled() set IsEnabled to false.");
        Assert.That(button.IsVisible, Is.True, "Disabled() should leave IsVisible as true.");
        assertions.Add("> PASS: Disabled() left IsVisible as true.");

        Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
    }

    [Test]
    public void Test_Hide_Method_Sets_StateAndUIProperties()
    {
        // ARRANGE
        var assertions = new List<string>();
        var button = new CancelButton();

        // ACT
        button.Hide();

        // ASSERT
        Assert.That(button.ButtonState, Is.EqualTo(ButtonStateEnum.Hidden), "Hide() should set ButtonState to Hidden.");
        assertions.Add("> PASS: Hide() set ButtonState to Hidden.");
        Assert.That(button.IsVisible, Is.False, "Hide() should set IsVisible to false.");
        assertions.Add("> PASS: Hide() set IsVisible to false.");

        Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
    }
    #endregion

    #region UpdateUI Logic Tests
    [Test]
    public void Test_UpdateUI_ConfiguresEnabledProperly()
    {
        // ARRANGE
        var assertions = new List<string>();
        var button = new CancelButton
        {
            ButtonSize = SizeEnum.Normal,
            ButtonStyle = ButtonStyleEnum.IconAndText,
            Text = "Cancel"
        };

        // Set initial state to Disabled then switch to Enabled
        button.Disabled();
        button.Enabled();

        // ACT
        button.UpdateUI();

        // ASSERT - Check that UI-related properties are set as expected.
        Assert.That(button.IsEnabled, Is.True, "After UpdateUI in Enabled state, IsEnabled should be true.");
        assertions.Add("> PASS: UpdateUI in Enabled state sets IsEnabled to true.");
        Assert.That(button.IsVisible, Is.True, "After UpdateUI in Enabled state, IsVisible should be true.");
        assertions.Add("> PASS: UpdateUI in Enabled state sets IsVisible to true.");
        // Additional assertions might check if Text and ImageSource are set based on ButtonStyle.
        Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
    }
    #endregion
}
