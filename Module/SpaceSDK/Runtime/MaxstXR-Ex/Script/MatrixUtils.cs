using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace maxstAR
{
    public partial class MatrixUtils
    {
        internal static float[] GenGLMatrix(Vector3 pos)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            var glMatrix = new float[16];
            glMatrix[0] = matrix[0, 0];
            glMatrix[1] = matrix[1, 0];
            glMatrix[2] = matrix[2, 0];
            glMatrix[3] = matrix[3, 0];

            glMatrix[4] = matrix[0, 1];
            glMatrix[5] = matrix[1, 1];
            glMatrix[6] = matrix[2, 1];
            glMatrix[7] = matrix[3, 1];

            glMatrix[8] = matrix[0, 2];
            glMatrix[9] = matrix[1, 2];
            glMatrix[10] = matrix[2, 2];
            glMatrix[11] = matrix[3, 2];

            glMatrix[12] = pos.x; //matrix[0, 3];
            glMatrix[13] = pos.y; //matrix[1, 3];
            glMatrix[14] = pos.z; //matrix[2, 3];
            glMatrix[15] = matrix[3, 3];
            return glMatrix;
        }
    }
}
