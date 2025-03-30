﻿#nullable enable
namespace XrmTools.WebApi.Types;

public class OrderExpression
{
    public string Alias { get; set; }
    public string AttributeName { get; set; }
    public string Entityname { get; set; }
    public OrderType OrderType { get; set; }
}
#nullable restore