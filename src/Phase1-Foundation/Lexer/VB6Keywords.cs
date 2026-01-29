namespace VB6Keywords
{
    #region Data Type Keywords
    public enum DataTypeKeyword
    {
        vbNull = 1,
        vbEmpty = 0,
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
        vbArray = 8192
    }
    #endregion

    #region Date and Time Keywords
    public enum DateTimeKeyword
    {
        vbSunday = 1,
        vbMonday = 2,
        vbTuesday = 3,
        vbWednesday = 4,
        vbThursday = 5,
        vbFriday = 6,
        vbSaturday = 7,
        vbUseSystemDayOfWeek = 0,
        vbFirstJan1 = 1,
        vbFirstFourDays = 2,
        vbFirstFullWeek = 3
    }
    #endregion

    #region Color Keywords
    public enum ColorKeyword
    {
        vbBlack = 0x000000,
        vbRed = 0xFF,
        vbGreen = 0xFF00,
        vbYellow = 0xFFFF,
        vbBlue = 0xFF0000,
        vbMagenta = 0xFF00FF,
        vbCyan = 0xFFFF00,
        vbWhite = 0xFFFFFF
    }
    #endregion

    #region Miscellaneous Keywords
    public enum MiscKeyword
    {
        vbObjectError = unchecked((int)0x80040000),
        vbTrue = -1,
        vbFalse = 0
    }
    #endregion

    #region Tristate Keywords
    public enum TristateKeyword
    {
        vbUseDefault = -2,
        vbTriStateTrue = -1,
        vbTriStateFalse = 0
    }
    #endregion

    #region Comparison Keywords
    public enum ComparisonKeyword
    {
        vbBinaryCompare = 0,
        vbTextCompare = 1,
        vbDatabaseCompare = 2
    }
    #endregion

    #region File I/O Keywords
    public enum FileIOKeyword
    {
        vbNormal = 0,
        vbReadOnly = 1,
        vbHidden = 2,
        vbSystem = 4,
        vbArchive = 32,
        vbAlias = 64
    }
    #endregion

    #region Mode Keywords
    public enum ModeKeyword
    {
        vbFormControlMenu = 0,
        vbModal = 1,
        vbModeless = 0
    }
    #endregion
}
