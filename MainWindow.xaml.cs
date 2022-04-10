using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace eMeetVolumeControl
{

    /// <summary>
    /// method: SEND RECEIVE
    /// target: byte[0]
    /// </summary>
    public enum ReportID
    {
        PRIMARY = 0x07
    }

    /// <summary>
    /// method: SEND RECEIVE
    /// target: byte[1]
    /// </summary>
    public enum Setting
    {
        DEVICE = 0x02,
        INPUT = 0x09,
    }

    /// <summary>
    /// method: SEND RECEIVE
    /// target: byte[2]
    /// </summary>
    public enum ControlMode
    {
        BUTTON_MODE_GET = 0x01,
        BUTTON_MODE_SET = 0x02,
        BUTTON_POST = 0x03,
        BUTTON_GET = 0x06,
        BUTTON_SET = 0x07,
        LED_RED_SET = 0x08,
        VERSION_GET = 0x09,
        EMEET_ROOM_GET = 0x0a,
        EMEET_ROOM_SET = 0x0b,
        LED_GET = 0x0c,
        LED_SET = 0x0d,
        FACTORY_RESSET = 0x14
    }

    /// <summary>
    /// method: SEND RECEIVE
    /// 
    /// ControlMode.BUTTON_GET
    /// ControlMode.BUTTON_SET
    /// target: byte[7]
    /// 
    /// ControlMode.LED_GET
    /// ControlMode.LED_SET
    /// target: byte[8]
    /// </summary>
    public enum FunctionToggle
    {
        ON = 0x01,
        OFF = 0x00
    }

    /// <summary>
    /// method: SEND
    /// ControlMode.LED_RED_SET
    /// target: byte[8]
    /// topic : FunctionButtonPush.UNMUTE is sent when FunctionRedLed.ON
    /// </summary>
    public enum FunctionRedLed
    {
        ON = 0x2f,
        OFF = 0x30
    }

    /// <summary> 
    /// method: SEND RECEIVE
    /// ControlMode.BUTTON_MODE_SET
    /// ControlMode.BUTTON_MODE_GET
    /// ControlMode.EMEET_ROOM_GET
    /// ControlMode.EMEET_ROOM_SET
    /// target: byte[8] or byte[10]
    /// </summary>
    public enum FunctionDial
    {
        VOLUME_CONTROL = 0x00,
        NEXT_PREV = 0x01
    }

    /// <summary>
    /// method: SEND RECEIVE
    /// ControlMode.BUTTON_MODE_SET
    /// ControlMode.BUTTON_MODE_GET
    /// target: byte[7]
    /// 
    /// ControlMode.EMEET_ROOM_GET
    /// ControlMode.EMEET_ROOM_SET
    /// topic : byte[7] must always be set to CONFERENCE
    /// </summary>
    public enum FunctionDialMode
    {
        CONFERENCE = 0x00,
        MUSIC = 0x01,
        CUSTOM = 0x02
    }

    /// <summary>
    /// method: RECEIVE
    /// ControlMode.BUTTON_POST
    /// target: byte[8]
    /// topic: Varies depending on FunctionRedLed.
    /// </summary>
    public enum FunctionButtonPushRecive
    {
        MUTE = 0x47,
        UNMUTE = 0x48
    }

    /// <summary>
    /// method: SEND RECEIVE
    /// ControlMode.BUTTON_MODE_GET
    /// ControlMode.BUTTON_MODE_SET
    /// target: byte[9]
    /// </summary>
    public enum FunctionButtonPush
    {
        SPEAKER = 0x02,
        MIC = 0x03,
        PLAY_PAUSE = 0x04,
    }

    /// <summary>
    /// method: SEND RECEIVE
    /// ControlMode.EMEET_ROOM_GET
    /// ControlMode.EMEET_ROOM_SET
    /// target: byte[8],byte[9] or byte[10] 
    /// </summary>
    public enum FunctionButtonPushEmeet
    {
        SPEAKER = 0x02,
        MIC = 0x03,
        PLAY_PAUSE = 0x04,
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private IEnumerable<HidDevice> HidDeviceList = HidDevices.Enumerate(0x328F, 0x0021);
        private bool readstate;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DeviceLoadButton_Click(object sender, RoutedEventArgs e)
        {
            HidDeviceList = HidDevices.Enumerate(0x328F, 0x0021);
            DeviceLabel.Content = "見つかりません";

            if (HidDeviceList.Count() > 0)
            {
                DeviceLabel.Content = "見つかった子デバイス : " + HidDeviceList.Count();
            }
        }

        private async void DialLoadButton_Click(object sender, RoutedEventArgs e)
        {
            readstate = true;
            DialLabel.Content = HidDeviceList.ElementAt(0).Description;
            int count = 0;
            while (readstate)
            {
                HidReport read = await HidDeviceList.ElementAt(1).ReadReportAsync(5);
                byte raw = read.Data[0];
                if (raw == 233)
                {
                    count++;
                }
                else
                {
                    count--;
                }
                DialLabel.Content = count;
            }
            DialLabel.Content = "停止";
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            readstate = false;
        }

        private void LedCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (LedCheckBox.IsChecked == null) return;
            byte[] data = new byte[32];
            data[0] = (byte)ReportID.PRIMARY;
            data[1] = (byte)Setting.DEVICE;
            data[2] = (byte)ControlMode.LED_SET;

            if ((bool)LedCheckBox.IsChecked)
            {
                data[8] = (byte)FunctionToggle.ON;
            }
            WriteAsync(1, data);
        }

        private void RedCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (RedCheckBox.IsChecked == null) return;
            byte[] data = new byte[32];
            data[0] = (byte)ReportID.PRIMARY;
            data[1] = (byte)Setting.INPUT;
            data[2] = (byte)ControlMode.LED_RED_SET;
            data[8] = (byte)FunctionRedLed.OFF;

            if ((bool)RedCheckBox.IsChecked)
            {
                data[8] = (byte)FunctionRedLed.ON;
            }
            WriteAsync(1, data);
        }

        private void DisableCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (DisableCheckBox.IsChecked == null) return;
            byte[] data = new byte[32];
            data[0] = (byte)ReportID.PRIMARY;
            data[1] = (byte)Setting.INPUT;
            data[2] = (byte)ControlMode.BUTTON_SET;
            data[7] = (byte)FunctionToggle.ON;

            if ((bool)DisableCheckBox.IsChecked)
            {
                data[7] = (byte)FunctionToggle.OFF;
            }
            WriteAsync(1, data);
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[32];
            data[0] = (byte)ReportID.PRIMARY;
            data[1] = Convert.ToByte(one.Text, 16);
            data[2] = Convert.ToByte(two.Text, 16);
            data[5] = Convert.ToByte(three.Text, 16);
            data[6] = Convert.ToByte(fore.Text, 16);
            data[7] = Convert.ToByte(five.Text, 16);
            data[8] = Convert.ToByte(six.Text, 16);
            data[9] = Convert.ToByte(seven.Text, 16);
            data[10] = Convert.ToByte(eight.Text, 16);

            WriteAsync(1, data);
        }

        private void SettingLoadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SettingSaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FactoryResetButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[32];
            data[0] = (byte)ReportID.PRIMARY;
            data[1] = (byte)Setting.DEVICE;
            data[2] = (byte)ControlMode.FACTORY_RESSET;
            WriteAsync(1, data);
        }

        private async void WriteAsync(int deviceindex, byte[] data)
        {
            try
            {
                if (!await HidDeviceList.ElementAt(deviceindex).WriteAsync(data))
                {
                    _ = MessageBox.Show("失敗");
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                _ = MessageBox.Show("デバイスが見つからない為失敗しました");
            }
        }
    }
}