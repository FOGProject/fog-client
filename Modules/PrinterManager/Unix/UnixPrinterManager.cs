using System;
using System.Collections.Generic;

namespace FOG.Modules.PrinterManager
{
    class UnixPrinterManager : PrintManagerBridge
    {
        public override List<string> GetPrinters()
        {
            throw new NotImplementedException();
        }

        protected override void AddiPrint(iPrintPrinter printer)
        {
            throw new NotImplementedException();
        }

        protected override void AddLocal(LocalPrinter printer)
        {
            throw new NotImplementedException();
        }

        protected override void AddNetwork(NetworkPrinter printer)
        {
            throw new NotImplementedException();
        }

        protected override void AddCUPS(CUPSPrinter printer)
        {
            throw new NotImplementedException();
        }

        public override void Remove(string name)
        {
            throw new NotImplementedException();
        }

        public override void Default(string name)
        {
            throw new NotImplementedException();
        }
    }
}
