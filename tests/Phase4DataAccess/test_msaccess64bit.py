from pathlib import Path
import sys
import tempfile
import unittest

REPO_ROOT = Path(__file__).resolve().parents[2]
ACCESS_MODULE_PATH = REPO_ROOT / "src" / "Phase4-DataAccess" / "Access"
sys.path.insert(0, str(ACCESS_MODULE_PATH))

from msaccess64bit import Access64Driver


class FakeProgrammingError(Exception):
    pass


class FakeTable:
    def __init__(self, table_name, table_type):
        self.table_name = table_name
        self.table_type = table_type


class FakeCursor:
    def __init__(self, rows=None, fetch_error=None, tables=None):
        self.rows = rows if rows is not None else []
        self.fetch_error = fetch_error
        self._tables = tables if tables is not None else []
        self.executed = []
        self.executed_many = []
        self.closed = False

    def execute(self, query, params=None):
        self.executed.append((query, params))

    def fetchall(self):
        if self.fetch_error is not None:
            raise self.fetch_error
        return self.rows

    def executemany(self, query, seq_of_params):
        self.executed_many.append((query, list(seq_of_params)))

    def tables(self):
        return list(self._tables)

    def close(self):
        self.closed = True


class FakeConnection:
    def __init__(self, cursor):
        self._cursor = cursor
        self.commits = 0
        self.closed = False

    def cursor(self):
        return self._cursor

    def commit(self):
        self.commits += 1

    def close(self):
        self.closed = True


class FakeOdbc:
    ProgrammingError = FakeProgrammingError

    def __init__(self, connection):
        self.connection = connection
        self.connection_strings = []

    def connect(self, connection_string):
        self.connection_strings.append(connection_string)
        return self.connection


class Access64DriverTests(unittest.TestCase):
    def test_connect_execute_and_list_tables(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            database_path = Path(temp_dir) / "sample.accdb"
            database_path.write_bytes(b"")

            cursor = FakeCursor(
                rows=[("row1",), ("row2",)],
                tables=[FakeTable("Customers", "TABLE"), FakeTable("sysdiagrams", "SYSTEM TABLE")],
            )
            connection = FakeConnection(cursor)
            odbc = FakeOdbc(connection)

            with Access64Driver(database_path, odbc_module=odbc) as driver:
                rows = driver.execute("SELECT * FROM Customers")
                tables = driver.list_tables()

            self.assertEqual([("row1",), ("row2",)], rows)
            self.assertEqual(["Customers"], tables)
            self.assertIn("Dbq=", odbc.connection_strings[0])
            self.assertTrue(cursor.closed)
            self.assertTrue(connection.closed)

    def test_execute_returns_none_for_non_query_operations(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            database_path = Path(temp_dir) / "sample.accdb"
            database_path.write_bytes(b"")

            cursor = FakeCursor(fetch_error=FakeProgrammingError())
            connection = FakeConnection(cursor)
            driver = Access64Driver(database_path, odbc_module=FakeOdbc(connection))
            driver.connect()

            result = driver.execute("UPDATE Customers SET Name = ?", ("Alice",))

            self.assertIsNone(result)
            self.assertEqual([("UPDATE Customers SET Name = ?", ("Alice",))], cursor.executed)

    def test_executemany_commits_transaction(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            database_path = Path(temp_dir) / "sample.accdb"
            database_path.write_bytes(b"")

            cursor = FakeCursor()
            connection = FakeConnection(cursor)
            driver = Access64Driver(database_path, odbc_module=FakeOdbc(connection))
            driver.connect()

            driver.executemany("INSERT INTO Customers VALUES (?)", [("Alice",), ("Bob",)])

            self.assertEqual(1, connection.commits)
            self.assertEqual(
                [("INSERT INTO Customers VALUES (?)", [("Alice",), ("Bob",)])],
                cursor.executed_many,
            )

    def test_connect_raises_for_missing_database_file(self):
        driver = Access64Driver("missing.accdb", odbc_module=FakeOdbc(FakeConnection(FakeCursor())))

        with self.assertRaises(FileNotFoundError):
            driver.connect()


if __name__ == "__main__":
    unittest.main()
