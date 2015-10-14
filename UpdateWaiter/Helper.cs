
using Zazzles;

namespace FOG
{
    public static class UpdateWaiterHelper
    {
        /// <summary>
        ///     Spawn an update waiter
        /// </summary>
        /// <param name="fileName">The file that the update waiter should spawn once the update is complete</param>
        public static void SpawnUpdateWaiter(string fileName)
        {
            Log.Entry("UserService", "Spawning update waiter");

            ProcessHandler.RunClientEXE("FOGUpdateWaiter.exe", $"\"{fileName}\"");
        }
    }
}
