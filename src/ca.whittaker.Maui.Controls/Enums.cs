using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ca.whittaker.Maui.Controls
{

    public enum SiginButtonTypeEnum { Generic, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple }
    public enum BaseButtonTypeEnum { Signin, Signout, Save, Edit, Cancel, Facebook, Linkedin, Google, Tiktok, Microsoft, Apple, Undo, Camera, Zoom_In, Zoom_Out, Refresh, Media_Stop, Media_Play_Green, Media_Play, Media_Pause, Bullet_Ball_Red }
    public enum FieldTypeEnum { Text, Email, Url, Chat, Username }
    public enum ButtonStateEnum { Enabled, Disabled, Pressed, Hidden }
    public enum ChangeStateEnum { Changed, NotChanged }
    public enum ValidationStateEnum { Valid, FormatError, RequiredFieldError }
    public enum FormStateEnum { Enabled, Disabled, Hidden, Undo, Saved, Clear, Initialize }
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
