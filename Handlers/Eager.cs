namespace FOG.Handlers
{
    public static class Eager
    {
        public static void Initalize()
        {
#pragma warning disable 642
            if (Power.Power.Updating) ;
            if (Middleware.Configuration.TestMAC != null) ;
#pragma warning restore 642
        }
    }
}
