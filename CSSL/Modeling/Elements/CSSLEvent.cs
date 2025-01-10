using CSSL.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSSL.Modeling.Elements
{
    public class CSSLEvent : IComparable<CSSLEvent>, IIdentity
    {
        public int Id { get; }

        public int ModelElementId { get; }
        public int SubModelElementId { get; }

        public double Time { get; }

        public delegate void CSSLEventAction(CSSLEvent csslevent);

        public CSSLEventAction action { get; }

        internal CSSLEvent(double time, CSSLEventAction action)
        {
            Time = time;
            this.action = action;
        }

        internal CSSLEvent(double time, CSSLEventAction action, int id)
        {
            Time = time;
            this.action = action;
            this.Id = id;
        }

        internal CSSLEvent(double time, CSSLEventAction action, int id, int modelElementId)
        {
            Time = time;
            this.action = action;
            this.Id = id;
            this.ModelElementId = modelElementId;
        }

        internal CSSLEvent(double time, CSSLEventAction action, int id, int modelElementId, int subModelElementId)
        {
            Time = time;
            this.action = action;
            this.Id = id;
            this.ModelElementId = modelElementId;
            this.SubModelElementId = subModelElementId;
        }

        internal void Execute()
        {
            action.Invoke(this);
        }

        public int CompareTo(CSSLEvent other)
        {
            if (Time < other.Time)
            {
                return -1;
            }

            if (Time > other.Time)
            {
                return 1;
            }

            if (Id < other.Id)
            {
                return -1;
            }

            if (Id > other.Id)
            {
                return 1;
            }

            if (ReferenceEquals(this, other))
            {
                return 0;
            }
            else
            {
                throw new Exception("Times and ids were equal, but references were not, in CSSLEvent compareTo.");
            }
        }
    }
}

