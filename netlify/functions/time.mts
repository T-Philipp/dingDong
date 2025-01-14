export default async () => {
  const utcDate = new Date()
  const deDate = utcDate.toLocaleString('de-DE', { timeZone: 'Europe/Berlin' })
  return new Response(deDate)
}
