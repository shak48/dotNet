using System;
using System.Threading;
using FTD2XX_NET;

class Program
{
    static void Main(string[] args)
    {
        FTDI ftdi = new FTDI();

        FTDI.FT_STATUS status = ftdi.OpenByIndex(0); // Open the first available FTDI device
        if (status != FTDI.FT_STATUS.FT_OK)
        {
            Console.WriteLine("Failed to open FTDI device: " + status.ToString());
            return;
        }

        byte bitModeByte = 0b11110000; // Set bit 4 (GPIOL0), bit 5 (GPIOL1), bit 6 (GPIOL2), and bit 7 (GPIOL3) as inputs
        byte[] readData = new byte[1]; // Initialize the read data buffer with a length of 1

        try
        {
            // Configure GPIOL0 (bit 4), GPIOL1 (bit 5), GPIOL2 (bit 6), and GPIOL3 (bit 7) for input mode
            status = ftdi.SetBitMode(bitModeByte, FTDI.FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG);//need to make this change.
            if (status != FTDI.FT_STATUS.FT_OK)
            {
                Console.WriteLine("Failed to set bit mode: " + status.ToString());
                return;
            }

            while (ftdi.IsOpen)
            {
                // Read a byte of data into the buffer
                uint bytesRead = 0;
                status = ftdi.Read(readData, 1, ref bytesRead);

                if (status != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to read data: " + status.ToString());
                    break;
                }

                // Extract the states of GPIOL3 (bit 7), GPIOL2 (bit 6), GPIOL1 (bit 5), and GPIOL0 (bit 4) from the byte
                bool pin7State = (readData[0] & 0b10000000) != 0;
                bool pin6State = (readData[0] & 0b01000000) != 0;
                bool pin5State = (readData[0] & 0b00100000) != 0;
                bool pin4State = (readData[0] & 0b00010000) != 0;

                Console.WriteLine("GPIOL3 (Bit 7) state: " + pin7State);
                Console.WriteLine("GPIOL2 (Bit 6) state: " + pin6State);
                Console.WriteLine("GPIOL1 (Bit 5) state: " + pin5State);
                Console.WriteLine("GPIOL0 (Bit 4) state: " + pin4State);

                Thread.Sleep(1000); // Read every 1 second
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            // Close the FTDI device when done
            ftdi.Close();
        }
    }
}


/*
To try.

To start I2C communication with a C232HM MPSSE FTDI device as a master device using .NET Framework in a console app, you can use the FTD2XX library provided by FTDI. Here's a basic example of how to do this:

Please note that you need to install the FTD2XX library and add a reference to it in your .NET Framework console application.

```csharp
using System;
using FTD2XX; // Make sure to add a reference to the FTD2XX library.

class Program
{
    static void Main(string[] args)
    {
        FTDI ftdi = new FTDI();

        // Open the FTDI device by its description or serial number
        FTDI.FT_DEVICE_INFO_NODE[] deviceList = new FTDI.FT_DEVICE_INFO_NODE[1];
        ftdi.GetDeviceList(deviceList);
        int deviceCount = deviceList.Length;

        if (deviceCount == 0)
        {
            Console.WriteLine("No FTDI devices found.");
            return;
        }

        // You can also use device serial number or other criteria to select the device.
        // For example: ftdi.OpenBySerialNumber("YourSerialNumber");

        FTDI.FT_STATUS status = ftdi.OpenByIndex(0); // Open the first FTDI device
        if (status != FTDI.FT_STATUS.FT_OK)
        {
            Console.WriteLine("Failed to open FTDI device. Error: " + status.ToString());
            return;
        }

        // Configure the FTDI device for MPSSE mode
        status = ftdi.SetBitMode(0x0, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
        if (status != FTDI.FT_STATUS.FT_OK)
        {
            Console.WriteLine("Failed to reset FTDI device. Error: " + status.ToString());
            return;
        }

        status = ftdi.SetBitMode(0x0, FTDI.FT_BIT_MODES.FT_BIT_MODE_MPSSE);
        if (status != FTDI.FT_STATUS.FT_OK)
        {
            Console.WriteLine("Failed to set MPSSE mode. Error: " + status.ToString());
            return;
        }

        // Configure I2C communication parameters
        ftdi.SetBaudRate(100000); // Set the I2C clock rate (100 kHz in this example)

        // Start I2C communication here. You can use the MPSSE commands to send I2C data.

        // Don't forget to close the FTDI device when you're done.
        ftdi.Close();

        Console.WriteLine("I2C communication completed.");
    }
}
```

This code opens the FTDI device, configures it for MPSSE mode, sets the I2C clock rate, and provides a starting point for your I2C communication. You will need to use the MPSSE commands to send and receive I2C data according to your specific requirements. Make sure to consult the FTDI documentation for the MPSSE commands and the capabilities of the C232HM MPSSE device.

*/
