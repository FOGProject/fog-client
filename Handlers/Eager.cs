namespace FOG.Handlers
{
    public static class Eager
    {
        public static void Initalize()
        {
            if (Power.Power.Updating) ;
            if (Middleware.Configuration.TestMAC != null) ;
            if (RegistryHandler.GetRoot() != null) ;
        }
    }
}
