using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace EyeTracking
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create Generator with following parameters
            //duration of tracking
            // minimum fixation time
            // maximum fixation time
            // minimum saccade angle
            // maximum saccade angle
            // Rate/ frequancy 
            // screen resolution horizontal in pixels
            // screen resolution vertical in pixels
            // distance between participant and screen
            // display size in cm
            Generator SampleDataGenerator = new Generator(3,100,250,300,500,50,1024,768,60,53.34f);
            string filename = SampleDataGenerator.GenerateSampleData();// this will generate csv
            Console.Write("File: "+ filename+" created.");
        }
    }

    /// <summary>
    /// Each entry in csv file is a represented as DataPoint
    /// with properties:
    /// X cordinate (x position on screen)
    /// Y cordinate (Y position on screen)
    /// TimeStamp (time at which the point was recorded)
    /// </summary>
    class DataPoint
    {
        float xCord;
        float yCord;
        string timeStamp;

        public DataPoint(float xCord, float yCord, string timeStamp)
        {
            this.xCord = xCord;
            this.yCord = yCord;
            this.timeStamp = timeStamp;
        }

        public float XCord { get => xCord; set => xCord = value; }
        public float YCord { get => yCord; set => yCord = value; }
        public string TimeStamp { get => timeStamp; set => timeStamp = value; }

        // returns the formatted string of cordinates to show in csv file
        public string getCordinatesInStringFormat()
        {
            return xCord.ToString()+","+ yCord.ToString();
        }
    }

    /// <summary>
    /// GazeMovementState is a state that switches from slow to fast suring the transition
    /// of fixation to saccade and fast to slow when transition from saccade to fixation.
    /// </summary>
    enum GazeMovementState
    {
        slow, // fixation state
        fast, // saccade state
        none
    }

    /// <summary>
    /// Generator generates csv sample data on the basis of giveb parameters
    /// </summary>
    class Generator
    {
        float duration;

        float fixationMin;
        float fixationMax;

        float saccadeMin;
        float saccadeMax;

        float velocityMin;
        float velocityMax;

        float tempoaralRes;

        float resX;
        float resY;

        float distPartcipantScreen;

        float disaplaySize;

        float degPerPixel;

        GazeMovementState currentState;

        float minDegreeForFixation;
        float maxDegreeForFixation;

        float ContinousFixationTime;
        float ContinousSaccadeTime;

        int totalDataPoints;

        float minFixationPix;
        float maxFixationPix;

        DataPoint[] dataPoints;

        public Generator(float duration, float fixationMin, float fixationMax, float saccadeMin, float saccadeMax, float tempoaralRes, float resX, float resY, float distPartcipantScreen, float disaplaySize)
        {
            this.duration = duration;
            this.fixationMin = fixationMin;
            this.fixationMax = fixationMax;
            this.saccadeMin = saccadeMin;
            this.saccadeMax = saccadeMax;
          
            this.tempoaralRes = tempoaralRes;
            this.resX = resX;
            this.resY = resY;
            this.distPartcipantScreen = distPartcipantScreen;
            this.disaplaySize = disaplaySize;

            degPerPixel = RadianToDegree(MathF.Atan2(0.5f * disaplaySize, distPartcipantScreen)) / (0.5f * resY);// degree per pixel calculation
            currentState = GazeMovementState.none;
            ContinousFixationTime = 0;
            ContinousSaccadeTime = 0;
            minDegreeForFixation = 1;
            maxDegreeForFixation = 5;

            totalDataPoints = (int)(duration * tempoaralRes);

            minFixationPix = ((1 / degPerPixel) * minDegreeForFixation) / totalDataPoints;// min distance that can be covered in pixels during fixation 
            maxFixationPix = ((1 / degPerPixel) * maxDegreeForFixation) / totalDataPoints;// max distance that can be covered in pixels during fixation

        }

        int currentDataIndex = 0;
        Stopwatch stopWatch = new Stopwatch();

        public string GenerateSampleData()
        {
            string fileName = "SampleData.csv";
            dataPoints = new DataPoint[totalDataPoints];
            stopWatch.Start();
            SwitchState();
            // generating data points 
            for (currentDataIndex = 0; currentDataIndex < totalDataPoints; currentDataIndex++)
            {
                if(currentState == GazeMovementState.slow)
                {
                    GenerateFixationPoint();
                    ContinousFixationTime += (duration*1000) / totalDataPoints;
                }
                else// currentState == GazeMovementState.fast
                {
                    GenerateSaccadePoint();
                    ContinousSaccadeTime += (duration * 1000) / totalDataPoints;
                }

                SwitchState();
                Thread.Sleep((int)((1.0f/ tempoaralRes) *1000.0f)); // wait for (1/50)*1000 = 20 ms before transition of state
            }

            stopWatch.Stop();

            // write data to csv file
            var csv = new StringBuilder();
            var first = "XCordinate,YCordinate";// for title of table in csv
            var second = "Timestamp";// for title of table in csv

            var newLine = string.Format("{0},{1}", first, second);
            csv.AppendLine(newLine);
            foreach (DataPoint d in dataPoints)
            {
                first = d.getCordinatesInStringFormat();
                second = d.TimeStamp;

                newLine = string.Format("{0},{1}", first, second);
                csv.AppendLine(newLine);
                File.WriteAllText(fileName, csv.ToString());
            }
            return fileName;
        }
        /// <summary>
        /// Generates saccade point within the range of minimum fixation pixels and maximum fixation pixels
        /// </summary>
        private void GenerateFixationPoint()
        {
            if(currentDataIndex == 0)
            {
                dataPoints[currentDataIndex] = new DataPoint(0,0, stopWatch.Elapsed.TotalMilliseconds.ToString());
                //Console.Write("xcord: " + dataPoints[currentDataIndex].XCord + "  " + "ycord: " + dataPoints[currentDataIndex].YCord + "  " + "Time: " + dataPoints[currentDataIndex].TimeStamp + "\n");
            }
            else
            {
                DataPoint prevPoint = dataPoints[currentDataIndex-1];
                var ranX = GetRandomNumberFloat(minFixationPix,maxFixationPix);
                var ranY = GetRandomNumberFloat(minFixationPix, maxFixationPix);
                var xcord = prevPoint.XCord + (float)ranX;
                var ycord = prevPoint.YCord + (float)ranY;
                if(xcord >= resX)// checking if randomly generated X and Y coordinates are in the screen or not.
                {
                    ranX = 0;
                }
                if (ycord >= resY)
                {
                    ranY = 0;
                }

                dataPoints[currentDataIndex] = new DataPoint(prevPoint.XCord + (float)ranX, prevPoint.XCord + (float)ranY, stopWatch.Elapsed.TotalMilliseconds.ToString());
                //Console.Write("xcord: "+ dataPoints[currentDataIndex].XCord+"  "+ "ycord: " + dataPoints[currentDataIndex].YCord+"  "+ "Time: "+ dataPoints[currentDataIndex].TimeStamp + "\n");
            }
        }
        /// <summary>
        /// Generates saccade point within the range of minimum saccade angle and maximum saccade angle
        /// </summary>
        private void GenerateSaccadePoint()
        {
            if (currentDataIndex == 0)
            {
                dataPoints[currentDataIndex] = new DataPoint(0, 0, stopWatch.Elapsed.TotalMilliseconds.ToString());
                //Console.Write("xcord: " + dataPoints[currentDataIndex].XCord + "  " + "ycord: " + dataPoints[currentDataIndex].YCord + "  " + "Time: " + dataPoints[currentDataIndex].TimeStamp + "\n");
            }
            else
            {
                DataPoint prevPoint = dataPoints[currentDataIndex - 1];

                float minSaccadePix = ((1 / degPerPixel) * saccadeMin) / totalDataPoints;// converion of degree to pixels
                float maxSaccadePix = ((1 / degPerPixel) * saccadeMax) / totalDataPoints;// converion of degree to pixels

                var ranX = GetRandomNumberFloat(minSaccadePix, maxSaccadePix);
                var ranY = GetRandomNumberFloat(minSaccadePix, maxSaccadePix);

                var xcord = prevPoint.XCord + (float)ranX;
                var ycord = prevPoint.YCord + (float)ranY;
                if (xcord >= resX)// checking if randomly generated X and Y coordinates are in the screen or not.
                {
                    ranX = 0;
                }
                if (ycord >= resY)
                {
                    ranY = 0;
                }
                dataPoints[currentDataIndex] = new DataPoint(prevPoint.XCord + (float)ranX, prevPoint.XCord + (float)ranY, stopWatch.Elapsed.TotalMilliseconds.ToString());
                //Console.Write("xcord: " + dataPoints[currentDataIndex].XCord + "  " + "ycord: " + dataPoints[currentDataIndex].YCord + "  " + "Time: " + dataPoints[currentDataIndex].TimeStamp+"\n");
            }
        }
        /// <summary>
        /// switches state on the basis of min max fixation times and min max saccade time 
        /// during fixation state(slow) it wil stay in that state until maximum fixation is not reached. After the maximum fixation 
        /// value is achieved state machine switches to saccade(fast), it will stay in that state with 50% chance on every step.
        /// </summary>
        private void SwitchState()
        {
            if(ContinousFixationTime > fixationMax)
            {
                currentState = GazeMovementState.fast;
                ContinousFixationTime = 0;
            }
            else if(ContinousFixationTime < fixationMin && ContinousSaccadeTime == 0)
            {
                currentState = GazeMovementState.slow;
            }
            else if(ContinousSaccadeTime > 0 && ContinousSaccadeTime < 90)// limiting time of saccade between greater than 0 ms and less than 90 ms
            {
                float r = GetRandomNumber(0,100);
                if(r < 50)// 50% chance that saccade keep on happening
                {
                    currentState = GazeMovementState.fast;
                    ContinousFixationTime = 0;
                }
                else
                {
                    currentState = GazeMovementState.slow;
                    ContinousFixationTime = 0;
                    ContinousSaccadeTime = 0;
                }
            }
         
        }

        /// <summary>
        /// Util Funtions (Random number generators and conversion functions)
        /// </summary>
        private static readonly Random getrandom = new Random();
        // gives random integer between range
        public static int GetRandomNumber(int min, int max)
        {
            return getrandom.Next(min, max);
        }
        // gives random float between range
        private static double GetRandomNumberFloat(double minimum, double maximum)
        {
            return getrandom.NextDouble() * (maximum - minimum) + minimum;
        }

        // converts radian to degree
        private float RadianToDegree(float angle)
        {
            return angle * (180.0f / MathF.PI);
        }
    }
}
