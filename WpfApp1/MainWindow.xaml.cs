using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;



namespace SerialPortWpf
{
    public partial class MainWindow : Window
    {
        private SerialPort serialPort;
        public int BaudRate { get; set; }
        private bool isPortOpen = false;
        private StringBuilder logBuilder = new StringBuilder();
        private StringBuilder dataBuffer = new StringBuilder();
        private Regex commaRegex = new Regex(@"[^,\r\n]*,[^,\r\n]*");
        private int matchedLineCount = 0;


        public MainWindow()
        {
            InitializeComponent();

            // Create an instance of XmlConfig
            XmlConfig config = new XmlConfig();

            // Set the FilePath property to the path of the configuration file
            config.FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");

            // Load configuration
            config = XmlConfig.LoadConfig(config.FilePath);
            if (config != null)
            {
                // Populate the COM port ComboBox with available ports
                string[] availablePorts = SerialPort.GetPortNames();
                foreach (string port in availablePorts)
                {
                    ComPortComboBox.Items.Add(port);
                }

                // Add baud rate items dynamically to the ComboBox
                BaudRateComboBox.Items.Add(config.BaudRate.ToString());
                BaudRateComboBox.SelectedItem = config.BaudRate.ToString();
            }
            else
            {
                // Handle the case when configuration loading fails
                MessageBox.Show("Failed to load configuration. Please check the configuration file.");
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
            try
            {
                string data = serialPort.ReadExisting();
                // Split the data into lines

                string[] lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        // Iterate through the lines and append lines with a comma
        foreach (string line in lines)
        {
            if (matchedLineCount < 100 && commaRegex.IsMatch(line)) // Change "10" to the desired line count
            {
                dataBuffer.AppendLine(line);
                logBuilder.AppendLine(line);
                matchedLineCount++;
            }
        }

                // Update the TextBox on the UI thread
                Dispatcher.Invoke(() =>
                {
                    MessageTextBox.AppendText(data);
                });
                dataBuffer.Clear();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

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
                    logText = Environment.NewLine + logText;

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

        public class AppConfig
        {
            public int BaudRate { get; set; }
            public string FilePath { get; set; } // Add the FilePath property


            public static AppConfig LoadConfig(string filePath)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<AppConfig>(json);
                }
                catch (Exception ex)
                {
                    // Handle any configuration loading errors here
                    Console.WriteLine($"Error loading configuration: {ex.Message}");
                    return null;
                }
            }
        }

    }
}
