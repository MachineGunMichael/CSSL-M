using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Linq;
using System.Security.Cryptography;

namespace CSSL.RL
{
    public class RLControllerONNX
    {
        public RLLayerBase RLLayer { get; }

        private CancellationTokenSource cts { get; set; }

        //private enum Flag : byte
        //{
        //    WAIT, RESET, ACT, CANCEL
        //}

        public byte currentFlag { get; set; }

        private string ONNXmodel { get; set; }

        private Tensor<float> input;
        private InferenceSession session;

        public RLControllerONNX(RLLayerBase RLLayer, string ONNXmodelpath)
        {
            this.RLLayer = RLLayer;
            ONNXmodel = ONNXmodelpath;

            session = new InferenceSession(ONNXmodel);




            //SetFlag(Flag.WAIT);
        }

        //public void Dispose()
        //{
        //    writerResponse.Dispose();

        //    readerAction.Dispose();

        //    readerFlag.Dispose();
        //    writerFlag.Dispose();
        //}

        public void Run()
        {
            //Console.WriteLine("Ready to run. Press spacebar to cancel.");


            cts = new CancellationTokenSource();

            Reset();

            while (true)
            {
                Act();
            }



            //Task.Run(() =>
            //{
            //    Reset();

            //    while (!cts.IsCancellationRequested)
            //    {
            //        //Flag flag = Wait();

            //        //switch (flag)
            //        //{
            //        //    case Flag.ACT:
            //        //        Act();
            //        //        break;
            //        //    case Flag.RESET:
            //        //        Reset();
            //        //        break;
            //        //    case Flag.CANCEL:
            //        //        break;
            //        //}

            //        Act();
            //    }

            //    //Dispose();
            //});

            //while (true)
            //{
            //    if (Console.ReadKey(true).Key == ConsoleKey.Spacebar)
            //    {
            //        //Console.WriteLine("Successfully canceled.");
            //        cts.Cancel();
            //        break;
            //    }
            //}

        }

        //private void SetFlag(Flag flag)
        //{
        //    currentFlag = (byte)flag;
        //}

        //private Flag ReadFlag()
        //{
        //    return (Flag)currentFlag;
        //}

        private int ReadAction()
        {
            List<int> stateOHC = RLLayer.StateOHC;
            bool[] actionMask = RLLayer.ActionMask;

            //// Convert State 
            //var state = RLLayer.state;
            //List<int> stateOHC = new List<int>() { };
            //for (int i = 0; i < state.Length; i++)
            //{
            //    if (i < state.Length - 2)
            //    {
            //        for (int j = 0; j < state[i].Length; j++)
            //        {
            //            for (int k = 0; k < state[i][j].Length; k++)
            //                stateOHC.Add(Convert.ToInt32(state[i][j][k]));
            //        }
            //    }
            //    else
            //    {
            //        for (int k = 0; k < state[i][0].Length; k++)
            //            stateOHC.Add(Convert.ToInt32(state[i][0][k]));
            //    }
            //}

            //// Action mask
            //bool[] actionMask;
            //int line = Array.IndexOf(state[state.Length - 1][0], 1.0);

            //if (Array.IndexOf((state[line][1].Skip(Math.Max(0, state[line][1].Length - 5))).ToArray(), 1.0) == 4) //Mould cleaning
            //    actionMask = new bool[2] { false, true };
            //else if (state[line][3][0] == 0) //Not yet in Flex window
            //    actionMask = new bool[2] { false, true };
            //else if ((state[line][3][0] == 1) & (Array.IndexOf(state[state.Length - 2][0], 1.0) > 0))  //In Flex window & enough resources available
            //    actionMask = new bool[2] { true, true };
            //else if ((state[line][3][0] == 1) & (Array.IndexOf(state[state.Length - 2][0], 1.0) == 0)) //In Flex window & not enough resources available
            //    actionMask = new bool[2] { false, true };
            //else
            //{
            //    actionMask = new bool[2] { false, false };
            //    throw new Exception("Something wrong with action masking");
            //}

            // ONNX prediction
            input = new DenseTensor<float>(new[] { stateOHC.Count });

            for (int i = 0; i < stateOHC.Count; i++)
            {
                input[i] = stateOHC[i];
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", input)
            };

            var results = session.Run(inputs);
            var output = results.First().AsEnumerable<float>().ToArray();
            var max = output.Max();
            int action = output.ToList().IndexOf(max);

            // Apply action mask
            if (actionMask[action] == false)
                action = actionMask.ToList().IndexOf(true);

            return action;
        }

        //private Flag Wait()
        //{
        //    Flag flag;

        //    while (!cts.IsCancellationRequested)
        //    {
        //        flag = ReadFlag();

        //        if (flag != Flag.WAIT)
        //        {
        //            return flag;
        //        }
        //    }

        //    return Flag.CANCEL;
        //}

        public void Reset()
        {

            Console.WriteLine("Reset");
            Response response = RLLayer.Reset();



            //try
            //{
            //Console.WriteLine("Reset");
            //Response response = RLLayer.Reset();
            //SetFlag(Flag.WAIT);
            //}
            //catch (Exception exception)
            //{
            //    throw exception;
            //}
            //finally
            //{
            //    Console.WriteLine("Reset");
            //    Response response = RLLayer.Reset();

            //}
        }

        public void Act()
        {
            //try
            //{
            int action = ReadAction();
            Response response = RLLayer.Act(action);

            if (response.IsEnded)
                Reset();


            //SetFlag(Flag.WAIT);
            //}
            //catch (Exception exception)
            //{
            //    throw exception;
            //}
        }


    }
}
