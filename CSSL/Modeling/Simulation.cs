﻿using CSSL.Calendar;
using CSSL.Modeling.Elements;
using CSSL.Observer;
using CSSL.Reporting;
using CSSL.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSSL.Modeling
{
    public class Simulation : IDisposable, IName, IGetTime
    {
        public Simulation(string name, string outputDirectory = null)
        {
            Name = name;
            MyExecutive = new Executive(this);
            MyModel = new Model(name + "_Model", this);
            MyExperiment = new Experiment(name + "_Experiment", outputDirectory);
        }

        public Executive MyExecutive { get; }

        public Model MyModel { get; }

        public Experiment MyExperiment { get; }

        private ReplicationExecutionProcess replicationExecutionProcess { get; set; }

        public string Name { get; }

        public string GetEndStateIndicator => replicationExecutionProcess.MyEndStateIndicator.ToString();

        public double GetTime => MyExecutive.Time;

        public double GetPreviousEventTime => MyExecutive.PreviousEventTime;

        public double GetWallClockTime => MyExecutive.WallClockTime;

        public TimeSpan GetWallClockTimeSpan => replicationExecutionProcess.GetWallClockTimeSpan;

        public void Run()
        {
            try
            {
                replicationExecutionProcess = new ReplicationExecutionProcess(this);
                replicationExecutionProcess.TryRunAll();
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
        }

        public void Abort()
        {
            try
            {
                Simulation.ReplicationExecutionProcess.EndStateIndicator.STOP_CONDITION_SATISFIED.Equals(true);
                Dispose();
            }
            catch (Exception e )
            {
                Console.WriteLine($"Could not abort simulation: {e.Message}");
            }
        }

        public SimulationReporter MakeSimulationReporter()
        {
            return new SimulationReporter(this);
        }

        private class ReplicationExecutionProcess : IterativeProcess<int>
        {
            private Simulation simulation;

            public ReplicationExecutionProcess(Simulation simulation)
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

            protected sealed override int NextIteration()
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

            protected sealed override void DoEnd()
            {
                base.DoEnd();
                simulation.MyModel.StrictlyOnExperimentEnd();
            }
        }
    }
}
