using MauiReactor;
using MauiReactor.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiReactorWithFluxor.Resources.Styles;

class ApplicationTheme : Theme
{
    public static Color Primary { get; } = Color.FromRgba(81, 43, 212, 255); // #512BD4
    public static Color PrimaryDark { get; } = Color.FromRgba(172, 153, 234, 255); // #AC99EA
    public static Color PrimaryDarkText { get; } = Color.FromRgba(36, 36, 36, 255); // #242424
    public static Color Secondary { get; } = Color.FromRgba(223, 216, 247, 255); // #DFD8F7
    public static Color SecondaryDarkText { get; } = Color.FromRgba(152, 128, 229, 255); // #9880E5
    public static Color Tertiary { get; } = Color.FromRgba(43, 11, 152, 255); // #2B0B98

    public static Color White { get; } = Colors.White; // #FFFFFF
    public static Color Black { get; } = Colors.Black; // #000000
    public static Color Magenta { get; } = Color.FromRgba(214, 0, 170, 255); // #D600AA
    public static Color MidnightBlue { get; } = Color.FromRgba(25, 6, 73, 255); // #190649
    public static Color OffBlack { get; } = Color.FromRgba(31, 31, 31, 255); // #1F1F1F
    public static Color OffWhite { get; } = Color.FromRgba(241, 241, 241, 255); // #F1F1F1

    public static Color Gray100 { get; } = Color.FromRgba(225, 225, 225, 255); // #E1E1E1
    public static Color Gray200 { get; } = Color.FromRgba(200, 200, 200, 255); // #C8C8C8
    public static Color Gray300 { get; } = Color.FromRgba(172, 172, 172, 255); // #ACACAC
    public static Color Gray400 { get; } = Color.FromRgba(145, 145, 145, 255); // #919191
    public static Color Gray500 { get; } = Color.FromRgba(110, 110, 110, 255); // #6E6E6E
    public static Color Gray600 { get; } = Color.FromRgba(64, 64, 64, 255); // #404040
    public static Color Gray900 { get; } = Color.FromRgba(33, 33, 33, 255); // #212121
    public static Color Gray950 { get; } = Color.FromRgba(20, 20, 20, 255); // #141414

    protected override void OnApply()
    {
        ActivityIndicatorStyles.Default = _ =>
            _.Color(IsLightTheme ? Primary : White);

        IndicatorViewStyles.Default = _ => _
            .IndicatorColor(IsLightTheme ? Gray200 : Gray500)
            .SelectedIndicatorColor(IsLightTheme ? Gray950 : Gray100);

        BorderStyles.Default = _ => _
            .Stroke(IsLightTheme ? Gray200 : Gray500)
            .StrokeShape(new Rectangle())
            .StrokeThickness(1);


        BoxViewStyles.Default = _ => _
            .BackgroundColor(IsLightTheme ? Gray950 : Gray200);

        ButtonStyles.Default = _ => _
            .TextColor(IsLightTheme ? White : PrimaryDarkText)
            .BackgroundColor(IsLightTheme ? Primary : PrimaryDark)
            .FontFamily("OpenSansRegular")
            .FontSize(14)
            .BorderWidth(0)
            .CornerRadius(8)
            .Padding(14, 10)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disable", MauiControls.Button.TextColorProperty, IsLightTheme ? Gray950 : Gray200)
            .VisualState("CommonStates", "Disable", MauiControls.Button.BackgroundColorProperty, IsLightTheme ? Gray200 : Gray600);

        CheckBoxStyles.Default = _ => _
            .Color(IsLightTheme ? Primary : White)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disable", MauiControls.CheckBox.ColorProperty, IsLightTheme ? Gray300 : Gray600);

        DatePickerStyles.Default = _ => _
            .TextColor(IsLightTheme ? Gray900 : White)
            .BackgroundColor(Colors.Transparent)
            .FontFamily("OpenSansRegular")
            .FontSize(14)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disable", MauiControls.DatePicker.TextColorProperty, IsLightTheme ? Gray200 : Gray500);

        EditorStyles.Default = _ => _
            .TextColor(IsLightTheme ? Black : White)
            .BackgroundColor(Colors.Transparent)
            .FontFamily("OpenSansRegular")
            .FontSize(14)
            .PlaceholderColor(IsLightTheme ? Gray200 : Gray500)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disable", MauiControls.Editor.TextColorProperty, IsLightTheme ? Gray300 : Gray600);


        EntryStyles.Default = _ => _
            .TextColor(IsLightTheme ? Black : White)
            .BackgroundColor(Colors.Transparent)
            .FontFamily("OpenSansRegular")
            .FontSize(14)
            .PlaceholderColor(IsLightTheme ? Gray200 : Gray500)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disable", MauiControls.Entry.TextColorProperty, IsLightTheme ? Gray300 : Gray600);

        ImageButtonStyles.Default = _ => _
            .Opacity(1)
            .BorderColor(Colors.Transparent)
            .BorderWidth(0)
            .CornerRadius(0)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disable", MauiControls.ImageButton.OpacityProperty, 0.5);

        LabelStyles.Default = _ => _
            .TextColor(IsLightTheme ? Black : White)
            .BackgroundColor(Colors.Transparent)
            .FontFamily("OpenSansRegular")
            .FontSize(14)
            .VisualState("CommonStates", "Disable", MauiControls.Label.TextColorProperty, IsLightTheme ? Gray300 : Gray600);

        LabelStyles.Themes["Headline"] = _ => _
            .TextColor(IsLightTheme ? MidnightBlue : White)
            .FontSize(32)
            .HorizontalOptions(MauiControls.LayoutOptions.Center)
            .HorizontalTextAlignment(TextAlignment.Center);

        LabelStyles.Themes["SubHeadline"] = _ => _
            .TextColor(IsLightTheme ? MidnightBlue : White)
            .FontSize(24)
            .HorizontalOptions(MauiControls.LayoutOptions.Center)
            .HorizontalTextAlignment(TextAlignment.Center);

        PickerStyles.Default = _ => _
            .TextColor(IsLightTheme ? Gray900 : White)
            .TitleColor(IsLightTheme ? Gray900 : Gray200)
            .BackgroundColor(Colors.Transparent)
            .FontFamily("OpenSansRegular")
            .FontSize(14)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disable", MauiControls.Picker.TextColorProperty, IsLightTheme ? Gray300 : Gray600)
            .VisualState("CommonStates", "Disable", MauiControls.Picker.TitleColorProperty, IsLightTheme ? Gray300 : Gray600);

        ProgressBarStyles.Default = _ => _
            .ProgressColor(IsLightTheme ? Primary : White)
            .VisualState("CommonStates", "Disable", MauiControls.ProgressBar.ProgressColorProperty, IsLightTheme ? Gray300 : Gray600);

        RadioButtonStyles.Default = _ => _
            .BackgroundColor(Colors.Transparent)
            .TextColor(IsLightTheme ? Black : White)
            .FontFamily("OpenSansRegular")
            .FontSize(14)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disable", MauiControls.RadioButton.TextColorProperty, IsLightTheme ? Gray300 : Gray600);

        RefreshViewStyles.Default = _ => _
            .RefreshColor(IsLightTheme ? Gray900 : Gray200);

        SearchBarStyles.Default = _ => _
            .TextColor(IsLightTheme ? Gray900 : White)
            .PlaceholderColor(Gray500)
            .CancelButtonColor(Gray500)
            .BackgroundColor(Colors.Transparent)
            .FontFamily("OpenSansRegular")
            .FontSize(14)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disable", MauiControls.SearchBar.TextColorProperty, IsLightTheme ? Gray300 : Gray600)
            .VisualState("CommonStates", "Disable", MauiControls.SearchBar.PlaceholderColorProperty, IsLightTheme ? Gray300 : Gray600);

        //SearchHandlerStyles.Default = _ => _
        //    .TextColor(IsLightTheme ? Gray900 : White)
        //    .PlaceholderColor(Gray500)
        //    .BackgroundColor(Colors.Transparent)
        //    .FontFamily("OpenSansRegular")
        //    .FontSize(14)
        //    .VisualState("CommonStates", "Disable", MauiControls.SearchHandler.TextColorProperty, IsLightTheme ? Gray300 : Gray600)
        //    .VisualState("CommonStates", "Disable", MauiControls.SearchHandler.PlaceholderColorProperty, IsLightTheme ? Gray300 : Gray600);

        ShadowStyles.Default = _ => _
            .Radius(15)
            .Opacity(0.5f)
            .Brush(IsLightTheme ? White : White)
            .Offset(new Point(10, 10));

        SliderStyles.Default = _ => _
            .MinimumTrackColor(IsLightTheme ? Primary : White)
            .MaximumTrackColor(IsLightTheme ? Gray200 : Gray600)
            .ThumbColor(IsLightTheme ? Primary : White)
            .VisualState("CommonStates", "Disable", MauiControls.Slider.MinimumTrackColorProperty, IsLightTheme ? Gray300 : Gray600)
            .VisualState("CommonStates", "Disable", MauiControls.Slider.MaximumTrackColorProperty, IsLightTheme ? Gray300 : Gray600)
            .VisualState("CommonStates", "Disable", MauiControls.Slider.ThumbColorProperty, IsLightTheme ? Gray300 : Gray600);

        SwipeItemStyles.Default = _ => _
            .BackgroundColor(IsLightTheme ? White : Black);

        SwitchStyles.Default = _ => _
            .OnColor(IsLightTheme ? Primary : White)
            .ThumbColor(White)
            .VisualState("CommonStates", "Disable", MauiControls.Switch.OnColorProperty, IsLightTheme ? Gray300 : Gray600)
            .VisualState("CommonStates", "Disable", MauiControls.Switch.ThumbColorProperty, IsLightTheme ? Gray300 : Gray600)
            .VisualState("CommonStates", "On", MauiControls.Switch.OnColorProperty, IsLightTheme ? Secondary : Gray200)
            .VisualState("CommonStates", "On", MauiControls.Switch.ThumbColorProperty, IsLightTheme ? Primary : White)
            .VisualState("CommonStates", "Off", MauiControls.Switch.ThumbColorProperty, IsLightTheme ? Gray400 : Gray500);


        TimePickerStyles.Default = _ => _
            .TextColor(IsLightTheme ? Gray900 : White)
            .BackgroundColor(Colors.Transparent)
            .FontFamily("OpenSansRegular")
            .FontSize(14)
            .MinimumHeightRequest(44)
            .MinimumWidthRequest(44)
            .VisualState("CommonStates", "Disable", MauiControls.TimePicker.TextColorProperty, IsLightTheme ? Gray300 : Gray600);

        TitleBarStyles.Default = _ => _
            .MinimumHeightRequest(32)
            .VisualState("TitleActiveStates", "TitleBarTitleActive", MauiControls.TitleBar.BackgroundColorProperty, Colors.Transparent)
            .VisualState("TitleActiveStates", "TitleBarTitleActive", MauiControls.TitleBar.ForegroundColorProperty, IsLightTheme ? Black : White)
            .VisualState("TitleActiveStates", "TitleBarTitleInactive", MauiControls.TitleBar.BackgroundColorProperty, IsLightTheme ? White : Black)
            .VisualState("TitleActiveStates", "TitleBarTitleInactive", MauiControls.TitleBar.ForegroundColorProperty, IsLightTheme ? Gray400 : Gray500);

        PageStyles.Default = _ => _
            .Padding(0)
            .BackgroundColor(IsLightTheme ? White : OffBlack);

        ShellStyles.Default = _ => _
            .Set(MauiControls.Shell.BackgroundColorProperty, IsLightTheme ? White : OffBlack)
            .Set(MauiControls.Shell.ForegroundColorProperty, IsLightTheme ? Black : SecondaryDarkText)
            .Set(MauiControls.Shell.TitleColorProperty, IsLightTheme ? Black : SecondaryDarkText)
            .Set(MauiControls.Shell.DisabledColorProperty, IsLightTheme ? Gray200 : Gray950)
            .Set(MauiControls.Shell.UnselectedColorProperty, IsLightTheme ? Gray200 : Gray200)
            .Set(MauiControls.Shell.NavBarHasShadowProperty, false)
            .Set(MauiControls.Shell.TabBarBackgroundColorProperty, IsLightTheme ? White : Black)
            .Set(MauiControls.Shell.TabBarForegroundColorProperty, IsLightTheme ? Magenta : White)
            .Set(MauiControls.Shell.TabBarTitleColorProperty, IsLightTheme ? Magenta : White)
            .Set(MauiControls.Shell.TabBarUnselectedColorProperty, IsLightTheme ? Gray900 : Gray200);

        NavigationPageStyles.Default = _ => _
            .Set(MauiControls.NavigationPage.BarBackgroundColorProperty, IsLightTheme ? White : OffBlack)
            .Set(MauiControls.NavigationPage.BarTextColorProperty, IsLightTheme ? Gray200 : White)
            .Set(MauiControls.NavigationPage.IconColorProperty, IsLightTheme ? Gray200 : White);

        TabbedPageStyles.Default = _ => _
            .Set(MauiControls.TabbedPage.BarBackgroundColorProperty, IsLightTheme ? White : Gray950)
            .Set(MauiControls.TabbedPage.BarTextColorProperty, IsLightTheme ? Magenta : White)
            .Set(MauiControls.TabbedPage.UnselectedTabColorProperty, IsLightTheme ? Gray200 : Gray950)
            .Set(MauiControls.TabbedPage.SelectedTabColorProperty, IsLightTheme ? Gray950 : Gray200);
    }
}
