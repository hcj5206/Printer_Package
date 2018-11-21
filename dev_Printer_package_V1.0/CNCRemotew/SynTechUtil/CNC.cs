namespace CarryLine.SynTechUtil
{
    using CarryLine.Common;
    using DialogLibray.CarryLineClass;
    using DialogLibray.CommonClass;
    using Syntec.Remote;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    public static class CNC
    {
        private static Dictionary<string, SyntecRemoteCNC> dicCnc = new Dictionary<string, SyntecRemoteCNC>();

        public static bool Emegercency(SyntecRemoteCNC cnc, int emegercencyPort)
        {
            if (cnc == null)
            {
                return false;
            }
            if (cnc.WRITE_plc_register(emegercencyPort, emegercencyPort, new int[1]) != 0)
            {
                return false;
            }
            Thread.Sleep(CarryLine.Common.Common.ResetPortInterval);
            if (cnc.WRITE_plc_register(emegercencyPort, emegercencyPort, new int[] { 1 }) != 0)
            {
                return false;
            }
            Thread.Sleep(CarryLine.Common.Common.ResetPortInterval);
            if (cnc.WRITE_plc_register(emegercencyPort, emegercencyPort, new int[1]) != 0)
            {
                return false;
            }
            return true;
        }

        public static SyntecRemoteCNC GetCNCStation(string stationIP)
        {
            if (!dicCnc.ContainsKey(stationIP))
            {
                SyntecRemoteCNC ecnc = new SyntecRemoteCNC(stationIP);
                dicCnc.Add(stationIP, ecnc);
            }
            if (!GetInformation(dicCnc[stationIP], new CNCInformation()))
            {
                return null;
            }
            return dicCnc[stationIP];
        }

        public static CNCAlarm GetCurrentAlarm(CNCMachine machine)
        {
            if (machine == null)
            {
                return null;
            }
            return GetCurrentAlarm(GetCNCStation(machine.MachineIP.ToString()));
        }

        public static CNCAlarm GetCurrentAlarm(SyntecRemoteCNC cnc)
        {
            bool flag;
            string[] strArray;
            DateTime[] timeArray;
            if (cnc == null)
            {
                return null;
            }
            if ((cnc.READ_alm_current(out flag, out strArray, out timeArray) != 0) || !flag)
            {
                return null;
            }
            CNCAlarm alarm = new CNCAlarm();
            try
            {
                alarm.IsAlarm = flag;
                alarm.AlarmMsg = strArray.ToList<string>();
                alarm.AlarmTime = timeArray.ToList<DateTime>();
            }
            catch
            {
                flag = false;
            }
            return alarm;
        }

        public static bool GetFeedPercent(CNCMachine machine, out int data)
        {
            data = 0;
            if (machine == null)
            {
                return false;
            }
            return GetFeedPercent(GetCNCStation(machine.MachineIP.ToString()), out data);
        }

        public static bool GetFeedPercent(SyntecRemoteCNC cnc, out int data)
        {
            data = 0;
            if (cnc != null)
            {
                int[] plcData = new int[1];
                if (cnc.READ_plc_register(0x10, 0x10, out plcData) == 0)
                {
                    data = plcData[0];
                    return true;
                }
            }
            return false;
        }

        public static bool GetFeedSet(CNCMachine machine, out int data)
        {
            data = 0;
            if (machine == null)
            {
                return false;
            }
            return GetFeedSet(GetCNCStation(machine.MachineIP.ToString()), out data);
        }

        public static bool GetFeedSet(SyntecRemoteCNC cnc, out int data)
        {
            data = 0;
            if (cnc != null)
            {
                int[] plcData = new int[1];
                if (cnc.READ_plc_register(4, 4, out plcData) == 0)
                {
                    data = plcData[0];
                    return true;
                }
            }
            return false;
        }

        public static CNCAlarm GetHistoryAlarm(CNCMachine machine)
        {
            if (machine == null)
            {
                return null;
            }
            return GetHistoryAlarm(GetCNCStation(machine.MachineIP.ToString()));
        }

        public static CNCAlarm GetHistoryAlarm(SyntecRemoteCNC cnc)
        {
            string[] strArray;
            DateTime[] timeArray;
            if (cnc.READ_alm_history(out strArray, out timeArray) == 0)
            {
                CNCAlarm alarm = new CNCAlarm {
                    AlarmMsg = strArray.ToList<string>(),
                    AlarmTime = timeArray.ToList<DateTime>()
                };
                if (alarm.AlarmMsg.Count > 0)
                {
                    return alarm;
                }
            }
            return null;
        }

        public static bool GetInformation(CNCMachine machine, CNCInformation cncInfo)
        {
            if (machine == null)
            {
                return false;
            }
            return GetInformation(GetCNCStation(machine.MachineIP.ToString()), cncInfo);
        }

        public static bool GetInformation(SyntecRemoteCNC cnc, CNCInformation cncInfo)
        {
            if (cnc == null)
            {
                return false;
            }
            short axes = 0;
            short maxAxes = 0;
            string cncType = "";
            string series = "";
            string str3 = "";
            string[] axisName = null;
            try
            {
                short num3 = cnc.READ_information(out axes, out cncType, out maxAxes, out series, out str3, out axisName);
                if (num3 == 0)
                {
                    try
                    {
                        cncInfo.HostIP = cnc.Host;
                        cncInfo.Axes = axes;
                        cncInfo.CncType = cncType;
                        cncInfo.MaxAxes = maxAxes;
                        cncInfo.Series = series;
                        cncInfo.Nc_Ver = str3;
                        cncInfo.AxisName = axisName.ToList<string>();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                cncInfo.ErrMsg = cnc.Host + ":" + num3.ToString();
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool GetMachineMode(CNCMachine machine, out EnumCNCMachinMode mode)
        {
            return GetMachineMode(GetCNCStation(machine.MachineIP.ToString()), out mode);
        }

        public static bool GetMachineMode(SyntecRemoteCNC cnc, out EnumCNCMachinMode mode)
        {
            mode = EnumCNCMachinMode.None;
            if (cnc != null)
            {
                int[] plcData = new int[1];
                if (cnc.READ_plc_register(13, 13, out plcData) == 0)
                {
                    mode = (EnumCNCMachinMode) plcData[0];
                    return true;
                }
            }
            return false;
        }

        public static bool GetMachineReadMode(CNCMachine machine, out int data, CNCCheckPort port)
        {
            data = -1;
            return (((machine != null) && (port != null)) && GetMachineReadMode(GetCNCStation(machine.MachineIP.ToString()), out data, port.PortNumber));
        }

        public static bool GetMachineReadMode(SyntecRemoteCNC cnc, out int data, int port)
        {
            data = -1;
            if (cnc != null)
            {
                int[] plcData = new int[1];
                if (cnc.READ_plc_register(port, port, out plcData) == 0)
                {
                    data = plcData[0];
                    return true;
                }
            }
            return false;
        }

        public static bool GetMachineRunMode(CNCMachine machine, out int data, CNCCheckPort port)
        {
            data = -1;
            return (((machine != null) && (port != null)) && GetMachineRunMode(GetCNCStation(machine.MachineIP.ToString()), out data, port.PortNumber));
        }

        public static bool GetMachineRunMode(SyntecRemoteCNC cnc, out int data, int port)
        {
            data = -1;
            if (cnc != null)
            {
                int[] plcData = new int[1];
                if (cnc.READ_plc_register(port, port, out plcData) == 0)
                {
                    data = plcData[0];
                    return true;
                }
            }
            return false;
        }

        public static bool GetMachineTimeInfo(SyntecRemoteCNC cnc, CNCTime cncTime)
        {
            int num;
            int num2;
            int num3;
            int num4;
            if ((cnc != null) && (cnc.READ_time(out num, out num2, out num3, out num4) == 0))
            {
                cncTime.PowerOnTime = num;
                cncTime.AccumulateCuttingTime = num2;
                cncTime.CuttingTimePerCycle = num3;
                cncTime.WorkTime = num4;
                return true;
            }
            return false;
        }

        public static bool GetNCPointer(CNCMachine machine, out int pointer)
        {
            pointer = 0;
            return GetNCPointer(GetCNCStation(machine.MachineIP.ToString()), out pointer);
        }

        public static bool GetNCPointer(SyntecRemoteCNC cnc, out int pointer)
        {
            pointer = 0;
            return (cnc.READ_nc_pointer(out pointer) == 0);
        }

        public static bool GetPartCount(CNCMachine machine, CNCPartCount cncPartCount)
        {
            if (machine == null)
            {
                return false;
            }
            return GetPartCount(GetCNCStation(machine.MachineIP.ToString()), cncPartCount);
        }

        public static bool GetPartCount(SyntecRemoteCNC cnc, CNCPartCount cncPartCount)
        {
            int num;
            int num2;
            int num3;
            if ((cnc != null) && (cnc.READ_part_count(out num, out num2, out num3) == 0))
            {
                cncPartCount.PartCount = num;
                cncPartCount.RequirePartCout = num2;
                cncPartCount.TotalPartCount = num3;
                return true;
            }
            return false;
        }

        public static bool GetPlcABit(CNCMachine machine, int port, out byte data)
        {
            data = 0;
            if (machine == null)
            {
                return false;
            }
            return GetPlcABit(GetCNCStation(machine.MachineIP.ToString()), port, out data);
        }

        public static bool GetPlcABit(SyntecRemoteCNC cnc, int port, out byte data)
        {
            data = 0;
            byte[] plcData = new byte[1];
            if ((cnc != null) && (cnc.READ_plc_abit(port, port, out plcData) == 0))
            {
                data = plcData[0];
                return true;
            }
            return false;
        }

        public static bool GetPLCAdreesData(CNCMachine machine, CNCCheckPort port, out int data)
        {
            data = -1;
            if ((machine == null) || (port == null))
            {
                return false;
            }
            byte num = 0;
            bool flag = false;
            switch (port.AddressType)
            {
                case EnumCNCAddress.I:
                    flag = GetPlcIBit(machine, port.PortNumber, out num);
                    data = num;
                    return flag;

                case EnumCNCAddress.O:
                    flag = GetPlcOBit(machine, port.PortNumber, out num);
                    data = num;
                    return flag;

                case EnumCNCAddress.C:
                    flag = GetPlcCBit(machine, port.PortNumber, out num);
                    data = num;
                    return flag;

                case EnumCNCAddress.S:
                    flag = GetPlcSBit(machine, port.PortNumber, out num);
                    data = num;
                    return flag;

                case EnumCNCAddress.A:
                    flag = GetPlcABit(machine, port.PortNumber, out num);
                    data = num;
                    return flag;

                case EnumCNCAddress.R:
                    return GetRegisterData(machine, port.PortNumber, out data);
            }
            return flag;
        }

        public static bool GetPLCAdreesData(CNCMachine machine, CNCPort port, out int data)
        {
            data = -1;
            if ((machine == null) || (port == null))
            {
                return false;
            }
            byte num = 0;
            bool flag = false;
            switch (port.AddressType)
            {
                case EnumCNCAddress.I:
                    flag = GetPlcIBit(machine, port.PortNumber, out num);
                    data = num;
                    return flag;

                case EnumCNCAddress.O:
                    flag = GetPlcOBit(machine, port.PortNumber, out num);
                    data = num;
                    return flag;

                case EnumCNCAddress.C:
                    flag = GetPlcCBit(machine, port.PortNumber, out num);
                    data = num;
                    return flag;

                case EnumCNCAddress.S:
                    flag = GetPlcSBit(machine, port.PortNumber, out num);
                    data = num;
                    return flag;

                case EnumCNCAddress.A:
                    flag = GetPlcABit(machine, port.PortNumber, out num);
                    data = num;
                    return flag;

                case EnumCNCAddress.R:
                    return GetRegisterData(machine, port.PortNumber, out data);
            }
            return flag;
        }

        public static bool GetPlcCBit(CNCMachine machine, int port, out byte data)
        {
            data = 0;
            if (machine == null)
            {
                return false;
            }
            return GetPlcCBit(GetCNCStation(machine.MachineIP.ToString()), port, out data);
        }

        public static bool GetPlcCBit(SyntecRemoteCNC cnc, int port, out byte data)
        {
            data = 0;
            byte[] plcData = new byte[1];
            if ((cnc != null) && (cnc.READ_plc_cbit(port, port, out plcData) == 0))
            {
                data = plcData[0];
                return true;
            }
            return false;
        }

        public static bool GetPlcIBit(CNCMachine machine, int port, out byte data)
        {
            data = 0;
            if (machine == null)
            {
                return false;
            }
            return GetPlcIBit(GetCNCStation(machine.MachineIP.ToString()), port, out data);
        }

        public static bool GetPlcIBit(SyntecRemoteCNC cnc, int port, out byte data)
        {
            data = 0;
            byte[] plcData = new byte[1];
            if ((cnc != null) && (cnc.READ_plc_ibit(port, port, out plcData) == 0))
            {
                data = plcData[0];
                return true;
            }
            return false;
        }

        public static bool GetPlcOBit(CNCMachine machine, int port, out byte data)
        {
            data = 0;
            if (machine == null)
            {
                return false;
            }
            return GetPlcOBit(GetCNCStation(machine.MachineIP.ToString()), port, out data);
        }

        public static bool GetPlcOBit(SyntecRemoteCNC cnc, int port, out byte data)
        {
            data = 0;
            byte[] plcData = new byte[1];
            if ((cnc != null) && (cnc.READ_plc_obit(port, port, out plcData) == 0))
            {
                data = plcData[0];
                return true;
            }
            return false;
        }

        public static bool GetPlcSBit(CNCMachine machine, int port, out byte data)
        {
            data = 0;
            if (machine == null)
            {
                return false;
            }
            return GetPlcSBit(GetCNCStation(machine.MachineIP.ToString()), port, out data);
        }

        public static bool GetPlcSBit(SyntecRemoteCNC cnc, int port, out byte data)
        {
            data = 0;
            byte[] plcData = new byte[1];
            if ((cnc != null) && (cnc.READ_plc_sbit(port, port, out plcData) == 0))
            {
                data = plcData[0];
                return true;
            }
            return false;
        }

        public static string GetPlcVer(CNCMachine machine)
        {
            if (machine == null)
            {
                return "";
            }
            return GetPlcVer(GetCNCStation(machine.MachineIP.ToString()));
        }

        public static string GetPlcVer(SyntecRemoteCNC cnc)
        {
            if (cnc != null)
            {
                string version = "";
                if (cnc.READ_plc_ver(out version) == 0)
                {
                    return version;
                }
            }
            return "";
        }

        public static int GetPortValue(TransferStation station, CNCCheckPort port)
        {
            int data = -1;
            if (!GetPLCAdreesData(station, port, out data))
            {
                data = -1;
            }
            return data;
        }

        public static int GetPortValue(TransferStation station, CNCPort port)
        {
            int data = -1;
            if (!GetPLCAdreesData(station, port, out data))
            {
                data = -1;
            }
            return data;
        }

        public static bool GetPosition(CNCMachine machine, CNCPosition cncPosition)
        {
            if (machine == null)
            {
                return false;
            }
            return GetPosition(GetCNCStation(machine.MachineIP.ToString()), cncPosition);
        }

        public static bool GetPosition(SyntecRemoteCNC cnc, CNCPosition cncPosition)
        {
            if (cnc != null)
            {
                short decPoint = 0;
                string[] axisName = null;
                string[] unit = null;
                float[] mach = null;
                float[] abs = null;
                float[] rel = null;
                float[] dist = null;
                short num2 = cnc.READ_position(out axisName, out decPoint, out unit, out mach, out abs, out rel, out dist);
                if (num2 == 0)
                {
                    cncPosition.AxisName = axisName.ToList<string>();
                    cncPosition.DecPoint = decPoint;
                    cncPosition.Unit = unit.ToList<string>();
                    cncPosition.Mach = mach.ToList<float>();
                    cncPosition.Abs = abs.ToList<float>();
                    cncPosition.Rel = rel.ToList<float>();
                    cncPosition.Dist = dist.ToList<float>();
                    return true;
                }
                cncPosition.ErrMsg = cnc.Host + " : " + num2.ToString();
            }
            return false;
        }

        public static bool GetRegisterData(CNCMachine machine, int registerPort, out int data)
        {
            data = -1;
            if (machine == null)
            {
                return false;
            }
            return GetRegisterData(GetCNCStation(machine.MachineIP.ToString()), registerPort, out data);
        }

        public static bool GetRegisterData(SyntecRemoteCNC cnc, int registerPort, out int data)
        {
            data = -1;
            if (cnc != null)
            {
                int[] plcData = new int[1];
                if (cnc.READ_plc_register(registerPort, registerPort, out plcData) == 0)
                {
                    data = plcData[0];
                    return true;
                }
            }
            return false;
        }

        public static bool GetRegisterString(CNCMachine machine, int registerPort, out string str)
        {
            str = "";
            if (machine == null)
            {
                return false;
            }
            return GetRegisterString(GetCNCStation(machine.MachineIP.ToString()), registerPort, out str);
        }

        public static bool GetRegisterString(SyntecRemoteCNC cnc, int registerPort, out string str)
        {
            str = "";
            return (cnc.READ_plc_register(registerPort, out str) == 0);
        }

        public static bool GetSpindle(CNCMachine machine, CNCSpindle cncSpindle)
        {
            if (machine == null)
            {
                return false;
            }
            return GetSpindle(GetCNCStation(machine.MachineIP.ToString()), cncSpindle);
        }

        public static bool GetSpindle(SyntecRemoteCNC cnc, CNCSpindle cncSpindle)
        {
            if (cnc != null)
            {
                float ovFeed = 0f;
                float ovSpindle = 0f;
                float actFeed = 0f;
                int actSpindle = 0;
                short num5 = cnc.READ_spindle(out ovFeed, out ovSpindle, out actFeed, out actSpindle);
                if (num5 == 0)
                {
                    cncSpindle.OvFeed = ovFeed;
                    cncSpindle.OvSpindle = ovSpindle;
                    cncSpindle.ActFeed = actFeed;
                    cncSpindle.ActSpindle = actSpindle;
                    return true;
                }
                cncSpindle.ErrMsg = cnc.Host + ": " + num5.ToString();
            }
            return false;
        }

        public static bool GetSpindlePercent(CNCMachine machine, out int data)
        {
            data = 0;
            if (machine == null)
            {
                return false;
            }
            return GetSpindlePercent(GetCNCStation(machine.MachineIP.ToString()), out data);
        }

        public static bool GetSpindlePercent(SyntecRemoteCNC cnc, out int data)
        {
            data = 0;
            if (cnc == null)
            {
                return false;
            }
            int[] plcData = new int[1];
            if (cnc.READ_plc_register(15, 15, out plcData) != 0)
            {
                return false;
            }
            switch (plcData[0])
            {
                case 1:
                    data = 50;
                    break;

                case 2:
                    data = 60;
                    break;

                case 3:
                    data = 70;
                    break;

                case 4:
                    data = 80;
                    break;

                case 5:
                    data = 90;
                    break;

                case 6:
                    data = 100;
                    break;

                case 7:
                    data = 110;
                    break;

                case 8:
                    data = 120;
                    break;

                default:
                    data = 0;
                    break;
            }
            return true;
        }

        public static bool GetSpindleSet(CNCMachine machine, out int data)
        {
            data = 0;
            if (machine == null)
            {
                return false;
            }
            return GetSpindleSet(GetCNCStation(machine.MachineIP.ToString()), out data);
        }

        public static bool GetSpindleSet(SyntecRemoteCNC cnc, out int data)
        {
            data = 0;
            if (cnc != null)
            {
                int[] plcData = new int[1];
                if (cnc.READ_plc_register(0x24, 0x24, out plcData) == 0)
                {
                    data = plcData[0];
                    return true;
                }
            }
            return false;
        }

        public static bool GetStatus(CNCMachine machine, CNCStatus cncStatus)
        {
            if (machine == null)
            {
                return false;
            }
            return GetStatus(GetCNCStation(machine.MachineIP.ToString()), cncStatus);
        }

        public static bool GetStatus(SyntecRemoteCNC cnc, CNCStatus cncStatus)
        {
            if (cnc != null)
            {
                int curSeq = 0;
                string mainProg = "";
                string curProg = "";
                string mode = "";
                string status = "";
                string alarm = "";
                string eMG = "";
                short num2 = cnc.READ_status(out mainProg, out curProg, out curSeq, out mode, out status, out alarm, out eMG);
                if (num2 == 0)
                {
                    cncStatus.MainProg = mainProg;
                    cncStatus.CurProg = curProg;
                    cncStatus.CurSeq = curSeq;
                    cncStatus.Mode = mode;
                    cncStatus.Alarm = alarm;
                    cncStatus.EMG = eMG;
                    return true;
                }
                cncStatus.ErrMsg = cnc.Host + " : Error:" + num2.ToString();
            }
            return false;
        }

        public static bool GetWorkCoord_Ext(CNCMachine machine, ref List<float> data)
        {
            if (machine == null)
            {
                return false;
            }
            return GetWorkCoord_Ext(GetCNCStation(machine.MachineIP.ToString()), ref data);
        }

        public static bool GetWorkCoord_Ext(SyntecRemoteCNC cnc, ref List<float> data)
        {
            float[] numArray;
            if ((cnc != null) && (cnc.READ_work_coord_single("EXT", out numArray) == 0))
            {
                data = numArray.ToList<float>();
                return true;
            }
            return false;
        }

        public static bool GetWorkCoord_G54(CNCMachine machine, ref List<float> data)
        {
            if (machine == null)
            {
                return false;
            }
            return GetWorkCoord_G54(GetCNCStation(machine.MachineIP.ToString()), ref data);
        }

        public static bool GetWorkCoord_G54(SyntecRemoteCNC cnc, ref List<float> data)
        {
            float[] numArray;
            if ((cnc != null) && (cnc.READ_work_coord_single("G54", out numArray) == 0))
            {
                data = numArray.ToList<float>();
                return true;
            }
            return false;
        }

        public static bool IsUSBExist(SyntecRemoteCNC cnc)
        {
            return cnc.isUSBExist();
        }

        public static string MainBoardPlatformName(CNCMachine machine)
        {
            if (machine == null)
            {
                return "";
            }
            return MainBoardPlatformName(GetCNCStation(machine.MachineIP.ToString()));
        }

        public static string MainBoardPlatformName(SyntecRemoteCNC cnc)
        {
            return cnc.MainBoardPlatformName;
        }

        public static void MoveAxis(CNCMachine machine, float axisXMachMax, float axisXMachMin, float axisYMachMax, float axisYMachMin, CNCPort xPosPort, CNCPort yPosPort, CNCPort moveStartPort, List<float> dist)
        {
            if (((machine != null) && (xPosPort != null)) && ((yPosPort != null) && (moveStartPort != null)))
            {
                SyntecRemoteCNC cNCStation = GetCNCStation(machine.MachineIP.ToString());
                if (cNCStation != null)
                {
                    MoveAxis(cNCStation, dist, axisXMachMax, axisXMachMin, axisYMachMax, axisYMachMin, xPosPort.PortNumber, yPosPort.PortNumber, moveStartPort.PortNumber, machine.ResetPortIntervalTime.Value);
                }
            }
        }

        public static void MoveAxis(SyntecRemoteCNC cnc, List<float> dist, float axisXMachMax, float axisXMachMin, float axisYMachMax, float axisYMachMin, int axisXRegisterAddr, int axisYRegisterAddr, int moveStartPort, int resetPortInterval)
        {
            CNCPosition cncPosition = new CNCPosition();
            if (GetPosition(cnc, cncPosition))
            {
                if ((cncPosition.AxisName.Count > 0) && (dist.Count > 0))
                {
                    float num = cncPosition.Mach[0] + dist[0];
                    if (num > axisXMachMax)
                    {
                        num = axisXMachMax;
                    }
                    if (num < axisXMachMin)
                    {
                        num = axisXMachMin;
                    }
                    num *= CarryLine.Common.Common.AxisUnitRate;
                    cnc.WRITE_plc_register(axisXRegisterAddr, axisXRegisterAddr, new int[] { (int) num });
                }
                if ((cncPosition.AxisName.Count > 1) && (dist.Count > 1))
                {
                    float num2 = cncPosition.Mach[1] + dist[1];
                    if (num2 > axisYMachMax)
                    {
                        num2 = axisYMachMax;
                    }
                    if (num2 < axisYMachMin)
                    {
                        num2 = axisYMachMin;
                    }
                    num2 *= CarryLine.Common.Common.AxisUnitRate;
                    cnc.WRITE_plc_register(axisYRegisterAddr, axisYRegisterAddr, new int[] { (int) num2 });
                }
                cnc.WRITE_plc_register(moveStartPort, moveStartPort, new int[1]);
                Thread.Sleep(resetPortInterval);
                cnc.WRITE_plc_register(moveStartPort, moveStartPort, new int[] { 1 });
                Thread.Sleep(resetPortInterval);
                cnc.WRITE_plc_register(moveStartPort, moveStartPort, new int[1]);
            }
        }

        public static void MoveAxisToAbsPosition(CNCMachine machine, float axisXMachMax, float axisXMachMin, float axisYMachMax, float axisYMachMin, CNCPort xPosPort, CNCPort yPosPort, CNCPort moveStartPort, float absX, float absY)
        {
            if (((machine != null) && (xPosPort != null)) && ((yPosPort != null) && (moveStartPort != null)))
            {
                SyntecRemoteCNC cNCStation = GetCNCStation(machine.MachineIP.ToString());
                if (cNCStation != null)
                {
                    CNCPosition cncPosition = new CNCPosition();
                    if (GetPosition(cNCStation, cncPosition))
                    {
                        List<float> dist = new List<float>();
                        if (cncPosition.AxisName.Count > 0)
                        {
                            dist.Add(absX - cncPosition.Abs[0]);
                        }
                        else
                        {
                            dist.Add(0f);
                        }
                        if (cncPosition.AxisName.Count > 1)
                        {
                            dist.Add(absY - cncPosition.Abs[1]);
                        }
                        else
                        {
                            dist.Add(0f);
                        }
                        MoveAxis(cNCStation, dist, axisXMachMax, axisXMachMin, axisYMachMax, axisYMachMin, xPosPort.PortNumber, yPosPort.PortNumber, moveStartPort.PortNumber, machine.ResetPortIntervalTime.Value);
                    }
                }
            }
        }

        public static bool Pause(CNCMachine machine, CNCPort port)
        {
            return  (((machine != null) && (port != null)) && Pause(GetCNCStation(machine.MachineIP.ToString()), port.PortNumber, machine.ResetPortIntervalTime.Value));
        }

        public static bool Pause(SyntecRemoteCNC cnc, int pausePort, int intervalTime)
        {
            if (cnc == null)
            {
                return false;
            }
            if (cnc.WRITE_plc_register(pausePort, pausePort, new int[1]) != 0)
            {
                return false;
            }
            Thread.Sleep(intervalTime);
            if (cnc.WRITE_plc_register(pausePort, pausePort, new int[] { 1 }) != 0)
            {
                return false;
            }
            Thread.Sleep(intervalTime);
            if (cnc.WRITE_plc_register(pausePort, pausePort, new int[1]) != 0)
            {
                return false;
            }
            return true;
        }

        public static bool Reset(CNCMachine machine, CNCPort port)
        {
            return (((machine != null) && (port != null)) && Reset(GetCNCStation(machine.MachineIP.ToString()), port.PortNumber, machine.ResetPortIntervalTime.Value));
        }

        public static bool Reset(SyntecRemoteCNC cnc, int resetPort, int intervalTime)
        {
            if (cnc == null)
            {
                return false;
            }
            if (cnc.WRITE_plc_register(resetPort, resetPort, new int[1]) != 0)
            {
                return false;
            }
            Thread.Sleep(intervalTime);
            if (cnc.WRITE_plc_register(resetPort, resetPort, new int[] { 1 }) != 0)
            {
                return false;
            }
            Thread.Sleep(intervalTime);
            if (cnc.WRITE_plc_register(resetPort, resetPort, new int[1]) != 0)
            {
                return false;
            }
            return true;
        }

        public static string SeriesNo(CNCMachine machine)
        {
            if (machine == null)
            {
                return "";
            }
            return SeriesNo(GetCNCStation(machine.MachineIP.ToString()));
        }

        public static string SeriesNo(SyntecRemoteCNC cnc)
        {
            return cnc.SeriesNo;
        }

        public static void SetFeedPercent(CNCMachine machine, int data)
        {
            if (machine != null)
            {
                SetFeedPercent(GetCNCStation(machine.MachineIP.ToString()), data);
            }
        }

        public static void SetFeedPercent(SyntecRemoteCNC cnc, int data)
        {
            if (cnc != null)
            {
                int[] plcData = new int[] { data };
                cnc.WRITE_plc_register(0x10, 0x10, plcData);
            }
        }

        public static bool SetMachineMode(CNCMachine machine, EnumCNCMachinMode mode)
        {
            return SetMachineMode(GetCNCStation(machine.MachineIP.ToString()), mode);
        }

        public static bool SetMachineMode(SyntecRemoteCNC cnc, EnumCNCMachinMode mode)
        {
            if (cnc == null)
            {
                return false;
            }
            int num = 0;
            switch (mode)
            {
                case EnumCNCMachinMode.None:
                    num = 0;
                    break;

                case EnumCNCMachinMode.Edit:
                    num = 1;
                    break;

                case EnumCNCMachinMode.Auto:
                    num = 2;
                    break;

                case EnumCNCMachinMode.MDI:
                    num = 3;
                    break;

                case EnumCNCMachinMode.JOG:
                    num = 4;
                    break;

                case EnumCNCMachinMode.INCJOG:
                    num = 5;
                    break;

                case EnumCNCMachinMode.MPG:
                    num = 6;
                    break;

                case EnumCNCMachinMode.HOME:
                    num = 7;
                    break;
            }
            return (cnc.WRITE_plc_register(13, 13, new int[] { num }) == 0);
        }

        public static void SetMachineModeNext(CNCMachine mchine)
        {
            EnumCNCMachinMode mode;
            SyntecRemoteCNC cNCStation = GetCNCStation(mchine.MachineIP.ToString());
            if ((cNCStation != null) && GetMachineMode(cNCStation, out mode))
            {
                int num =  ((int)(mode + 1) % CarryLine.Common.Common.MachineModeCount);
                SetMachineMode(cNCStation, (EnumCNCMachinMode) num);
            }
        }

        public static void SetSpindlePercent(CNCMachine machine, int data)
        {
            if (machine != null)
            {
                SetSpindlePercent(GetCNCStation(machine.MachineIP.ToString()), data);
            }
        }

        public static void SetSpindlePercent(SyntecRemoteCNC cnc, int data)
        {
            if (cnc != null)
            {
                int[] plcData = new int[] { data };
                cnc.WRITE_plc_register(15, 15, plcData);
            }
        }

        public static bool SetWorkCoord_Ext(CNCMachine machine, List<float> data)
        {
            if (machine == null)
            {
                return false;
            }
            return SetWorkCoord_Ext(GetCNCStation(machine.MachineIP.ToString()), data);
        }

        public static bool SetWorkCoord_Ext(SyntecRemoteCNC cnc, List<float> data)
        {
            return (cnc.WRITE_work_coord_single("EXT", data.ToArray()) == 0);
        }

        public static bool SetWorkCoord_G54(CNCMachine machine, List<float> data)
        {
            if (machine == null)
            {
                return false;
            }
            return SetWorkCoord_G54(GetCNCStation(machine.MachineIP.ToString()), data);
        }

        public static bool SetWorkCoord_G54(SyntecRemoteCNC cnc, List<float> data)
        {
            return (cnc.WRITE_work_coord_single("G54", data.ToArray()) == 0);
        }
        public static bool Start(CNCMachine machine, CNCPort startPort, CNCPort resetPort)
        {
            return ((((machine != null) && (startPort != null)) && (resetPort != null)) && Start(GetCNCStation(machine.MachineIP.ToString()), startPort.PortNumber, resetPort.PortNumber, machine.ResetPortIntervalTime.Value));
        }
        public static bool Start(SyntecRemoteCNC cnc, int startPort, int resetPort, int intervalTime)
        {
            if (cnc == null)
            {
                return false;
            }
            if (!SetMachineMode(cnc, EnumCNCMachinMode.Auto))
            {
                return false;
            }
            if (!Reset(cnc, resetPort, intervalTime))
            {
                return false;
            }
            Thread.Sleep(intervalTime);
            if (cnc.WRITE_plc_register(startPort, startPort, new int[1]) != 0)
            {
                return false;
            }
            Thread.Sleep(intervalTime);
            if (cnc.WRITE_plc_register(startPort, startPort, new int[] { 1 }) != 0)
            {
                return false;
            }
            Thread.Sleep(intervalTime);
            if (cnc.WRITE_plc_register(startPort, startPort, new int[1]) != 0)
            {
                return false;
            }
            return true;
        }

        public static void StartAction(TransferStation station, CNCPort startPort)
        {
            WritePLCAddressData(station, startPort, 1);
        }

        public static bool StartVacuumPump(TransferStation station, CNCPort startPort)
        {
            WritePLCAddressData(station, startPort, 1);
            int data = -1;
            GetPLCAdreesData(station, startPort, out data);
            return (data == 1);
        }

        public static void StopAction(TransferStation station, CNCPort startPort)
        {
            WritePLCAddressData(station, startPort, 0);
        }

        public static bool StopVacuumPump(TransferStation station, CNCPort startPort)
        {
            WritePLCAddressData(station, startPort, 0);
            Thread.Sleep(500);
            int data = -1;
            GetPLCAdreesData(station, startPort, out data);
            return (data == 0);
        }

        public static void WriteNcFileToSynTech(string originfullFileName, string destFolder)
        {
            string fileName = Path.GetFileName(originfullFileName);
            string destFileName = Path.Combine(destFolder, fileName);
            File.Copy(originfullFileName, destFileName, true);
        }

        public static bool WriteNcMain(CNCMachine machine, string fileName)
        {
            return WriteNcMain(GetCNCStation(machine.MachineIP.ToString()), fileName);
        }

        public static bool WriteNcMain(SyntecRemoteCNC cnc, string fileName)
        {
            if (cnc == null)
            {
                return false;
            }
            if (cnc.WRITE_nc_main(fileName) == 0)
            {
                Thread.Sleep(CarryLine.Common.Common.ResetPortInterval);
                CNCStatus cncStatus = new CNCStatus();
                if (!GetStatus(cnc, cncStatus))
                {
                    return false;
                }
                if (string.Compare(cncStatus.MainProg, fileName) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool WritePLCAddressData(CNCMachine machine, CNCPort port, int data)
        {
            if ((machine == null) || (port == null))
            {
                return false;
            }
            bool flag = false;
            switch (port.AddressType)
            {
                case EnumCNCAddress.I:
                    return WritePlcIBit(machine, port.PortNumber, (byte) data);

                case EnumCNCAddress.O:
                case EnumCNCAddress.A:
                    return flag;

                case EnumCNCAddress.C:
                    return WritePlcCBit(machine, port.PortNumber, (byte) data);

                case EnumCNCAddress.S:
                    return WritePlcSBit(machine, port.PortNumber, (byte) data);

                case EnumCNCAddress.R:
                    return WriteRegisterData(machine, port.PortNumber, data);
            }
            return flag;
        }

        public static bool WritePlcCBit(CNCMachine machine, int port, byte data)
        {
            if (machine == null)
            {
                return false;
            }
            return WritePlcCBit(GetCNCStation(machine.MachineIP.ToString()), port, data);
        }

        public static bool WritePlcCBit(SyntecRemoteCNC cnc, int port, byte data)
        {
            return (cnc.WRITE_plc_cbit(port, port, new byte[] { data }) == 0);
        }

        public static bool WritePlcIBit(CNCMachine machine, int port, byte data)
        {
            if (machine == null)
            {
                return false;
            }
            return WritePlcIBit(GetCNCStation(machine.MachineIP.ToString()), port, data);
        }

        public static bool WritePlcIBit(SyntecRemoteCNC cnc, int port, byte data)
        {
            return (cnc.WRITE_plc_ibit(port, port, new byte[] { data }) == 0);
        }
        public static bool WritePlcSBit(CNCMachine machine, int port, byte data)
        {
            if (machine == null)
            {
                return false;
            }
            return WritePlcSBit(GetCNCStation(machine.MachineIP.ToString()), port, data);
        }

        public static bool WritePlcSBit(SyntecRemoteCNC cnc, int port, byte data)
        {
            return (cnc.WRITE_plc_sbit(port, port, new byte[] { data }) == 0);
        }

        public static bool WriteRegisterData(CNCMachine machine, int registerPort, int data)
        {
            if (machine == null)
            {
                return false;
            }
            return WriteRegisterData(GetCNCStation(machine.MachineIP.ToString()), registerPort, data);
        }

        public static bool WriteRegisterData(SyntecRemoteCNC cnc, int registerPort, int data)
        {
            return (cnc.WRITE_plc_register(registerPort, registerPort, new int[] { data }) == 0);
        }
    }
}

