using ca.whittaker.Maui.Controls;
using ca.whittaker.Maui.Controls.Buttons;
using System.Diagnostics;
namespace ca.whittaker.Maui.Controls.Demo.Views;

public class TestForm : ContentPage
{
    public UndoButton? undoButton;
    public Label? label;

    public TestForm()
    {
        undoButton = new UndoButton
        {
            ButtonSize = SizeEnum.Large,
            ButtonState = ButtonStateEnum.Enabled
        };
        //undoButton.Enabled();
        label = new Label
        {
            Text = "TestForm",
            TextColor = Colors.Red,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Create a Grid with two rows
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        // Place the label in the first row and the button in the second row
        grid.Children.Add(label);
        Grid.SetRow(label, 0);

        grid.Children.Add(undoButton);
        Grid.SetRow(undoButton, 1);

        undoButton.Clicked += UndoButton_Clicked;

        Content = grid;
    }

    private void UndoButton_Clicked(object? sender, EventArgs e)
    {
        Debug.WriteLine("clicked");
    }
}
