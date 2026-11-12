# Report construction checklist

1. Import FactSales with FactSales.pq and load dbo.DimAccount with the SQL Server connector.
2. Create active many-to-one relationship FactSales[AccountId] -> DimAccount[AccountId].
3. Add the DAX measures individually; format Gross Margin % as percent.
4. Executive page: Revenue, Profit, Margin and Avg Ticket cards; SalesDate revenue line; Sector revenue bar; Region slicer; account-level matrix.
5. Pipeline page: Open Pipeline card and order-status table with account, date and sales amount.
6. Publish to a Power BI workspace, validate real totals and permissions, then set report ID and embed URL in web/.env.local.

This repo has the modeling instructions, not an exported or pre-published .pbix report.
