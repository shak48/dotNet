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
        byte data = 0; // Initialize the data byte
        bool toggleStateGPIOL0 = false; // Initialize the toggle state for GPIOL0 (bit 4)
        bool toggleStateGPIOL1 = false; // Initialize the toggle state for GPIOL1 (bit 5)
        bool toggleStateGPIOL2 = false; // Initialize the toggle state for GPIOL2 (bit 6)
        bool toggleStateGPIOL3 = false; // Initialize the toggle state for GPIOL3 (bit 7)

        try
        {
            // Set bit 4 (GPIOL0), bit 5 (GPIOL1), bit 6 (GPIOL2), and bit 7 (GPIOL3) as outputs
            bitModeByte |= 0b11110000; // Binary representation for bits 4, 5, 6, and 7

            // Configure GPIOL0 (bit 4), GPIOL1 (bit 5), GPIOL2 (bit 6), and GPIOL3 (bit 7) for output mode
            status = ftdi.SetBitMode(bitModeByte, FTDI.FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG);
            if (status != FTDI.FT_STATUS.FT_OK)
            {
                Console.WriteLine("Failed to set bit mode: " + status.ToString());
                return;
            }

            while (ftdi.IsOpen)
            {
                // Toggle the state of GPIOL0 (bit 4)
                toggleStateGPIOL0 = !toggleStateGPIOL0;

                // Toggle the state of GPIOL1 (bit 5)
                toggleStateGPIOL1 = !toggleStateGPIOL1;

                // Toggle the state of GPIOL2 (bit 6)
                toggleStateGPIOL2 = !toggleStateGPIOL2;

                // Toggle the state of GPIOL3 (bit 7)
                toggleStateGPIOL3 = !toggleStateGPIOL3;

                // Update the data byte based on the toggle states
                data = (byte)((toggleStateGPIOL3 ? 0b10000000 : 0b00000000) |
                               (toggleStateGPIOL2 ? 0b01000000 : 0b00000000) |
                               (toggleStateGPIOL1 ? 0b00100000 : 0b00000000) |
                               (toggleStateGPIOL0 ? 0b00010000 : 0b00000000));

                // Write the current data byte to GPIOL0 (bit 4), GPIOL1 (bit 5), GPIOL2 (bit 6), and GPIOL3 (bit 7)
                uint bytesWritten = 0;
                status = ftdi.Write(new byte[] { data }, 1, ref bytesWritten);

                if (status != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to write data: " + status.ToString());
                    break;
                }

                Console.WriteLine("GPIOL0 (Bit 4) state toggled: " + toggleStateGPIOL0);
                Console.WriteLine("GPIOL1 (Bit 5) state toggled: " + toggleStateGPIOL1);
                Console.WriteLine("GPIOL2 (Bit 6) state toggled: " + toggleStateGPIOL2);
                Console.WriteLine("GPIOL3 (Bit 7) state toggled: " + toggleStateGPIOL3);

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
