// Add as a Power Query M query named FactSales in Power BI Desktop.
let
  Source = Sql.Database("localhost","MeridianInsights"),
  Data = Source{[Schema="dbo",Item="FactSales"]}[Data],
  Typed = Table.TransformColumnTypes(Data,{{"SaleId",Int64.Type},{"AccountId",Int64.Type},{"OrderDate",type datetime},{"SalesAmount",Currency.Type},{"CostAmount",Currency.Type},{"Status",type text}}),
  WithDate = Table.AddColumn(Typed,"SaleDate",each Date.From([OrderDate]),type date),
  Valid = Table.SelectRows(WithDate,each List.Contains({"Open","Completed","Cancelled"},[Status]))
in Valid
