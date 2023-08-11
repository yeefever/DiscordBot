using System.Collections.Generic;

public class Column
{
    public int width { get; set; }
    public string title { get; set; }
    public string dataIndex { get; set; }
    public string align { get; set; }
}

public class DataColumn
{ 
    public String username { get; set; }
    public String mon { get; set; }
    public String tue { get; set; }
    public String wed { get; set; }
    public String thu { get; set; }
    public String fri { get; set; }
    public String sat { get; set; }
    public String sun { get; set; }
}

public class AvailabilityData
{
    public string title { get; set; }
    public List<Column> columns { get; set; }
    public List<Object> dataSource { get; set; }
}