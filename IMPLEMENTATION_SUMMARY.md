# Parametric FetchXML - Implementation Summary

## Overview
This pull request implements support for parametric FetchXML queries as described in the original issue. The implementation allows developers to define parameters in FetchXML files using two syntaxes:

1. **Element-based**: `<param name='paramName'>default content</param>`
2. **Value-based**: `{{paramName}}` or `{{paramName:defaultValue}}`

## What Has Been Implemented

### 1. Backend Components

#### Models (`src/XrmTools/FetchXml/Model/`)
- **FetchParameter.cs**: New model class representing a parameter
  - `Name`: Parameter identifier
  - `DefaultValue`: Optional default value
  - `IsElementParameter`: Distinguishes between element and value parameters
  - `InnerXml`: For element parameters, stores XML content

- **FetchQuery.cs**: Updated to include `Parameters` collection

#### Parser (`src/XrmTools/FetchXml/CodeGen/FetchXmlParser.cs`)
- Added `ValueParameterRegex` for detecting `{{paramName}}` syntax
- New `CollectParameters` method that recursively scans XML for:
  - `<param>` elements
  - `{{paramName}}` patterns in attribute values
- New `ParseParamElement` method for parsing `<param>` elements
- New `GetInnerXml` method for extracting default XML content

#### Code Generation Template (`src/XrmTools/CodeGenTemplates/FetchXml.sbncs`)
Updated Scriban template to:
- Check if parameters exist
- Generate a private template constant when parameters are present
- Generate method with parameter arguments (with defaults where applicable)
- Generate parameter replacement logic using:
  - Regex for element parameters
  - String.Replace for value parameters
- Maintain backward compatibility (non-parametric queries work as before)

#### Utilities (`src/XrmTools/FetchXml/FetchXmlParameterReplacer.cs`)
- `ReplaceParameters`: Replaces parameters with values
- `UpdateDefaultValues`: Updates FetchXML to persist default values
- `EscapeXmlValue`: Helper for XML escaping

### 2. Frontend Components

#### UI Models (`src/XrmTools/UI/`)
- **FetchParameterModel.cs**: View model for parameter binding
  - Properties for name, value, default value
  - Tracks whether parameter has default
  - Tracks whether parameter is element-based

- **FetchParameterEditorViewModel.cs**: Dialog view model
  - Manages observable collection of parameters
  - Handles parameter selection
  - Validates required parameters have values
  - Provides OK/Cancel commands

#### UI Controls
- **FetchParameterEditorDialog.xaml**: WPF dialog
  - Split layout: parameter list (left) and value editor (right)
  - Visual indicators for required parameters (red asterisk)
  - Multi-line editor for element parameters
  - Single-line editor for value parameters
  - Integrated with VS theming

- **FetchParameterEditorDialog.xaml.cs**: Code-behind (minimal)

- **InverseBooleanToVisibilityConverter.cs**: XAML converter for visibility binding

#### Services
- **FetchParameterEditor.cs**: MEF-exported service
  - Implements `IFetchParameterEditor` interface
  - Shows parameter dialog
  - Returns dictionary of values or null if cancelled

### 3. Testing

#### Unit Tests (`src/UnitTests/XrmTools.Tests/FetchXml/FetchXmlParameterTests.cs`)
Tests for:
- Element parameter detection (with/without defaults)
- Value parameter detection (with/without defaults)
- Multiple mixed parameters
- Default value parsing

**Note**: Tests require Windows + .NET Framework 4.8 to run.

### 4. Documentation

#### Implementation Guide (`docs/ParametricFetchXML.md`)
- Comprehensive documentation of all components
- Parameter syntax reference
- Integration instructions for preview workflow
- Testing checklist
- Future enhancement ideas

#### Examples (`docs/examples/fetchxml/`)
Five complete example files demonstrating:
1. Element parameter without default
2. Value parameter without default
3. Element parameter with default
4. Value parameter with default
5. Mixed parameters (combination of all types)

Plus detailed README with usage examples.

## What Needs to Be Done (Windows Environment Required)

### 1. Preview Workflow Integration
The parameter dialog needs to be integrated into the FetchXML preview workflow in `BrowserMargin.cs`:

```csharp
// In ExecuteFetchXmlAsync method:
// 1. Parse FetchXML to detect parameters
// 2. If parameters without defaults exist, show dialog
// 3. If user confirms, replace parameters
// 4. Optionally save defaults back to file
// 5. Continue with preview
```

See `docs/ParametricFetchXML.md` for detailed integration code.

### 2. Testing in Visual Studio
The following need to be tested in a Windows/Visual Studio environment:
- Build the VS extension project
- Run unit tests
- Test parameter detection with example files
- Test code generation with parameters
- Test UI dialog appearance and functionality
- Test parameter value persistence
- Test preview with parameters

### 3. File Update Logic
Implement logic to save parameter values back to the FetchXML file as defaults:
- Read current file content
- Parse to find parameters
- Use `FetchXmlParameterReplacer.UpdateDefaultValues`
- Write updated content back to file
- Refresh editor view

## Code Generation Examples

### Input FetchXML
```xml
<fetch>
  <entity name="{{entityName:account}}">
    <attribute name="accountid" />
    <param name="filterXml">
      <filter type="and">
        <condition attribute="name" operator="like" value="{{searchTerm:test}}" />
      </filter>
    </param>
  </entity>
</fetch>
```

### Generated Code
```csharp
internal static partial class FetchQueries
{
    private const string MyQuery_Template = @"...";
    
    public static EntityCollection QueryMyQuery(
        this IOrganizationService service,
        string entityName = "account",
        string filterXml = "<filter type='and'>...</filter>",
        string searchTerm = "test")
    {
        var fetchXml = MyQuery_Template;
        
        // Replace {{entityName}} with provided value
        fetchXml = fetchXml.Replace("{{entityName}}", entityName);
        fetchXml = fetchXml.Replace("{{entityName:account}}", entityName);
        
        // Replace <param name='filterXml' /> with provided value
        fetchXml = Regex.Replace(fetchXml, @"<param\s+name\s*=\s*['\""]filterXml['\""]...", 
            filterXml ?? string.Empty, ...);
        
        // Replace {{searchTerm}} with provided value
        fetchXml = fetchXml.Replace("{{searchTerm}}", searchTerm);
        fetchXml = fetchXml.Replace("{{searchTerm:test}}", searchTerm);
        
        return service.RetrieveMultiple(new FetchExpression(fetchXml));
    }
}
```

## Architecture Decisions

1. **Two Parameter Syntaxes**: Element-based for XML fragments, value-based for simple text values
2. **Default Values**: Optional, specified inline with the parameter
3. **Code Generation**: Uses Scriban template system (consistent with existing code generators)
4. **UI Framework**: WPF with VS theming (consistent with existing dialogs like EnvironmentEditor)
5. **Service Pattern**: MEF-based dependency injection (consistent with existing services)
6. **Backward Compatibility**: Non-parametric queries continue to work exactly as before

## Dependencies

- Scriban (already in use)
- Microsoft.Language.Xml (already in use for parsing)
- System.Text.RegularExpressions (built-in)
- WPF and Visual Studio Shell APIs (already in use)

## Testing Strategy

1. **Unit Tests**: Verify parameter detection logic
2. **Integration Tests**: Test with example files in VS
3. **Manual Testing**: Exercise all parameter combinations
4. **Regression Testing**: Verify existing non-parametric queries still work

## Known Limitations

1. **Windows Only**: VS extension requires Windows environment
2. **No Type Validation**: Parameters are always strings; no type checking
3. **No IntelliSense**: No auto-completion for parameter names (future enhancement)
4. **Manual Integration**: Preview workflow integration requires manual code changes

## Related Files

### Modified Files
- `src/XrmTools/FetchXml/Model/FetchQuery.cs`
- `src/XrmTools/FetchXml/CodeGen/FetchXmlParser.cs`
- `src/XrmTools/CodeGenTemplates/FetchXml.sbncs`

### New Files
- `src/XrmTools/FetchXml/Model/FetchParameter.cs`
- `src/XrmTools/FetchXml/FetchXmlParameterReplacer.cs`
- `src/XrmTools/UI/FetchParameterModel.cs`
- `src/XrmTools/UI/FetchParameterEditorViewModel.cs`
- `src/XrmTools/UI/FetchParameterEditorDialog.xaml`
- `src/XrmTools/UI/FetchParameterEditorDialog.xaml.cs`
- `src/XrmTools/UI/FetchParameterEditor.cs`
- `src/XrmTools/UI/InverseBooleanToVisibilityConverter.cs`
- `src/UnitTests/XrmTools.Tests/FetchXml/FetchXmlParameterTests.cs`
- `docs/ParametricFetchXML.md`
- `docs/examples/fetchxml/*.fetch.xml`
- `docs/examples/fetchxml/README.md`

## Next Steps for Developer with Windows Environment

1. Open solution in Visual Studio
2. Build the project (may need to restore NuGet packages)
3. Run unit tests to verify parameter detection
4. Test code generation with example FetchXML files
5. Implement preview workflow integration following the guide in `docs/ParametricFetchXML.md`
6. Test the complete end-to-end workflow
7. Consider adding the file update logic for persisting defaults
8. Add any necessary error handling and validation

## Conclusion

This implementation provides a solid foundation for parametric FetchXML queries. All core functionality is in place:
- Parameter detection and parsing ✅
- Code generation with parameters ✅
- UI for parameter input ✅
- Utility classes for parameter replacement ✅
- Comprehensive documentation and examples ✅

The remaining work (preview integration and testing) requires a Windows environment with Visual Studio installed and is straightforward to complete following the provided documentation.
