-- ================================================
-- Table: Department
-- ================================================
CREATE TABLE Department (
    DepartmentID INT PRIMARY KEY IDENTITY,
    DepartmentName NVARCHAR(100) NOT NULL
);

-- Insert sample data into Department table
INSERT INTO Department (DepartmentName) VALUES ('Human Resources');
INSERT INTO Department (DepartmentName) VALUES ('Engineering');
INSERT INTO Department (DepartmentName) VALUES ('Sales');

-- ================================================
-- Table: Employees
-- ================================================
CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY IDENTITY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    BirthDate DATE NOT NULL,
    DepartmentID INT FOREIGN KEY REFERENCES Department(DepartmentID)
);

-- Insert sample data into Employees table
INSERT INTO Employees (FirstName, LastName, BirthDate, DepartmentID) 
VALUES ('Nada', 'Hassan', '2003-08-01', 1); -- Human Resources

INSERT INTO Employees (FirstName, LastName, BirthDate, DepartmentID) 
VALUES ('Shams', 'Elmodaser', '2004-02-14', 2); -- Engineering

-- ================================================
-- SQL Queries for CRUD Operations
-- ================================================

-- Example of a SELECT statement to retrieve all departments
SELECT * FROM Department;

-- Example of an INSERT statement to add a new employee
INSERT INTO Employees (FirstName, LastName, BirthDate, DepartmentID) 
VALUES ('Nooran', 'Ahmed', '2001-09-21', 3); -- Sales

-- Example of an UPDATE statement to modify an existing employee's details
UPDATE Employees
SET FirstName = 'Mariam'
WHERE EmployeeID = 1;

-- Example of a DELETE statement to remove an employee
DELETE FROM Employees
WHERE EmployeeID = 2;

-- Example of a SELECT statement to get the current server date
SELECT GETDATE() AS CurrentDate;
