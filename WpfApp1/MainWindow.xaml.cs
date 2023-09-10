using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows;



namespace SerialPortWpf
{
    public partial class MainWindow : Window
    {
        private SerialPort serialPort;
        private bool isPortOpen = false;
        private StringBuilder logBuilder = new StringBuilder();


        public MainWindow()
        {
            InitializeComponent();
            // Populate the COM port ComboBox with available ports
            string[] availablePorts = SerialPort.GetPortNames();
            foreach (string port in availablePorts)
            {
                ComPortComboBox.Items.Add(port);
            }
            // Add baud rate items dynamically to the ComboBox
            int[] baudRates = { 9600, 11400, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
            foreach (int baudRate in baudRates)
            {
                BaudRateComboBox.Items.Add(baudRate.ToString());
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

                // Attempt to convert the selected baud rate to an integer
                if (int.TryParse(BaudRateComboBox.SelectedItem.ToString(), out int selectedBaudRate))
                {
                    // Initialize and configure the serial port using the selected COM port and baud rate
                    serialPort = new SerialPort(ComPortComboBox.SelectedItem.ToString(), selectedBaudRate, Parity.None, 8, StopBits.One);

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
                    MessageBox.Show("Please select a valid baud rate.");
                    Console.WriteLine(BaudRateComboBox.SelectedItem);
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

            // Combine the timestamp and received data and comma for .csv file
            String messageWithcomma = $"{timestamp} , {receivedData}";
            logBuilder.AppendLine(messageWithcomma);

            if (receivedData.Length > 1)
            {
                receivedData = receivedData.Substring(0);
            }
            else
            {
                // Handle cases where the received data is shorter than 5 characters
                // You can decide what to do in such cases, e.g., skip the data or display an error message.
                // For now, we'll just skip the data.
                return;
            }

            // Update the message window with the received data
            Dispatcher.Invoke(() =>
            {
                MessageTextBox.AppendText(messageWithTimestamp);//+ Environment.NewLine);
                MessageTextBox.ScrollToEnd(); // Scroll to the end to show the latest message
            });

        }
        private void SaveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logText = logBuilder.ToString();

                if (!string.IsNullOrEmpty(logText))
                {
                    // Specify the directory path
                    string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "logs", "SerialPortWpf");

                    // Ensure the directory exists, create it if not
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    string fileName = $"SerialLog_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv";
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "logs", "SerialPortWpf", fileName);

                    // Add the first line with time and voltage
                    string firstLine = $"Time,Voltage";
                    logText = firstLine + Environment.NewLine + logText;

                    File.WriteAllText(filePath, logText);


                    MessageBox.Show($"Log saved to {filePath}", "Log Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No data to save.", "Empty Log", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving log: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the serial port is open
            if (serialPort != null && serialPort.IsOpen)
            {
                // Get the text from the TextBox
                string textToSend = InputTextBox.Text.Trim();

                // Send the text to the serial port
                serialPort.WriteLine(textToSend);

                // Optionally, display the sent text in the message window
                MessageTextBox.AppendText("Sent>> "+textToSend + Environment.NewLine);
                MessageTextBox.ScrollToEnd(); // Scroll to the end to show the latest message

                // Clear the TextBox for user input
                InputTextBox.Clear();

            }
            else
            {
                MessageBox.Show("Serial port is not open. Please open the serial port first.");
            }
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
