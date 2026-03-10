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

from pathlib import Path
import sys

try:
    import pyodbc  # type: ignore
except ImportError:  # pragma: no cover - covered indirectly through injected test doubles
    pyodbc = None


class Access64Driver:
    def __init__(self, mdb_path, odbc_module=None):
        self.mdb_path = Path(mdb_path)
        self.odbc = odbc_module if odbc_module is not None else pyodbc
        self.conn = None
        self.cursor = None

    def __enter__(self):
        self.connect()
        return self

    def __exit__(self, exc_type, exc, exc_tb):
        self.close()
        return False

    def connect(self):
        if self.cursor is not None and self.conn is not None:
            return self

        if self.odbc is None:
            raise RuntimeError("pyodbc is required to connect to Access databases.")

        if not self.mdb_path.exists():
            raise FileNotFoundError(self.mdb_path)

        self.conn = self.odbc.connect(self._build_connection_string())
        self.cursor = self.conn.cursor()
        return self

    def execute(self, query, params=None):
        self._ensure_connected()
        if params is None:
            self.cursor.execute(query)
        else:
            self.cursor.execute(query, params)

        programming_error = getattr(self.odbc, "ProgrammingError", Exception)
        try:
            return self.cursor.fetchall()
        except programming_error:
            return None

    def executemany(self, query, seq_of_params):
        self._ensure_connected()
        self.cursor.executemany(query, seq_of_params)
        self.conn.commit()

    def list_tables(self):
        self._ensure_connected()
        return [table.table_name for table in self.cursor.tables() if table.table_type == "TABLE"]

    def commit(self):
        if self.conn:
            self.conn.commit()

    def close(self):
        if self.cursor:
            self.cursor.close()
            self.cursor = None
        if self.conn:
            self.conn.close()
            self.conn = None

    def _build_connection_string(self):
        return (
            r"Driver={Microsoft Access Driver (*.mdb, *.accdb)};"
            rf"Dbq={self.mdb_path};"
        )

    def _ensure_connected(self):
        if self.cursor is None or self.conn is None:
            raise RuntimeError("Database not connected.")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python access_driver.py <mdb_file_path>")
        sys.exit(1)

    mdb_file = sys.argv[1]
    try:
        with Access64Driver(mdb_file) as driver:
            print("Successfully connected to", mdb_file)
            print("Tables in the database:")
            for table_name in driver.list_tables():
                print(" -", table_name)
    except Exception as e:
        print("An error occurred:", e)