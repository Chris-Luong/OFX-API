# Overview

This document outlines the requirements for implementing a Currency Exchange API using C# and .NET 8 or later. The API enables users to create currency exchange quotes, retrieve quotes, and initiate transfers based on those quotes.
The technical implementation should be doable in under 2 hours.

## Technical requirements

### Framework and Language

- Platform: .NET 8 or later
- Language: C#
- Testing: xUnit and Moq libraries were used
- API Type: REST

## Currency Support

Supported Currency Pairs:

- Sell Currencies (ISO Codes): AUD, USD, EUR
- Buy Currencies (ISO Codes): USD, INR, PHP
  Note:
- The same currency cannot be used for both sell and buy operations.
- A pair comprises of a Sell Currency and a Buy Currency. e.g. Sell Currency AUD and Buy Currency USD

## API Endpoints

### 1. Create Quote

Creates a new currency exchange quote based on the provided currencies and amount.

**Endpoint**: POST /transfers/quote

**Request Body:**

```json
{
    "sellCurrency": "AUD",
    "buyCurrency": "USD",
    "amount": 1234.50
}
```

**Required Fields:**

- sellCurrency: ISO currency code for the currency to sell
- buyCurrency: ISO currency code for the currency to buy
- amount: Decimal value representing the amount to exchange

**Response:** 201 Created

```json
{
    "quoteId": "2966024b-a7a7-408c-b17a-6de51f75e83a",
    "ofxRate": 0.768333,
    "inverseOfxRate": 1.30151,
    "convertedAmount": 948.50
}
```

**Validation Rules:**

1. Both currencies must be from the supported currency lists
2. Sell and buy currencies cannot be identical
3. Amount must be greater than zero
4. All fields are required

### 2. Retrieve Quote

Retrieves a previously created quote using its identifier.

**Endpoint:** GET /transfers/quote/{quoteId}

**Response:** 200 OK

```json
{
    "quoteId": "2966024b-a7a7-408c-b17a-6de51f75e83a",
    "ofxRate": 0.768333,
    "inverseOfxRate": 1.30151,
    "convertedAmount": 948.50
}
```

**Error Handling:**

- Return appropriate HTTP status code if quote is not found

### 3. Create Transfer

Creates a new transfer using a previously generated quote.

**Endpoint:** POST /transfers

**Request Body:**

```json
{
    "quoteId": "2966024b-a7a7-408c-b17a-6de51f75e83a",
    "payer": {
        "id": "c96e4a58-cbf0-4ffb-8ec7-a3adbe4653e6",
        "name": "John Doe",
        "transferReason": "Invoice"
    },
    "recipient": {
        "name": "Clint Wood",
        "accountNumber": "90823482132",
        "bankCode": "21398",
        "bankName": "Bank Of America"
    }
}
```

**Required Fields:**

- quoteId: Valid UUID of an existing quote
- payer.id: Valid UUID
- payer.name: Non-empty string
- payer.transferReason: Non-empty string
- recipient.name: Non-empty string
- recipient.accountNumber: Non-empty string
- recipient.bankCode: Non-empty string
- recipient.bankName: Non-empty string

**Response:** 201 Created

```json
{
    "transferId": "47b56d56-d528-4bd7-b0c7-f6ff4ee487b1",
    "status": "Processing",
    "transferDetails": {
        "quoteId": "2966024b-a7a7-408c-b17a-6de51f75e83a",
        "payer": {
            "id": "c96e4a58-cbf0-4ffb-8ec7-a3adbe4653e6",
            "name": "John Doe",
            "transferReason": "Invoice"
        },
        "recipient": {
            "name": "Clint Wood",
            "accountNumber": "90823482132",
            "bankCode": "21398",
            "bankName": "Bank Of America"
        }
    },
    "estimatedDeliveryDate": "2024-12-31T00:00:00.000Z"
}
```

### 4. Retrieve Transfer

Retrieves a previously created transfer using its identifier.

**Endpoint:** GET /transfers/{transferId}

**Response:** 200 OK

- Same structure as Create Transfer response

## Implementation Notes

### Data Storage

- Quotes and transfers should be stored in memory
- No persistent storage implementation is required
- Exchange rates should be retrieved from the public site and cached in memory

### Transfer Status

Valid transfer status values:

- Created
- Processing
- Processed
- Failed

### Error Handling

The API should return appropriate HTTP status codes and error messages for:

- Invalid currency pairs
- Invalid or missing required fields
- Non-existent quotes or transfers
- Any other validation failures

### Date Handling

- All dates should be in ISO 8601 format with UTC timezone
- Estimated delivery date should be set to the current date + 1 day in future.
