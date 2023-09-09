using System.IO.Ports;
using System.Windows;
using System;

namespace SerialPortWpf
{
    public partial class MainWindow : Window
    {
        private SerialPort serialPort;
        private bool isPortOpen = false;
        public MainWindow()
        {
            InitializeComponent();
            // Populate the COM port ComboBox with available ports
            string[] availablePorts = SerialPort.GetPortNames();
            foreach (string port in availablePorts)
            {
                ComPortComboBox.Items.Add(port);
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null)
            {
                serialPort = new SerialPort();

                // Configure serial port settings
                serialPort.PortName = "COM14"; // Change to your desired port
                serialPort.BaudRate = 9600;   // Change to your desired baud rate
                serialPort.DataBits = 8;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;

                try
                {
                    // Open the serial port
                    serialPort.Open();
                    isPortOpen = true;
                    OpenButton.Content = "Close Serial Port";
                    serialPort.DataReceived += SerialPort_DataReceived;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening serial port: " + ex.Message);
                }
            }
            else
            {
                // Close the serial port
                serialPort.Close();
                isPortOpen= false;
                OpenButton.Content = "Open Serial Port";
                serialPort = null;
            }
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Read data from the serial port
            string receivedData = serialPort.ReadLine();

            // Update the message window with the received data
            Dispatcher.Invoke(() =>
            {
                MessageTextBox.AppendText(receivedData + Environment.NewLine);
                MessageTextBox.ScrollToEnd(); // Scroll to the end to show the latest message
            });
        }

        private void RefreshPortsButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the existing items in the ComboBox
            ComPortComboBox.Items.Clear();
            // Populate the ComboBox with available ports again
            string[] availablePorts = SerialPort.GetPortNames();
            foreach (string port in availablePorts)
            {
                ComPortComboBox.Items.Add(port);
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Ensure the serial port is closed when the window is closed
            if (isPortOpen)
            {
                serialPort.Close();
                isPortOpen= false;
            }
        }
    }
}
