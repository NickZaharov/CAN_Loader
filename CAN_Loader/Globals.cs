using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAN_Loader
{
    public static class Globals
    {
        //CAN message buffer
        public static byte[] packetBuffer = new byte[19];
        public static uint gPacketID;
        public static int gWordNumber;
        public static string filePath;
        ///Переменная хранящая результат выполнения загрузки
        public static int gLoaderResponse;

        //Connection status
        public const int status_InProgram = 2;
        public const int status_InLoader = 1;
        public const int status_OK = 1;
        public const int status_ERROR = 0;
        public const int status_DISCONNECT = -1;
        public const int status_SILENCE = -2;

        //Loader commands
        public const uint _CMD_ECHO = 1;
        public const uint _CMD_INFO = 2;

        public const uint CMD_GET_LAST_RX_DATA = 3;

        public const uint _CMD_FLASH_WRITE_START = 100;
        public const uint _CMD_FLASH_WRITE_NEXT_WORD = 101;
        public const uint _CMD_FLASH_WRITE_FINISH = 111;

        public const uint _CMD_BOOT_LOCK = 124;
        public const uint _CMD_BOOT_UNLOCK = 125;
        public const uint _CMD_FLASH_EARSE = 126;
        public const uint _CMD_RESET = 127;
        public const uint _CMD_GET_MD5 = 128;
        public const uint _CMD_JUMP_TO_APP = 129;

        public const uint _CMD_START_PLC = 201;
        public const uint _CMD_STOP_PLC = 202;
        public const uint _CMD_RESET_DEBUG_VARIABLES = 203;
        public const uint _CMD_RESUME_DEBUG = 204;
        public const uint _CMD_SUSPEND_DEBUG = 205;
        public const uint _CMD_REGISTER_DEBUG_VARIABLES = 206;
        public const uint _CMD_GET_DEBUG_VARIABLES = 207;
        public const uint _CMD_GET_DEBUG_VARIABLES_NEXT = 208;
        public const uint _CMD_GET_DEBUG_VARIABLES_FINISH = 209;
        public const uint _CMD_FREE_DEBUG_DATA = 210;

        //Response with the first byte of data
        public const uint _OK = 1;
        public const uint _ERROR = 0;

        public const uint _ACK_ERROR = 0xff;
        public const uint _ACK_INFO = 1;
        public const uint _HOST_VERSION = 1;
        public const uint _HOST_BOOTLOADER_ID = 170;
        public const uint _HOST_PROGRAM_ID = 187;
    }
}