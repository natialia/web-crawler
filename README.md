# Individual Web Crawler

Ein ASP.NET Core WebAPI-Projekt zur gezielten Durchsuchung von Webseiten und Analyse von PDF-Dokumenten inkl. Wortstatistiken und Wordclouds.

## Features

- Benutzerregistrierung, Login & Passwort-Reset per E-Mail-Link (60 Sekunden gültig)
- Webcrawler mit auswählbarer Tiefe (1–3)
- Download & Speicherung von PDF-Dateien mit Metadaten (Datum, Quelle)
- Analyse von PDF-Inhalten: Extraktion der 10 häufigsten Wörter
- Suche nach häufigen Wörtern
- Wordcloud-Generierung für:
  - Einzelne oder mehrere PDFs
  - PDFs in einem angegebenen Zeitintervall

## Technologie-Stack

- **Backend:** C# mit ASP.NET Core 8 (WebAPI)
- **Datenbank:** PostgreSQL (Docker, EF Core Migrations)
- **PDF-Verarbeitung:** PdfPig
- **Crawling:** HtmlAgilityPack
- **Wordcloud-Visualisierung:** [wordcloud2.js](https://github.com/timdream/wordcloud2.js)
- **Frontend:** HTML & JavaScript (minimal)
- **Entwicklung:** Visual Studio 2022+ oder GitHub Codespaces

## Projekt starten (lokal)

### 1. Repository klonen

    ```bash
    git clone https://github.com/natialia/web-crawler.git
    cd web-crawler

## 2. Docker starten

Falls noch nicht eingerichtet, PostgreSQL via Docker starten:

    ```bash
    docker compose up --build

## 3. Anwendung starten

  Die UI ist über http://localhost:8080/index.html erreichbar.

## 4. Swagger

Die Ednpoints sind über http://localhost:8080/swagger erreichbar.


## Beispiel-API-Endpunkte

POST /api/Auth/register – Registrierung

POST /api/Auth/login – Login

POST /api/Auth/forgot-password – Passwort zurücksetzen

POST /api/Crawler/start – Crawling starten

GET /api/User/profile/history – Crawling-Historie ansehen

POST /api/User/profile/wordcloud – Wordcloud aus ausgewählten PDFs

POST /api/User/profile/wordcloud/time – Wordcloud aus Zeitintervall

## Hinweise

- Die Anwendung speichert persönliche Crawling-Daten und PDF-Wortstatistiken nutzerspezifisch.
- Alle verwendeten Bibliotheken sind Open Source.
- Keine SQLite-Unterstützung – ausschließlich PostgreSQL via Docker.
