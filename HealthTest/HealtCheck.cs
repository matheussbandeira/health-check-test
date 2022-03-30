using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace HealthTest
{
    public class HealtCheck
    {
        private const string MEMINFO_FILEPATH = "/proc/meminfo";
        private const string CPUSTAT_FILEPATH = "/tmp/cpuAvailable.txt";
        private const string CPUSTAT_CMD = $"top -i -bn1 > {CPUSTAT_FILEPATH}";

        public async Task<MemoryMetrics> GetMetrics()
        {
            if (IsUnix())
            {
                return await GetUnixMetrics();
            }

            return GetWindowsMetrics();
        }

        private bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return isUnix;
        }

        private MemoryMetrics GetWindowsMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split("\n");
            var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.FreeMemory = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0) + "MB";

            //

            info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "cpu get loadpercentage";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            lines = output.Trim().Split("\n");
            metrics.FreeCpu = lines[1].Trim() + "%";

            return metrics;
        }

        private async Task<MemoryMetrics> GetUnixMetrics()
        {
            string memAvailableLine = await ReadLineStartingWithAsync(MEMINFO_FILEPATH, "MemAvailable");

            if (string.IsNullOrWhiteSpace(memAvailableLine))
                memAvailableLine = await ReadLineStartingWithAsync(MEMINFO_FILEPATH, "MemFree");

            var parsedMemAvailable = int.Parse(Regex.Match(memAvailableLine.Trim(), @"\d+", RegexOptions.IgnoreCase).Value) / 1024 + "MB";

            string output = "";

            var info = new ProcessStartInfo();
            info.FileName = "bash";
            info.Arguments = $"-c \"{CPUSTAT_CMD}\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            string cpuAvailableLine = await ReadLineStartingWithAsync(CPUSTAT_FILEPATH, "%Cpu(s)");

            var parsedCpuAvailable = Regex.Match(cpuAvailableLine.Trim(), @"([\d.]+)\sid", RegexOptions.IgnoreCase).Groups[1].Value + "%";

            return new MemoryMetrics {
                FreeCpu = parsedCpuAvailable,
                FreeMemory = parsedMemAvailable
            };
        }

        private static async Task<string> ReadLineStartingWithAsync(string path, string lineStartsWith)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 512, FileOptions.SequentialScan | FileOptions.Asynchronous))
            using (var r = new StreamReader(fs, Encoding.ASCII))
            {
                string line;
                while ((line = await r.ReadLineAsync()) != null)
                {
                    if (line.StartsWith(lineStartsWith, StringComparison.Ordinal))
                        return line;
                }
            }

            return null;
        }
    }
}
