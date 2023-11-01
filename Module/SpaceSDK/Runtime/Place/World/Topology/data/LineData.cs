using System;

namespace MaxstXR.Place
{
    public class Linker
    {
        public Guid FromGUID { get; set; }
        public Guid ToGUID { get; set; }
        public LinkerType linkerType = LinkerType.TwoWay;
    }

    public enum LinkerType
    {
        TwoWay = 0,
        OneWay
    }
}