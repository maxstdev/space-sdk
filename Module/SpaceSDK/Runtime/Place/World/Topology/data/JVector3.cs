using Newtonsoft.Json;
using UnityEngine;

namespace MaxstXR.Place
{
    /// <summary>
    /// This is a vector class for the convenience of Json Convert and Z UP settings.
    /// Y and Z values ​​are swapped when converting with Unity.Vector3.
    /// </summary>
    public class JVector3
    {
        [JsonProperty("x")]
        public float x;
        [JsonProperty("y")]
        public float y;
        [JsonProperty("z")]
        public float z;
        public JVector3()
        {
            x = y = z = 0f;
        }
        public JVector3(JVector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public JVector3(Vector3 v)
        {
            x = v.x;
            y = v.z;
            z = v.y;
        }
        public JVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public JVector3(double x, double y, double z)
        {
            this.x = (float)x;
            this.y = (float)y;
            this.z = (float)z;
        }

        public JVector3(float f)
        {
            x = y = z = f;
        }

        public void Set(Vector3 vector)
        {
            x = vector.x;
            y = vector.z;
            z = vector.y;
        }
        public void Set(JVector3 vector)
        {
            x = vector.x;
            y = vector.z;
            z = vector.y;
        }
        public Vector3 ToVector3()
        {
            return new Vector3(-x, y, z);
        }

        public override string ToString()
        {
            return $"{x},{y},{z}";
        }

        public static bool operator ==(JVector3 lhs, JVector3 rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }
        public static bool operator !=(JVector3 lhs, JVector3 rhs)
        {
            return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
        }

        public override bool Equals(object op1)
        {
            return x == ((JVector3)op1).x && y == ((JVector3)op1).y && z == ((JVector3)op1).z;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public JVector3 Copy()
        {
            return Clone() as JVector3;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}