using System.ComponentModel;
using System.Reflection;
using ca.whittaker.Maui.Controls.Forms;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace ca.whittaker.UnitTesting.Maui.Controls;

// Ensure a dummy Application for BindableProperty resolution
[SetUpFixture]
public class MauiTestInitializer
{
    [OneTimeSetUp]
    public void Init()
    {
        if (Application.Current == null)
            Application.Current = new Application();
    }
}

