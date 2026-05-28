# Copilot Instructions

## Project Guidelines
- Code generation must respect both #nullable enabled and #nullable disabled scenarios in generated code, and avoid nullable warnings where practical.
- Generated code must support both #nullable enabled and disabled scenarios, as Dataverse sets missing primitive Custom API request parameters to their CLR defaults.