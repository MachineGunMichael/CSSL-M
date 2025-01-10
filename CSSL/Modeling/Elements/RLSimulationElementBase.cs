using CSSL.RL;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSSL.Modeling.Elements
{
    public abstract class RLSimulationElementBase: ModelElementBase
    {
        private InferenceSession session;

        public RLSimulationElementBase(ModelElementBase parent, string name, string ONNXmodel) : base(parent, name)
        {
            session = new InferenceSession(ONNXmodel);
        }

        public int GetSingleAction(DenseTensor<float> state)
        {
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", state)
            };

            var results = session.Run(inputs);
            var output = results.First().AsEnumerable<float>().ToArray();
            var max = output.Max();
            int action = output.ToList().IndexOf(max);

            return action;
        }

        public int[] GetMultiAction(DenseTensor<float> state)
        {
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", state)
            };

            var results = session.Run(inputs);
            int[] actions = new int[results.Count];
            for (int i = 0; i < results.Count; i++)
            {
                var output = results.ToList()[i].AsEnumerable<float>().ToArray();
                var max = output.Max();
                int action = output.ToList().IndexOf(max);
                actions[i] = action;
            }

            return actions;
        }

        public (int[], float[]) GetMultiActionTuple(DenseTensor<float> state)
        {
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", state)
            };

            var results = session.Run(inputs);
            int[] actions = new int[results.Count];
            float[] logits = new float[results.Count];
            for(int i=0; i < results.Count; i++)
            {
                var output = results.ToList()[i].AsEnumerable<float>().ToArray();
                var max = output.Max();
                int action = output.ToList().IndexOf(max);
                actions[i] = action;
                logits[i] = max;
            }

            return (actions, logits);
        }

    }
}
