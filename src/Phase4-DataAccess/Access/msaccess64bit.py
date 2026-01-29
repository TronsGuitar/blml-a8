#!/usr/bin/env python3
"""
Access64Driver - A simple driver for MS Access 2007 MDB files using the ACE OLEDB provider.
This driver provides functionality similar to the Microsoft Jet Driver while working in 64-bit mode.
You must have the Microsoft Access Database Engine (2010 Redistributable or later) installed.

Usage:
    driver = Access64Driver("path_to_file.mdb")
    driver.connect()
    results = driver.execute("SELECT * FROM SomeTable")
    for row in results:
        print(row)
    driver.close()
"""

import pyodbc
import sys

class Access64Driver:
    def __init__(self, mdb_path):
        self.mdb_path = mdb_path
        self.conn = None
        self.cursor = None

    def connect(self):
        # Build the connection string. The driver supports both .mdb and .accdb files.
        connection_str = (
            r"Driver={Microsoft Access Driver (*.mdb, *.accdb)};"
            r"Dbq=" + self.mdb_path + ";"
        )
        self.conn = pyodbc.connect(connection_str)
        self.cursor = self.conn.cursor()

    def execute(self, query, params=None):
        if self.cursor is None:
            raise Exception("Database not connected.")
        if params is None:
            self.cursor.execute(query)
        else:
            self.cursor.execute(query, params)
        try:
            return self.cursor.fetchall()
        except pyodbc.ProgrammingError:
            return None

    def executemany(self, query, seq_of_params):
        if self.cursor is None:
            raise Exception("Database not connected.")
        self.cursor.executemany(query, seq_of_params)
        self.conn.commit()

    def commit(self):
        if self.conn:
            self.conn.commit()

    def close(self):
        if self.cursor:
            self.cursor.close()
        if self.conn:
            self.conn.close()


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python access_driver.py <mdb_file_path>")
        sys.exit(1)
    mdb_file = sys.argv[1]
    driver = Access64Driver(mdb_file)
    try:
        driver.connect()
        print("Successfully connected to", mdb_file)
        # List available tables in the database.
        tables = driver.cursor.tables()
        print("Tables in the database:")
        for table in tables:
            if table.table_type == "TABLE":
                print(" -", table.table_name)
    except Exception as e:
        print("An error occurred:", e)
    finally:
        driver.close()