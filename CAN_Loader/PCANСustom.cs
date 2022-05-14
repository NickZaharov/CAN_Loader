using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Peak.Can.Basic;

// Type alias for a PCAN-Basic channel handle
using TPCANHandle = System.UInt16;

namespace CAN_Loader
{
    class PCANСustom
    {
        /// Sets the PCANHandle (Hardware Channel)
        const TPCANHandle PcanHandle = PCANBasic.PCAN_USBBUS1;
        /// Sets the bitrate for normal CAN devices
        const TPCANBaudrate Bitrate = TPCANBaudrate.PCAN_BAUD_250K;
        /// Initialize PCAN connection
        public TPCANMsg MsgBuffer;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool PCANInitialize()
        {
            if (PCANBasic.Initialize(PcanHandle, Bitrate) == TPCANStatus.PCAN_ERROR_OK)
                return true;
            return false;
        }

        /// <summary>
        /// Sends a CAN message with extended ID (without data)
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>Operation status</returns>
        public TPCANStatus WriteMessage(uint cmd)
        {
            var msgCanMessage = new TPCANMsg()
            {
                DATA = new byte[8],
                ID = (0xFF00 | cmd) << 8,
                LEN = 0,
                MSGTYPE = TPCANMessageType.PCAN_MESSAGE_EXTENDED
            };
            return PCANBasic.Write(PcanHandle, ref msgCanMessage);
        }

        /// <summary>
        /// Sends a CAN message with extended ID, and data bytes
        /// </summary>
        /// <param name="cmd"></param>
        /// /// <param name="data"></param>
        /// <returns>Operation status</returns>
        public TPCANStatus WriteMessage(uint cmd, byte[] data)
        {
            var msgCanMessage = new TPCANMsg()
            {
                DATA = new byte[8],
                ID = (0xFF00 | cmd) << 8,
                LEN = (byte)data.Length,
                MSGTYPE = TPCANMessageType.PCAN_MESSAGE_EXTENDED
            };
            for (int i = 0; i < data.Length; i++)
                msgCanMessage.DATA[i] = data[i];

            return PCANBasic.Write(PcanHandle, ref msgCanMessage);
        }

        /// <summary>
        /// Reads a CAN message from the receive queue of a PCAN Channel
        /// </summary>
        /// <returns></returns>
        public bool ReadMessage()
        {
            int timeout = 0;
            while (true)
            {
                PCANBasic.Read(PcanHandle, out TPCANMsg CANMsg);
                if (CANMsg.ID != 0)
                {
                    
                    MsgBuffer = CANMsg;
                    return true;
                }
                timeout++;
                if (timeout > 200) 
                    return false;
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Reads (with LARGE TIMEOUT) a CAN message from the receive queue of a PCAN Channel
        /// </summary>
        /// <returns></returns>
        public bool ReadMessageLargeTimeOut()
        {
            int timeout = 0;
            while (true)
            {
                PCANBasic.Read(PcanHandle, out TPCANMsg CANMsg);
                if (CANMsg.ID != 0)
                {
                    Console.WriteLine(Convert.ToString(CANMsg.ID, 16));
                    MsgBuffer = CANMsg;
                    return true;
                }
                timeout++;
                if (timeout > 10000)
                    return false;
                Thread.Sleep(1);
            }
        }
    }
}
