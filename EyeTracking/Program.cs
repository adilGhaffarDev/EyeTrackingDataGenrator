using System;
using System.Threading;

namespace EyeTracking
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
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

        float ContinousFixationTime = 0;
        //float ContinousFixationTime = 0;
        DataPoint[] dataPoints;

        public Generator(float duration, float fixationMin, float fixationMax, float saccadeMin, float saccadeMax, float velocityMin, float velocityMax, float tempoaralRes, float resX, float resY, float distPartcipantScreen, float disaplaySize)
        {
            this.duration = duration;
            this.fixationMin = fixationMin;
            this.fixationMax = fixationMax;
            this.saccadeMin = saccadeMin;
            this.saccadeMax = saccadeMax;
            this.velocityMin = velocityMin;
            this.velocityMax = velocityMax;
            this.tempoaralRes = tempoaralRes;
            this.resX = resX;
            this.resY = resY;
            this.distPartcipantScreen = distPartcipantScreen;
            this.disaplaySize = disaplaySize;

            degPerPixel = RadianToDegree(MathF.Atan2(0.5f * disaplaySize, distPartcipantScreen)) / (0.5f * resY);
            currentState = GazeMovementState.none;

        }
        int currentDataIndex = 0;
        public string GenerateSampleData()
        {
            string fileName = "SampleData.csv";
            int totalDataPoints = (int)(duration * tempoaralRes);
            dataPoints = new DataPoint[totalDataPoints];
            SwitchState();
            for (int currentDataIndex = 0; currentDataIndex < totalDataPoints; currentDataIndex++)
            {
                if(currentState == GazeMovementState.slow)
                {
                    GenerateFixationPoint();
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
            }
        }

        private void GenerateSaccadePoint()
        {

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
                ContinousFixationTime += 0;
            }
            else
            {
                int r = GetRandomNumber(0,100);
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
    }



}
