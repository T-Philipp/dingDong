export default async (): Promise<Response> => {
  // Get the current UTC date
  const utcDate = new Date()

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

  // Create output object
  const output = {
    dateTime: deDateTime,
    date: deDate,
    day: deDay,
    time: deTime
  }

  // Return response with JSON output and appropriate headers
  return new Response(JSON.stringify(output), {
    headers: {
      'Content-Type': 'application/json',
      'Access-Control-Allow-Origin': '*'
    }
  })
}
