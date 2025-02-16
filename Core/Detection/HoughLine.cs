using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Detection
{
    public class HoughLine
    {
        // STEPS
        // 1. Apply Canny on RGB image
        // 2. Create Accumulator array and fill with votes based on Canny edge detected image
        // 3. Filter out the first nth highest vote and return them as IEnumerable<LinePolar> descending by votes

    }
}
