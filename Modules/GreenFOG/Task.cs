namespace FOG.Modules.GreenFOG
{
    class Task
    {
        public int Minutes { get; private set; }
        public int Hours { get; private set; }
        public bool Reboot { get; private set; }

        public Task(int minutes, int hours, bool reboot)
        {
            this.Minutes = minutes;
            this.Hours = hours;
            this.Reboot = reboot;
        }
    }
}
