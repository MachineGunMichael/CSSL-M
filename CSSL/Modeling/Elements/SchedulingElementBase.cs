using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSSL.Modeling.Elements.CSSLEvent;

namespace CSSL.Modeling.Elements
{
    [Serializable]
    public abstract class SchedulingElementBase : ModelElementBase
    {
        public SchedulingElementBase(ModelElementBase parent, string name) : base(parent, name)
        {
        }

        public SchedulingElementBase(ModelElementBase parent, string name, int id) : base(parent, name, id)
        {
        }

        protected void ScheduleEvent(double time, CSSLEventAction action)
        {
            GetExecutive.ScheduleEvent(time, action);
        }

        protected void ScheduleEvent(double time, CSSLEventAction action, int id)
        {
            GetExecutive.ScheduleEvent(time, action, id);
        }

        protected void ScheduleEvent(double time, CSSLEventAction action, int id, int modelElementId)
        {
            GetExecutive.ScheduleEvent(time, action, id, modelElementId);
        }

        protected void ScheduleEvent(double time, CSSLEventAction action, int id, int modelElementId, int subModelElementId)
        {
            GetExecutive.ScheduleEvent(time, action, id, modelElementId, subModelElementId);
        }

        protected void ScheduleEndEvent(double time)
        {
            GetExecutive.ScheduleEndEvent(time);
        }

        protected void ScheduleEndEventNow()
        {
            GetExecutive.ScheduleEndEventNow();
        }
    }
}
