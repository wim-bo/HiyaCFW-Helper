﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HiyaCFW_Helper
{
    public partial class HelperProcess
    {
        // No$GBA footer header: DSi eMMC CID/CPU
        private static readonly byte[] footerHeader = new byte[] { 0x44, 0x53, 0x69, 0x20, 0x65, 0x4d, 0x4d, 0x43, 0x20, 0x43, 0x49, 0x44, 0x2f, 0x43, 0x50, 0x55 };

        // Convert byte array to hex string
        // Taken from https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/14333437#14333437
        private static string ByteToHexBitFiddle(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        private async Task<bool> CheckNAND()
        {
            log.Report("• Checking NAND file...");
            status.Report($"Checking {nandFile}...");

            // Fail if file does not exist
            if (!File.Exists(nandFile))
            {
                return true;
            }

            // Read the NAND file
            using (FileStream fileStream = new FileStream(nandFile, FileMode.Open, FileAccess.Read, FileShare.Read, 16, true))
            {
                // Go to the No$GBA footer offset
                fileStream.Seek(-64, SeekOrigin.End);
                // Read the footer's header :-)
                byte[] bstr = new byte[16];
                await fileStream.ReadAsync(bstr, 0, 16, cancellationToken);

                // Check if the footer's header matches with the read data
                if (!footerHeader.SequenceEqual(bstr))
                {
                    return true;
                }

                // Read the CID
                await fileStream.ReadAsync(bstr, 0, 16, cancellationToken);
                log.Report($"  - eMMC CID: {ByteToHexBitFiddle(bstr)}");

                // Read the console ID
                bstr = new byte[8];
                await fileStream.ReadAsync(bstr, 0, 8, cancellationToken);
                log.Report($"  - Console ID: {ByteToHexBitFiddle(bstr.Reverse().ToArray())}\r\n");
            }

            return false;
        }
    }
}
