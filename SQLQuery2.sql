CREATE DATABASE data;
USE data; 

-- Create Departments table
CREATE TABLE Department (
    DepartmentID INT PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL
);

-- Create Employees table with foreign key constraint
CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY IDENTITY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    BirthDate DATE,
    DepartmentID INT,
    CONSTRAINT FK_Department_Employees FOREIGN KEY (DepartmentID)
        REFERENCES Department(DepartmentID)
);


