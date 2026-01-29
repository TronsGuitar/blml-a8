namespace VB6Constants
{
    /// <summary>
    /// Color constants (VB6 built-in)
    /// </summary>
    public enum ColorConstants : int
    {
        vbBlack = 0x0,
        vbRed = 0xFF,
        vbGreen = 0xFF00,
        vbYellow = 0xFFFF,
        vbBlue = 0xFF0000,
        vbMagenta = 0xFF00FF,
        vbCyan = 0xFFFF00,
        vbWhite = 0xFFFFFF
    }

    public enum FileModeConstants : int
    {
        vbInput = 1,
        vbOutput = 2,
        vbRandom = 4,
        vbAppend = 8,
        vbBinary = 32
    }

    public enum FileAttributeConstants : int
    {
        vbNormal = 0,
        vbReadOnly = 1,
        vbHidden = 2,
        vbSystem = 4,
        vbDirectory = 16,
        vbArchive = 32,
        vbAlias = 64
    }

    public enum MessageBoxButtonConstants : int
    {
        vbOKOnly = 0,
        vbOKCancel = 1,
        vbAbortRetryIgnore = 2,
        vbYesNoCancel = 3,
        vbYesNo = 4,
        vbRetryCancel = 5
    }

    public enum MessageBoxIconConstants : int
    {
        vbCritical = 16,
        vbQuestion = 32,
        vbExclamation = 48,
        vbInformation = 64
    }

    public enum MessageBoxDefaultButtonConstants : int
    {
        vbDefaultButton1 = 0,
        vbDefaultButton2 = 256,
        vbDefaultButton3 = 512,
        vbDefaultButton4 = 768
    }

    public enum MessageBoxModalityConstants : int
    {
        vbApplicationModal = 0,
        vbSystemModal = 4096
    }

    public enum MessageBoxAdditionalFlags : int
    {
        vbMsgBoxHelpButton = 16384,
        vbMsgBoxSetForeground = 65536,
        vbMsgBoxRight = 524288,
        vbMsgBoxRtlReading = 1048576
    }

    public enum MessageBoxResultConstants : int
    {
        vbOK = 1,
        vbCancel = 2,
        vbAbort = 3,
        vbRetry = 4,
        vbIgnore = 5,
        vbYes = 6,
        vbNo = 7
    }

    public enum SystemColorConstants : int
    {
        vbScrollBars = unchecked((int)0x80000000),
        vbDesktop = unchecked((int)0x80000001),
        vbActiveTitleBar = unchecked((int)0x80000002),
        vbInactiveTitleBar = unchecked((int)0x80000003),
        vbMenuBar = unchecked((int)0x80000004),
        vbWindowBackground = unchecked((int)0x80000005),
        vbWindowFrame = unchecked((int)0x80000006),
        vbMenuText = unchecked((int)0x80000007),
        vbWindowText = unchecked((int)0x80000008),
        vbTitleBarText = unchecked((int)0x80000009),
        vbActiveBorder = unchecked((int)0x8000000A),
        vbInactiveBorder = unchecked((int)0x8000000B),
        vbApplicationWorkspace = unchecked((int)0x8000000C),
        vbHighlight = unchecked((int)0x8000000D),
        vbHighlightText = unchecked((int)0x8000000E),
        vbButtonFace = unchecked((int)0x8000000F),
        vbButtonShadow = unchecked((int)0x80000010),
        vbGrayText = unchecked((int)0x80000011),
        vbButtonText = unchecked((int)0x80000012),
        vbInactiveCaptionText = unchecked((int)0x80000013),
        vb3DHighlight = unchecked((int)0x80000014),
        vb3DDKShadow = unchecked((int)0x80000015),
        vb3DLight = unchecked((int)0x80000016),
        vbInfoText = unchecked((int)0x80000017),
        vbInfoBackground = unchecked((int)0x80000018)
    }

    public enum PrinterColorMode : int
    {
        vbPRCMMonochrome = 1,
        vbPRCMColor = 2
    }

    public enum PrinterDuplex : int
    {
        vbPRDPSimplex = 1,
        vbPRDPHorizontal = 2,
        vbPRDPVertical = 3
    }

    public enum PrinterOrientation : int
    {
        vbPRORPortrait = 1,
        vbPRORLandscape = 2
    }

    public enum PrinterPrintQuality : int
    {
        vbPRPQDraft = -1,
        vbPRPQLow = -2,
        vbPRPQMedium = -3,
        vbPRPQHigh = -4
    }

    public enum VariantTypeConstants : int
    {
        vbEmpty = 0,
        vbNull = 1,
        vbInteger = 2,
        vbLong = 3,
        vbSingle = 4,
        vbDouble = 5,
        vbCurrency = 6,
        vbDate = 7,
        vbString = 8,
        vbObject = 9,
        vbError = 10,
        vbBoolean = 11,
        vbVariant = 12,
        vbDataObject = 13,
        vbDecimal = 14,
        vbByte = 17,
        vbLongLong = 20,
        vbUserDefinedType = 36,
        vbArray = 8192
    }

    public enum FormAlignment : int
    {
        vbLeftJustify = 1,
        vbRightJustify = 0,
        vbCenter = 2
    }

    public enum BorderStyleForm : int
    {
        vbBSNone = 0,
        vbFixedSingle = 1,
        vbSizable = 2,
        vbFixedDouble = 3,
        vbFixedToolWindow = 4,
        vbSizableToolWindow = 5
    }

    public enum BorderStyleShape : int
    {
        vbTransparent = 0,
        vbBSSolid = 1,
        vbBSDash = 2,
        vbBSDashDot = 3,
        vbBSDashDotDot = 4,
        vbBSInsideSolid = 5
    }
}
