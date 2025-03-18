// Skeleton Program for the AQA AS Summer 2024 examination
// this code should be used in conjunction with the Preliminary Material
// written by the AQA Programmer Team
// developed in a Visual Studio 2019 .net framework

// Version number: 0.0.1

using System;
using System.IO;

namespace QSim
{
    class QueueSimCS
    {
        /*
         * Universal program constants here
         * MAX_Q_SIZE will change the length of BuyerQ and be the max cannot be changed by user
         * WARNING will cause program to crash if buyers exceed queue size 5 is minimum with standard data
         * EFFECTS causes more memory to have to be created but no significant difference
         * 
         * MAX_TILLS controls the size of int[,] Tills and how high the user can change the settings to
         * WARNING will cause program to crash if too slow min with default data is 2
         * EFFECTS causes unecassary memory to be reserved no significant issue
         * 
         * MAX_TIME controls the size of int[,] Data and how high the user can set it to
         * WARNING will crash if the simulation takes longer than max time
         * EFFECTS causes unecessary data to be reserved no significant issue
         * 
         * TILL_SPEED effect the time taken to process a customer and cannot be changed by user
         * WARNING too low may have effect on time
         * EFFECTS lower values mean more simulation higher values mean quicker
         */
        const string BLANK = "   ";
        const int MAX_Q_SIZE = 5;
        const int MAX_TILLS = 2;
        const int MAX_TIME = 50;
        const int TILL_SPEED = 3;

        const int TIME_IDLE = 0;
        const int TIME_BUSY = 1;
        const int TIME_SERVING = 2;

        const int ARRIVAL_TIME = 0;
        const int ITEMS = 1;

        // indices for Stats data structure
        const int MAX_Q_LENGTH = 0;
        const int MAX_WAIT = 1;
        const int TOTAL_WAIT = 2;
        const int TOTAL_Q = 3;
        const int TOTAL_Q_OCCURRENCE = 4;
        const int TOTAL_NO_WAIT = 5;

        /*
         * struct used in creation of BuyerQ
         * @param BuyerID is just a number for the buyer
         * @param Waiting time is how long the person has been in the queue
         * @param Items in Basket is how many items they have left
         */
        public struct Q_Node
        {
            public string BuyerID;
            public int WaitingTime;
            public int ItemsInBasket;
        }
        
        /*
         * resets implicitly all data structures to 0
         * @param Tills must be an array of dimensions (MAX_TILLS + 1 , 3) to avoid error
         * @param Stats must be of length 10
         * @returns nothing however has implicitly setted the values to 0 or blank
         */
        public static void ResetDataStructures(int[] Stats, int[,] Tills, Q_Node[] BuyerQ)
        {
            for (int i = 0; i <= 9; i++)
            {
                Stats[i] = 0;
            }

            for (int Count = 0; Count <= MAX_TILLS; Count++)
            {
                for (int i = 0; i <= 2; i++)
                {
                    Tills[Count, i] = 0;
                }
            }

            for (int i = 0; i < MAX_Q_SIZE; i++)
            {
                BuyerQ[i].BuyerID = BLANK;
                BuyerQ[i].WaitingTime = 0;
                BuyerQ[i].ItemsInBasket = 0;
            }
        }

        /*
         * sets SimulationTime and NoOfTills as default 2 and 10 but allows user to change them
         * retricted between 1 and MAX_TIME and 1 and MAX_TILLS
         * @param have no restrictions
         * @param inputs must be only digits with no spaces
         * @returns a console message to let the user see what they have changed
         */
        public static void ChangeSettings(ref int SimulationTime, ref int NoOfTills)
        {
            SimulationTime = 10;
            NoOfTills = 2;
            Console.WriteLine("Settings set for this simulation:");
            Console.WriteLine("=================================");
            Console.WriteLine($"Simulation time: {SimulationTime}");
            Console.WriteLine($"Tills operating: {NoOfTills}");
            Console.WriteLine("=================================");
            Console.WriteLine();
            Console.Write("Do you wish to change the settings?  Y/N: ");
            string Answer = Console.ReadLine();
            if (Answer == "Y")
            {
                Console.WriteLine($"Maximum simulation time is {MAX_TIME} time units");
                Console.Write("Simulation run time: ");
                SimulationTime = Convert.ToInt32(Console.ReadLine());
                while (SimulationTime > MAX_TIME || SimulationTime < 1)
                {
                    Console.WriteLine($"Maximum simulation time is {MAX_TIME} time units");
                    Console.Write("Simulation run time: ");
                    SimulationTime = Convert.ToInt32(Console.ReadLine());
                }
                Console.WriteLine($"Maximum number of tills is {MAX_TILLS}");
                Console.Write("Number of tills in use: ");
                NoOfTills = Convert.ToInt32(Console.ReadLine());
                while (NoOfTills > MAX_TILLS || NoOfTills < 1)
                {
                    Console.WriteLine($"Maximum number of tills is {MAX_TILLS}");
                    Console.Write("Number of tills in use: ");
                    NoOfTills = Convert.ToInt32(Console.ReadLine());
                }
            }
        }

        /*
         * Attempts to read in data from the file in debug bin called simulation data
         * @param data passed by implicit reference must be a (MAX_TIME + 1, 2) array
         * @param datafile must be format {arival time (single digit only)}:{number of items}
         * @returns the file values seperated by colons in the textfile by implicit reference
         */
        public static void ReadInSimulationData(int[,] Data)
        {
            StreamReader FileIn = new StreamReader("SimulationData.txt");
            string DataString = FileIn.ReadLine();
            int Count = 0;
            while (!FileIn.EndOfStream)
            {
                Count += 1;
                Data[Count, ARRIVAL_TIME] = Convert.ToInt32(DataString[0].ToString());
                Data[Count, ITEMS] = Convert.ToInt32(DataString.Substring(2));
                DataString = FileIn.ReadLine();
            }
            FileIn.Close();
        }

        /*
         * Outputs a message to the console to indicate the categories
         * @param nothing 
         * @returns console message
         */
        public static void OutputHeading()
        {
            Console.WriteLine();
            Console.WriteLine("Time Buyer  | Start Till Serve | Till Time Time Time |      Queue");
            Console.WriteLine("     enters | serve      time  | num- idle busy ser- | Buyer Wait Items");
            Console.WriteLine("     (items)| buyer            | ber            ving | ID    time in");
            Console.WriteLine("            |                  |                     |            basket");
        }

        /*
         * Adds a buyer to the end of the queue and increments by 1
         * @param QLength must be within MAX_Q_SIZE
         * @param buyer number must be within range of data
         * @returns implicitly returns the queue and explicitly references QLength
         * WARNING any change to data is passed to the main program
         */
        public static void BuyerJoinsQ(int[,] Data, Q_Node[] BuyerQ, ref int QLength, int BuyerNumber)
        {
            int ItemsInBasket = Data[BuyerNumber, ITEMS];
            BuyerQ[QLength].BuyerID = $"B{BuyerNumber}";
            BuyerQ[QLength].ItemsInBasket = ItemsInBasket;
            QLength += 1;
        }

        /*
         * Outputs the buyer and their number of items then runs BuyerJoinsQ()
         * @param all values must be non null
         * @param buyer number must be within Data.Length
         * @returns BuyerQ by implicit reference
         * @returns QLength by explicit reference
         * WARNING change to any variables will affect them in the main program
         */
        public static void BuyerArrives(int[,] Data, Q_Node[] BuyerQ, ref int QLength, int BuyerNumber, ref int NoOfTills, int[] Stats)
        {
            Console.WriteLine($"  B{BuyerNumber}({Data[BuyerNumber, ITEMS]})");
            BuyerJoinsQ(Data, BuyerQ, ref QLength, BuyerNumber);
        }

        /*
         * Will do a linear search through Tills to find the first instance of a freetill
         * @param Tills is a (MAX_TILLS + 1, 3) array
         * @param NoOfTills must be a number to indicate the number of tills < Tills.Length
         * @returns index of free till if one exists
         * @returns -1 otherwise
         */
        public static int FindFreeTill(int[,] Tills, int NoOfTills)
        {
            bool FoundFreeTill = false;
            int TillNumber = 0;
            while (!FoundFreeTill && TillNumber < NoOfTills)
            {
                TillNumber += 1;
                if (Tills[TillNumber, TIME_SERVING] == 0)
                {
                    FoundFreeTill = true;
                }
            }
            if (FoundFreeTill)
            {
                return TillNumber;
            }
            else
            {
                return -1;
            }
        }

        /*
         * Explicity changes by reference the values of ThisBuyerID, ThisBuyerWaitingTime and ThisBuyerItems to the first buyer in the queue
         * Shuffles all data along up 1 and sets the final value to its initial state
         * Substracts -1 from QLength by reference
         * @param BuyerQ must be at least the size of QLength
         * @param all others have no conditions apart from that they are none null
         * @returns next customer by explicit reference
         * @returns a console statement of the buyer ID with 17 spaces of buffer
         */
        public static void ServeBuyer(Q_Node[] BuyerQ, ref int QLength, ref string ThisBuyerID, ref int ThisBuyerWaitingTime, ref int ThisBuyerItems)
        {
            ThisBuyerID = BuyerQ[0].BuyerID;
            ThisBuyerWaitingTime = BuyerQ[0].WaitingTime;
            ThisBuyerItems = BuyerQ[0].ItemsInBasket;
            for (int Count = 0; Count < QLength; Count++)
            {
                BuyerQ[Count].BuyerID = BuyerQ[Count + 1].BuyerID;
                BuyerQ[Count].WaitingTime = BuyerQ[Count + 1].WaitingTime;
                BuyerQ[Count].ItemsInBasket = BuyerQ[Count + 1].ItemsInBasket;
            }
            BuyerQ[QLength].BuyerID = BLANK;
            BuyerQ[QLength].WaitingTime = 0;
            BuyerQ[QLength].ItemsInBasket = 0;
            QLength -= 1;
            Console.Write($"{ThisBuyerID,17}");
        }

        /*
         * adds waiting time to the total wait if > the max wait time it rewrites it if no wait it adds 1 to no wait
         * @param stats is passed by implicit reference
         * @returns the updated values of stats implicitly
         */
        public static void UpdateStats(int[] Stats, int WaitingTime)
        {
            Stats[TOTAL_WAIT] += WaitingTime;
            if (WaitingTime > Stats[MAX_WAIT])
            {
                Stats[MAX_WAIT] = WaitingTime;
            }
            if (WaitingTime == 0)
            {
                Stats[TOTAL_NO_WAIT] += 1;
            }
        }

        /*
         * Calculates serving time and implicitly references Tills to assign one of its variables serving time
         * @param ThisTill must be an index within range of Tills
         * @returns Tills with an implicitly referenced change
         * @returns A console statement giving the till number and its serving time
         * WARNING any change of Tills is implicitly passed back
         */
        public static void CalculateServingTime(int[,] Tills, int ThisTill, int NoOfItems)
        {
            int ServingTime = (NoOfItems / TILL_SPEED) + 1;
            Tills[ThisTill, TIME_SERVING] = ServingTime;
            Console.WriteLine($"{ThisTill,6}{ServingTime,6}");
        }

        /*
         * Increases waiting time 
         */
        public static void IncrementTimeWaiting(Q_Node[] BuyerQ, int QLength)
        {
            for (int Count = 0; Count < QLength; Count++)
            {
                BuyerQ[Count].WaitingTime += 1;
            }
        }

        public static void UpdateTills(int[,] Tills, int NoOfTills)
        {
            for (int TillNumber = 0; TillNumber <= NoOfTills; TillNumber++)
            {
                if (Tills[TillNumber, TIME_SERVING] == 0)
                {
                    Tills[TillNumber, TIME_IDLE] += 1;
                }
                else
                {
                    Tills[TillNumber, TIME_BUSY] += 1;
                    Tills[TillNumber, TIME_SERVING] -= 1;
                }
            }
        }

        /*
         * simply prints a console message to display the state of registers and the queue
         * @param NoOfTills must be < Tills.Length
         * @param QLength must be < BuyerQ.Length
         * @returns a console message
         * WARNING any values changed are passed back to the main program
         */
        public static void OutputTillAndQueueStates(int[,] Tills, int NoOfTills, Q_Node[] BuyerQ, int QLength)
        {
            for (int i = 1; i <= NoOfTills; i++)
            {
                Console.WriteLine($"{i,36}{Tills[i, TIME_IDLE],5}{Tills[i, TIME_BUSY],5}{Tills[i, TIME_SERVING],6}");
            }
            Console.WriteLine("                                                    ** Start of queue **");
            for (int i = 0; i < QLength; i++)
            {
                Console.WriteLine($"{BuyerQ[i].BuyerID,57}{BuyerQ[i].WaitingTime,7}{BuyerQ[i].ItemsInBasket,6}");
            }
            Console.WriteLine("                                                    *** End of queue ***");
            Console.WriteLine("------------------------------------------------------------------------");
        }

        /*
         * looks for any free tills if one found it takes a buyer off the queue if there is one
         * adds this buyers wait time to stats and calculates the tills serving time
         * tries to find another free till
         * 
         * if no free tills or the queue is empty it will increment waiting time for those in the queue
         * updates tills to process items
         * updates stats to add queue occurence and overide max queue length if it is >
         * then outputs the queue states
         */
        public static void Serving(int[,] Tills, ref int NoOfTills, Q_Node[] BuyerQ, ref int QLength, int[] Stats)
        {
            int TillFree;
            string BuyerID = "";
            int WaitingTime = 0;
            int ItemsInBasket = 0;
            TillFree = FindFreeTill(Tills, NoOfTills);
            while (TillFree != -1 && QLength > 0)
            {
                ServeBuyer(BuyerQ, ref QLength, ref BuyerID, ref WaitingTime, ref ItemsInBasket);
                UpdateStats(Stats, WaitingTime);
                CalculateServingTime(Tills, TillFree, ItemsInBasket);
                TillFree = FindFreeTill(Tills, NoOfTills);
            }
            IncrementTimeWaiting(BuyerQ, QLength);
            UpdateTills(Tills, NoOfTills);
            if (QLength > 0)
            {
                Stats[TOTAL_Q_OCCURRENCE] += 1;
                Stats[TOTAL_Q] += QLength;
            }
            if (QLength > Stats[MAX_Q_LENGTH])
            {
                Stats[MAX_Q_LENGTH] = QLength;
            }
            OutputTillAndQueueStates(Tills, NoOfTills, BuyerQ, QLength);
        }

        public static bool TillsBusy(int[,] Tills, int NoOfTills)
        {
            bool IsBusy = false;
            int TillNumber = 0;
            while (!IsBusy && TillNumber <= NoOfTills)
            {
                if (Tills[TillNumber, TIME_SERVING] > 0)
                {
                    IsBusy = true;
                }
                TillNumber += 1;
            }
            return IsBusy;
        }

        public static void OutputStats(int[] Stats, int BuyerNumber, int SimulationTime)
        {
            double AverageWaitingTime, AverageQLength;
            Console.WriteLine("The simulation statistics are:");
            Console.WriteLine("==============================");
            Console.WriteLine($"The maximum queue length was: {Stats[MAX_Q_LENGTH]} buyers");
            Console.WriteLine($"The maximum waiting time was: {Stats[MAX_WAIT]} time units");
            Console.WriteLine($"{BuyerNumber} buyers arrived during {SimulationTime} time units");
            AverageWaitingTime = Math.Round((double)Stats[TOTAL_WAIT] / BuyerNumber, 1);
            Console.WriteLine($"The average waiting time was: {AverageWaitingTime} time units");
            if (Stats[TOTAL_Q_OCCURRENCE] > 0)
            {
                AverageQLength = Math.Round((double)Stats[TOTAL_Q] / Stats[TOTAL_Q_OCCURRENCE], 2);
                Console.WriteLine($"The average queue length was: {AverageQLength} buyers");
            }
            Console.WriteLine($"{Stats[TOTAL_NO_WAIT]} buyers did not need to queue");
        }

        /*
         * First few lines up to OuputHeading reserve memory and initialise values
         * reads in the first buyer arrival from queue 
         * runs for simulation time iterations
         * sees if the person in the queue has arrived if so adds them to the queue and puts next buyer time
         * otherwise it waits and prints a line in the console
         * serves the people in the queue
         * 
         * gives the program time to empty the queue and process tills
         * adds this time to a variable called extra time
         * 
         * @param nothing and no conditions
         * @returns only console messages for the simulation
         */
        public static void QueueSimulator()
        {
            int BuyerNumber = 0;
            int QLength = 0;
            int[] Stats = new int[10];
            int[,] Tills = new int[MAX_TILLS + 1, 3];
            int[,] Data = new int[MAX_TIME + 1, 2];
            Q_Node[] BuyerQ = new Q_Node[MAX_Q_SIZE];
            int SimulationTime = 0;
            int NoOfTills = 0;
            int TimeToNextArrival;
            int ExtraTime;
            int TimeUnit;
            ResetDataStructures(Stats, Tills, BuyerQ);
            ChangeSettings(ref SimulationTime, ref NoOfTills);
            ReadInSimulationData(Data);
            OutputHeading();
            TimeToNextArrival = Data[BuyerNumber + 1, ARRIVAL_TIME];
            for (TimeUnit = 0; TimeUnit < SimulationTime; TimeUnit++)
            {
                TimeToNextArrival -= 1;
                Console.Write($"{TimeUnit,3}");
                if (TimeToNextArrival == 0)
                {
                    BuyerNumber += 1;
                    BuyerArrives(Data, BuyerQ, ref QLength, BuyerNumber, ref NoOfTills, Stats);
                    TimeToNextArrival = Data[BuyerNumber + 1, ARRIVAL_TIME];
                }
                else
                {
                    Console.WriteLine();
                }
                Serving(Tills, ref NoOfTills, BuyerQ, ref QLength, Stats);
            }
            ExtraTime = 0;
            while (QLength > 0)
            {
                TimeUnit = SimulationTime + ExtraTime;
                Console.WriteLine($"{TimeUnit,3}");
                Serving(Tills, ref NoOfTills, BuyerQ, ref QLength, Stats);
                ExtraTime += 1;
            }
            while (TillsBusy(Tills, NoOfTills))
            {
                TimeUnit = SimulationTime + ExtraTime;
                Console.WriteLine($"{TimeUnit,3}");
                UpdateTills(Tills, NoOfTills);
                OutputTillAndQueueStates(Tills, NoOfTills, BuyerQ, QLength);
                ExtraTime += 1;
            }
            OutputStats(Stats, BuyerNumber, SimulationTime);
        }

        static void Main(string[] args)
        {
            QueueSimulator();
            Console.Write("Press Enter to finish");
            Console.ReadLine();
        }

    }
}
