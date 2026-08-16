# Copilot Instructions

## Project Guidelines
- Code generation must respect both #nullable enabled and #nullable disabled scenarios in generated code, and avoid nullable warnings where practical.
- Generated code must support both #nullable enabled and disabled scenarios, as Dataverse sets missing primitive Custom API request parameters to their CLR defaults.
- XrmTools.WebApi should consume public XrmTools.Meta.Attributes types, and the XrmTools.Meta.Attributes package version/reference should be increased and updated when enabling this behavior.

## Web API Guidelines and Facts
- In Dataverse Web API responses, primary key values are returned even when not explicitly included in $select for entity lookups; do not assume missing $select on primary keys causes null IDs.