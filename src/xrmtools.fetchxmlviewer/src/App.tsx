import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import './App.css'
import { AgGridReact } from 'ag-grid-react'
import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-quartz.css'

import { ModuleRegistry, AllCommunityModule } from 'ag-grid-community';
ModuleRegistry.registerModules([AllCommunityModule]);

// Types for data
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
// This will trigger a Web API request to the webapi:// endpoint and update the grid.
declare global {
  interface Window {
    fetchXmlQuery?: (fetchXml: string) => Promise<void>
    setResultTransform?: (fn: ((rows: any[]) => any[]) | null | undefined) => void
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

  const isDark = useIsDarkMode()
  const themeClass = isDark ? 'ag-theme-quartz-dark' : 'ag-theme-quartz'

  const autoSizeCols = useCallback(() => {
    const api = gridApiRef.current
    if (!api) return
    const allIds = api.getColumns()?.map((c: any) => c.getColId()) ?? []
    if (allIds.length) {
      api.autoSizeColumns(allIds, false)
    }
  }, [])

  const setDataIntoGrid = useCallback((data: any[]) => {
    const rows = transformRef.current(data)
    setRowData(rows)
    const first = rows[0]
    const cols: any[] = first
      ? Object.keys(first).map((k) => ({ field: k }))
      : []
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
      const end = performance.now()
      setLastRunMs(Math.round(end - start))
      setTimeout(autoSizeCols, 0)
    } catch (e) {
      console.error(e)
      setRowData([])
      setColumnDefs([])
      setLastRunMs(null)
    } finally {
      setLoading(false)
    }
  }, [autoSizeCols, setDataIntoGrid])

  // Expose global for VSIX host
  useEffect(() => {
    window.fetchXmlQuery = runFetchXml
    window.setResultTransform = (fn) => {
      transformRef.current = fn ?? defaultTransform
    }
    return () => {
      if (window.fetchXmlQuery === runFetchXml) delete window.fetchXmlQuery
      delete window.setResultTransform
    }
  }, [runFetchXml])

  const onGridReady = useCallback((params: any) => {
    gridApiRef.current = params.api
  }, [])

  const onRefreshClick = useCallback(() => {
    const q = lastQueryRef.current
    if (q) {
      runFetchXml(q)
    } else {
      autoSizeCols()
    }
  }, [runFetchXml, autoSizeCols])

  const statusText = useMemo(() => {
    const count = rowData.length
    const time = lastRunMs != null ? `${lastRunMs} ms` : '—'
    return `Records: ${count} | Time: ${time}`
  }, [rowData.length, lastRunMs])

  const gridOptions: any = useMemo(() => ({
    rowSelection: 'multiple',
    animateRows: true,
    enableCellTextSelection: true,
    defaultColDef: {
      sortable: true,
      filter: true,
      resizable: true,
      flex: 1,
      minWidth: 80,
    },
    onGridReady,
  }), [onGridReady])

  return (
    <div className="app">
      <div className="toolbar">
        <div className="status">{statusText}</div>
        <button className="refresh-btn" onClick={onRefreshClick} disabled={loading}>
          {loading ? 'Loading…' : 'Refresh'}
        </button>
      </div>
      <div className="content">
        <div className={themeClass} style={{ height: '100%', width: '100%' }}>
          <AgGridReact
            gridOptions={gridOptions}
            columnDefs={columnDefs}
            rowData={rowData}
          />
        </div>
      </div>
    </div>
  )
}

// Hook to detect dark mode; relies on prefers-color-scheme.
function useIsDarkMode() {
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
