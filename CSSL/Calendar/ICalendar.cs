﻿using CSSL.Modeling;
using CSSL.Modeling.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSSL.Calendar
{
    public interface ICalendar
    {
        CSSLEvent Next();

        bool HasNext();

        CSSLEvent PeekNext();

        CSSLEvent PeekAt(int index);

        void Add(CSSLEvent e);

        void AddNow(CSSLEvent e);

        void Cancel(CSSLEvent e);

        void CancelAll();

        int Size();

        bool IsEmpty();
    }
}
