using UnityEngine;

namespace MaxstXR.Place
{
    public class LineData
    {
        public JVector3 FromPostion { get; set; }
        public JVector3 ToPostion { get; set; }

        public LineData(JVector3 fromPostion, JVector3 toPostion)
        {
            FromPostion = fromPostion;
            ToPostion = toPostion;
        }
    }
}