-- ReceiptRen database
-- Import with the XAMPP MySQL command-line client (run from the folder holding this file):
--   C:\xampp\mysql\bin\mysql.exe -u root < database.sql

CREATE DATABASE IF NOT EXISTS receiptren
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_general_ci;

USE receiptren;

DROP TABLE IF EXISTS receipts;
DROP TABLE IF EXISTS categories;

-- One category has many receipts.
CREATE TABLE categories (
    category_id   INT AUTO_INCREMENT PRIMARY KEY,
    category_name VARCHAR(80) NOT NULL UNIQUE
);

-- Core data of the system.
-- ON DELETE RESTRICT: MySQL itself refuses to delete a category that still has receipts.
CREATE TABLE receipts (
    receipt_id   INT AUTO_INCREMENT PRIMARY KEY,
    category_id  INT           NOT NULL,
    receipt_date DATE          NOT NULL,
    store_name   VARCHAR(120)  NOT NULL,
    amount       DECIMAL(12,2) NOT NULL,
    note         VARCHAR(255)  NOT NULL DEFAULT '',
    CONSTRAINT fk_receipt_category FOREIGN KEY (category_id)
        REFERENCES categories (category_id) ON DELETE RESTRICT,
    CONSTRAINT chk_receipt_amount CHECK (amount > 0)
);

-- Default categories provided on first run.
INSERT INTO categories (category_name) VALUES
    ('Food'),
    ('Transport'),
    ('Rent'),
    ('Airtime/Data'),
    ('Health');
