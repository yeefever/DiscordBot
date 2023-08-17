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
    public String day1 { get; set; }
    public String day2 { get; set; }
    public String day3 { get; set; }
    public String day4 { get; set; }
    public String day5 { get; set; }
    public String day6 { get; set; }
    public String day7 { get; set; }
    public String day8 { get; set; }
    public String day9 { get; set; }
    public String day0 { get; set; }
}

public class AvailabilityData
{
    public string title { get; set; }
    public List<Column> columns { get; set; }
    public List<Object> dataSource { get; set; }
}