namespace TopStation.Common
{
    using DialogLibray.CommonClass;
    using DialogLibray.NetWorkClass;
    using PrintLibray;
    using System;
    using System.Net;

    public static class Common
    {
        private static float _axisUnitRate = 1000f;
        private static string _jobListSaveFileName;
        private static PrintLibray.LabelSet _labelSet;
        private static string _lastWorkToolSub;
        private static string _lastWorkToolSubFileName;
        private static int _machineModeCount = 8;
        private static PrintLibray.NcCodeSet _ncCodeSet;
        private static int _reConnectCount;
        private static int _resetPortInterval = 200;
        private static WorkPoint _serverPoint;
        private static CNCStation _topMachine;

        static Common()
        {
            CNCStation station = new CNCStation {
                NcSharedFolder = "",
                SetFileName = "topMachineBaseSet.dll",
                CarryLineIP = IPAddress.Parse("192.168.1.10"),
                MachineType = EnumCNCMachineType.正面机台,
                MachineName = "正面机台",
                MachineIP = IPAddress.Parse("192.168.1.11"),
                AutoLineNumber = 1,
                StationNumber = 1,
                AxisXMachineMax = 1300.0,
                AxisXMachineMin = 0.0,
                AxisYMachineMax = 2560.0,
                AxisYMachineMin = 0.0,
                AxisZMachineMin = -300.0,
                AxisZMachineMax = 3.0,
                MirrorType = EnumProductMirorType.不镜像,
                RotateType = EnumProductRotateType.不旋转,
                XMovePort = new CNCPort("X移动启动端口", 0x7da, EnumCNCAddress.R),
                YMovePort = new CNCPort("Y移动启动端口", 0x7dc, EnumCNCAddress.R),
                ZMovePort = new CNCPort("Z移动启动端口", 0x7de, EnumCNCAddress.R),
                FastMoveSelectPort = new CNCPort("快速移动切换端口", 0x7e6, EnumCNCAddress.R),
                ResetPort = new CNCPort("机台复位端口", 0x7fa, EnumCNCAddress.R),
                ProductStartPort = new CNCPort("机台加工启动端口", 0x7f8, EnumCNCAddress.R),
                PausePort = new CNCPort("机台暂停启动端口", 0x7f9, EnumCNCAddress.R),
                EmergencyPort = new CNCPort("机台急停启动端口", 0x7fb, EnumCNCAddress.R),
                GetMaterialStartPort = new CNCPort("抓料启动端口", 0x53b, EnumCNCAddress.R),
                PushMaterialStartPort = new CNCPort("送料启动端口", 0x53c, EnumCNCAddress.R),
                StoreTableStartPort = new CNCPort("储料台转动启动端口", 0x650, EnumCNCAddress.R),
                ReadModeCheckPort = new CNCCheckPort("机台准备状态检测端口", 0x7f6, EnumCNCAddress.R),
                RunModeCheckPort = new CNCCheckPort("机台运行状态检测端口", 0x7f7, EnumCNCAddress.R),
                ProductFinishedCheckPort = new CNCCheckPort("加工完成检测端口", 0x4d5, EnumCNCAddress.R),
                StoreTableStartOrStopCheckPort = new CNCCheckPort("抓料时储料台是否转动检测端口", 0x6e1, EnumCNCAddress.R),
                ResetPortIntervalTime = new CNCParam("重启端口的时间间隔", 200, "毫秒"),
                GetMaterialWaitingInterval = new CNCParam("抓料启动等待时间", 500, "毫秒"),
                GetMaterialCheckInterval = new CNCParam("抓料是否完成检测时间间隔", 0x3e8, "毫秒"),
                GetMaterialCheckCount = new CNCParam("抓料是否完成的最大检测次数", 0x2710, "次"),
                PushMaterialWaitingInverval = new CNCParam("启动推料等待时间", 500, "毫秒"),
                WaitingProductCheckInterval = new CNCParam("推料是否完成检测时间间隔", 0x3e8, "毫秒"),
                WaitingProductCheckCount = new CNCParam("推料完成是否最大检测次数", 0x2710, "次"),
                WaitingFromCarryLineGetMaterialInverval = new CNCParam("获取抓料完成信息的时间间隔", 0x3e8, "毫秒"),
                WaitingFromCarryLineGetMaterialCheckCount = new CNCParam("获取抓料完成信息的最大检查次数", 10, "次")
            };
            _topMachine = station;
            PrintLibray.LabelSet set = new PrintLibray.LabelSet {
                LabelSetSaveFileName = "labelPrintSet.dll",
                AutoPrintName = "Bar Code Printer TP9403",
                SecondPrintName = "pdfFactory Pro",
                IncludeSheetLabel = true,
                LabelType = EnumLabelPosition.贴到最大的工件,
                LabelXDist = 30f,
                LabelYDist = 40f,
                NotLabelWidth = 35f,
                SheetLabelXPostion = 1120f,
                SheetLabelYPostion = 2340f
            };
            _labelSet = set;
            WorkPoint point = new WorkPoint {
                SetFileName = "ServerPointSet.dll",
                PointName = "服务器",
                Ip = IPAddress.Parse("192.168.65.148"),
                PortNumber = 0xc350
            };
            _serverPoint = point;
            _reConnectCount = 20;
            PrintLibray.NcCodeSet set2 = new PrintLibray.NcCodeSet {
                BackDist = 40.0,
                IsSlipping = true,
                SecondCutPercent = 0.4,
                SecondCutThick = 25.0,
                SetFileName = "ncCodeSet.dll",
                SlippingDist = 40.0,
                SmallPartWidth = 160.0,
                CombineCutToolPath = true
            };
            _ncCodeSet = set2;
            _lastWorkToolSubFileName = "lastWorkToolSet.dll";
            _lastWorkToolSub = "-玻璃门内";
            _jobListSaveFileName = "DLFileList.dl";
        }

        public static float AxisUnitRate
        {
            get => 
                _axisUnitRate;
            set
            {
                _axisUnitRate = value;
            }
        }

        public static string JobListSaveFileName
        {
            get => 
                _jobListSaveFileName;
            set
            {
                _jobListSaveFileName = value;
            }
        }

        public static PrintLibray.LabelSet LabelSet
        {
            get => 
                _labelSet;
            set
            {
                _labelSet = value;
            }
        }

        public static string LastWorkToolSub
        {
            get => 
                _lastWorkToolSub;
            set
            {
                _lastWorkToolSub = value;
            }
        }

        public static string LastWorkToolSubFileName
        {
            get => 
                _lastWorkToolSubFileName;
            set
            {
                _lastWorkToolSubFileName = value;
            }
        }

        public static int MachineModeCount
        {
            get => 
                _machineModeCount;
            set
            {
                _machineModeCount = value;
            }
        }

        public static PrintLibray.NcCodeSet NcCodeSet
        {
            get => 
                _ncCodeSet;
            set
            {
                _ncCodeSet = value;
            }
        }

        public static int ReConnectCount
        {
            get => 
                _reConnectCount;
            set
            {
                _reConnectCount = value;
            }
        }

        public static int ResetPortInterval
        {
            get => 
                _resetPortInterval;
            set
            {
                _resetPortInterval = value;
            }
        }

        public static WorkPoint ServerPoint
        {
            get => 
                _serverPoint;
            set
            {
                _serverPoint = value;
            }
        }

        public static CNCStation TopMachine
        {
            get => 
                _topMachine;
            set
            {
                _topMachine = value;
            }
        }
    }
}

