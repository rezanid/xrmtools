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

## Integration with Preview

When opening a FetchXML file with parameters in Visual Studio:
1. If auto-preview is enabled and there are parameters without defaults, a dialog will appear
2. The dialog lists all parameters (left side) and allows entering values (right side)
3. Required parameters (without defaults) are marked with a red asterisk (*)
4. Clicking OK triggers the preview with the provided values
5. The provided values are saved back to the FetchXML as default values

## Parameter Dialog Features

- **Parameter List:** Shows all parameters with their names and default values (if any)
- **Value Editor:** 
  - Multi-line text box for element parameters (XML content)
  - Single-line text box for value parameters
- **Visual Indicators:**
  - Red asterisk (*) for required parameters
  - Gray italic text showing default values
- **Buttons:**
  - **OK:** Validates all required parameters have values, then proceeds with preview
  - **Cancel:** Cancels the operation and closes the dialog
