﻿using System.IO.Ports;
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

        private void OpenCloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isPortOpen)
            {
                // Check if a COM port is selected in the ComboBox
                if (ComPortComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port.");
                    return;
                }

                // Initialize and configure the serial port using the selected COM port
                serialPort = new SerialPort(ComPortComboBox.SelectedItem.ToString(), 9600, Parity.None, 8, StopBits.One);

                try
                {
                    // Open the serial port
                    serialPort.Open();
                    isPortOpen = true;
                    OpenCloseButton.Content = "Close Serial Port";

                    // Attach an event handler for data received
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
                isPortOpen = false;
                OpenCloseButton.Content = "Open Serial Port";
            }
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Read data from the serial port
            string receivedData = serialPort.ReadLine();

            // Create a timestamp with the current time
            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            // Combine the timestamp and received data
            string messageWithTimestamp = $"{timestamp} - {receivedData}";

            // Update the message window with the received data
            Dispatcher.Invoke(() =>
            {
                MessageTextBox.AppendText(messageWithTimestamp);//+ Environment.NewLine);
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
