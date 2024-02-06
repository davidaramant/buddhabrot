using System.Text;
using Hardware.Info;
using Humanizer.Bytes;

namespace Buddhabrot.Core.Utilities;

public static class ComputerDescription
{
	public static string GetMultiline()
	{
		var (os, cpu, ram) = GetInfo();

		var sb = new StringBuilder();
		sb.AppendLine(os);
		sb.AppendLine(cpu);
		sb.AppendLine(ram);

		return sb.ToString();
	}

	public static string GetSingleLine()
	{
		var (os, cpu, ram) = GetInfo();

		return os + " | " + cpu + " | " + ram;
	}

	public static (string OS, string CPU, string RAM) GetInfo()
	{
		var hardwareInfo = new HardwareInfo(useAsteriskInWMI: false);
		hardwareInfo.RefreshOperatingSystem();
		hardwareInfo.RefreshMemoryStatus();
		hardwareInfo.RefreshCPUList(includePercentProcessorTime: false);

		static string FormatFrequency(uint mhz) => mhz == 0 ? string.Empty : $"{mhz / 1000d:N2} GHz ";

		var cpus = hardwareInfo.CpuList.Select(cpu =>
			$"{cpu.Name.Trim()} {FormatFrequency(cpu.MaxClockSpeed)}{cpu.NumberOfCores} Cores"
		);

		return (
			OS: $"{hardwareInfo.OperatingSystem.Name} ({hardwareInfo.OperatingSystem.VersionString})",
			CPU: string.Join(',', cpus),
			RAM: new ByteSize(hardwareInfo.MemoryStatus.TotalPhysical).ToString()
		);
	}
}
