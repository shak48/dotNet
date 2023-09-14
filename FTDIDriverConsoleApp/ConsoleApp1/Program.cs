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

        byte bitModeByte = 0; // Initialize the bit mode byte
        bool toggleState = false; // Initialize the toggle state

        try
        {
            // Set bit 7 (GPIOL3) as an output
            bitModeByte |= 0b10000000; // Binary representation for bit 7

            // Configure GPIOL3 (bit 7) for output mode
            status = ftdi.SetBitMode(bitModeByte, FTDI.FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG);
            if (status != FTDI.FT_STATUS.FT_OK)
            {
                Console.WriteLine("Failed to set bit mode: " + status.ToString());
                return;
            }

            while (ftdi.IsOpen)
            {
                // Toggle the state of GPIOL3 (bit 7)
                toggleState = !toggleState;

                // Write the current state to GPIOL3 (bit 7)
                byte data = (byte)(toggleState ? 0b10000000 : 0b00000000);
                uint bytesWritten = 0;
                status = ftdi.Write(new byte[] { data }, 1, ref bytesWritten);

                if (status != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to write data: " + status.ToString());
                    break;
                }

                Console.WriteLine("GPIOL3 (Bit 7) state toggled: " + toggleState);

                Thread.Sleep(1000); // Toggle every 1 second
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
