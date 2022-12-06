using System.Text;
using Hardware.Info;
using Humanizer.Bytes;

namespace Buddhabrot.Core.Utilities
{
    public static class ComputerDescription
    {
        public static string GetMultiline()
        {
            var hardwareInfo = new HardwareInfo(useAsteriskInWMI: false);
            hardwareInfo.RefreshOperatingSystem();
            hardwareInfo.RefreshMemoryStatus();
            hardwareInfo.RefreshCPUList(includePercentProcessorTime: false);

            var sb = new StringBuilder();

            sb.AppendLine($"{hardwareInfo.OperatingSystem.Name} ({hardwareInfo.OperatingSystem.VersionString})");

            foreach (var cpu in hardwareInfo.CpuList)
            {
                sb.AppendLine($"{cpu.Name.Trim()} ({cpu.NumberOfLogicalProcessors} Cores, {cpu.MaxClockSpeed:N0} MHz)");
            }

            sb.AppendLine(new ByteSize(hardwareInfo.MemoryStatus.TotalPhysical).ToString());

            return sb.ToString();
        }


        public static string GetSingleLine()
        {
            var hardwareInfo = new HardwareInfo(useAsteriskInWMI: false);
            hardwareInfo.RefreshOperatingSystem();
            hardwareInfo.RefreshMemoryStatus();
            hardwareInfo.RefreshCPUList(includePercentProcessorTime: false);

            static string FormatFrequency(uint mhz) => mhz == 0 ? string.Empty : $"{mhz / 1000d:N2} GHz ";

            var cpus = hardwareInfo.CpuList.Select(cpu =>
                $"{cpu.Name.Trim()} {FormatFrequency(cpu.MaxClockSpeed)}{cpu.NumberOfCores} Cores");

            return
                $"{hardwareInfo.OperatingSystem.Name} ({hardwareInfo.OperatingSystem.VersionString})" +
                $" | {string.Join(',', cpus)}" +
                $" | {new ByteSize(hardwareInfo.MemoryStatus.TotalPhysical)}";
        }
    }
}