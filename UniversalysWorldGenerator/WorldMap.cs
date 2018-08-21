using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Drawing;

namespace UniversalysWorldGenerator
{
    class WorldMap
    {
        public const int LONGITUDE = 200, LATITUDE = 250, REGION = 500, RANDOMSTEP = 6;
        public Bitmap mapPaint = new Bitmap(LATITUDE, LONGITUDE);
        public int[,] mapMask = new int[LONGITUDE, LATITUDE];
        List<Point> regionCenter = new List<Point>();
        Region[] Regions = new Region[REGION];
        Random dice = new Random();
        public List<int> notTreatedRegions = new List<int>();



        public WorldMap()
        {
        }

        /// <summary>
        /// Generate the region mask
        /// </summary>
        public void GenerateRegions()
        {

            int i = 0, j, rand;
            Point current = new Point();
            Point neighbor;
            Region region;
            Queue<Point> iterativePoint = new Queue<Point>();
            int numNeighbors, numFutureNeighbors;
            int[] regionNeighbors = new int[4];
            Point[] futureNeighbors = new Point[4];


            // Initializing the mapMask to -1 as we want to use 0 for the first regionID
            for (int k = 0; k < LONGITUDE; k++)
            {
                for (int l = 0; l < LATITUDE; l++)
                {
                    mapMask[k, l] = -1;
                }
            }


            while (i < REGION)
            {
                // These two conditions are testing to get more region in the center band, to compensate for map distorsion due to the flat representation.
                // It is very basic and only skew probabilities, not force these out
                current.X = dice.Next(((2 * i / REGION) * LONGITUDE) / 5, LONGITUDE - ((2 * i / REGION) * LONGITUDE) / 5);
                current.Y = dice.Next(0, LATITUDE);

                // Filling the map with the various starting points. The neighbor points are also locked out to allow for some minimal distance between points.
                // Note that if a point lands on a blocked area, it won't generate a region, which allows to slightly vary their final number from map to map
                // We make sure to not get out of the map, however. Also we cannot add an element that already is part of the queue there
                // One note: no region is cycling around the poles, as we consider our map finishing a little before these. This simplify some calculations!
                if (mapMask[current.X, current.Y] == -1)
                {
                    mapMask[current.X, current.Y] = i;

                    if (current.X > 0)
                    {
                        mapMask[current.X - 1, current.Y] = -2;
                        neighbor = new Point(current.X - 1, current.Y);
                        iterativePoint.Enqueue(neighbor);
                    }
                    if (current.X < LONGITUDE - 1)
                    {
                        mapMask[current.X + 1, current.Y] = -2;
                        neighbor = new Point(current.X + 1, current.Y);
                        iterativePoint.Enqueue(neighbor);
                    }
                    if (current.Y > 0)
                    {
                        mapMask[current.X, current.Y - 1] = -2;
                        neighbor = new Point(current.X, current.Y - 1);
                        iterativePoint.Enqueue(neighbor);
                    }
                    else
                    {
                        mapMask[current.X, LATITUDE - 1] = -2;
                        neighbor = new Point(current.X, LATITUDE - 1);
                        iterativePoint.Enqueue(neighbor);
                    }
                    if (current.Y < LATITUDE - 1)
                    {
                        mapMask[current.X, current.Y + 1] = -2;
                        neighbor = new Point(current.X, current.Y + 1);
                        iterativePoint.Enqueue(neighbor);
                    }
                    else
                    {
                        mapMask[current.X, 0] = -2;
                        neighbor = new Point(current.X, 0);
                        iterativePoint.Enqueue(neighbor);
                    }
                    // We have a list of region center that will be passed to regions themselves. It will serve for distance calculation.
                    // Due to how spread works, it is NOT the geometric center!
                    regionCenter.Add(current);
                    region = new Region(current.X, current.Y);
                    Regions[i] = region;
                    i++;
                }
            }

            // Now for the filling of the void. On each member of iterativePoint, we test the neighbors and have a probability to convert it to a neighboring region.
            // If not converted, we add it again at the end of the queue, and we use this to ensure our regions aren't too regular that way.
            // Due to how we fill the map, no space shall be forgotten. We however have to check there for duplicates in the queue this time.
            while (iterativePoint.Count > 0)
            {
                numNeighbors = 0;
                numFutureNeighbors = 0;
                current = iterativePoint.Dequeue();

                // We first create a small array of neighboring regions, if those exist
                // If not we add the empty neighbor to a list of points to fill IF we fill this point
                if (current.X > 0)
                {
                    if (mapMask[current.X - 1, current.Y] > 0)
                    {
                        regionNeighbors[numNeighbors] = mapMask[current.X - 1, current.Y];
                        numNeighbors++;
                    }
                    else
                    {
                        neighbor = new Point(current.X - 1, current.Y);
                        futureNeighbors[numFutureNeighbors] = neighbor;
                        numFutureNeighbors++;
                    }
                }
                if (current.X < LONGITUDE - 1)
                {
                    if (mapMask[current.X + 1, current.Y] > 0)
                    {
                        regionNeighbors[numNeighbors] = mapMask[current.X + 1, current.Y];
                        numNeighbors++;
                    }
                    else
                    {
                        neighbor = new Point(current.X + 1, current.Y);
                        futureNeighbors[numFutureNeighbors] = neighbor;
                        numFutureNeighbors++;
                    }
                }
                if (current.Y > 0)
                {
                    if (mapMask[current.X, current.Y - 1] > 0)
                    {
                        regionNeighbors[numNeighbors] = mapMask[current.X, current.Y - 1];
                        numNeighbors++;
                    }
                    else
                    {
                        neighbor = new Point(current.X, current.Y - 1);
                        futureNeighbors[numFutureNeighbors] = neighbor;
                        numFutureNeighbors++;
                    }
                }
                else
                {
                    if (mapMask[current.X, LATITUDE - 1] > 0)
                    {
                        regionNeighbors[numNeighbors] = mapMask[current.X, LATITUDE - 1];
                        numNeighbors++;
                    }
                    else
                    {
                        neighbor = new Point(current.X, LATITUDE - 1);
                        futureNeighbors[numFutureNeighbors] = neighbor;
                        numFutureNeighbors++;
                    }
                }
                if (current.Y < LATITUDE - 1)
                {
                    if (mapMask[current.X, current.Y + 1] > 0)
                    {
                        regionNeighbors[numNeighbors] = mapMask[current.X, current.Y + 1];
                        numNeighbors++;
                    }
                    else
                    {
                        neighbor = new Point(current.X, current.Y + 1);
                        futureNeighbors[numFutureNeighbors] = neighbor;
                        numFutureNeighbors++;
                    }
                }
                else
                {
                    if (mapMask[current.X, 0] > 0)
                    {
                        regionNeighbors[numNeighbors] = mapMask[current.X, 0];
                        numNeighbors++;
                    }
                    else
                    {
                        neighbor = new Point(current.X, 0);
                        futureNeighbors[numFutureNeighbors] = neighbor;
                        numFutureNeighbors++;
                    }
                }

                // Once done, we randomly see if we fill it or not. there is 10% (TEST value) chance per empty space around the point that we won't add anything, meaning that a fully surrounded
                // point will be filled no matter what. The reason being that in that case, we don't need to worry about giving an irregular shape to our regions!
                /*
                 00000   00000   00003   01033   11333  
                 00000   00003   01133   11133   11133
                 00103 > 01113 > 01113 > 11113 > 11113   Example of what we would see in execution
                 00000   00120   01123   11123   11123
                 00020   00222   02222   22222   22222

                 00000   00000   00103   01133   11133  
                 00000   00103   01113   11113   11113
                 00103 > 01133 > 11133 > 11133 > 11133   Another example with the same starting point
                 00000   00023   01223   01223   21223
                 00020   00220   02223   22223   22223    */

                // Get a random number to see if we treat the current pixel in iterativePoint
                rand = (short)dice.Next(1, 11);
                if (rand >= numFutureNeighbors)
                {
                    // We treat the pixel
                    // mapMask[X,Y] takes the value of the region it now belongs
                    rand = (short)dice.Next(0, numNeighbors);
                    mapMask[current.X, current.Y] = regionNeighbors[rand];
                    i = 0;
                    // Now adding the neighbor regions to the one we extended, if these exist
                    while (i < numFutureNeighbors)
                    {
                        // We extend iterativePoint for each regions around without a region
                        if (!iterativePoint.Contains(futureNeighbors[i]))
                        {
                            iterativePoint.Enqueue(futureNeighbors[i]);
                        }
                        i++;
                    }
                }
                else
                {
                    // Here, we simply push back the point in the queue
                    iterativePoint.Enqueue(current);
                }
            }

            for (i = 0; i < LONGITUDE; i++)
            {
                for (j = 0; j < LATITUDE; j++)
                {
                    if (i != 0)
                    {
                        if (mapMask[i, j] != mapMask[i - 1, j])
                        {
                            if (!Regions[mapMask[i, j]].neighbors.Contains(mapMask[i - 1, j]))
                            {
                                Regions[mapMask[i, j]].neighbors.Add(mapMask[i - 1, j]);
                            }
                        }
                    }
                    if (i != LONGITUDE - 1)
                    {
                        if (mapMask[i, j] != mapMask[i + 1, j])
                        {
                            if (!Regions[mapMask[i, j]].neighbors.Contains(mapMask[i + 1, j]))
                            {
                                Regions[mapMask[i, j]].neighbors.Add(mapMask[i + 1, j]);
                            }
                        }
                    }
                    if (j != 0)
                    {
                        if (mapMask[i, j] != mapMask[i, j - 1])
                        {
                            if (!Regions[mapMask[i, j]].neighbors.Contains(mapMask[i, j - 1]))
                            {
                                Regions[mapMask[i, j]].neighbors.Add(mapMask[i, j - 1]);
                            }
                        }
                    }
                    else
                    {
                        if (mapMask[i, j] != mapMask[i, LATITUDE - 1])
                        {
                            if (!Regions[mapMask[i, j]].neighbors.Contains(mapMask[i, LATITUDE - 1]))
                            {
                                Regions[mapMask[i, j]].neighbors.Add(mapMask[i, LATITUDE - 1]);
                            }
                        }
                    }
                    if (j != LATITUDE - 1)
                    {
                        if (mapMask[i, j] != mapMask[i, j + 1])
                        {
                            if (!Regions[mapMask[i, j]].neighbors.Contains(mapMask[i, j + 1]))
                            {
                                Regions[mapMask[i, j]].neighbors.Add(mapMask[i, j + 1]);
                            }
                        }
                    }
                    else
                    {
                        if (mapMask[i, j] != mapMask[i, 0])
                        {
                            if (!Regions[mapMask[i, j]].neighbors.Contains(mapMask[i, 0]))
                            {
                                Regions[mapMask[i, j]].neighbors.Add(mapMask[i, 0]);
                            }
                        }
                    }
                }
            }
        }

        public void DrawRegionMap()
        {
            int i = 0, j = 0, colorRed = 0, colorGreen = 0, colorBlue = 0;
            Color[] colorTable = new Color[REGION];
            while (i < LONGITUDE)
            {
                j = 0;
                while (j < LATITUDE)
                {
                    if (colorTable[mapMask[i, j]].IsEmpty)
                    {
                        colorTable[mapMask[i, j]] = Color.FromArgb(colorRed, colorGreen, colorBlue);
                        colorRed += 50;
                        if (colorRed > 255)
                        {
                            colorGreen += 16;
                            if (colorGreen > 255)
                            {
                                colorBlue += 16;
                                if (colorBlue > 255)
                                {
                                    colorBlue -= 255;
                                }
                                colorGreen -= 255;
                            }
                            colorRed -= 255;
                        }
                    }
                    mapPaint.SetPixel(j, i, colorTable[mapMask[i, j]]);
                    j++;
                }
                i++;
            }
            mapPaint.Save("C:\\Users\\Stagiaire.TAZ\\source\\repos\\UniversalysWorldGenerator\\UniversalysWorldGenerator\\map\\map.png");
        }

        public void CreateLandmass(int type, int size)
        {

            Region region;
            Queue<int> neighbors;
            int currentRegionID;
            int i, rand;
            List<int> treatedEmptyRegions = new List<int>();

            // 1) Choose the center to spread from
            rand = dice.Next(0, notTreatedRegions.Count);
            currentRegionID = notTreatedRegions[rand];
            region = Regions[currentRegionID];

            switch (type)
            {
                case 1:
                    region.height = dice.Next(0, 10) + dice.Next(0, 10) - 75;
                    break;
                case 2:
                    region.height = dice.Next(0, 5) + dice.Next(0, 20) + 5;
                    break;
                case 3:
                    region.height = dice.Next(0, 30) + dice.Next(0, 10) + 10;
                    break;
                case 4:
                    region.height = dice.Next(0, 40) + dice.Next(0, 10) - 50;
                    if (region.height < 0)
                    {
                        region.isSea = true;
                    }
                    break;
            }

            neighbors = new Queue<int>(region.neighbors);
            notTreatedRegions.Remove(currentRegionID);

            // 2) Use the neighbors to potentially spread the area type
            while (neighbors.Count > 0 && size >= 0)
            {
                currentRegionID = neighbors.Dequeue();
                if (notTreatedRegions.Contains(currentRegionID) && !treatedEmptyRegions.Contains(currentRegionID))
                {
                    region = Regions[currentRegionID];

                    if (size > dice.Next(1, 101))
                    {
                        switch (type)
                        {
                            case 1:
                                region.height = dice.Next(0, 10) + dice.Next(0, 10) - 75 - (160 - size) / 12;
                                break;
                            case 2:
                                region.height = dice.Next(0, 5) + dice.Next(0, 20) + 5;
                                break;
                            case 3:
                                region.height = dice.Next(0, 30) + dice.Next(0, 10) + 10;
                                break;
                            case 4:
                                region.height = dice.Next(0, 40) + dice.Next(0, 10) - 50 + (160 - size) / 12;
                                if (region.height < 0)
                                {
                                    region.isSea = true;
                                }
                                break;
                        }
                        if (region.height == 0)
                        {
                            region.height = -1;
                            region.isSea = true;
                        }
                        i = 0;
                        while (i < region.neighbors.Count)
                        {
                            neighbors.Enqueue(region.neighbors[i]);
                            i++;
                        }
                        notTreatedRegions.Remove(currentRegionID);
                    }
                    else
                    {
                        treatedEmptyRegions.Add(currentRegionID);
                    }
                    size -= RANDOMSTEP;

                }
            }

        }

        public void CreateMountainRange()
        {
            Region region;

            int i;
            int rand;
            double globalHeight;
            List<int> hillyRegions = new List<int>();
            List<int> nonCommonNeighbors = new List<int>();
            List<int> globalNeighbors = new List<int>();
            // 1) Choose the starting point to spread from
            do
            {
                rand = dice.Next(0, REGION);
            } while (Regions[rand].isWater());
            region = Regions[rand];
            globalHeight = dice.Next(50, 71) + dice.Next(0, 31);
            region.height = Convert.ToInt32(((100 - region.height) * (globalHeight / 100)));
            region.isMountainRange = true;

            //Add region Neighbors to global neighbors list.
            globalNeighbors.AddRange(region.neighbors);
            hillyRegions.AddRange(region.neighbors);

            i = dice.Next(3, 12 - RANDOMSTEP);


            // 2) Use the neighbors to spread
            while (i > 0)
            {

                rand = dice.Next(0, hillyRegions.Count);

                region = Regions[hillyRegions[rand]];
                if (region.height > 0)
                {
                    region.height = Convert.ToInt32(((100 - region.height) * (globalHeight / 100)));

                }
                else
                {
                    region.height = Convert.ToInt32(((region.height) * (globalHeight / 100)));
                }
                region.isMountainRange = true;
                hillyRegions.AddRange(region.neighbors);
                foreach (int item in region.neighbors)
                {
                    if (hillyRegions.Contains(item))
                    {
                        hillyRegions.Remove(item);
                    }
                }
                //Add region Neighbors to global neighbors list.
                globalNeighbors.AddRange(region.neighbors);


                i--;
            }

            //If in the global neighbors list the current evalued item is not a MoutainRange change it to Hill.
            i = 0;
            int j;
            while (i < globalNeighbors.Count)
            {
                if (!Regions[globalNeighbors[i]].isMountainRange)
                {
                    Regions[globalNeighbors[i]].isHilly = true;

                }
                j = i + 1;
                while (j < globalNeighbors.Count)
                {
                    if (globalNeighbors[i] == globalNeighbors[j])
                    {
                        globalNeighbors.RemoveAt(j);
                    }
                    else
                    {
                        j++;
                    }
                }
                i++;
            }
        }

        public void GenerateLandmass()
        {
            int rand;

            for (int i = 0; i < REGION; i++)
            {
                notTreatedRegions.Add(i);
            }

            while (notTreatedRegions.Count > 0)
            {
                rand = dice.Next(0, 12);
                switch (rand)
                {
                    case 0:
                        CreateLandmass(1, 360);
                        break;
                    case 1:
                        CreateLandmass(1, 360);
                        break;
                    case 2:
                        CreateLandmass(1, 320);
                        break;
                    case 3:
                        CreateLandmass(1, 260);
                        break;
                    case 4:
                        CreateLandmass(1, 240);
                        break;
                    case 5:
                        CreateLandmass(2, 250);
                        break;
                    case 6:
                        CreateLandmass(2, 200);
                        break;
                    case 7:
                        CreateLandmass(2, 150);
                        break;
                    case 8:
                        CreateLandmass(2, 120);
                        break;
                    case 9:
                        CreateLandmass(3, 180);
                        break;
                    case 10:
                        CreateLandmass(3, 120);
                        break;
                    case 11:
                        CreateLandmass(4, 240);
                        break;
                }

            }

            CreateMountainRange();
            CreateMountainRange();
            CreateMountainRange();
            CreateMountainRange();
            CreateMountainRange();
            CreateMountainRange();
        }

        public void DrawHeightMap()
        {
            int i = 0, j = 0, colorRed = 0, colorGreen = 0, colorBlue = 0;
            Color[] colorTable = new Color[REGION];
            while (i < LONGITUDE)
            {
                j = 0;
                while (j < LATITUDE)
                {
                    if (Regions[mapMask[i, j]].height <= 0)
                    {
                        colorRed = 0;
                        colorGreen = 120 + Regions[mapMask[i, j]].height;
                        colorBlue = 100 - Regions[mapMask[i, j]].height * 3 / 2;
                    }
                    else
                    {
                        colorRed = 100 + Regions[mapMask[i, j]].height * 5 / 4;
                        colorGreen = 80 + (Regions[mapMask[i, j]].height) * 3 / 2;
                        colorBlue = 40 + (Regions[mapMask[i, j]].height) * 2;
                    }
                    //if (Regions[mapMask[i, j]].isSea == true)
                    //{
                    //    colorRed = 0;
                    //    colorGreen = 0;
                    //    colorBlue = 0;
                    //}
                    mapPaint.SetPixel(j, i, Color.FromArgb(colorRed, colorGreen, colorBlue));
                    j++;
                }
                i++;
            }
            mapPaint.Save("C:\\Users\\Stagiaire.TAZ\\source\\repos\\UniversalysWorldGenerator\\UniversalysWorldGenerator\\map\\mapHeight.png");
        }

        public void GenerateTemperature()
        {
            int rand;

            for (int i = 0; i < REGION; i++)
            {
                rand = dice.Next(0, 11);
                Regions[i].temperature = 75 + rand - 320 * (Math.Abs(Regions[i].X - (LONGITUDE / 2))) / LONGITUDE;
                if (Regions[i].isSea)
                {
                    rand = dice.Next(5, 15);
                    Regions[i].temperature += rand;
                    //Regions[i].temperature = 100;
                }
            }
        }

        public void DrawTemperatureMap()
        {
            int i = 0, j = 0, colorRed = 16, colorGreen = 0, colorBlue = 0;
            Color[] colorTable = new Color[REGION];
            while (i < LONGITUDE)
            {
                j = 0;
                while (j < LATITUDE)
                {

                    colorRed = 120 + Regions[mapMask[i, j]].temperature;
                    colorGreen = 220 - Math.Abs(Regions[mapMask[i, j]].temperature) * 2;
                    colorBlue = 120 - Regions[mapMask[i, j]].temperature;

                    mapPaint.SetPixel(j, i, Color.FromArgb(colorRed, colorGreen, colorBlue));
                    j++;
                }
                i++;
            }
            mapPaint.Save("C:\\Users\\Stagiaire.TAZ\\source\\repos\\UniversalysWorldGenerator\\UniversalysWorldGenerator\\map\\mapTemperature.png");
        }

        public string CursorPosition(int x, int y)
        {
            string result = "";

            result += "ID REGION : " + mapMask[y, x].ToString() + Environment.NewLine;

            result += "REGION HEIGHT : " + Regions[mapMask[y, x]].height.ToString() + Environment.NewLine;

            result += "TEMPERATURE : " +  Regions[mapMask[y, x]].temperature.ToString() + Environment.NewLine;

            result += "Sea : " + Regions[mapMask[y, x]].isSea.ToString() + Environment.NewLine;

            result += Environment.NewLine; result += Environment.NewLine;

            result += "REGION 499 Height : " + Regions[499].height + Environment.NewLine;
            result += "REGION 499 Temperature : " + Regions[499].temperature + Environment.NewLine;
            result += "X : " + Regions[499].Y + " ; Y : " + +Regions[499].X + " " + Environment.NewLine;

            return result;
        }

    }

}

