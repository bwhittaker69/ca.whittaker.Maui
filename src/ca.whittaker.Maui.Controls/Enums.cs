﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls
{

    public enum SiginButtonTypeEnum { Generic, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple }
    public enum ImageResourceEnum { 
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
    public enum TextBoxFieldTypeEnum { Text, Email, Url, Chat, Username }
    public enum FieldAccessModeEnum { Editable, ReadOnly, Hidden }
    public enum ButtonStateEnum { Enabled, Disabled, Pressed, Hidden }
    public enum ChangeStateEnum { Changed, NotChanged }
    public enum ValidationStateEnum { Valid, FormatError, RequiredFieldError }
    public enum FormAccessModeEnum { Editable, ViewOnly, Hidden }
    public enum FormFieldsStateEnum { Editing, ReadOnly, Hidden }
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
