using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls
{

    public enum SiginButtonTypeEnum { Generic, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple }
    public enum ButtonIconEnum { 
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
        Replace2,
        Save, 
        Signin, 
        Signout, 
        Tiktok, 
        Undo, 
        Zoom_In, 
        Zoom_Out
    }

    public enum TextBoxDataTypeEnum { Plaintext, Email, Url, Richtext, Username, Integer, Numeric, Currency }
    public enum EditorDataTypeEnum { Plaintext, Richtext }
    public enum FieldAccessModeEnum { Editable, Editing, ViewOnly, Hidden }
    public enum ButtonStateEnum { Enabled, Disabled, Pressed, Hidden }
    public enum ChangeStateEnum { Changed, NotChanged }
    public enum ValidationStateEnum { Valid, FormatError, RequiredFieldError }
    public enum FormAccessModeEnum { Editable, Editing, ViewOnly, Hidden }
    public enum ButtonStyleEnum { IconOnly, IconAndText, TextOnly }
    public enum SizeEnum
    {
        XXXSmall = -20,
        XXSmall = -15,
        XSmall = -10,
        Small = -5,
        Normal = 0,
        Large = 5,
        XLarge= 10,
        XXLarge = 15
    }
}
