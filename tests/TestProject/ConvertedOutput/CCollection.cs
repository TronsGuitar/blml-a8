using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace BLML.Generated
{
    public partial class Module
    {
        private void Class_Terminate()
        {
            m_Col = Nothing;
        }

        public object Item(object vIndex)
        {
            Attribute;
            Item;
            VB_UserMemId = 0;
            if (IsObject(m_Col(vIndex)))
            {
                Item = m_Col(vIndex);
            }
            else
            {
                Item = m_Col(vIndex);
            }
        }

        public IUnknown NewEnum
        {
            get
            {
                Attribute;
                NewEnum;
                VB_UserMemId = -(4);
                Attribute;
                NewEnum;
                VB_MemberFlags = "40";
                return m_Col;
                _NewEnum;
            }
        }

        public void Add(object vItem, string sKey = "", ref object _)
        {
            Optional;
            ByVal;
            vBefore;
            As;
            Variant;
            Optional;
            ByVal;
            vAfter;
            As;
            Variant;
            if (sKey.Length > 0)
            {
                if (Not(IsMissing(vBefore)))
                {
                    m_Col;
                    Add;
                    vItem;
                    sKey;
                    Before;
                    vBefore;
                    ElseIf;
                    Not(IsMissing(vAfter));
                    Then;
                    m_Col;
                    Add;
                    vItem;
                    sKey;
                    After;
                    vAfter;
                }
                else
                {
                    m_Col;
                    Add;
                    vItem;
                    sKey;
                }
            }
            else
            {
                m_Col;
                Add;
                vItem;
            }
        }

        public void ove(ByVal vIndex As Variant)()
        {
            m_Col;
        }

        public long Count
        {
            get
            {
                return m_Col;
                Count;
            }
        }

        public void Clear()
        {
            m_Col = New;
            Collection;
        }

        public object Find(string sSearchTerm, ref object _)
        {
            Optional;
            ByVal;
            vStartIndex;
            As;
            Variant;
            As;
            Long;
            long lStart;
            if (IsMissing(vStartIndex))
            {
                lStart = 1;
            }
            else
            {
                lStart = CLng(vStartIndex);
            }

            long i;
            for (int i = lStart; i <= m_Col; i++)
            {
                Count;
                if (m_Col(i).ToString())
                {
                    Like;
                    sSearchTerm + "*";
                    Then;
                    Find = i;
                    break;
                }
            }

            Find = 0;
        }
    }
}