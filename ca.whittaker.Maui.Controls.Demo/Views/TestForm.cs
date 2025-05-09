using ca.whittaker.Maui.Controls;
using ca.whittaker.Maui.Controls.Buttons;
using ca.whittaker.Maui.Controls.Demo.ViewModels;
using System.Diagnostics;
namespace ca.whittaker.Maui.Controls.Demo.Views;

public class TestForm : ContentPage
{
    public UndoButton? undoButton;
    public Label? label;
    public Entry? entry;
    public Picker? picker;
    public Button edit;
    public Button save;
    public Button cancel;
    public int selectedItem = 0;

    public class UserTestClass
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public UserTestClass(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }
    }
    List<UserTestClass?> users = new List<UserTestClass?> 
    {  
        null,
        new UserTestClass("brett"), 
        new UserTestClass("marina")
    };
    public TestForm()
    {


        undoButton = new UndoButton
        {
            ButtonSize = SizeEnum.Large,
            ButtonState = ButtonStateEnum.Enabled
        };

        label = new Label
        {
            Text = "TestForm",
            TextColor = Colors.Red,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        entry = new Entry
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Placeholder="entry control",
            TextColor= Colors.White
        };

        picker = new Picker
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            ItemsSource = users,
            Title = "picker",
            ItemDisplayBinding = new Binding(nameof(UserTestClass.Name)),
            SelectedItem = users[selectedItem]
        };

        // configure buttons to expand evenly
        edit = new Button { Text = "edit", VerticalOptions = LayoutOptions.Start, HeightRequest = -1 };
        save = new Button { Text = "save", VerticalOptions = LayoutOptions.Start, HeightRequest = -1 };
        cancel = new Button { Text = "cancel", VerticalOptions = LayoutOptions.Start, HeightRequest = -1 };

        cancel.Clicked += Cancel_Clicked;
        edit.Clicked += Edit_Clicked;
        save.Clicked += Save_Clicked;


        // ADD ALL THREE BUTTONS ONTO THIS LINE
        var horizontal = new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.Fill,
            Children = { edit, save, cancel }
        };

        // Create a Grid with two rows
        var grid = new Grid
        {
            RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                }
        };

        grid.Children.Add(label);
        Grid.SetRow(label, 0);

        grid.Children.Add(entry);
        Grid.SetRow(entry, 1);

        grid.Children.Add(picker);
        Grid.SetRow(picker, 2);

        grid.Children.Add(horizontal);
        Grid.SetRow(horizontal, 3);

        grid.Children.Add(undoButton);
        Grid.SetRow(undoButton, 4);



        undoButton.Clicked += UndoButton_Clicked;

        Content = grid;
    }

    private void Save_Clicked(object? sender, EventArgs e)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            if (picker != null)
            {
                picker.IsEnabled = false;
                selectedItem = picker.SelectedIndex;
            }
        });
    }

    private void Edit_Clicked(object? sender, EventArgs e)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            if (picker != null)
            {
                picker.IsEnabled = true;
                picker.SelectedIndex = selectedItem;
            }
        });
    }

    private void Cancel_Clicked(object? sender, EventArgs e)
    {
        UiThreadHelper.RunOnMainThread(() =>
        {
            if (picker != null)
            {
                picker.IsEnabled = false;
                picker.SelectedIndex = selectedItem;
            }
        });
    }

    private void UndoButton_Clicked(object? sender, EventArgs e)
    {
        Debug.WriteLine("clicked");
    }
}
