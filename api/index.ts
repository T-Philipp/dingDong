import express, { Response } from 'express'
import path from 'path'

type PythonTime = {
  year: number
  month: number
  day: number
  hour: number
  minute: number
  second: number
  weekday: number
}

const app = express()
app.use(express.json())
app.use(express.urlencoded({ extended: true }))

// Get the current UTC date
const utcDate: Date = new Date()

// Convert UTC date to Berlin time
const deDateTime: string = utcDate.toLocaleString('de-DE', {
  timeZone: 'Europe/Berlin'
})
const deDate: string = utcDate.toLocaleString('de-DE', {
  timeZone: 'Europe/Berlin',
  day: '2-digit',
  month: 'short',
  year: 'numeric'
})
const deDay: string = utcDate.toLocaleString('de-DE', {
  timeZone: 'Europe/Berlin',
  weekday: 'short'
})
const deTime: string = utcDate.toLocaleString('de-DE', {
  timeZone: 'Europe/Berlin',
  timeStyle: 'medium'
})
const pythonTime: PythonTime = {
  year: parseInt(
    utcDate.toLocaleString('de-DE', {
      timeZone: 'Europe/Berlin',
      year: 'numeric'
    })
  ),
  month: parseInt(
    utcDate.toLocaleString('de-DE', {
      timeZone: 'Europe/Berlin',
      month: 'numeric'
    })
  ),
  day: parseInt(
    utcDate.toLocaleString('de-DE', {
      timeZone: 'Europe/Berlin',
      day: '2-digit'
    })
  ),
  hour: parseInt(
    utcDate.toLocaleString('de-DE', {
      timeZone: 'Europe/Berlin',
      hour: 'numeric'
    })
  ),
  minute: parseInt(
    utcDate.toLocaleString('de-DE', {
      timeZone: 'Europe/Berlin',
      minute: 'numeric'
    })
  ),
  second: parseInt(
    utcDate.toLocaleString('de-DE', {
      timeZone: 'Europe/Berlin',
      second: 'numeric'
    })
  ),
  weekday: utcDate.getDay()
}

// Create output object
const zeiten_output: {
  dateTime: string
  date: string
  day: string
  time: string
  pythonTime: PythonTime
} = {
  dateTime: deDateTime,
  date: deDate,
  day: deDay,
  time: deTime,
  pythonTime: pythonTime
}

// Serve the main HTML file
app.get('/', function (_, res: Response): void {
  res.sendFile(path.join(__dirname, '..', 'sites', 'index.html'))
})

// Serve the JSON file with additional times
app.get('/zus-zeiten.json', function (_, res: Response): void {
  res.setHeader('Content-Type', 'application/json')
  res.sendFile(path.join(__dirname, '..', 'sites', 'zus-zeiten.json'))
})

// Serve the current time in Berlin
app.get('/time', function (_, res: Response): void {
  res.setHeader('Content-Type', 'application/json')
  res.send(JSON.stringify(zeiten_output))
})

// Start the server
// app.listen(4000, (): void => console.log('Server ready on port 3000.'))

export default app
