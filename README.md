# Currency Converter API

A REST API built with ASP.NET Core (.NET) that converts currencies using real exchange rates from [ExchangeRate-API](https://www.exchangerate-api.com).

---

## Getting an API Key

1. Go to [https://www.exchangerate-api.com](https://www.exchangerate-api.com)
2. Sign up for a free account — no credit card required
3. Copy your API key from the dashboard

---

## Configuration

Open `appsettings.json` and replace the placeholder with your key:

```json
{
  "ExchangeRateApiKey": "YOUR_API_KEY_HERE"
}
```

---

## Running the Project

```bash
dotnet run
```

The API will be available at `https://localhost:{port}` (the exact port is shown in the terminal output).

---

## Endpoints

### 1. Get Exchange Rates for a Base Currency

**GET** `/api/currency/rates/{baseCurrency}`

Returns all available exchange rates relative to the given base currency.

**Example request:**
```
GET /api/currency/rates/USD
```

**Example response:**
```json
{
  "baseCurrency": "USD",
  "lastUpdated": "Fri, 09 Apr 2026 00:00:01 +0000",
  "rates": {
    "USD": 1,
    "EUR": 0.9201,
    "ARS": 1063.5,
    "GBP": 0.7712
  }
}
```

**Error responses:**
- `400 Bad Request` — invalid currency code (not 3 letters, or unsupported)
- `502 Bad Gateway` — exchange rate provider is unreachable

---

### 2. Convert an Amount Between Currencies

**GET** `/api/currency/convert?from={from}&to={to}&amount={amount}`

| Query param | Type    | Description                    |
|-------------|---------|--------------------------------|
| `from`      | string  | Source currency code (e.g. USD)|
| `to`        | string  | Target currency code (e.g. ARS)|
| `amount`    | decimal | Amount to convert              |

**Example request:**
```
GET /api/currency/convert?from=USD&to=ARS&amount=100
```

**Example response:**
```json
{
  "from": "USD",
  "to": "ARS",
  "amount": 100,
  "convertedAmount": 106350.0000,
  "exchangeRate": 1063.5,
  "timestamp": "Fri, 09 Apr 2026 00:00:01 +0000"
}
```

**Error responses:**
- `400 Bad Request` — invalid or unsupported currency code, or amount ≤ 0
- `502 Bad Gateway` — exchange rate provider is unreachable

---

### 3. List Supported Currencies

**GET** `/api/currency/supported`

Returns all currency codes and their full names supported by the exchange rate provider.

**Example request:**
```
GET /api/currency/supported
```

**Example response:**
```json
{
  "count": 161,
  "currencies": [
    { "code": "USD", "name": "US Dollar" },
    { "code": "EUR", "name": "Euro" },
    { "code": "ARS", "name": "Argentine Peso" }
  ]
}
```

**Error responses:**
- `502 Bad Gateway` — exchange rate provider is unreachable

---

## Caching

Exchange rates are cached in memory for **1 hour** to avoid hitting the external API on every request. The supported currencies list is also cached for 1 hour.
