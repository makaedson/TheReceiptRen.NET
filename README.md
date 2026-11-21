# ReceiptRen

Personal spending tracker and monthly insights system (.NET final exam project).
Windows Forms app in C# on .NET 10, with MySQL (XAMPP) and raw ADO.NET through MySqlConnector.

## Features

- Add, edit, delete and search receipts (date range, category, amount range, store keyword)
- Category management (a category in use cannot be deleted)
- Monthly report: total, category breakdown, top 3, biggest expense, daily average, busiest day, pie chart
- Month-to-month comparison with percentage change
- Insights engine that turns the numbers into plain sentences
- Report export to text or PDF through the `IReportGenerator` interface

## Setup

1. Start **MySQL** in the XAMPP Control Panel.
2. Import the database from the folder containing `database.sql`:
   `cmd /c "C:\xampp\mysql\bin\mysql.exe -u root < database.sql"`
3. Open `ReceiptRen.sln` in Visual Studio and press F5, or run `dotnet run --project ReceiptRen`.

If your MySQL root user has a password, change it in `ReceiptRen/Database.cs`.

## Database

| Table | Fields |
|-------|--------|
| categories | category_id (PK), category_name |
| receipts | receipt_id (PK), category_id (FK), receipt_date, store_name, amount, note |

One category has many receipts.

## Project structure

| File | Role |
|------|------|
| Receipt, Category, ReceiptFilter, MonthlyReport | Model classes |
| Database | Connection string |
| ReceiptController, CategoryController | Database access (ADO.NET) |
| ReportService | Report, comparison and insights (LINQ) |
| IReportGenerator, TextReportGenerator, PdfReportGenerator | Export |
| MainForm, ReceiptForm, CategoryForm, ReportForm, PieChartPanel | User interface |
