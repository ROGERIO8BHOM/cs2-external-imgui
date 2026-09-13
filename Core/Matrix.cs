using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swed64;
namespace CS2.Core
{
    public sealed class ViewMatrix
    {
        public float m11, m12, m13, m14;
        public float m21, m22, m23, m24;
        public float m31, m32, m33, m34;
        public float m41, m42, m43, m44;
        
        
        public ViewMatrix ReadMatrix(Swed memory, IntPtr matrixAddress)
        {
            ViewMatrix vMatrix = new ViewMatrix();
            var matrix = memory.ReadMatrix(matrixAddress);

            // Primeira linha
            vMatrix.m11 = matrix[0];
            vMatrix.m12 = matrix[1];
            vMatrix.m13 = matrix[2];
            vMatrix.m14 = matrix[3];

            //Segunda linha
            vMatrix.m21 = matrix[4];
            vMatrix.m22 = matrix[5];
            vMatrix.m23 = matrix[6];
            vMatrix.m24 = matrix[7];

            //Terceira linha
            vMatrix.m31 = matrix[8];
            vMatrix.m32 = matrix[9];
            vMatrix.m33 = matrix[10];
            vMatrix.m34 = matrix[11];

            //Quarta linha
            vMatrix.m41 = matrix[12];
            vMatrix.m42 = matrix[13];
            vMatrix.m43 = matrix[14];
            vMatrix.m44 = matrix[15];

            return vMatrix;
        }


    }



}
