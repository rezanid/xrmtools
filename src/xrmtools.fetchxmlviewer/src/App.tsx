import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import './App.css'

import type { ColDef } from "ag-grid-community";
import { ModuleRegistry, AllCommunityModule } from 'ag-grid-community';
ModuleRegistry.registerModules([AllCommunityModule]);

import { AgGridReact } from 'ag-grid-react'

interface FetchXmlResponse {
  value: any[]
}

// Placeholder transform function for future customization
function defaultTransform(rows: any[]): any[] {
  return rows
}

// Global API to run FetchXML from VSIX host
// The host can call window.fetchXmlQuery(fetchXml: string)
// Optionally set a result transformation via window.setResultTransform(fn)
declare global {
  interface Window {
    fetchXmlQuery?: (fetchXml: string) => Promise<void>
    setResultTransform?: (fn: ((rows: any[]) => any[]) | null | undefined) => void
    renderFetchXmlResult?: (payloadOrRows: any, meta?: { elapsedMs?: number | null, error?: { code: string, message: string } | null }) => Promise<void>
    setLoading?: (isLoading: boolean) => void
    setHostTheme?: (mode: 'light' | 'dark') => void
  }
}

function App() {
  const gridApiRef = useRef<any | null>(null)
  const lastQueryRef = useRef<string | null>(null)
  const transformRef = useRef<(rows: any[]) => any[]>(defaultTransform)
  const [rowData, setRowData] = useState<any[]>([])
  const [columnDefs, setColumnDefs] = useState<any[]>([])
  const [loading, setLoading] = useState(false)
  const [lastRunMs, setLastRunMs] = useState<number | null>(null)
  const [error, setError] = useState<{ code?: string, message: string } | null | undefined>(null)

  const defaultColDef: ColDef = {
    flex: 1,
  };

  // Host-provided theme override; null => fall back to system
  const [hostTheme, setHostThemeState] = useState<'light' | 'dark' | null>(null)
  const systemDark = useSystemDarkMode()
  const isDark = hostTheme ? hostTheme === 'dark' : systemDark

  const agThemeMode = isDark ? 'dark' : 'light'

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
    const cols: any[] = first ? Object.keys(first).filter(k => !k.startsWith('@')).map((k) => ({ field: k, headerName: k })) : []
    setColumnDefs(cols)
  }, [])

  const runFetchXml = useCallback(async (fetchXml: string) => {
    setLoading(true)
    lastQueryRef.current = fetchXml
    const start = performance.now()
    try {
      const res = await fetch('webapi://fetchxml', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ fetchXml }),
      })
      if (!res.ok) throw new Error(`Request failed: ${res.status} ${res.statusText}`)
      const payload: FetchXmlResponse = await res.json()
      const rows = Array.isArray(payload?.value) ? payload.value : []
      setDataIntoGrid(rows)
      setLastRunMs(Math.round(performance.now() - start))
      setTimeout(autoSizeCols, 0)
      setError(null)
    } catch (e) {
      console.error(e)
      setRowData([])
      setColumnDefs([])
      setLastRunMs(null)
    } finally {
      setLoading(false)
    }
  }, [autoSizeCols, setDataIntoGrid])

  useEffect(() => {
    window.fetchXmlQuery = runFetchXml
    window.setResultTransform = (fn) => { transformRef.current = fn ?? defaultTransform }
    window.renderFetchXmlResult = async (payloadOrRows: any, meta?: { elapsedMs?: number | null, error?: { code: string, message: string } | null }) => {
      try {
        const rows = Array.isArray(payloadOrRows) ? payloadOrRows : Array.isArray(payloadOrRows?.value) ? payloadOrRows.value : []
        const error = meta?.error
        setDataIntoGrid(rows)
        setLastRunMs(typeof meta?.elapsedMs === 'number' ? Math.round(meta.elapsedMs) : null)
        setError(error)
        setTimeout(autoSizeCols, 0)
      } catch (err) { console.error(err) }
      }
    window.setLoading = setLoading
    window.setHostTheme = (mode: 'light' | 'dark') => {
      setHostThemeState(mode)
      document.documentElement.setAttribute('data-theme', mode)
    }
    return () => {
      if (window.fetchXmlQuery === runFetchXml) delete window.fetchXmlQuery
      delete window.setResultTransform
      delete window.renderFetchXmlResult
      delete window.setLoading
      delete window.setHostTheme
    }
  }, [runFetchXml, setDataIntoGrid, autoSizeCols])

  const onGridReady = useCallback((params: any) => { gridApiRef.current = params.api }, [])

  const onRefreshClick = useCallback(() => {
    const q = lastQueryRef.current
    if (q) runFetchXml(q); else autoSizeCols()
  }, [runFetchXml, autoSizeCols])

  const statusText = useMemo(() => {
    const count = rowData.length
    const time = lastRunMs != null ? `${lastRunMs} ms` : '-'
    return error ? `Error: ${error.message}` : `Records: ${count} | Time: ${time} | `
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
        <button className="refresh-btn" onClick={onRefreshClick} disabled={loading}>
          {loading ? 'Loading...' : 'Refresh'}
        </button>
      </div>
      <div className="content">
        <div data-ag-theme-mode={agThemeMode} style={{ height: '100%', width: '100%' }}>
          <AgGridReact gridOptions={gridOptions} columnDefs={columnDefs} rowData={rowData} defaultColDef={defaultColDef} />
        </div>
      </div>
    </div>
  )
}

// Fallback to system-level dark detection when host doesn't set a theme
function useSystemDarkMode() {
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
