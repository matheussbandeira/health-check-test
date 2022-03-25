using System.Diagnostics;

namespace HealthTest
{
    public class SystemDataService
    {
        private readonly PerformanceCounter _ramCounter;

        public SystemDataService()
        {
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }

        public string getAvailableRAM()
        {
            return _ramCounter.NextValue() + "MB";
        }
    }
}
