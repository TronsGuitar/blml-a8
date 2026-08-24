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
            if (m_IsOpen)
            {
                CloseFile;
            }
        }

        public string FilePath
        {
            get
            {
                return m_FilePath;
            }
        }

        public bool IsOpen
        {
            get
            {
                return m_IsOpen;
            }
        }

        public long FileSize()
        {
            if (m_IsOpen)
            {
                FileSize = LOF(m_FileNum);
            }
            else
            {
                On;
                Error;
                Resume;
                Next;
                FileSize = FileLen(m_FilePath);
                On;
                Error;
                GoTo;
                0;
            }
        }

        public bool OpenForOutput(string sPath)
        {
            On;
            Error;
            GoTo;
            ErrOut;
            if (m_IsOpen)
            {
                CloseFile;
                m_FilePath = sPath;
                m_FileNum = FreeFile;
                Open;
                m_FilePath;
                for (int Output = As; Output <= ForBody; Output++)
                {
                    m_FileNum;
                    m_IsOpen = true;
                    m_Mode = "Output";
                    OpenForOutput = true;
                    break;
                    ErrOut;
                    OpenForOutput = false;
                    End;
                    Function;
                    Public;
                    Function;
                    OpenForInput(ByVal, sPath, As, String);
                    As;
                    Boolean;
                    On;
                    Error;
                    GoTo;
                    ErrIn;
                    if (m_IsOpen)
                    {
                        CloseFile;
                        m_FilePath = sPath;
                        m_FileNum = FreeFile;
                        Open;
                        m_FilePath;
                        for (int Input = As; Input <= ForBody; Input++)
                        {
                            m_FileNum;
                            m_IsOpen = true;
                            m_Mode = "Input";
                            OpenForInput = true;
                            break;
                            ErrIn;
                            OpenForInput = false;
                            End;
                            Function;
                            Public;
                            Function;
                            OpenForAppend(ByVal, sPath, As, String);
                            As;
                            Boolean;
                            On;
                            Error;
                            GoTo;
                            ErrApp;
                            if (m_IsOpen)
                            {
                                CloseFile;
                                m_FilePath = sPath;
                                m_FileNum = FreeFile;
                                Open;
                                m_FilePath;
                                for (int Append = As; Append <= ForBody; Append++)
                                {
                                    m_FileNum;
                                    m_IsOpen = true;
                                    m_Mode = "Append";
                                    OpenForAppend = true;
                                    break;
                                    ErrApp;
                                    OpenForAppend = false;
                                    End;
                                    Function;
                                    Public;
                                    Sub;
                                    WriteLine(ByVal, sLine, As, String);
                                    if (/* Unsupported Op: And */)
                                    {
                                        Print;
                                        m_FileNum;
                                        sLine;
                                    }

                                    End;
                                    Sub;
                                    Public;
                                    Sub;
                                    WriteDelimited(ParamArray, Fields(), As, Variant);
                                    if (/* Unsupported Op: And */)
                                    {
                                        long i;
                                        string sLine;
                                        for (int i = LBound(Fields); i <= UBound(Fields); i++)
                                        {
                                            if (i > LBound(Fields))
                                            {
                                                sLine = sLine + ",";
                                                sLine = sLine + Fields(i).ToString();
                                                Next;
                                                i;
                                                Write;
                                                m_FileNum;
                                                sLine;
                                            }

                                            End;
                                            Sub;
                                            Public;
                                            Function;
                                            ReadLine();
                                            As;
                                            String;
                                            if (/* Unsupported Op: And */)
                                            {
                                                string sLine;
                                                Line;
                                                Input;
                                                m_FileNum;
                                                sLine;
                                                ReadLine = sLine;
                                            }

                                            End;
                                            Function;
                                            Public;
                                            Function;
                                            ReadAll();
                                            As;
                                            String;
                                            if (/* Unsupported Op: And */)
                                            {
                                                string sAll;
                                                string sLine;
                                                while (Not(EOF(m_FileNum)))
                                                {
                                                    Line;
                                                    Input;
                                                    m_FileNum;
                                                    sLine;
                                                    if (sAll.Length > 0)
                                                    {
                                                        sAll = sAll + "\r\n";
                                                        sAll = sAll + sLine;
                                                        Loop;
                                                        ReadAll = sAll;
                                                    }

                                                    End;
                                                    Function;
                                                    Public;
                                                    Function;
                                                    AtEndOfFile();
                                                    As;
                                                    Boolean;
                                                    if (m_IsOpen)
                                                    {
                                                        AtEndOfFile = EOF(m_FileNum);
                                                    }
                                                    else
                                                    {
                                                        AtEndOfFile = true;
                                                    }

                                                    End;
                                                    Function;
                                                    Public;
                                                    Sub;
                                                    CloseFile();
                                                    if (m_IsOpen)
                                                    {
                                                        Close;
                                                        m_FileNum;
                                                        m_IsOpen = false;
                                                        m_FileNum = 0;
                                                        m_Mode = "";
                                                    }

                                                    End;
                                                    Sub;
                                                    Public;
                                                    Sub;
                                                    DeleteFile();
                                                    if (m_IsOpen)
                                                    {
                                                        CloseFile;
                                                        On;
                                                        Error;
                                                        Resume;
                                                        Next;
                                                        if (m_FilePath.Length > 0)
                                                        {
                                                            Kill;
                                                            m_FilePath;
                                                            On;
                                                            Error;
                                                            GoTo;
                                                            0;
                                                        }

                                                        Sub;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}