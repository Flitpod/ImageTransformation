using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class DoubleExtender
    {
        public static double ToRadian(this double degree)
        {
            return Math.PI * (degree / 180);
        }
    }
}
