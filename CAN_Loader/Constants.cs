﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAN_Loader
{
    public static class Constants
    {
        //Defines
        public const int dSPI_CAN_PACKET_SIZE = 19;
        public const int dSPI_CAN_PACKET_CHECKSUM_LOCATION = 18;
        //USB Commands
        public const byte dTRANSMIT_MESSAGE_EV = 0xA3;
        //////////////////////////////////////////////////
        ///

        //Loader commands
        public const int _CMD_ECHO = 1;
        public const int _CMD_INFO = 2;


        public const int _CMD_FLASH_WRITE_START = 100;
        public const int _CMD_FLASH_WRITE_NEXT_WORD = 101;
        public const int _CMD_FLASH_WRITE_FINISH = 111;


        public const int _CMD_BOOT_LOCK = 124;
        public const int _CMD_BOOT_UNLOCK = 125;
        public const int _CMD_FLASH_EARSE = 126;
        public const int _CMD_RESET = 127;
        public const int _CMD_GET_MD5 = 128;
        public const int _CMD_JUMP_TO_APP = 129;

        public const int _CMD_START_PLC = 201;
        public const int _CMD_STOP_PLC = 202;
        public const int _CMD_RESET_DEBUG_VARIABLES = 203;
        public const int _CMD_RESUME_DEBUG = 204;
        public const int _CMD_SUSPEND_DEBUG = 205;
        public const int _CMD_REGISTER_DEBUG_VARIABLES = 206;
        public const int _CMD_GET_DEBUG_VARIABLES = 207;
        public const int _CMD_GET_DEBUG_VARIABLES_NEXT = 208;
        public const int _CMD_GET_DEBUG_VARIABLES_FINISH = 209;
        public const int _CMD_FREE_DEBUG_DATA = 210;

        public const int _OK = 1;
        public const int _ERROR = 0;

        public const int _ACK_ERROR = 0xff;
        public const int _ACK_INFO = 1;
        public const int _HOST_VERSION = 1;
        public const int _HOST_BOOTLOADER_ID = 170;
        public const int _HOST_PROGRAM_ID = 187;
    }
}