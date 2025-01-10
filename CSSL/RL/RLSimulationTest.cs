using CSSL.Modeling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CSSL.RL
{
    public class RLSimulationTest : Simulation
    {
        public RLSimulationTest(string name, string outputDirectory = null) : base(name, outputDirectory)
        {
            MyExperiment.LengthOfReplication = double.PositiveInfinity;
            replicationExecutionProcess = new ReplicationExecutionProcess(this);
        }

        private ReplicationExecutionProcess replicationExecutionProcess { get; set; }

        public void StartTrain()
        {
            if (MyExperiment.GetCurrentReplicationNumber() == 0)
            {
                MyExperiment.StrictlyOnExperimentStart(true);

                MyModel.StrictlyOnExperimentStart();
            }

            replicationExecutionProcess.NextIteration();

            MyExecutive.TryInitialize();

            MyExperiment.StrictlyOnReplicationStart();

            MyModel.StrictlyOnReplicationStart();
        }

        public void Train()
        {
            MyExecutive.TryRunAll();
        }

        public void EndTrain()
        {
            if (MyExperiment.GetCurrentReplicationNumber() != 1)
                MyModel.StrictlyOnReplicatioEnd();

            if (MyExperiment.GetCurrentReplicationNumber() == (MyExperiment.NumberOfReplications + 1))
            {
                replicationExecutionProcess.DoEnd();

                int pid = Process.GetCurrentProcess().Id;
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
                //MyModel.StrictlyOnExperimentEnd();
            }
        }

        private class ReplicationExecutionProcess : IterativeProcess<int>
        {
            private RLSimulationTest simulation;

            public ReplicationExecutionProcess(RLSimulationTest simulation)
            {
                this.simulation = simulation;
            }

            protected override bool HasNext => simulation.MyExperiment.HasMoreReplications;

            protected sealed override void DoInitialize()
            {
                base.DoInitialize();
                simulation.MyExperiment.StrictlyOnExperimentStart(false);
                simulation.MyModel.StrictlyOnExperimentStart();
            }

            public int NextIteration()
            {
                return simulation.MyExperiment.IncrementCurrentReplicationNumber();
            }

            protected sealed override void RunIteration()
            {
                NextIteration();
                simulation.MyExecutive.TryInitialize();
                simulation.MyModel.StrictlyOnReplicationStart();
                simulation.MyExecutive.TryRunAll();
                simulation.MyModel.StrictlyOnReplicatioEnd();
            }

            public void DoEnd()
            {
                base.DoEnd();
                simulation.MyModel.StrictlyOnExperimentEnd();
            }
        }
    }
}
