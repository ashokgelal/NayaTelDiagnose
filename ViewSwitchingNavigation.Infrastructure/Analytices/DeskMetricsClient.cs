using Newtonsoft.Json;
using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Com.DeskMetrics
{
    
    public enum DMAOption
    {
        CrashReportingEnabled = 0
    }


    public enum DMASessionType
    {
        Installer = 0,
        Uninstaller,
        Application,
    }


    public enum DMAExitState
    {
        Succeeded = 0,
        Failed,
        Cancelled
    }


    public enum DMAErrorMode
    {
        FailSilent,
        RaiseException
    }


    public class ClickData
    {

        public string ClickId { get; set; }

        public string TrackingLinkKey { get; set; }
    }


    public class DeskMetricsException : Exception
    {

        public DeskMetricsException(string message) : base(message) {}
    }


    /// <summary>
    /// DeskMetrics
    /// </summary>
    public class DeskMetricsClient
    {
        // DMA_WAIT_INFINITE -> 0xffffffff
        public static readonly uint DMA_WAIT_INFINITE = uint.MaxValue;

        /// DMA_EVENT_NAME_MAX_LENGTH -> 64
        public static readonly int DMA_EVENT_NAME_MAX_LENGTH = 64;

        /// DMA_EVENT_BODY_MAX_LENGTH -> 8999
        public static readonly int DMA_EVENT_BODY_MAX_LENGTH = 8999;

        /// DMA_EVENT_PROPERTY_NAME_MAX_LENGTH -> 255
        public static readonly int DMA_EVENT_PROPERTY_NAME_MAX_LENGTH = 255;

        /// DMA_CLICK_ID_LENGTH -> 37
        public static readonly int DMA_CLICK_ID_LENGTH = 37;

        /// DMA_TRACKING_LINK_KEY_MAX_LENGTH -> 20
        public static readonly int DMA_TRACKING_LINK_KEY_MAX_LENGTH = 20;


        private enum DMAError
        {
            DMA_ERR_OK = 0
        }


        #region Native

        private class NativeMethods
        {

            /// Return Type: dma_wchar_t*
            ///err: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            [DllImportAttribute("dma.dll", EntryPoint = "dma_error_string_w", CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr dma_error_string_w(DMAError err);

            
            [DllImportAttribute("dma.dll", EntryPoint = "dma_set_option", CallingConvention = CallingConvention.StdCall)]
            public static extern void dma_set_option(DMAOption option, bool enabled);

            [DllImportAttribute("dma.dll", EntryPoint = "dma_get_option", CallingConvention = CallingConvention.StdCall)]
            public static extern bool dma_get_option(DMAOption option);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///project_key: dma_wchar_t*
            ///tracking_id: dma_wchar_t*
            ///out_click_id: dma_wchar_t*
            ///out_tracking_link_id: dma_wchar_t*
            [DllImportAttribute("dma.dll", EntryPoint = "dma_get_click_data_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_click_data_w([MarshalAs(UnmanagedType.LPWStr)] string project_key, [MarshalAs(UnmanagedType.LPWStr)] string tracking_id, [MarshalAs(UnmanagedType.LPWStr), Out()] StringBuilder out_click_id, [MarshalAs(UnmanagedType.LPWStr), Out()] StringBuilder out_tracking_link_id);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///timeout_ms: unsigned int
            ///out_timestamp: unsigned int*
            [DllImportAttribute("dma.dll", EntryPoint = "dma_get_timestamp", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_timestamp([MarshalAs(UnmanagedType.U4)] uint timeout_ms, [MarshalAs(UnmanagedType.U4), Out()] out uint out_timestamp);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///reg_value_path: dma_wchar_t*
            ///reg_view_64: dma_bool_t->int
            [DllImportAttribute("dma.dll", EntryPoint = "dma_load_properties_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_load_properties_w([MarshalAs(UnmanagedType.LPWStr)] string reg_value_path, [MarshalAs(UnmanagedType.Bool)] bool reg_view_64);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///reg_value_path: dma_wchar_t*
            ///reg_view_64: dma_bool_t->int
            [DllImportAttribute("dma.dll", EntryPoint = "dma_save_properties_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_save_properties_w([MarshalAs(UnmanagedType.LPWStr)] string reg_value_path, [MarshalAs(UnmanagedType.Bool)] bool reg_view_64);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value: dma_wchar_t*
            ///overwrite: dma_bool_t->int
            [DllImportAttribute("dma.dll", EntryPoint = "dma_set_property_string_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_set_property_string_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] string value, [MarshalAs(UnmanagedType.Bool)] bool overwrite);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value: int
            ///overwrite: dma_bool_t->int
            [DllImportAttribute("dma.dll", EntryPoint = "dma_set_property_int64_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_set_property_int64_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.I8)] long value, [MarshalAs(UnmanagedType.Bool)] bool overwrite);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value: double
            ///overwrite: dma_bool_t->int
            [DllImportAttribute("dma.dll", EntryPoint = "dma_set_property_double_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_set_property_double_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.R8)] double value, [MarshalAs(UnmanagedType.Bool)] bool overwrite);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value: dma_bool_t->int
            ///overwrite: dma_bool_t->int
            [DllImportAttribute("dma.dll", EntryPoint = "dma_set_property_bool_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_set_property_bool_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.Bool)] bool value, [MarshalAs(UnmanagedType.Bool)] bool overwrite);



            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value_out: dma_wchar_t*
            ///length_characters_inout: unsigned int*
            [DllImportAttribute("dma.dll", EntryPoint = "dma_get_property_string_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_property_string_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr), Out()] StringBuilder value_out, [MarshalAs(UnmanagedType.U4), In(), Out()] ref uint length_characters_inout);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value_out: int*
            [DllImportAttribute("dma.dll", EntryPoint = "dma_get_property_int64_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_property_int64_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.I8), Out()] out long value_out);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value_out: double*
            [DllImportAttribute("dma.dll", EntryPoint = "dma_get_property_double_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_property_double_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.R8)] out double value_out);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value_out: dma_bool_t*
            [DllImportAttribute("dma.dll", EntryPoint = "dma_get_property_bool_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_property_bool_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.Bool), Out()] out bool value_out);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///event_name: dma_wchar_t*
            ///json_string: dma_wchar_t*
            [DllImportAttribute("dma.dll", EntryPoint = "dma_send_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_send_w([MarshalAs(UnmanagedType.LPWStr)] string event_name, [MarshalAs(UnmanagedType.LPWStr)] string json_string);

            [DllImportAttribute("dma.dll", EntryPoint = "dma_blocking_send_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_blocking_send_w([MarshalAs(UnmanagedType.LPWStr)] string event_name, [MarshalAs(UnmanagedType.LPWStr)] string json_string);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///session_type: dma_session_type_t->Anonymous_e8ffe953_de48_42ea_9d0d_b7ef7e1f008c
            ///project_key: dma_wchar_t*
            [DllImportAttribute("dma.dll", EntryPoint = "dma_start_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_start_w(DMASessionType session_type, [MarshalAs(UnmanagedType.LPWStr)] string project_key);


            /// Return Type: DMAError->Anonymous_62642021_e054_4c29_bb8d_d192e79f3647
            ///exit_state: dma_exit_state_t->Anonymous_5e66b748_b67b_40fa_9f3a_10f9423b4845
            ///timeout_ms: unsigned int
            [DllImportAttribute("dma.dll", EntryPoint = "dma_stop", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_stop(DMAExitState exit_state, [MarshalAs(UnmanagedType.U4)] uint timeout_ms);

        }

        #endregion


        #region Native64

        private class NativeMethods64
        {

            /// Return Type: dma_wchar_t*
            ///err: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_error_string_w", CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr dma_error_string_w(DMAError err);


            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_set_option", CallingConvention = CallingConvention.StdCall)]
            public static extern void dma_set_option(DMAOption option, bool enabled);

            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_get_option", CallingConvention = CallingConvention.StdCall)]
            public static extern bool dma_get_option(DMAOption option);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///project_key: dma_wchar_t*
            ///tracking_id: dma_wchar_t*
            ///out_click_id: dma_wchar_t*
            ///out_tracking_link_id: dma_wchar_t*
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_get_click_data_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_click_data_w([MarshalAs(UnmanagedType.LPWStr)] string project_key, [MarshalAs(UnmanagedType.LPWStr)] string tracking_id, [MarshalAs(UnmanagedType.LPWStr), Out()] StringBuilder out_click_id, [MarshalAs(UnmanagedType.LPWStr), Out()] StringBuilder out_tracking_link_id);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///timeout_ms: unsigned int
            ///out_timestamp: unsigned int*
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_get_timestamp", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_timestamp([MarshalAs(UnmanagedType.U4)] uint timeout_ms, [MarshalAs(UnmanagedType.U4), Out()] out uint out_timestamp);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///reg_value_path: dma_wchar_t*
            ///reg_view_64: dma_bool_t->int
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_load_properties_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_load_properties_w([MarshalAs(UnmanagedType.LPWStr)] string reg_value_path, [MarshalAs(UnmanagedType.Bool)] bool reg_view_64);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///reg_value_path: dma_wchar_t*
            ///reg_view_64: dma_bool_t->int
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_save_properties_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_save_properties_w([MarshalAs(UnmanagedType.LPWStr)] string reg_value_path, [MarshalAs(UnmanagedType.Bool)] bool reg_view_64);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value: dma_wchar_t*
            ///overwrite: dma_bool_t->int
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_set_property_string_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_set_property_string_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] string value, [MarshalAs(UnmanagedType.Bool)] bool overwrite);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value: int
            ///overwrite: dma_bool_t->int
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_set_property_int64_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_set_property_int64_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.I8)] long value, [MarshalAs(UnmanagedType.Bool)] bool overwrite);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value: double
            ///overwrite: dma_bool_t->int
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_set_property_double_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_set_property_double_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.R8)] double value, [MarshalAs(UnmanagedType.Bool)] bool overwrite);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value: dma_bool_t->int
            ///overwrite: dma_bool_t->int
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_set_property_bool_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_set_property_bool_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.Bool)] bool value, [MarshalAs(UnmanagedType.Bool)] bool overwrite);



            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value_out: dma_wchar_t*
            ///length_characters_inout: unsigned int*
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_get_property_string_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_property_string_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr), Out()] StringBuilder value_out, [MarshalAs(UnmanagedType.U4), In(), Out()] ref uint length_characters_inout);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value_out: int*
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_get_property_int64_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_property_int64_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.I8), Out()] out long value_out);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value_out: double*
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_get_property_double_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_property_double_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.R8)] out double value_out);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///name: dma_wchar_t*
            ///value_out: dma_bool_t*
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_get_property_bool_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_get_property_bool_w([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.Bool), Out()] out bool value_out);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///event_name: dma_wchar_t*
            ///json_string: dma_wchar_t*
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_send_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_send_w([MarshalAs(UnmanagedType.LPWStr)] string event_name, [MarshalAs(UnmanagedType.LPWStr)] string json_string);

            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_blocking_send_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_blocking_send_w([MarshalAs(UnmanagedType.LPWStr)] string event_name, [MarshalAs(UnmanagedType.LPWStr)] string json_string);


            /// Return Type: DMAError->Anonymous_89f2ec8d_0146_40b7_9db4_f0aa919ae780
            ///session_type: dma_session_type_t->Anonymous_e8ffe953_de48_42ea_9d0d_b7ef7e1f008c
            ///project_key: dma_wchar_t*
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_start_w", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_start_w(DMASessionType session_type, [MarshalAs(UnmanagedType.LPWStr)] string project_key);
            

            /// Return Type: DMAError->Anonymous_62642021_e054_4c29_bb8d_d192e79f3647
            ///exit_state: dma_exit_state_t->Anonymous_5e66b748_b67b_40fa_9f3a_10f9423b4845
            ///timeout_ms: unsigned int
            [DllImportAttribute("dma_x64.dll", EntryPoint = "dma_stop", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            public static extern DMAError dma_stop(DMAExitState exit_state, [MarshalAs(UnmanagedType.U4)] uint timeout_ms);

        }

        #endregion


        private static readonly DeskMetricsClient _instance = new DeskMetricsClient();


        DeskMetricsClient()
        {
            ErrorMode = DMAErrorMode.FailSilent;
            #if DEBUG
            ErrorMode = DMAErrorMode.RaiseException;
            #endif
        }


        ~DeskMetricsClient()
        {
            Stop(DMAExitState.Succeeded, DMA_WAIT_INFINITE);
        }


        /// <summary>
        /// Gets the DeskMetrics instance.
        /// </summary>
        public static DeskMetricsClient Instance
        {
            get
            {
                return _instance;
            }
        }


        /// <summary>
        /// Gets the or sets the error handling behavior of the SDK.
        /// </summary>
        public DMAErrorMode ErrorMode { get; set; }


        /// <summary>
        /// Sets an SDK option.  Options must be configured before starting the SDK.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="enabled"></param>
        public void SetOption(DMAOption option, bool enabled)
        {
            try
            {
                if (IntPtr.Size == 4)
                    NativeMethods.dma_set_option(option, enabled);
                else
                    NativeMethods64.dma_set_option(option, enabled);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }

        /// <summary>
        /// Gets an SDK option.
        /// </summary>
        /// <param name="option"></param>
        /// <returns>A boolean indicating whether the option is enabled</returns>
        public bool GetOption(DMAOption option)
        {
            try
            {
                if (IntPtr.Size == 4)
                    return NativeMethods.dma_get_option(option);
                else
                    return NativeMethods64.dma_get_option(option);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
            return false;
        }


        /// <summary>
        /// Gets click data associated with the given projectKey and trackingId if available.
        /// </summary>
        /// <param name="projectKey"></param>
        /// <param name="trackingId"></param>
        /// <returns>An object containing the click data.</returns>
        public ClickData GetClickData(string projectKey, string trackingId)
        {
            StringBuilder clickId = new StringBuilder(DMA_CLICK_ID_LENGTH);
            StringBuilder trackingLinkId = new StringBuilder(DMA_TRACKING_LINK_KEY_MAX_LENGTH);

            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_get_click_data_w(projectKey, trackingId, clickId, trackingLinkId));
                else
                    Check(NativeMethods64.dma_get_click_data_w(projectKey, trackingId, clickId, trackingLinkId));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }

            return new ClickData() { ClickId = clickId.ToString(), TrackingLinkKey = trackingId.ToString() };
        }


        /// <summary>
        /// Gets the current time from a network time resource as a unix timestamp.
        /// </summary>
        /// <param name="timeoutMs">Number of milliseconds to wait before falling back to the system time.</param>
        /// <returns></returns>
        public uint GetTimestamp(uint timeoutMs)
        {
            uint timestamp = 0;

            try
            {
                if (IntPtr.Size == 4)
                    NativeMethods.dma_get_timestamp(timeoutMs, out timestamp);
                else
                    NativeMethods64.dma_get_timestamp(timeoutMs, out timestamp);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }

            return timestamp;
        }


        /// <summary>
        /// Loads properties from the given registry value.
        /// </summary>
        /// <param name="regValuePath">Path to the registry value.</param>
        /// <param name="regView64">Boolean indicating whether to use the 64-bit view of the registry on 64-bit systems.</param>
        /// <returns></returns>
        public bool LoadProperties(string regValuePath, bool regView64)
        {
            bool result = false;

            try
            {
                if (IntPtr.Size == 4)
                    result = (DMAError.DMA_ERR_OK == NativeMethods.dma_load_properties_w(regValuePath, regView64));
                else
                    result = (DMAError.DMA_ERR_OK == NativeMethods64.dma_load_properties_w(regValuePath, regView64));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }

            return result;
        }


        /// <summary>
        /// Saves properties to the given registry value. DeskMetrics will create the
        /// value and its immiediate parent key if not already present.
        /// </summary>
        /// <param name="regValuePath">Path to the registry value.</param>
        /// <param name="regView64">Boolean indicating whether to use the 64-bit view of the registry on 64-bit systems.</param>
        public void SaveProperties(string regValuePath, bool regView64)
        {
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_save_properties_w(regValuePath, regView64));
                else
                    Check(NativeMethods64.dma_save_properties_w(regValuePath, regView64));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }


        /// <summary>
        /// Sets a string property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="overwrite"></param>
        public void SetProperty(string name, string value, bool overwrite)
        {
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_set_property_string_w(name, value, overwrite));
                else
                    Check(NativeMethods64.dma_set_property_string_w(name, value, overwrite));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }


        /// <summary>
        /// Sets a long property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="overwrite"></param>
        public void SetProperty(string name, long value, bool overwrite)
        {
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_set_property_int64_w(name, value, overwrite));
                else
                    Check(NativeMethods64.dma_set_property_int64_w(name, value, overwrite));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }


        /// <summary>
        /// Sets a double property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="overwrite"></param>
        public void SetProperty(string name, double value, bool overwrite)
        {
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_set_property_double_w(name, value, overwrite));
                else
                    Check(NativeMethods64.dma_set_property_double_w(name, value, overwrite));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }


        /// <summary>
        /// Sets a boolean property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="overwrite"></param>
        public void SetProperty(string name, bool value, bool overwrite)
        {
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_set_property_bool_w(name, value, overwrite));
                else
                    Check(NativeMethods64.dma_set_property_bool_w(name, value, overwrite));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }


        /// <summary>
        /// Gets the string property with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>True if the property exists.</returns>
        public bool GetProperty(string name, ref string value)
        {
            bool result = false;

            try
            {
                uint length = 0;

                if (IntPtr.Size == 4)
                {
                    // get length
                    if (DMAError.DMA_ERR_OK == NativeMethods.dma_get_property_string_w(name, null, ref length))
                    {
                        // constraint to int.MaxValue
                        if (length > int.MaxValue)
                            length = int.MaxValue;

                        // allocate builder
                        StringBuilder builder = new StringBuilder((int)length);
                        
                        result = (DMAError.DMA_ERR_OK == NativeMethods.dma_get_property_string_w(name, builder, ref length));
                    }
                }
                else
                {
                    // get length
                    if (DMAError.DMA_ERR_OK == NativeMethods64.dma_get_property_string_w(name, null, ref length))
                    {
                        // constraint to int.MaxValue
                        if (length > int.MaxValue)
                            length = int.MaxValue;

                        // allocate builder
                        StringBuilder builder = new StringBuilder((int)length);
                        
                        result = (DMAError.DMA_ERR_OK == NativeMethods64.dma_get_property_string_w(name, builder, ref length));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }

            return result;
        }


        /// <summary>
        /// Gets the long property with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>True if the property exists.</returns>
        public bool GetProperty(string name, ref long value)
        {
            bool result = false;

            try
            {
                if (IntPtr.Size == 4)
                    result = (DMAError.DMA_ERR_OK == NativeMethods.dma_get_property_int64_w(name, out value));
                else
                    result = (DMAError.DMA_ERR_OK == NativeMethods64.dma_get_property_int64_w(name, out value));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }

            return result;
        }


        /// <summary>
        /// Gets the double property with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>True if the property exists.</returns>
        public bool GetProperty(string name, ref double value)
        {
            bool result = false;

            try
            {
                if (IntPtr.Size == 4)
                    result = (DMAError.DMA_ERR_OK == NativeMethods.dma_get_property_double_w(name, out value));
                else
                    result = (DMAError.DMA_ERR_OK == NativeMethods64.dma_get_property_double_w(name, out value));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }

            return result;
        }


        /// <summary>
        /// Gets the boolean property with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>True if the property exists.</returns>
        public bool GetProperty(string name, ref bool value)
        {
            bool result = false;

            try
            {
                if (IntPtr.Size == 4)
                    result = (DMAError.DMA_ERR_OK == NativeMethods.dma_get_property_bool_w(name, out value));
                else
                    result = (DMAError.DMA_ERR_OK == NativeMethods64.dma_get_property_bool_w(name, out value));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }

            return result;
        }


        /// <summary>
        /// Start sending events.
        /// </summary>
        /// <param name="sessionType"></param>
        /// <param name="projectKey"></param>
        public void Start(DMASessionType sessionType, string projectKey)
        {
            // We want to do all the crash reporting in C#-land, so backup the crash reporting preference,
            // and set it to false while we initialize the native sdk.
            bool crashReportingEnabled = GetOption(DMAOption.CrashReportingEnabled);
            SetOption(DMAOption.CrashReportingEnabled, false);
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_start_w(sessionType, projectKey));
                else
                    Check(NativeMethods64.dma_start_w(sessionType, projectKey));
                    
                // Respect the developer's crash reporting preference.
                // If Start() is called multiple times, only the first dma_start call will succeed, so we
                // know this code is only executed once.
                if (crashReportingEnabled)
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
            finally
            {
                // Restore the preference so things look normal again
                SetOption(DMAOption.CrashReportingEnabled, crashReportingEnabled);
            }
        }



        /// <summary>
        /// Stop sending events.
        /// </summary>
        /// <param name="exitState"></param>
        /// <param name="timeoutMs">Number of milliseconds to wait for the exit event to be sent before unloading the SDK.</param>
        public void Stop(DMAExitState exitState, uint timeoutMs)
        {
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_stop(exitState, timeoutMs));
                else
                    Check(NativeMethods64.dma_stop(exitState, timeoutMs));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }


        /// <summary>
        /// Sends an event with the given name and body.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="jsonString"></param>
        public void Send(string eventName, string jsonString)
        {
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_send_w(eventName, jsonString));
                else
                    Check(NativeMethods64.dma_send_w(eventName, jsonString));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }


        /// <summary>
        /// Sends an event with the given name and body.  Blocks until the event is finished sending.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="jsonString"></param>
        public void BlockingSend(string eventName, string jsonString)
        {
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_blocking_send_w(eventName, jsonString));
                else
                    Check(NativeMethods64.dma_blocking_send_w(eventName, jsonString));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }


        /// <summary>
        /// Sends an event with the given name and body.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventData"></param>
        public void Send(string eventName, object eventData)
        {
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_send_w(eventName, JsonConvert.SerializeObject(eventData)));
                else
                    Check(NativeMethods64.dma_send_w(eventName, JsonConvert.SerializeObject(eventData)));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }


        /// <summary>
        /// Sends an event with the given name and body.  Blocks until the request is finished sending.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventData"></param>
        public void BlockingSend(string eventName, object eventData)
        {
            try
            {
                if (IntPtr.Size == 4)
                    Check(NativeMethods.dma_blocking_send_w(eventName, JsonConvert.SerializeObject(eventData)));
                else
                    Check(NativeMethods64.dma_blocking_send_w(eventName, JsonConvert.SerializeObject(eventData)));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (ErrorMode == DMAErrorMode.RaiseException)
                    throw e;
            }
        }


        private void Check(DMAError result)
        {
            if (result != DMAError.DMA_ERR_OK)
            {
                throw new DeskMetricsException(Marshal.PtrToStringUni(NativeMethods64.dma_error_string_w(result)));
            }
        }


        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                var ex = args.ExceptionObject as Exception;
                if (ex != null)
                {
                    BlockingSend("crash", new
                    {
                        type = ex.GetType().FullName,
                        message = ex.Message,
                        details = ex.StackTrace
                    });
                }
            }
            catch (Exception)
            {
            }
        }

    }

}