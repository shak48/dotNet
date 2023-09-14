using System;
using System.Threading;
using FTD2XX_NET;


class Program
{
    static void Main(string[] args)
    {
        FTDI ftdi = new FTDI();

        FTDI.FT_STATUS status = ftdi.OpenByIndex(0); // Open the first available FTDI device
        if (status != FTDI.FT_STATUS.FT_OK && status != FTDI.FT_STATUS.FT_DEVICE_NOT_FOUND)
        {
            Console.WriteLine("Failed to open FTDI device: " + status.ToString());
            return;
        }
        else if (status == FTDI.FT_STATUS.FT_DEVICE_NOT_FOUND)
        {
            Console.WriteLine("FTDI device not found. Please ensure it's connected.");
            System.Threading.Thread.Sleep(5000);
            return;
        }

        // Set pin 5 as an output pin
        status = ftdi.SetBitMode(0x1F, FTDI.FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG);
        if (status != FTDI.FT_STATUS.FT_OK)
        {
            Console.WriteLine("Failed to set bit mode: " + status.ToString());
            ftdi.Close();
            return;
        }

        bool toggleState = true;
        try
        {
            while (ftdi.IsOpen) // Check if the device is still open/connected
            {
                // Write the new state to the FTDI device (bit 5)
                byte[] data = { (byte)(toggleState ? 0x20 : 0x00) };
                uint bytesWritten = 0;
                status = ftdi.Write(data, 1, ref bytesWritten);
                if (status != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to write data: " + status.ToString());
                    break;
                }

                toggleState = !toggleState; // Toggle the state
                Console.WriteLine($"{toggleState.ToString()} {bytesWritten}");
                System.Threading.Thread.Sleep(1000); // Wait for 1 second
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



