using System;
using System.Text;
using System.IO;
using System.Security;
using System.ComponentModel;
using Microsoft.VisualBasic.CompilerServices; // This namespace is VB-specific; for a pure C# solution, remove or replace references.

namespace Microsoft.VisualBasic.CompilerServices
{
    #region BACKWARDS COMPATIBILITY
    //WARNING WARNING WARNING WARNING WARNING
    //This code exists to support Everett compiled applications. Make sure you understand
    //the backwards compatibility ramifications of any edit you make in this region.
    //WARNING WARNING WARNING WARNING WARNING

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class VB6BinaryFile : VB6RandomFile
    {
        //============================================================================
        // Constructor
        //============================================================================
        public VB6BinaryFile(string FileName, OpenAccess access, OpenShare share)
            : base(FileName, access, share, -1)
        {
        }

        // the implementation of Lock in base class VB6RandomFile does not handle m_lRecordLen=-1
        internal override void Lock(long lStart, long lEnd)
        {
            if (lStart > lEnd)
            {
                throw new ArgumentException(GetResourceString(ResID.Argument_InvalidValue1, "Start"));
            }

            long absRecordLength;
            long lStartByte;
            long lLength;

            if (m_lRecordLen == -1)
            {
                // if record len is -1, then using absolute bytes
                absRecordLength = 1;
            }
            else
            {
                absRecordLength = m_lRecordLen;
            }

            lStartByte = (lStart - 1) * absRecordLength;
            lLength = (lEnd - lStart + 1) * absRecordLength;

            m_file.Lock(lStartByte, lLength);
        }

        // see Lock description
        internal override void Unlock(long lStart, long lEnd)
        {
            if (lStart > lEnd)
            {
                throw new ArgumentException(GetResourceString(ResID.Argument_InvalidValue1, "Start"));
            }

            long absRecordLength;
            long lStartByte;
            long lLength;

            if (m_lRecordLen == -1)
            {
                // if record len is -1, then using absolute bytes
                absRecordLength = 1;
            }
            else
            {
                absRecordLength = m_lRecordLen;
            }

            lStartByte = (lStart - 1) * absRecordLength;
            lLength = (lEnd - lStart + 1) * absRecordLength;
            m_file.Unlock(lStartByte, lLength);
        }

        public override OpenMode GetMode()
        {
            return OpenMode.Binary;
        }

        internal override long Seek()
        {
            //m_file.position is the last read byte as a zero based offset
            //Seek returns the position of the next byte to read
            return (m_position + 1);
        }

        internal override void Seek(long BaseOnePosition)
        {
            if (BaseOnePosition <= 0)
            {
                throw VbMakeException(vbErrors.BadRecordNum);
            }

            long BaseZeroPosition = BaseOnePosition - 1;

            m_file.Position = BaseZeroPosition;
            m_position = BaseZeroPosition;

            if (m_sr != null)
            {
                m_sr.DiscardBufferedData();
            }
        }

        internal override long LOC()
        {
            return m_position;
        }

        internal override bool CanInput()
        {
            return true;
        }

        internal override bool CanWrite()
        {
            return true;
        }

        [SecurityCritical]
        internal override void Input(ref object Value)
        {
            Value = InputStr();
        }

        internal override void Input(ref string Value)
        {
            Value = InputStr();
        }

        internal override void Input(ref char Value)
        {
            string s = InputStr();

            if (s.Length > 0)
            {
                Value = s[0];
            }
            else
            {
                Value = '\0';
            }
        }

        internal override void Input(ref bool Value)
        {
            // BooleanType.FromString is VB-specific. Replace with equivalent logic.
            Value = BooleanType.FromString(InputStr());
        }

        internal override void Input(ref byte Value)
        {
            // ByteType.FromObject is VB-specific. Replace with appropriate parsing.
            Value = ByteType.FromObject(InputNum(VariantType.Byte));
        }

        internal override void Input(ref short Value)
        {
            Value = ShortType.FromObject(InputNum(VariantType.Short));
        }

        internal override void Input(ref int Value)
        {
            Value = IntegerType.FromObject(InputNum(VariantType.Integer));
        }

        internal override void Input(ref long Value)
        {
            Value = LongType.FromObject(InputNum(VariantType.Long));
        }

        internal override void Input(ref float Value)
        {
            Value = SingleType.FromObject(InputNum(VariantType.Single));
        }

        internal override void Input(ref double Value)
        {
            Value = DoubleType.FromObject(InputNum(VariantType.Double));
        }

        internal override void Input(ref decimal Value)
        {
            Value = DecimalType.FromObject(InputNum(VariantType.Decimal));
        }

        internal override void Input(ref DateTime Value)
        {
            // DateType.FromString is VB-specific. Replace with DateTime.Parse or similar.
            Value = DateType.FromString(InputStr(), GetCultureInfo());
        }

        internal override void Put(string Value, long RecordNumber = 0, bool StringIsFixedLength = false)
        {
            ValidateWriteable();
            PutString(RecordNumber, Value);
        }

        internal override void Get(ref string Value, long RecordNumber = 0, bool StringIsFixedLength = false)
        {
            ValidateReadable();

            int ByteLength;
            if (Value == null)
            {
                ByteLength = 0;
            }
            else
            {
                // m_Encoding is presumably defined in base class
                ByteLength = m_Encoding.GetByteCount(Value);
            }
            Value = GetFixedLengthString(RecordNumber, ByteLength);
        }

        protected override string InputStr()
        {
            int lChar;

            // If file is not readable, mimic VB6 behavior
            if ((m_access != OpenAccess.ReadWrite) && (m_access != OpenAccess.Read))
            {
                // Using a NullReferenceException hack as in original comments
                NullReferenceException JustNeedTheMessage = new NullReferenceException();
                throw new NullReferenceException(JustNeedTheMessage.Message, 
                    new IOException(GetResourceString(ResID.FileOpenedNoRead)));
            }

            // read past any leading spaces or tabs
            lChar = SkipWhiteSpaceEOF();

            if (lChar == lchDoubleQuote)
            {
                lChar = m_sr.Read();
                m_position += 1;
                return ReadInField(FIN_QSTRING);
            }
            else
            {
                return ReadInField(FIN_STRING);
            }
        }
    }
    #endregion
}