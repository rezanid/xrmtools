# Parametric FetchXML Implementation Guide

## Overview
This document describes the implementation of parametric FetchXML queries, which allows developers to define parameters in FetchXML files that can be provided at runtime or during preview.

## Parameter Syntax

### Element-Based Parameters
Element-based parameters use `<param>` elements within the FetchXML structure:

```xml
<!-- Without default value -->
<param name='filterXml' />

<!-- With default value -->
<param name='filterXml'>
  <filter type='and'>
    <condition attribute='address1_city' operator='eq' value='Redmond' />
  </filter>
</param>
```

### Value-Based Parameters
Value-based parameters use double curly braces `{{paramName}}` in attribute values:

```xml
<!-- Without default value -->
<entity name="{{entityName}}">

<!-- With default value -->
<entity name="{{entityName:account}}">
```

## Backend Implementation

### 1. Parameter Model (`FetchParameter.cs`)
The `FetchParameter` class represents a single parameter with:
- `Name`: Parameter name
- `DefaultValue`: Optional default value
- `IsElementParameter`: Flag indicating if it's element-based or value-based
- `InnerXml`: For element parameters, stores the inner XML content

### 2. Parser Updates (`FetchXmlParser.cs`)
The parser has been updated to:
- Detect `<param>` elements using the `CollectParameters` method
- Detect `{{paramName}}` and `{{paramName:defaultValue}}` patterns in attribute values using regex
- Store detected parameters in the `FetchQuery.Parameters` collection

### 3. Code Generation Template (`FetchXml.sbncs`)
The Scriban template now generates:
- A private template constant when parameters are present
- A parametric method signature with parameter arguments
- Parameter replacement logic for both element and value-based parameters
- Default values as optional parameters in the method signature

Example generated code:
```csharp
private const string MyQuery_Template = @"<fetch>...</fetch>";

public static EntityCollection QueryMyQuery(
    this IOrganizationService service, 
    string entityName = "account",
    string filterXml = "")
{
    var fetchXml = MyQuery_Template;
    // Replace parameters...
    return service.RetrieveMultiple(new FetchExpression(fetchXml));
}
```

## Frontend Implementation

### 1. UI Components

#### FetchParameterModel
View model for binding parameter data to the UI:
- Tracks parameter name, value, and default value
- Implements `INotifyPropertyChanged` via `ViewModelBase`

#### FetchParameterEditorViewModel
Dialog view model that:
- Manages a collection of parameters
- Handles parameter selection
- Validates that required parameters have values
- Provides OK/Cancel commands

#### FetchParameterEditorDialog.xaml
WPF dialog with:
- List of parameters on the left
- Value editor on the right (multi-line TextBox for element parameters, single-line for value parameters)
- Visual indicators for required parameters (red asterisk)
- OK/Cancel buttons

#### FetchParameterEditor Service
MEF-exported service implementing `IFetchParameterEditor`:
- Shows the parameter dialog
- Returns dictionary of parameter values if confirmed
- Returns null if cancelled

### 2. Integration Points (TODO)

The parameter dialog needs to be integrated into the preview workflow:

#### In `BrowserMargin.cs`:
1. **Before executing fetch** (`ExecuteFetchXmlAsync` method):
   - Parse the FetchXML to detect parameters
   - If parameters exist without defaults, show the parameter dialog
   - If user cancels, abort the fetch
   - If user confirms, replace parameters in the FetchXML

2. **After parameter input**:
   - Use `FetchXmlParameterReplacer.ReplaceParameters` to substitute values
   - Optionally update the source file with default values using `UpdateDefaultValues`

Example integration:
```csharp
private async Task<FetchQueryResultModel> ExecuteFetchXmlAsync(
    FetchXmlDocument document, 
    CancellationToken cancellationToken)
{
    // Parse to detect parameters
    var parser = new FetchXmlParser();
    var query = await parser.ParseAsync(document.XmlDocument, cancellationToken);
    
    // If parameters exist without defaults, show dialog
    if (query.Parameters.Any(p => string.IsNullOrEmpty(p.DefaultValue)))
    {
        var paramEditor = /* Get IFetchParameterEditor via MEF */;
        var values = await paramEditor.EditParametersAsync(query.Parameters);
        
        if (values == null)
        {
            // User cancelled
            return new FetchQueryResultModel { Result = null };
        }
        
        // Replace parameters in the FetchXML
        var updatedDocument = FetchXmlParameterReplacer.ReplaceParameters(
            document.XmlDocument, 
            values, 
            updateDefaults: false);
        var fetchXml = updatedDocument.ToFullString();
        
        // Optionally: Save defaults back to file
        // var updatedDocWithDefaults = FetchXmlParameterReplacer.UpdateDefaultValues(
        //     document.XmlDocument, query.Parameters, values);
        // await File.WriteAllTextAsync(filePath, updatedDocWithDefaults.ToFullString());
        
        // Continue with modified FetchXML...
    }
    
    // Rest of execution logic...
}
```

## Testing

### Unit Tests (`FetchXmlParameterTests.cs`)
Tests verify:
- Detection of element parameters with/without defaults
- Detection of value parameters with/without defaults  
- Detection of multiple mixed parameters
- Proper parsing of default values

**Note**: Tests require Windows environment with .NET Framework 4.8 to run.

### Manual Testing Checklist
1. Create FetchXML file with element parameter without default
2. Create FetchXML file with value parameter without default
3. Create FetchXML file with mixed parameters and defaults
4. Open file in Visual Studio
5. Trigger preview (if auto-preview is enabled)
6. Verify parameter dialog appears
7. Provide parameter values
8. Click OK and verify preview shows correct results
9. Re-open file and verify default values were saved (optional feature)

## Utility Classes

### FetchXmlParameterReplacer
Static utility class providing:
- `ReplaceParameters(XmlDocumentSyntax, Dictionary<string, string>, bool)`: Replace parameters with values using XML syntax tree
- `UpdateDefaultValues(XmlDocumentSyntax, List<FetchParameter>, Dictionary<string, string>)`: Update FetchXML to persist default values
- Uses XmlDocumentSyntax for type-safe XML manipulation instead of RegEx
- Proper XML escaping for replaced values

## Future Enhancements
1. **Parameter validation**: Validate parameter types (e.g., entity names exist)
2. **IntelliSense**: Provide parameter completion in the editor
3. **Parameter discovery**: Auto-discover common parameters from metadata
4. **Preview with parameters**: Show live preview as parameters change
5. **Parameter persistence**: Remember last-used values per query
