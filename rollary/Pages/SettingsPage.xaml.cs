using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace rollary
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public readonly SettingsPageArgs Args;

        public bool Enabled { get; private set; }
        public bool Disabled => !Enabled; // for bindings

        private readonly ResultSettings originalSettings;
        private readonly ResultSettings bufferSettings;

        public SettingsPage(ResultSettings originalSettings, SettingsPageArgs args, bool enabled)
        {
            this.originalSettings = originalSettings;
            bufferSettings = originalSettings.Clone();
            BindingContext = bufferSettings;

            Args = args;

            Enabled = enabled;

            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            originalSettings.SafeTransferFrom(bufferSettings);
        }

        #region reversion buttons

        private void RevertChangesButton_Clicked(object sender, EventArgs eventArgs)
        {
            bufferSettings.TransferFrom(originalSettings);
        }

        private void RevertToDefaultsButton_Clicked(object sender, EventArgs eventArgs)
        {
            bufferSettings.TransferFrom(new ResultSettings());
        }

        #endregion
    }

    public class EntryValueConverter : IValueConverter
    {
        // NaN used to represent invalid value (eg, empty string). Otherwise would cause binding exceptions

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float trueValue = (float)value;

            return float.IsNaN(trueValue) ? "" : trueValue.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // `value` is a boxed string
            string entryValue = System.Convert.ToString(value);

            // Filter only the digits. Can't seem to limit this in XAML easily on UWP
            string entryValueNumeric = new string(entryValue.Where(character => char.IsDigit(character)).ToArray());

            if (int.TryParse(entryValueNumeric, out int newValue))
            {
                return newValue;
            }
            return float.NaN;
        }
    }

    #region FrameScanWidth converters

    public class ExponentialSliderValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object page, CultureInfo culture)
        {
            float trueValue = (float)value;

            var settingsPage = (SettingsPage)page;
            var args = settingsPage.Args;

            if (float.IsNaN(trueValue))
            {
                return settingsPage.Slider_FrameScanWidth.Value;
            }

            return (float)Math.Log(trueValue, args.ImageWidth);
        }

        public object ConvertBack(object value, Type targetType, object page, CultureInfo culture)
        {
            // `value` is a boxed double
            float sliderValue = System.Convert.ToSingle(value);
            var args = ((SettingsPage)page).Args;

            return (float)Math.Round(Math.Pow(args.ImageWidth, sliderValue));
        }
    }

    #endregion

    public struct SettingsPageArgs
    {
        public int ImageWidth;
        public int ImageHeight;
    }
}