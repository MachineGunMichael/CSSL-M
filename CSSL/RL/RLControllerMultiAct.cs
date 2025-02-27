﻿using CSSL.Examples.AccessController;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CSSL.RL
{
    public class RLControllerMultiAct : IDisposable
    {
        public RLLayerBase RLLayer { get; }

        private CancellationTokenSource cts { get; set; }

        private MemoryMappedFile mmfResponse { get; }

        private MemoryMappedFile mmfAction { get; }

        private MemoryMappedFile mmfFlag { get; }

        private MemoryMappedViewStream viewStreamResponse { get; }

        private MemoryMappedViewStream viewStreamAction { get; }

        private MemoryMappedViewStream viewStreamFlag { get; }

        private BinaryWriter writerResponse { get; }

        private BinaryReader readerAction { get; }

        private BinaryReader readerFlag { get; }

        private BinaryWriter writerFlag { get; }

        private int nrActions { get; set; }

        private enum Flag : byte
        {
            WAIT, RESET, ACT, CANCEL
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RLLayer">Reference to instance of RLLayerBase.</param>
        /// <param name="responseLength">The allocated bytes for the response (state, reward, isDone and info combined).</param>
        public RLControllerMultiAct(RLLayerBase RLLayer, int responseLength = 1000, int actionLength = 1000, int NrActions = 0)
        {
            this.RLLayer = RLLayer;
            this.nrActions = NrActions;

            using (FileStream fs = new FileStream("response", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                mmfResponse = MemoryMappedFile.CreateFromFile(fs, null, responseLength, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
                viewStreamResponse = mmfResponse.CreateViewStream();
                writerResponse = new BinaryWriter(viewStreamResponse, Encoding.UTF7);
            }

            using (FileStream fs = new FileStream("action", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                mmfAction = MemoryMappedFile.CreateFromFile(fs, null, actionLength, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
                viewStreamAction = mmfAction.CreateViewStream();
                readerAction = new BinaryReader(viewStreamAction);
            }

            using (FileStream fs = new FileStream("flag", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                mmfFlag = MemoryMappedFile.CreateFromFile(fs, null, 1, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
                viewStreamFlag = mmfFlag.CreateViewStream();
                readerFlag = new BinaryReader(viewStreamFlag);
                writerFlag = new BinaryWriter(viewStreamFlag);
            }

            SetFlag(Flag.WAIT);
            this.nrActions = nrActions;
        }

        public void Dispose()
        {
            mmfResponse.Dispose();
            viewStreamResponse.Dispose();
            writerResponse.Dispose();

            mmfAction.Dispose();
            viewStreamAction.Dispose();
            readerAction.Dispose();

            mmfFlag.Dispose();
            viewStreamFlag.Dispose();
            readerFlag.Dispose();
            writerFlag.Dispose();

        }
        public void Run()
        {
            Console.WriteLine("Ready to run. Press spacebar to cancel.");

            cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    Flag flag = Wait();

                    switch (flag)
                    {
                        case Flag.ACT:
                            Act();
                            break;
                        case Flag.RESET:
                            Reset();
                            break;
                        case Flag.CANCEL:
                            break;
                    }
                }

                Dispose();
            });

            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                {
                    Console.WriteLine("Successfully canceled.");
                    cts.Cancel();
                    break;
                }
            }
        }

        private void SetFlag(Flag flag)
        {
            viewStreamFlag.Seek(0, SeekOrigin.Begin);
            writerFlag.Write((byte)flag);
        }

        private Flag ReadFlag()
        {
            viewStreamFlag.Seek(0, SeekOrigin.Begin);
            return (Flag)readerFlag.ReadByte();
        }

        private int[] ReadAction()
        {
            viewStreamAction.Seek(0, SeekOrigin.Begin);

            int[] actions = new int[nrActions];
            for(int i=0; i < nrActions; i++)
            {
                int action = readerAction.ReadInt32();
                actions[i] = action;
            }

            return actions;
        }

        private void WriteResponse(Response response)
        {
            viewStreamResponse.Seek(0, SeekOrigin.Begin);
            var serializedResponse = JsonSerializer.Serialize(response);
            byte[] bytesLength = BitConverter.GetBytes(serializedResponse.Length);
            byte[] bytesJSON = Encoding.UTF8.GetBytes(serializedResponse);
            writerResponse.Write(bytesLength);
            writerResponse.Write(bytesJSON);
        }

        private Flag Wait()
        {
            Flag flag;

            while (!cts.IsCancellationRequested)
            {
                flag = ReadFlag();

                if (flag != Flag.WAIT)
                {
                    return flag;
                }
            }

            return Flag.CANCEL;
        }

        public void Reset()
        {
            try
            {
                Console.WriteLine("Reset");
                Response response = RLLayer.Reset();

                WriteResponse(response);

                SetFlag(Flag.WAIT);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void Act()
        {
            try
            {
                int[] actions = ReadAction();

                Response response = RLLayer.Act(actions);

                WriteResponse(response);

                SetFlag(Flag.WAIT);
            }
            catch (Exception exception)
            {
                throw exception;
            }

        }
    }
}
