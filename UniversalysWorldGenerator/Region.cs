using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalysWorldGenerator
{
    class Region
    {
        public int X, Y;
        public int height = 0, temperature = 0, humidity = 0;
        public List<int> neighbors = new List<int>();
        public bool isMountainRange = false, isOcean = false, isContinent = false, isHilly = false, isSea = false, isLargeIsland = false;

        public Region(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool isWater()
        {
            return (height < 0);
        }

        public bool isLand()
        {
            return (height > 0);
        }
    }
}
