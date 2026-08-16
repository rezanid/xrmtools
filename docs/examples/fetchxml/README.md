# FetchXML Parameter Examples

This directory contains example FetchXML files demonstrating different types of parameters.

## Examples

### Example 1: Element-based parameter without default value
**File:** `example1-element-param-no-default.fetch.xml`

Uses a `<param>` element without any default content. When code is generated, the user will be prompted to provide a value for the `filterXml` parameter.

```xml
<param name='filterXml' />
```

### Example 2: Value-based parameter without default value
**File:** `example2-value-param-no-default.fetch.xml`

Uses `{{paramName}}` syntax in an attribute value. The user will be prompted for the `entityName` parameter.

```xml
<entity name="{{entityName}}">
```

### Example 3: Element-based parameter with default value
**File:** `example3-element-param-with-default.fetch.xml`

Uses a `<param>` element with XML content as the default value. The parameter is optional when calling the generated method.

```xml
<param name='filterXml'>
  <filter type='and'>
    <condition attribute='address1_city' operator='eq' value='Redmond' />
  </filter>
</param>
```

Generated method signature:
```csharp
public static EntityCollection QueryExample3(
    this IOrganizationService service, 
    string filterXml = "<filter type='and'>...</filter>")
```

### Example 4: Value-based parameter with default value
**File:** `example4-value-param-with-default.fetch.xml`

Uses `{{paramName:defaultValue}}` syntax. The parameter is optional with "account" as the default.

```xml
<entity name="{{entityName:account}}">
```

Generated method signature:
```csharp
public static EntityCollection QueryExample4(
    this IOrganizationService service, 
    string entityName = "account")
```

### Example 5: Mixed parameters
**File:** `example5-mixed-parameters.fetch.xml`

Demonstrates multiple parameter types in one query:
- `{{entityName:account}}` - value parameter with default
- `<param name='filterXml'>...</param>` - element parameter with default XML
- `{{cityName:Redmond}}` - value parameter with default
- `{{stateCode}}` - value parameter without default (required)
- `{{orderByAttribute:name}}` - value parameter with default

Generated method signature:
```csharp
public static EntityCollection QueryExample5(
    this IOrganizationService service,
    string entityName = "account",
    string filterXml = "<filter type='and'>...</filter>",
    string cityName = "Redmond",
    string stateCode,  // Required parameter
    string orderByAttribute = "name")
```

## Usage in Code

After code generation, you can use the queries like this:

```csharp
// Example 1 - Must provide all parameters
var results1 = service.QueryExample1(
    filterXml: "<filter><condition attribute='name' operator='like' value='%test%'/></filter>");

// Example 4 - Can use default or override
var results4a = service.QueryExample4(); // Uses default "account"
var results4b = service.QueryExample4(entityName: "contact");

// Example 5 - Mix of required and optional
var results5 = service.QueryExample5(
    stateCode: "0",  // Required
    cityName: "Seattle");  // Override default
```

## Auto-named parameters

When no name is provided in the `<param>` element, an automatic name will be generated based on the parameter type and position. For example:

```xml
<param />
```
This will generate a parameter named `p1` for the first unnamed parameter in the query.

Generated method signature:
```csharp
public static EntityCollection QueryExample1(
    this IOrganizationService service,
    string p1)
```

When no name is provided in a value-based parameter, an automatic name will also be generated:
```xml
<entity name="{{}}" />
```
This will generate a parameter named `p1` for the value-based parameter in the query.

Generated method signature:
```csharp
public static EntityCollection QueryExample2(
    this IOrganizationService service,
    string p1)
```

A parameter with no name can also have a default value:
```xml
<entity name="{{:defaultValue}}" />
```

This will generate a parameter named `p1` with a default value.
Generated method signature:
```csharp
public static EntityCollection QueryExample3(
    this IOrganizationService service,
    string p1 = "defaultValue")
```