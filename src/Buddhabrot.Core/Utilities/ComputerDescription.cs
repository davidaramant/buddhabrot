using System.Text;
using Hardware.Info;
using Humanizer.Bytes;

namespace Buddhabrot.Core.Utilities
{
    public static class ComputerDescription
    {
        public static string Get()
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
    }
}
