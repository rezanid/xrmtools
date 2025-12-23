import { useState, useCallback, useEffect } from 'react'
import './ParameterDialog.css'

export interface FetchParameter {
  name: string
  defaultValue?: string
  isElement: boolean
}

interface ParameterDialogProps {
  parameters: FetchParameter[]
  onOk: (values: Record<string, string>) => void
  onCancel: () => void
}

export function ParameterDialog({ parameters, onOk, onCancel }: ParameterDialogProps) {
  const [selectedParam, setSelectedParam] = useState<string | null>(
    parameters.length > 0 ? parameters[0].name : null
  )
  const [values, setValues] = useState<Record<string, string>>(() => {
    const initial: Record<string, string> = {}
    parameters.forEach(p => {
      initial[p.name] = p.defaultValue || ''
    })
    return initial
  })

  useEffect(() => {
    if (parameters.length > 0 && !selectedParam) {
      setSelectedParam(parameters[0].name)
    }
  }, [parameters, selectedParam])

  const handleParamClick = useCallback((paramName: string) => {
    setSelectedParam(paramName)
  }, [])

  const handleValueChange = useCallback((value: string) => {
    if (selectedParam) {
      setValues(prev => ({ ...prev, [selectedParam]: value }))
    }
  }, [selectedParam])

  const handleOk = useCallback(() => {
    onOk(values)
  }, [values, onOk])

  const selectedParameter = parameters.find(p => p.name === selectedParam)
  const currentValue = selectedParam ? values[selectedParam] : ''

  return (
    <div className="param-dialog-overlay">
      <div className="param-dialog">
        <div className="param-dialog-header">
          <h2>Set Parameter Values</h2>
        </div>
        <div className="param-dialog-content">
          <div className="param-list">
            <div className="param-list-header">Parameters</div>
            {parameters.map(param => (
              <div
                key={param.name}
                className={`param-item ${selectedParam === param.name ? 'param-item--selected' : ''}`}
                onClick={() => handleParamClick(param.name)}
                role="button"
                tabIndex={0}
                onKeyDown={e => e.key === 'Enter' && handleParamClick(param.name)}
                aria-label={`Parameter ${param.name}${param.defaultValue ? ' with default value' : ''}`}
              >
                <div className="param-item-name">{param.name}</div>
                {param.defaultValue && (
                  <div className="param-item-default" title="Has default value" aria-label="Has default value">✓</div>
                )}
              </div>
            ))}
          </div>
          <div className="param-editor">
            {selectedParameter ? (
              <>
                <div className="param-editor-header">
                  <span className="param-editor-name">{selectedParameter.name}</span>
                  {selectedParameter.defaultValue && (
                    <span className="param-editor-hint">Default: {selectedParameter.defaultValue}</span>
                  )}
                </div>
                {selectedParameter.isElement ? (
                  <textarea
                    className="param-editor-textarea"
                    value={currentValue}
                    onChange={e => handleValueChange(e.target.value)}
                    placeholder={selectedParameter.defaultValue ? "Using default value" : "Enter XML content"}
                  />
                ) : (
                  <input
                    type="text"
                    className="param-editor-input"
                    value={currentValue}
                    onChange={e => handleValueChange(e.target.value)}
                    placeholder={selectedParameter.defaultValue ? "Using default value" : "Enter value"}
                  />
                )}
              </>
            ) : (
              <div className="param-editor-empty">Select a parameter to edit</div>
            )}
          </div>
        </div>
        <div className="param-dialog-footer">
          <button className="param-btn param-btn--secondary" onClick={onCancel}>
            Cancel
          </button>
          <button className="param-btn param-btn--primary" onClick={handleOk}>
            OK
          </button>
        </div>
      </div>
    </div>
  )
}
