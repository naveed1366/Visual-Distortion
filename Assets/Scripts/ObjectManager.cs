using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
//using BurtSharp.Control;
//using BurtSharp.UnityInterface;
//using BurtSharp.UnityInterface.ClassExtensions;
using UnityEngine;
using TMPro;

public class ObjectManager : MonoBehaviour
{

    public GameObject[] objectPrefabs; // Pointer to Array of GameObjects that includes Start object prefab([0]) and Target object Prefab([1]), the array is used to instantiate prefabs and maniulate their positions
    public GameObject mouseObject; // Pointer to the mouse gameobject
    public TextMeshProUGUI scoreText;
    [NonSerialized] public Vector3 currentStartLocation;
    [NonSerialized] public Vector3 currentTargetLocation;

    [SerializeField] bool isMouseSolid = true;
    [SerializeField] bool centerGame = false;
    [SerializeField] int numberOfPairs = 5; // Adjustale number of pairs for start and target objects
    [SerializeField] float totalDistance = 20;// Adjustable distance between start and target object
    [SerializeField] float closenessThreshold = 0.25f;
    [SerializeField] float lowSpeedThreshold = 0.5f;
    [SerializeField] int baseLineTrials = 165;
    [SerializeField] int baseLineWithoutDistortion = 45;
    [SerializeField] int trainingTrials = 390;
    [SerializeField] int washOutTrials = 165;
    [SerializeField] float probabilityOfDistortion = 0.125f;

    private string experimentStateMessage;
    private int[] disturbanceArray;
    private bool loggingFlag = false;
    private int score = 0;
    private int randomIndex;
    private float[][] startPositionsArray; // Array for start object position used by PositionManager()
    private float[][] targetPositionsArray; // Array for target object position used by PositionManager()
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 mouseVelocity;
    private int scoreStep = 0;
    int seed = 5;

    // Start is called before the first frame update
    void Start()
    {
        PositionGenerator();
        GenerateRandomTicket();
        PositionObjects();
        score = 1;// Score is movement number at the beginning it starts at 1
        Cursor.visible = false;
        DisturbanceArrayGenerator();
    }
    // Update is called once per frame
    void Update()
    {
        
        if (isMouseSolid == false)
        {
            mouseObject.GetComponent<MeshRenderer>().enabled = false;
            mouseObject.transform.Find("FireChild").gameObject.SetActive(true);
        }
        else
        {
            mouseObject.GetComponent<MeshRenderer>().enabled = true;
            mouseObject.transform.Find("FireChild").gameObject.SetActive(false);
        }
        ExperimentPhaseMassage(score);
        Vector3 startLocation = objectPrefabs[0].transform.position;
        currentStartLocation = SetCurrentStartLocation(startLocation);
        Vector3 targetLocation = objectPrefabs[1].transform.position;
        currentTargetLocation = SetCurrentTargetLocation(targetLocation);
        mouseVelocity = mouseObject.GetComponent<CurserFollower>().getVelocity();
        if (loggingFlag == true)
        {
            scoreStep = 1;
            objectPrefabs[1].GetComponent<MeshRenderer>().enabled = true;
            logSave(numberOfPairs, baseLineTrials, trainingTrials, washOutTrials, Time.time, Time.deltaTime, score, disturbanceArray[score], objectPrefabs[0].transform.position.x, objectPrefabs[0].transform.position.y, objectPrefabs[0].transform.position.z, objectPrefabs[1].transform.position.x, objectPrefabs[1].transform.position.y, objectPrefabs[1].transform.position.z, mouseObject.transform.position.x, mouseObject.transform.position.y, mouseObject.transform.position.z, mouseVelocity.magnitude, LearningError());
        }
        else
        {
            scoreStep = 0;
            objectPrefabs[1].GetComponent<MeshRenderer>().enabled = false;
        }
        if (IsMouseAtLocation(startLocation) == true & IsMouseVelocityLow() == true)
        {
            // start Log
            loggingFlag = true;

        }
        if (IsMouseAtLocation(targetLocation) == true & IsMouseVelocityLow() == true)
        {
            score = score + scoreStep;
            GenerateRandomTicket(); // Reposition Objects
            PositionObjects(); // Reposition Objects
            loggingFlag = false; // Stop log
        }
        UpdateScore();
        GetDistortionDecision();
        LearningError();
    }
    private void DisturbanceArrayGenerator()
    {
        int[] probabilityRatio = FloatToRational(probabilityOfDistortion);
        int arrayLength = baseLineTrials + trainingTrials + washOutTrials + 1;
        disturbanceArray = new int[arrayLength];
        int[] baselineRandomIndex = RandomIndexInSection("baseline", probabilityRatio[1]);
        for(int counter = 0; counter < baselineRandomIndex.Length; counter++)
        {
            disturbanceArray[baselineRandomIndex[counter]] = 1;
        }
        // First set entire training range to 1
        for (int counter = baseLineTrials + 1; counter < baseLineTrials + trainingTrials + 1; counter++)
        {
            disturbanceArray[counter] = 1;
        }
        // Then set randomized indexes to zero for catch trials
        int[] trainingRandomIndex = RandomIndexInSection("training", probabilityRatio[1]);
        for(int counter = 0; counter < trainingRandomIndex.Length; counter++)
        {
            disturbanceArray[trainingRandomIndex[counter]] = 0;
        }
    }
    private int[] RandomIndexInSection(string phaseName, int sectionLength)
    {
        var seededRandomObject = new System.Random(seed);
        int outputLength; int[] output;
        if (phaseName == "baseline")
        {
            outputLength = (baseLineTrials - baseLineWithoutDistortion) / sectionLength;
            output = new int[outputLength];
            output[0] = seededRandomObject.Next(0, baseLineTrials - baseLineWithoutDistortion) % sectionLength;
            for (int counter = 1; counter < output.Length; counter++)
            {
                int randomInteger = baseLineWithoutDistortion + seededRandomObject.Next(0, baseLineTrials - baseLineWithoutDistortion);
                if ((output[counter] - baseLineWithoutDistortion == 0 && output[counter - 1] - baseLineWithoutDistortion == sectionLength - 1) || (output[counter] - baseLineWithoutDistortion == sectionLength - 1 && output[counter - 1] - baseLineWithoutDistortion == 0))
                {
                    seed += 1;
                    seededRandomObject = new System.Random(seed);
                    randomInteger = seededRandomObject.Next(0, baseLineTrials - baseLineWithoutDistortion);
                    output[counter] = baseLineWithoutDistortion + randomInteger % sectionLength;
                }
                else
                {
                    output[counter] = baseLineWithoutDistortion + randomInteger % sectionLength;
                }
                seed = seed * (counter + 1);
                seededRandomObject = new System.Random(seed);
            }
            return output;
        }
        else if (phaseName == "training")
        {
            outputLength = trainingTrials / sectionLength;
            output = new int[outputLength];
            output[0] = baseLineTrials + seededRandomObject.Next(0, trainingTrials) % sectionLength;
            for (int counter = 1; counter < output.Length; counter++)
            {
                int randomInteger = seededRandomObject.Next(0, trainingTrials);
                if ((output[counter] - baseLineTrials == 0 && output[counter - 1] - baseLineTrials == sectionLength - 1) || (output[counter] - baseLineTrials == sectionLength - 1 && output[counter - 1] - baseLineTrials == 0))
                {
                    seed += 1;
                    seededRandomObject = new System.Random(seed);
                    randomInteger = seededRandomObject.Next(0, trainingTrials);
                    output[counter] = baseLineTrials + randomInteger % sectionLength;
                }
                else
                {
                    output[counter] = baseLineTrials + randomInteger % sectionLength;
                }
                seed = seed * (counter + 1);
                seededRandomObject = new System.Random(seed);
            }
            return output;
        }
        else { return output = new int[1]; }
    }
    static int GCD(int a, int b)
    {
        return b == 0 ? a : GCD(b, a % b);
    }
    static int[] FloatToRational(float floatNumber)
    {
        float floatRemainder = floatNumber - (float)Math.Truncate((double)floatNumber);
        string floatRemainderString = Convert.ToString(floatRemainder);
        int numberOfFractionDigits = floatRemainderString.Length - 2;
        float wholeNumber = floatNumber * ((float)Math.Pow(10, (double)numberOfFractionDigits));
        int number = (int)wholeNumber;
        int[] result = new int[2];
        int gcdResult = GCD((int)Math.Pow(10, (double)numberOfFractionDigits), number);
        int tempDenuminator = ((int)Math.Pow(10, (double)numberOfFractionDigits)) / (number / gcdResult);
        result[0] = gcdResult / GCD(gcdResult, tempDenuminator); //Numinator
        result[1] = tempDenuminator / GCD(gcdResult, tempDenuminator); //Denuminator
        return result;
    }
    private void ExperimentPhaseMassage(int movementNumber)
    {
        if (movementNumber >= 0 && movementNumber <= baseLineWithoutDistortion) //  0 =< Movement Number <= 45 
        {
            experimentStateMessage = "\nPure BaseLine";
        }
        else if (movementNumber > baseLineWithoutDistortion && movementNumber <= baseLineTrials) //  45 < Movement Number <= 165 
        {
            experimentStateMessage = "\nBaseLine + Distortion";
        }
        else if (movementNumber > baseLineTrials && movementNumber <= baseLineTrials + trainingTrials) //  165 < Movement Number <= 165 + 390 
        {
            experimentStateMessage = "\nTraining Phase";
        }
        else if (movementNumber > baseLineTrials + trainingTrials && movementNumber <= baseLineTrials + trainingTrials + washOutTrials)
        {
            experimentStateMessage = "\nPure Wash Out";
        }
        else
        {
            probabilityOfDistortion = 0;
            experimentStateMessage = "\nEND";
        }
    }
    private void UpdateScore()
    {
        scoreText.text = "Movement= #" + score + "\nExperiment State:" + experimentStateMessage;
    }
    public float LearningError()
    {
        Vector3 mousePos = mouseObject.transform.position;
        Ray lineRay = new Ray(GetCurrentStartLocation(), GetCurrentTargetLocation() - GetCurrentStartLocation());
        float distance = Vector3.Cross(lineRay.direction, mousePos - lineRay.origin).magnitude;
        return distance;
    }
    public bool GetDistortionDecision()
    {
        if (disturbanceArray[score] == 1) { return true; }
        else { return false; }
    }
    private bool IsMouseAtLocation(Vector3 location)
    {
        if (Vector3.Distance(mouseObject.transform.position, location) <= closenessThreshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private bool IsMouseVelocityLow()
    {

        if (mouseVelocity.magnitude < lowSpeedThreshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void GenerateRandomTicket()
    {
        randomIndex = UnityEngine.Random.Range(0, numberOfPairs); // Generate new random index for SpawnObjects
    }
    public void PositionObjects()
    {
        startPosition = new Vector3(startPositionsArray[randomIndex][0], startPositionsArray[randomIndex][1], startPositionsArray[randomIndex][2]); // Randomly select start position
        targetPosition = new Vector3(targetPositionsArray[randomIndex][0], targetPositionsArray[randomIndex][1], targetPositionsArray[randomIndex][2]); // Randomly select target position
        objectPrefabs[0].transform.position = startPosition;
        objectPrefabs[1].transform.position = targetPosition;
    }
    public void PositionGenerator()
    {
        if (centerGame == false) // If game mode is not for start object to be placed at center
        {
            startPositionsArray = new float[numberOfPairs][]; // Generate start position in a circle/sphere of diameter=totalDistance and with angle= pi/numberOfPairs apart
            double pairsCount = (double)numberOfPairs;
            for (int counter = 0; counter < numberOfPairs; counter++)
            {
                double _counter = (double)counter;
                float distanceX = (totalDistance / 2) * ((float)Math.Cos(Math.PI * _counter / pairsCount));
                float distanceY = 0.0f;
                float distanceZ = (totalDistance / 2) * ((float)Math.Sin(Math.PI * _counter / pairsCount));
                startPositionsArray[counter] = new float[] { distanceX, distanceY, distanceZ };
            }
            targetPositionsArray = new float[numberOfPairs][]; // Pair target positions on the circle in front of start positions
            for (int counter = 0; counter < numberOfPairs; counter++)
            {
                double _counter = (double)counter;
                float distanceX = (totalDistance / 2) * ((float)Math.Cos(Math.PI + Math.PI * _counter / pairsCount));
                float distanceY = 0.0f;
                float distanceZ = (totalDistance / 2) * ((float)Math.Sin(Math.PI + Math.PI * _counter / pairsCount));
                targetPositionsArray[counter] = new float[] { distanceX, distanceY, distanceZ };
            }
        }
        else // If game mode requires start object to be placed at center and target object to change position
        {
            startPositionsArray = new float[numberOfPairs][]; // Generate start position at center
            double pairsCount = (double)numberOfPairs;
            for (int counter = 0; counter < numberOfPairs; counter++)
            {
                startPositionsArray[counter] = new float[] { 0.0f, 0.0f, 0.0f };
            }
            targetPositionsArray = new float[numberOfPairs][]; // Pair target positions on the circle in front of start positions
            for (int counter = 0; counter < numberOfPairs; counter++)
            {
                double _counter = (double)counter;
                float distanceX = (totalDistance) * ((float)Math.Cos(2 * Math.PI * _counter / pairsCount));
                float distanceY = 0.0f;
                float distanceZ = (totalDistance) * ((float)Math.Sin(2 * Math.PI * _counter / pairsCount));
                targetPositionsArray[counter] = new float[] { distanceX, distanceY, distanceZ };
            }
        }
    }
    public Vector3 SetCurrentStartLocation(Vector3 vector)
    {
        return vector;
    }
    public Vector3 GetCurrentStartLocation()
    {
        return currentStartLocation;
    }
    public Vector3 SetCurrentTargetLocation(Vector3 vector)
    {
        return vector;
    }
    public Vector3 GetCurrentTargetLocation()
    {
        return currentTargetLocation;
    }
    static public void logSave(int numberOfPairs, int numberOfBaseLineTrials, int numberOfTrainingTrials, int numberOfWashoutTrials, float Time, float DeltaTime, int MovementNumber, int DistortionFlag, float StartXData, float StartYData, float StartZData, float TargetXData, float TargetYData, float TargetZData, double PositionXdata, double PositionYdata, double PositionZdata, float mouseVelocity, float LearningError)
    {

        DateTime currentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTimeKind.Local);
        string currentDateString = currentTime.ToString("d"); currentDateString = currentDateString.Replace("/", "");
        string currentTimeString = currentTime.ToString("HH"); currentTimeString = currentTimeString.Replace(":", "");
        string fileName = "log_" + currentTimeString + "_" + currentDateString + "_N" + numberOfPairs + "B" + numberOfBaseLineTrials + "T" + numberOfTrainingTrials + "W" + numberOfWashoutTrials + ".csv";
        StreamWriter sw;
        FileInfo fi;
        fi = new FileInfo(Application.dataPath + fileName);
        sw = fi.AppendText();
        sw.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}", Time, DeltaTime, MovementNumber, DistortionFlag, StartXData, StartYData, StartZData, TargetXData, TargetYData, TargetZData, PositionXdata, PositionYdata, PositionZdata, mouseVelocity, LearningError);
        sw.Flush();
        sw.Close();
    }
}
//private bool BinaryRouletteWheel(float probability)
//{
//    if (probability > 1)
//    {
//        probability = probability / 100;
//    }
//    else if (probability >= 0 && probability <= 1)
//    {
//        probability = probability;
//    }
//    int numberOfPockets = 10;
//    int numberOfPrizes = (int)Math.Floor((double)probability * numberOfPockets);
//    bool[] pocketsArray = new bool[numberOfPockets];
//    for (int counter = 0; counter < numberOfPockets; counter++)
//    {
//        if (numberOfPrizes > 0)
//        {
//            pocketsArray[counter] = true;
//        }
//        else
//        {
//            pocketsArray[counter] = false;
//        }
//        numberOfPrizes -= 1;
//    }
//    int randomNumber = UnityEngine.Random.Range(0, numberOfPockets);
//    bool output = pocketsArray[randomNumber];
//    return output;
//}
//private bool IsMouseStationaryAt(Vector3 startPosition)
//{
//    if (IsMouseAtLocation(startPosition) == true)
//    {
//        float timeLimit = 0.5f;
//        float timer = 0;
//        timer += Time.deltaTime;
//        if (timer >= timeLimit)
//        {
//            return true;
//        }
//        else
//        {
//            return false;
//        }
//    }
//    else
//    {
//        return false;
//    }
//}