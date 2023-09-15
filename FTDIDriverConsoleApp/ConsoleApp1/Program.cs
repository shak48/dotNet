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
            status = ftdi.SetBitMode(bitModeByte, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
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
