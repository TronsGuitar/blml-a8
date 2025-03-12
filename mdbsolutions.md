Hi Brian, one way to isolate the problematic record is to look for dates that fall outside SQL Server's acceptable range. SQL Server datetime values must be between January 1, 1753, and December 31, 9999. In Access, run a query like this:

SELECT * FROM YourTable 
WHERE [DateField] < #1753-01-01# OR [DateField] > #9999-12-31#;

This can help you find any rows with values that may be causing the error. If the date field is stored as text or if you suspect hidden characters, you can try converting the date to a formatted string in Access. For example, use the Format function to display the date field with a fixed format, which might reveal extra characters:

SELECT ID, Format([DateField], "yyyy-mm-dd hh:nn:ss") AS FormattedDate 
FROM YourTable;

Then, inspect the output around the row where the import fails (in your case, around row 7555). You could also export the table to a CSV file and use a script—like in PowerShell—to process each row and log which ones cause conversion errors.

Another approach is to import the data in smaller batches. Start with the first 7500 rows and then incrementally add more rows until the error occurs. This can help pinpoint the exact record that triggers the issue.

These techniques should help you identify the record causing the import error. Keep up the great work, Brian—your attention to detail will get you through this challenge!

