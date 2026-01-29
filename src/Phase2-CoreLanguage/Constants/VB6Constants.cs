using System;

public static class VB6Constants
{
    #region Data Type Constants
    public const int vbNull = 1;
    public const int vbEmpty = 0;
    public const int vbInteger = 2;
    public const int vbLong = 3;
    public const int vbSingle = 4;
    public const int vbDouble = 5;
    public const int vbCurrency = 6;
    public const int vbDate = 7;
    public const int vbString = 8;
    public const int vbObject = 9;
    public const int vbError = 10;
    public const int vbBoolean = 11;
    public const int vbVariant = 12;
    public const int vbDataObject = 13;
    public const int vbDecimal = 14;
    public const int vbByte = 17;
    public const int vbArray = 8192;
    #endregion

    #region Date and Time Constants
    public const int vbSunday = 1;
    public const int vbMonday = 2;
    public const int vbTuesday = 3;
    public const int vbWednesday = 4;
    public const int vbThursday = 5;
    public const int vbFriday = 6;
    public const int vbSaturday = 7;
    public const int vbUseSystemDayOfWeek = 0;
    public const int vbFirstJan1 = 1;
    public const int vbFirstFourDays = 2;
    public const int vbFirstFullWeek = 3;
    #endregion

    #region String Constants
    public const char vbNullChar = '\0';
    public const char vbCr = '\r';
    public const char vbLf = '\n';
    public const string vbCrLf = "\r\n";
    public const char vbTab = '\t';
    public const char vbBack = '\b';
    public const char vbFormFeed = '\f';
    public const char vbVerticalTab = '\v';
    public static readonly string vbNullString = null; // Representing a null pointer
    #endregion

    #region Color Constants
    public const int vbBlack = 0x000000;
    public const int vbRed = 0xFF;
    public const int vbGreen = 0xFF00;
    public const int vbYellow = 0xFFFF;
    public const int vbBlue = 0xFF0000;
    public const int vbMagenta = 0xFF00FF;
    public const int vbCyan = 0xFFFF00;
    public const int vbWhite = 0xFFFFFF;
    #endregion

    #region Miscellaneous Constants
    public const int vbObjectError = unchecked((int)0x80040000);
    public const bool vbTrue = true;
    public const bool vbFalse = false;
    #endregion

    #region Tristate Constants
    public const int vbUseDefault = -2;
    public const int vbTriStateTrue = -1;
    public const int vbTriStateFalse = 0;
    #endregion

    #region Comparison Constants
    public const int vbBinaryCompare = 0;
    public const int vbTextCompare = 1;
    public const int vbDatabaseCompare = 2;
    #endregion

    #region File I/O Constants
    public const int vbNormal = 0;
    public const int vbReadOnly = 1;
    public const int vbHidden = 2;
    public const int vbSystem = 4;
    public const int vbArchive = 32;
    public const int vbAlias = 64;
    #endregion

    #region Mode Constants
    public const int vbFormControlMenu = 0;
    public const int vbModal = 1;
    public const int vbModeless = 0;
    #endregion
}
