using System;
using System.Runtime.InteropServices;

namespace Rayin
{
    internal class RayinAPI
    {
        #region dll宣告
        //登入回傳事件
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void OnLoginEvent(string status);

        //連線事件
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void OnConnectEvent();

        //斷線事件
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void OnDisConnectEvent();

        //錯誤事件
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void OnErrorEvent(int errCode, string errMsg);
        //新行情事件 
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void OnNewQuoteEvent(string NewQuote);

        //委託回報事件  
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void OnInstantAckEvent(string ExecType, string ClOrdId, string BranchId, string Account, string OrderDate, string OrderTime, string OrderID, string Symbol, int ExCode, string Side, string OrdType, int PriceFlag, Single Price, int OrderQty, int BeforeQty, int AfterQty, string TimeInForce, string errCode, string errMsg);

        //主動回報事件
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void OnNewAckEvent(string LineNo, string ClOrdId, string OrgClOrdId, string BranchId, string Account, string OrderDate, string OrderTime, string OrderID, string Symbol, int ExCode, string Side, string OrdType, int PriceFlag, Single Price, int OrderQty, string errCode, string errMsg, [MarshalAs(UnmanagedType.AnsiBStr)] string ExecType, int BeforeQty, int AfterQty, string TimeInForce, string UserData);

        //成交回報事件
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void OnNewMatEvent(string LineNo, string BranchId, string Account, string OrderID, string Symbol, int ExCode, string Side, string OrdType, string MatTime, Single MatPrice, int MatQty, string MatSeq);

        //訊息提醒事件
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void OnMsgAlertEvent(int errCode, string errMsg);

        //2022/06/30 取得委託序號 舊的不建議使用
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void NewGetClOrdId(ref IntPtr ClOrdId);
        //使用多執行緒模式 在使用Login函數之前設定
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetMultiThread(bool flag);



        //登入函數
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Login(string account, string password);

        //登入函數
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Logout();

        //下單連線函數
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ConnectToOrderServer();
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DisconnecrOrderServer();

        //回報連線函數
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ConnectToAckMatServer();
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DisconnecrAckMatServer();

        //行情連線函數
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ConnectToQuoteServer();
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool DisconnecrQuoteServer();

        //斷線函數
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Disconnect();

        //是否使用簡易行情 舊版行情已淘汰，預設已使用簡易行情
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetFastQuote(bool flag);

        //是否使用除錯模式
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetDebugMode(bool flag);

        //取得委託序號
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetClOrdId();


        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetRecvTimeout(int t);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOnLoginEvent(OnLoginEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOnOdrConnectEvent(OnConnectEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOnOdrDisConnectEvent(OnDisConnectEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOnOdrErrorEvent(OnErrorEvent e);

        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOnAckMatConnectEvent(OnConnectEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOnAckMatDisConnectEvent(OnDisConnectEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOnAckMatErrorEvent(OnErrorEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetMsgAlertEvent(OnMsgAlertEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOnInstantAckEvent(OnInstantAckEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetNewAckEvent(OnNewAckEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetNewMatEvent(OnNewMatEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetNewQuoteEvent(OnNewQuoteEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOnQuoteConnectEvent(OnConnectEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOnQuoteDisConnectEvent(OnDisConnectEvent e);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetServer(string host, int port);

        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void TestCallback(string branch_id, string cust_id, ref string errCode, ref string errMsg);

        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void AddQuote(int qtype, string stock_no, ref IntPtr errCode, ref IntPtr errMsg);
        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void DelQuote(int qtype, string stock_no, ref IntPtr errCode, ref IntPtr errMsg);

        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void NewOrder(string BranchId, string Account, string Symbol, int ExCode, string Side, string OrdType, Single Price, int PriceFlag, int OrderQty, string TimeInForce, string UserData, ref IntPtr ClOrdId, ref IntPtr errCode, ref IntPtr errMsg);

        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void ChgPriceOrder(string BranchId, string Account, string OrderID, string Symbol, string OrdType, int ExCode, Single Price, int PriceFlag, string TimeInForce, string UserData, ref IntPtr ClOrdId, ref IntPtr errCode, ref IntPtr errMsg);

        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void ChgQtyOrder(string BranchId, string Account, string OrderID, string Symbol, string OrdType, int ExCode, int OrderQty, int PriceFlag, string UserData, ref IntPtr ClOrdId, ref IntPtr errCode, ref IntPtr errMsg);

        [DllImport("RayinVTS.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Recover(string aValue);

        #endregion
    }
}
