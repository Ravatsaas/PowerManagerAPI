using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;


namespace PowerManagerAPI
{
    [Flags]
    public enum PowerMode
    {
        AC = 1,
        DC = 2,
    }

    public static class PowerManager
    {
        /// <summary>
        /// Gets the currently active power plan
        /// </summary>
        /// <returns>Guid of the currently acvtive plan</returns>
        public static Guid GetActivePlan()
        {
            IntPtr activePolicyGuidPtr = IntPtr.Zero;
            var res = PowerGetActiveScheme(IntPtr.Zero, out activePolicyGuidPtr);

            if (res != (uint)ErrorCode.SUCCESS)
                throw new Win32Exception((int)res);

            var guid = Marshal.PtrToStructure<Guid>(activePolicyGuidPtr);

            LocalFree(activePolicyGuidPtr);

            return guid;
        }

        /// <summary>
        /// Sets the active power plan
        /// </summary>
        /// <param name="planId">The plan that should be set active.</param>
        public static void SetActivePlan(Guid planId)
        {
            var res = PowerSetActiveScheme(IntPtr.Zero, ref planId);

            if (res != (uint)ErrorCode.SUCCESS)
                throw new Win32Exception((int)res);
        }

        /// <summary>
        /// Gets the friendly name of a power plan
        /// </summary>
        /// <param name="planId">The Guid of the power plan</param>
        /// <returns>Plan name</returns>
        public static string GetPlanName(Guid planId)
        {
            uint bufferSize = 255;
            IntPtr buffer =  Marshal.AllocHGlobal((int)bufferSize);

            try
            {
                var res = PowerReadFriendlyName(IntPtr.Zero, ref planId, IntPtr.Zero, IntPtr.Zero, buffer, ref bufferSize);

                if (res == (uint)ErrorCode.MORE_DATA) 
                {
                    // The buffer was too small. The API function has already updated the value that bufferSize points to 
                    // to be the needed size, so all we need is to create a buffer of that size and run the API call again.
                    Marshal.FreeHGlobal(buffer);
                    buffer = Marshal.AllocHGlobal((int)bufferSize); 
                    res = PowerReadFriendlyName(IntPtr.Zero, ref planId, IntPtr.Zero, IntPtr.Zero, buffer, ref bufferSize);
                }

                if (res != (uint)ErrorCode.SUCCESS)
                    throw new Win32Exception((int)res);

                return Marshal.PtrToStringUni(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// Sets the friendly name of a power plan
        /// </summary>
        /// <param name="planId">The Guid of the power plan</param>
        /// <param name="name">The new name</param>
        public static void SetPlanName (Guid planId, string name)
        {
            name += char.MinValue; // Null-terminate the name string.
            uint bufferSize = (uint)Encoding.Unicode.GetByteCount(name);

            var res = PowerWriteFriendlyName(IntPtr.Zero, ref planId, IntPtr.Zero, IntPtr.Zero, name, bufferSize);

            if (res != (uint)ErrorCode.SUCCESS)
                throw new Win32Exception((int)res);
        }

        /// <summary>
        /// Returns the description of a power plan.
        /// </summary>
        /// <param name="planId">Guid for the plan</param>
        /// <returns>Description</returns>
        public static string GetPlanDescription(Guid planId)
        {
            uint bufferSize = 255;
            IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);

            try
            {
                var res = PowerReadDescription(IntPtr.Zero, ref planId, IntPtr.Zero, IntPtr.Zero, buffer, ref bufferSize);

                if (res == (uint)ErrorCode.MORE_DATA)
                {
                    // The buffer was too small. The API function has already updated the value that bufferSize points to 
                    // to be the needed size, so all we need is to create a buffer of that size and run the API call again.
                    Marshal.FreeHGlobal(buffer);
                    buffer = Marshal.AllocHGlobal((int)bufferSize);
                    res = PowerReadDescription(IntPtr.Zero, ref planId, IntPtr.Zero, IntPtr.Zero, buffer, ref bufferSize);
                }

                if (res != (uint)ErrorCode.SUCCESS)
                    throw new Win32Exception((int)res);

                return Marshal.PtrToStringUni(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// Sets the description of a power plan
        /// </summary>
        /// <param name="planId">The Guid of the power plan</param>
        /// <param name="description">The new description</param>
        public static void SetPlanDescription(Guid planId, string description)
        {
            description += char.MinValue; // Null-terminate the description string.
            uint bufferSize = (uint)Encoding.Unicode.GetByteCount(description);

            var res = PowerWriteDescription(IntPtr.Zero, ref planId, IntPtr.Zero, IntPtr.Zero, description, bufferSize);

            if (res != (uint)ErrorCode.SUCCESS)
                throw new Win32Exception((int)res);
        }

        /// <summary>
        /// Creates a new power plan based on the provided source plan
        /// </summary>
        /// <param name="sourcePlanId">The Guid for the source plan.</param>
        /// <param name="targetPlanId">The Guid for the new plan to be created. If no guid is supplied one will be created.</param>
        /// <returns></returns>
        public static Guid DuplicatePlan(Guid sourcePlanId, Guid targetPlanId = new Guid())
        {
            if (targetPlanId == Guid.Empty)
                targetPlanId = Guid.NewGuid();

            var targetPlanPtr = Marshal.AllocHGlobal(Marshal.SizeOf(targetPlanId));
            uint res; 

            try
            {
                Marshal.StructureToPtr(targetPlanId, targetPlanPtr, false);
                res = PowerDuplicateScheme(IntPtr.Zero, ref sourcePlanId, ref targetPlanPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(targetPlanPtr);
            }

            if (res != (uint)ErrorCode.SUCCESS)
                throw new Win32Exception((int)res);

            return targetPlanId;
        }

        /// <summary>
        /// Deletes the specified power plan
        /// </summary>
        /// <param name="planId">Guid for the power plan to be deleted</param>
        public static void DeletePlan(Guid planId)
        {
            var res = PowerDeleteScheme(IntPtr.Zero, ref planId);

            if (res != (uint)ErrorCode.SUCCESS)
                throw new Win32Exception((int)res);
        }

        /// <summary>
        /// Gets the value for the specified power plan, power mode and setting
        /// </summary>
        /// <param name="plan">Guid of the power plan</param>
        /// <param name="subgroup">The subgroup to look in</param>
        /// <param name="setting">The settign to look up</param>
        /// <param name="powerMode">Power mode. AC or DC, but not both.</param>
        /// <returns></returns>
        public static uint GetPlanSetting(Guid plan, SettingSubgroup subgroup, Setting setting, PowerMode powerMode)
        {
            if (powerMode == (PowerMode.AC | PowerMode.DC))
                throw new ArgumentException("Can't get both AC and DC values at the same time, because they may be different.");

            Guid subgroupId = SettingIdLookup.SettingSubgroupGuids[subgroup];
            Guid settingId = SettingIdLookup.SettingGuids[setting];

            uint value = 0;
            uint res = 0;

            if (powerMode.HasFlag(PowerMode.AC))
            {
                res = PowerReadACValueIndex(IntPtr.Zero, ref plan, ref subgroupId, ref settingId, out value);
            }
            else if (powerMode.HasFlag(PowerMode.DC))
            {
                res = PowerReadDCValueIndex(IntPtr.Zero, ref plan, ref subgroupId, ref settingId, out value);
            }

            if (res != (uint)ErrorCode.SUCCESS)
                throw new Win32Exception((int)res);

            return value;
        }

        /// <summary>
        /// Alters a setting on a power plan.
        /// </summary>
        /// <param name="plan">The Guid for the plan you are changing</param>
        /// <param name="subgroup">The Guid for the subgroup the setting belongs to</param>
        /// <param name="setting">The Guid for the setting you are changing</param>
        /// <param name="powerMode">You can chose to alter the AC value, the DC value or both using the bitwise OR operator (|) to join the flags.</param>
        /// <param name="value">The new value for the setting. Run <code>powercfg -q</code> from the command line to list possible values</param>
        public static void SetPlanSetting(Guid plan, SettingSubgroup subgroup, Setting setting, PowerMode powerMode, uint value)
        {
            Guid subgroupId = SettingIdLookup.SettingSubgroupGuids[subgroup];
            Guid settingId = SettingIdLookup.SettingGuids[setting];

            if (powerMode.HasFlag(PowerMode.AC))
            {
                var res = PowerWriteACValueIndex(IntPtr.Zero, ref plan, ref subgroupId, ref settingId, value);
                if (res != (uint)ErrorCode.SUCCESS)
                    throw new Win32Exception((int)res);
            }
            if (powerMode.HasFlag(PowerMode.DC))
            {
                var res = PowerWriteDCValueIndex(IntPtr.Zero, ref plan, ref subgroupId, ref settingId, value);
                if (res != (uint)ErrorCode.SUCCESS)
                    throw new Win32Exception((int)res);
            }
        }

        #region DLL Imports

        [DllImport("powrprof.dll")]
        private static extern uint PowerGetActiveScheme(
            [In, Optional] IntPtr UserPowerKey,
            [Out] out IntPtr ActivePolicyGuid
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerSetActiveScheme(
            [In, Optional] IntPtr UserPowerKey,
            [In] ref Guid ActivePolicyGuid
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerDuplicateScheme(
            [In, Optional] IntPtr RootPowerKey,
            [In] ref Guid SourceSchemeGuid,
            [In] ref IntPtr DestinationSchemeGuid
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerDeleteScheme(
            [In, Optional]  IntPtr      RootPowerKey,
            [In]            ref Guid    SchemeGuid
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerReadFriendlyName(
            [In, Optional]  IntPtr      RootPowerKey,
            [In, Optional]  ref Guid    SchemeGuid,
            [In, Optional]  IntPtr      SubGroupOfPowerSettingsGuid,
            [In, Optional]  IntPtr      PowerSettingGuid,
            [Out, Optional] IntPtr      Buffer,
            [In, Out]       ref uint    BufferSize
        );

        [DllImport("powrprof.dll", CharSet = CharSet.Unicode)]
        private static extern uint PowerWriteFriendlyName(
            [In, Optional]  IntPtr      RootPowerKey,
            [In]            ref Guid    SchemeGuid,
            [In, Optional]  IntPtr      SubGroupOfPowerSettingsGuid,
            [In, Optional]  IntPtr      PowerSettingGuid,
            [In]            string      Buffer,
            [In]            UInt32      BufferSize
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerReadDescription(
            [In, Optional]  IntPtr      RootPowerKey,
            [In, Optional]  ref Guid    SchemeGuid,
            [In, Optional]  IntPtr      SubGroupOfPowerSettingsGuid,
            [In, Optional]  IntPtr      PowerSettingGuid,
            [Out, Optional] IntPtr      Buffer,
            [In, Out]       ref uint    BufferSize
        );

        [DllImport("powrprof.dll", CharSet = CharSet.Unicode)]
        private static extern uint PowerWriteDescription(
            [In, Optional]  IntPtr      RootPowerKey,
            [In]            ref Guid    SchemeGuid,
            [In, Optional]  IntPtr      SubGroupOfPowerSettingsGuid,
            [In, Optional]  IntPtr      PowerSettingGuid,
            [In]            string      Buffer,
            [In]            UInt32      BufferSize
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerReadACValueIndex(
            [In, Optional]  IntPtr      RootPowerKey,
            [In, Optional]  ref Guid    SchemeGuid,
            [In, Optional]  ref Guid    SubGroupOfPowerSettingsGuid,
            [In, Optional]  ref Guid    PowerSettingGuid,
            [Out]           out uint    AcValueIndex
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerWriteACValueIndex(
            [In, Optional]  IntPtr      RootPowerKey,
            [In]            ref Guid    SchemeGuid,
            [In, Optional]  ref Guid    SubGroupOfPowerSettingsGuid,
            [In, Optional]  ref Guid    PowerSettingGuid,
            [In]            uint        AcValueIndex
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerReadDCValueIndex(
            [In, Optional]  IntPtr      RootPowerKey,
            [In, Optional]  ref Guid    SchemeGuid,
            [In, Optional]  ref Guid    SubGroupOfPowerSettingsGuid,
            [In, Optional]  ref Guid    PowerSettingGuid,
            [Out]           out uint    DcValueIndex
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerWriteDCValueIndex(
            [In, Optional]  IntPtr      RootPowerKey,
            [In]            ref Guid    SchemeGuid,
            [In, Optional]  ref Guid    SubGroupOfPowerSettingsGuid,
            [In, Optional]  ref Guid    PowerSettingGuid,
            [In]            uint        DcValueIndex
        );



        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(
            [In]            IntPtr      hMem
        );
        #endregion
    }
}
