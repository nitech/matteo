# Matteo - Matematikk Treningsapp

## Kontekst og Hensikt
Matteo er en interaktiv matematikktreningsapp designet for å gjøre læring av multiplikasjon morsomt og engasjerende. Appen er spesielt utviklet for å fungere som en "låseskjerm" - en type applikasjon som ikke kan avsluttes før brukeren har fullført en spesifikk oppgave. Dette er implementert ved å bruke Windows' systemfunksjoner og Avalonia UI framework.

## Implementasjonsdetaljer

### 1. Matematikkoppgaver
- Genererer tilfeldige multiplikasjonsproblemer mellom tall 1-12
- Bruker en Dictionary<string, int> for å lagre problemstatistikk
- Lagrer statistikk i en JSON-fil (problem_stats.json)
- Implementerer umiddelbar tilbakemelding ved hjelp av MessageBoxManager
- Bruker Random-klassen for å generere tilfeldige tall

### 2. Sikkerhetsfunksjoner
- Fullskjermmodus implementert med WindowState.FullScreen
- Vindusdekorasjoner fjernet med SystemDecorations.None
- Lukking forhindret med Window.Closing event
- Systemtaster blokkert ved å override OnKeyDown
- Systemprosesser overvåket med Process.GetProcesses()
- Sikkerhetstimer kjører hver 500ms for å opprettholde sikkerhetstiltak

### 3. Visuelle Effekter
- Bakgrunnsanimasjoner implementert med Canvas og TextBlock
- Symboler animert med DispatcherTimer (50ms intervall)
- Farger generert med Color.FromArgb og semi-transparente verdier
- Animasjoner håndtert med Canvas.SetLeft og Canvas.SetTop
- Tilfeldige bevegelser implementert med Random.Next

### 4. Brukergrensesnitt
- Hovedvindu implementert med Avalonia Window
- Inputfelt for svar med KeyDown event for Enter-tast
- Fremgangsindikator viser problemsSolved/10
- Motiverende meldinger vises med MessageBoxManager
- Animasjoner trigges ved CheckAnswer()

## Tekniske Detaljer
- Bygget med .NET 8.0 og Avalonia UI
- Bruker Windows-specifikke funksjoner for sikkerhet
- Implementerer IWindowBaseImpl for vindushåndtering
- Bruker DispatcherTimer for animasjoner og sikkerhetssjekker
- Lagrer data lokalt med JSON-serialisering

## Sikkerhetsimplementasjon
- Fullskjermmodus opprettholdes med WindowState
- Systemprosesser overvåkes og avsluttes ved behov
- Systemtaster blokkeres ved å håndtere KeyDown events
- Vinduet holdes øverst med Topmost = true
- Taskbar-visning deaktivert med ShowInTaskbar = false

## Bruksflyt
1. Appen starter i fullskjermmodus
2. Brukeren får presentert multiplikasjonsproblemer
3. Hvert problem må besvares riktig
4. Statistikk oppdateres og lagres
5. Appen kan ikke avsluttes før 10 problemer er løst
6. Ved fullføring deaktiveres sikkerhetstiltak

## Visuelle Implementasjoner
- Bakgrunnselementer: TextBlock med matematiske symboler
- Fargepalett: Pastellfarger med alpha-verdier 30-70
- Animasjoner: Kombinasjon av horisontal og vertikal bevegelse
- Responsivitet: Bounds.Width og Bounds.Height for posisjonering

## Ytelsesoptimaliseringer
- Sikkerhetssjekker kjøres hver 500ms
- Prosesssjekker kjøres hver 2. sekund
- Bakgrunnsanimasjoner har try-catch for feilhåndtering
- Elementer fjernes når de går utenfor skjermen
- Tilfeldige tall genereres med én Random-instans 