using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls
{
    public enum SiginButtonTypeEnum { Generic, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple }
    public enum RegisterButtonTypeEnum { Generic, Email, SMS }

    /// <summary>
    /// Defines the icon to be displayed on a button (e.g., Save, Edit, Cancel, etc.).<br/>
    /// [Form Only]
    /// </summary>
    public enum ButtonIconEnum
    {
        Alarmclock,
        Alarmclock_Stop,
        Apple,
        Bookmark,
        Bookmark_Delete,
        Bullet_Ball_Glass_Green,
        Bullet_Ball_Red,
        Camera,
        Cancel,
        Edit,
        Facebook,
        Google,
        Linkedin,
        Media_Pause,
        Media_Play,
        Media_Play_Green,
        Media_Stop,
        Media_Stop_Red,
        Microsoft,
        Phone_Flip,
        Record_Circle,
        Refresh,
        Register,
        Replace2,
        Save,
        Signin,
        Signout,
        Tiktok,
        Undo,
        Zoom_In,
        Zoom_Out
    }

    public enum FieldLabelLayoutEnum { Left, Top }

    public enum TextBoxDataTypeEnum { Plaintext, Email, Url, Richtext, Username, Integer, Numeric, Currency }

    public enum EditorDataTypeEnum { Plaintext, Richtext }

    /// <summary>
    /// Defines the access and interaction mode of a form or form field.<br/>
    /// [Shared: Form + Fields]<br/>
    /// <br/>
    /// <b>Mode meanings:</b><br/>
    /// - Editable: Form and/or field is visible and can be tapped/focused to enter editing mode.<br/>
    /// - Editing: Form and/or field is actively being edited (focused, writable).<br/>
    /// - ViewOnly: Form and/or field is visible but fully read-only; used for display/layout only.<br/>
    /// - Hidden: Form and/or field is not visible and not interactive.<br/>
    /// <br/>
    /// <b>Visibility / Focusability / Editability:</b><br/>
    /// | Mode     | Visible | Focusable | Editable          |<br/>
    /// |----------|---------|-----------|-------------------|<br/>
    /// | Editable | ✅      | ✅        | ✅ (after focus) |<br/>
    /// | Editing  | ✅      | ✅        | ✅               |<br/>
    /// | ViewOnly | ✅      | ❌        | ❌               |<br/>
    /// | Hidden   | ❌      | ❌        | ❌               |<br/>
    /// </summary>
    public enum FieldAccessModeEnum { Editable, Editing, ViewOnly, Hidden }

    /// <summary>
    /// Represents the visual state of a button (visibility and whether it can be interacted with).<br/>
    /// </summary>
    public enum ButtonStateEnum { Enabled, Disabled, Pressed, Hidden }

    /// <summary>
    /// Represents whether a form field's value has changed compared to its original state.<br/>
    /// [Shared: Form + Fields]
    /// </summary>
    public enum ChangeStateEnum { Changed, NotChanged }

    /// <summary>
    /// Represents the validation status of a form field's current value.<br/>
    /// [Shared: Form + Fields]
    /// </summary>
    public enum ValidationStateEnum { Valid, FormatError, RequiredFieldError }

    /// <summary>
    /// FormAccessModeEnum State Transition Map:<br/>
    /// [Form Only]<br/>
    /// <br/>
    /// Editable --(Edit button clicked)--> Editing<br/>
    /// Editing  --(Save button clicked)--> Editable<br/>
    /// Editing  --(Cancel button clicked)--> Editable<br/>
    /// Editable --(None)--> Editable<br/>
    /// ViewOnly --(None)--> ViewOnly<br/>
    /// Hidden   --(None)--> Hidden<br/>
    /// <br/>
    /// <b>UI Changes:</b><br/>
    /// - Editing shows Save + Cancel buttons, hides Edit button.<br/>
    /// - Editable shows Edit button only.<br/>
    /// - ViewOnly/Hidden hide all buttons.<br/>
    /// </summary>
    public enum FormAccessModeEnum { Editable, Editing, ViewOnly, Hidden }
    public enum ButtonSizeEnum
    {
        Size12 = 12,
        Size16 = 16,
        Size29 = 29,
        Size32 = 32,
        Size36 = 36,
        Size40 = 40,
        Size48 = 48,
        Size58 = 58,
        Size64 = 64,
        Size72 = 72,
        Size76 = 76,
        Size80 = 80,
        Size87 = 87,
        Size96 = 96,
        Size108 = 108,
        Size120 = 120,
        Size128 = 128
    }
    public enum SizeEnum
    {
        XXXSmall = -20,
        XXSmall = -15,
        XSmall = -10,
        Small = -5,
        Normal = 0,
        Large = 5,
        XLarge = 10,
        XXLarge = 15
    }
    public static class ButtonSizeEnumExtensions
    {
        public static int ToDip(this ButtonSizeEnum s) => DeviceHelper.GetDipSize(s);
        public static int ToPx(this ButtonSizeEnum s) => DeviceHelper.GetPixelSize(s);
    }

}
