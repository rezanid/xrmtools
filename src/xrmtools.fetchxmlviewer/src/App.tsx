import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import './App.css'

import type { ColDef } from 'ag-grid-community'
import { ModuleRegistry, AllCommunityModule } from 'ag-grid-community'
ModuleRegistry.registerModules([AllCommunityModule])

import { AgGridReact } from 'ag-grid-react'
import { ParameterDialog, type FetchParameter } from './ParameterDialog'

interface HostEnvelope {
  v: number
  kind: string
  requestId?: string
  elapsedMs?: number
  error?: { code?: string; message: string }
  parameters?: FetchParameter[]
}

function defaultTransform(rows: any[]): any[] { return rows }

declare global {
  interface Window {
    setResultTransform?: (fn: ((rows: any[]) => any[]) | null | undefined) => void
    renderFetchXmlResult?: (payloadOrRows: any, meta?: { elapsedMs?: number | null, error?: { code: string, message: string } | null }) => Promise<void>
    setLoading?: (isLoading: boolean) => void
    fetchXmlRequestStarted?: (requestId: string) => void
  }
}

function App() {
  const gridApiRef = useRef<any | null>(null)
  const transformRef = useRef<(rows: any[]) => any[]>(defaultTransform)
  const [rowData, setRowData] = useState<any[]>([])
  const [columnDefs, setColumnDefs] = useState<any[]>([])
  const [loading, setLoading] = useState(false)
  const [lastRunMs, setLastRunMs] = useState<number | null>(null)
  const [error, setError] = useState<{ code?: string, message: string } | null | undefined>(null)
  const [currentRequestId, setCurrentRequestId] = useState<string | null>(null)
  const [parameters, setParameters] = useState<FetchParameter[]>([])
  const [showParamDialog, setShowParamDialog] = useState(false)

  const defaultColDef: ColDef = { flex: 1 }

  const agThemeMode = isSystemDarkMode() ? 'dark' : 'light'

  const autoSizeCols = useCallback(() => {
    const api = gridApiRef.current
    if (!api) return
    const allIds = api.getColumns()?.map((c: any) => c.getColId()) ?? []
    if (allIds.length) api.autoSizeColumns(allIds, false)
  }, [])

  const setDataIntoGrid = useCallback((data: any[]) => {
    const rows = transformRef.current(data)
    setRowData(rows)
    const first = rows[0]
    const cols: any[] = first ? Object.keys(first).filter(k => !k.startsWith('@')).map(k => ({ field: k, headerName: k })) : []
    setColumnDefs(cols)
  }, [])

  // Host -> web message channel
  useEffect(() => {
    const webview = (window as any).chrome?.webview
    if (!webview) return
    const handler = (evt: any) => {
      const msg: HostEnvelope = evt?.data
      if (!msg || typeof msg !== 'object') return
      if (msg.v !== 1) return
      switch (msg.kind) {
        case 'fetchxml/started':
          if (msg.requestId) {
            setCurrentRequestId(msg.requestId)
            setLoading(true)
            setError(null)
          }
          break
        case 'fetchxml/cancelled':
          if (msg.requestId && msg.requestId === currentRequestId) {
            setLoading(false)
            setCurrentRequestId(null)
          }
          break
        case 'fetchxml/result':
          if (msg.requestId && msg.requestId === currentRequestId) {
            setLoading(false)
            setCurrentRequestId(null)
            if (typeof msg.elapsedMs === 'number') setLastRunMs(msg.elapsedMs)
          }
          break
        case 'fetchxml/error':
          if (msg.requestId && msg.requestId === currentRequestId) {
            setLoading(false)
            setCurrentRequestId(null)
            setError(msg.error)
          }
          break
        case 'fetchxml/request-parameters':
          if (msg.parameters && msg.parameters.length > 0) {
            setParameters(msg.parameters)
            setShowParamDialog(true)
          }
          break
      }
    }
    webview.addEventListener('message', handler)
    return () => webview.removeEventListener('message', handler)
  }, [currentRequestId])

  // Host callbacks and globals that remain supported
  useEffect(() => {
    window.fetchXmlRequestStarted = (requestId: string) => {
      setCurrentRequestId(requestId)
      setLoading(true)
      setError(null)
    }
    window.setResultTransform = (fn) => { transformRef.current = fn ?? defaultTransform }
    window.renderFetchXmlResult = async (payloadOrRows: any, meta?: { elapsedMs?: number | null, error?: { code: string, message: string } | null }) => {
      try {
        const rows = Array.isArray(payloadOrRows) ? payloadOrRows : Array.isArray(payloadOrRows?.value) ? payloadOrRows.value : []
        const err = meta?.error
        setDataIntoGrid(rows)
        setLastRunMs(typeof meta?.elapsedMs === 'number' ? Math.round(meta.elapsedMs) : null)
        setError(err)
        setTimeout(autoSizeCols, 0)
      } catch (err) { console.error(err) }
    }
    window.setLoading = setLoading
    return () => {
      delete window.setResultTransform
      delete window.renderFetchXmlResult
      delete window.setLoading
      delete window.fetchXmlRequestStarted
    }
  }, [setDataIntoGrid, autoSizeCols])

  const onGridReady = useCallback((params: any) => { gridApiRef.current = params.api }, [])

  // Parameter dialog handlers
  const handleParamOk = useCallback((values: Record<string, string>) => {
    const webview = (window as any).chrome?.webview
    setShowParamDialog(false)
    try {
      webview?.postMessage({ v: 1, kind: 'fetchxml/parameters-provided', parameters: values })
    } catch (e) {
      console.error(e)
    }
  }, [])

  const handleParamCancel = useCallback(() => {
    const webview = (window as any).chrome?.webview
    setShowParamDialog(false)
    try {
      webview?.postMessage({ v: 1, kind: 'fetchxml/parameters-cancelled' })
    } catch (e) {
      console.error(e)
    }
  }, [])

  // Unified action button: Refresh when idle, Cancel when loading
  const onActionClick = useCallback(() => {
    const webview = (window as any).chrome?.webview
    if (loading && currentRequestId) {
      try { webview?.postMessage({ v: 1, kind: 'fetchxml/cancel', requestId: currentRequestId }) } catch (e) { console.error(e) }
      return
    }
    try { webview?.postMessage({ v: 1, kind: 'fetchxml/refresh' }) } catch (e) { console.error(e) }
  }, [loading, currentRequestId])

  const statusText = useMemo(() => {
    const count = rowData.length
    const time = lastRunMs != null ? `${lastRunMs} ms` : '-'
    return error ? `Error: ${error.message}` : `Records: ${count} | Time: ${time}`
  }, [rowData.length, lastRunMs, error])

  const gridOptions: any = useMemo(() => ({
    rowSelection: 'multiple',
    suppressFieldDotNotation: true,
    animateRows: true,
    enableCellTextSelection: true,
    defaultColDef: { sortable: true, filter: true, resizable: true, flex: 1, minWidth: 80 },
    onGridReady,
  }), [onGridReady])

  return (
    <div className="app">
      <div className={`toolbar ${error ? 'toolbar--error' : ''}`}>
        <div className="status">{statusText}</div>
        <button className="refresh-btn" onClick={onActionClick}>
          {loading && (
            <svg className="spinner" viewBox="0 0 24 24" width="14" height="14" aria-hidden="true" focusable="false">
              <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3" fill="none" opacity="0.25" />
              <path d="M12 2 A10 10 0 0 1 22 12" stroke="currentColor" strokeWidth="3" fill="none" strokeLinecap="round" />
            </svg>
          )}
          {loading ? 'Cancel' : 'Execute'}
        </button>
      </div>
      <div className="content">
        <div data-ag-theme-mode={agThemeMode} style={{ height: '100%', width: '100%' }}>
          <AgGridReact gridOptions={gridOptions} columnDefs={columnDefs} rowData={rowData} defaultColDef={defaultColDef} />
        </div>
      </div>
      {showParamDialog && (
        <ParameterDialog
          parameters={parameters}
          onOk={handleParamOk}
          onCancel={handleParamCancel}
        />
      )}
    </div>
  )
}

function isSystemDarkMode() {
  const mq = useMemo(() => matchMedia('(prefers-color-scheme: dark)'), [])
  const [isDark, setIsDark] = useState(mq.matches)
  useEffect(() => {
    const handler = (e: MediaQueryListEvent) => setIsDark(e.matches)
    mq.addEventListener('change', handler)
    return () => mq.removeEventListener('change', handler)
  }, [mq])
  return isDark
}

export default App
