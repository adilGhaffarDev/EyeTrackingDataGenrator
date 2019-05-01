using System;
using System.Diagnostics;
using System.Threading;

namespace EyeTracking
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Generator SampleDataGenerator = new Generator(3,100,250,300,500,50,1024,768,60,53.34f);
            SampleDataGenerator.GenerateSampleData();
        }

    }

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
    }

    enum GazeMovementState
    {
        slow,
        fast,
        none
    }

    class Generator
    {
        //Parameters: 
        //fixation duration min and max, 
        //saccade duration min and max and 
        //velocity min and max, 
        //temporal resolution(in Hz), 
        //X&Y- resolution in pixes, 
        //distance of the participant to the screen(in cm), 
        //and display size in cm.
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

            degPerPixel = RadianToDegree(MathF.Atan2(0.5f * disaplaySize, distPartcipantScreen)) / (0.5f * resY);
            currentState = GazeMovementState.none;
            ContinousFixationTime = 0;
            minDegreeForFixation = 1;
            maxDegreeForFixation = 5;

            totalDataPoints = (int)(duration * tempoaralRes);

            minFixationPix = ((1 / degPerPixel) * minDegreeForFixation) / totalDataPoints;
            maxFixationPix = ((1 / degPerPixel) * maxDegreeForFixation) / totalDataPoints;

        }

        int currentDataIndex = 0;

        public string GenerateSampleData()
        {
            string fileName = "SampleData.csv";
            dataPoints = new DataPoint[totalDataPoints];
            SwitchState();
            for (currentDataIndex = 0; currentDataIndex < totalDataPoints; currentDataIndex++)
            {
                if(currentState == GazeMovementState.slow)
                {
                    GenerateFixationPoint();
                    ContinousFixationTime += (duration*1000) / totalDataPoints;
                }
                else
                {
                    GenerateSaccadePoint();
                }

                SwitchState();
                Thread.Sleep((int)((1.0f/ tempoaralRes) *1000.0f));
            }


            return fileName;
        }

        private float RadianToDegree(float angle)
        {
            return angle * (180.0f / MathF.PI);
        }

        private void GenerateFixationPoint()
        {
            if(currentDataIndex == 0)
            {
                dataPoints[currentDataIndex] = new DataPoint(0,0,System.DateTime.Now.ToLongTimeString());
            }
            else
            {
                DataPoint prevPoint = dataPoints[currentDataIndex-1];
                var ranX = GetRandomNumberFloat(minFixationPix,maxFixationPix);
                var ranY = GetRandomNumberFloat(minFixationPix, maxFixationPix);
                dataPoints[currentDataIndex] = new DataPoint(prevPoint.XCord + (float)ranX, prevPoint.XCord + (float)ranY, System.DateTime.Now.ToLongTimeString());
                Console.Write("xcord: "+ dataPoints[currentDataIndex].XCord+"  "+ "ycord: " + dataPoints[currentDataIndex].YCord+"  "+ "Time: "+ dataPoints[currentDataIndex].TimeStamp + "\n");
            }
        }

        private void GenerateSaccadePoint()
        {
            if (currentDataIndex == 0)
            {
                dataPoints[currentDataIndex] = new DataPoint(-1, -1, System.DateTime.Now.ToLongTimeString());
                Console.Write("xcord: " + dataPoints[currentDataIndex].XCord + "  " + "ycord: " + dataPoints[currentDataIndex].YCord + "  " + "Time: " + dataPoints[currentDataIndex].TimeStamp + "\n");

            }
            else
            {
                dataPoints[currentDataIndex] = new DataPoint(-1, -1, System.DateTime.Now.ToLongTimeString());
                Console.Write("xcord: " + dataPoints[currentDataIndex].XCord + "  " + "ycord: " + dataPoints[currentDataIndex].YCord + "  " + "Time: " + dataPoints[currentDataIndex].TimeStamp+"\n");


                //DataPoint prevPoint = dataPoints[currentDataIndex - 1];
                //var ranX = GetRandomNumberFloat(minFixationPix, maxFixationPix);
                //var ranY = GetRandomNumberFloat(minFixationPix, maxFixationPix);
                //dataPoints[currentDataIndex] = new DataPoint(prevPoint.XCord + (float)ranX, prevPoint.XCord + (float)ranY, System.DateTime.Now.ToLongTimeString());
            }
        }

        private void SwitchState()
        {
            if(ContinousFixationTime > fixationMax)
            {
                currentState = GazeMovementState.fast;
                ContinousFixationTime = 0;
            }
            else if(ContinousFixationTime < fixationMin)
            {
                currentState = GazeMovementState.slow;
            }
            else
            {
                float r = GetRandomNumber(0,100);
                if(r < 50)
                {
                    currentState = GazeMovementState.fast;
                    ContinousFixationTime = 0;
                }
            }
        }

        private static readonly Random getrandom = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            return getrandom.Next(min, max);
        }

        public double GetRandomNumberFloat(double minimum, double maximum)
        {
            return getrandom.NextDouble() * (maximum - minimum) + minimum;
        }


    }

}
