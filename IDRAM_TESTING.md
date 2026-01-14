# Idram API Testing Examples

This file contains example API calls for testing the Idram payment integration.

## Prerequisites

1. Get an authentication token:

```bash
# Request verification code
curl -X POST http://localhost:5000/api/auth/request-code \
  -H "Content-Type: application/json" \
  -d '{"phone": "+37412345678", "name": "Test User"}'

# Response: { "authSessionId": "..." }

# Verify code and get token
curl -X POST http://localhost:5000/api/auth/auth \
  -H "Content-Type: application/json" \
  -d '{"authSessionId": "...", "code": "123456"}'

# Response: { "token": "eyJ..." }
```

2. Set your token:
```bash
export TOKEN="eyJ..."
```

## Test Cases

### 1. Create Idram Payment

```bash
curl -X POST http://localhost:5000/api/idram/create-payment \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "language": "EN",
    "description": "Test taxi ride payment",
    "amount": 1500.00,
    "billNo": "TEST_001",
    "email": "test@example.com"
  }'
```

**Expected Response:**
```json
{
  "paymentUrl": "https://banking.idram.am/Payment/GetPayment",
  "formFields": {
    "EDP_LANGUAGE": "EN",
    "EDP_REC_ACCOUNT": "100000114",
    "EDP_DESCRIPTION": "Test taxi ride payment",
    "EDP_AMOUNT": "1500.00",
    "EDP_BILL_NO": "TEST_001",
    "EDP_EMAIL": "test@example.com"
  }
}
```

### 2. Check Payment Status

```bash
curl -X GET http://localhost:5000/api/idram/status/TEST_001 \
  -H "Authorization: Bearer $TOKEN"
```

**Expected Response:**
```json
{
  "billNo": "TEST_001",
  "status": "Pending",
  "amount": 1500.00,
  "description": "Test taxi ride payment",
  "transactionId": null,
  "transactionDate": null,
  "createdAt": "2026-01-08T10:30:00Z",
  "completedAt": null
}
```

### 3. Simulate Idram Precheck (Server-to-Server)

This would normally be called by Idram, but you can test it:

```bash
curl -X POST http://localhost:5000/api/idram/result \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "EDP_PRECHECK=YES&EDP_BILL_NO=TEST_001&EDP_REC_ACCOUNT=100000114&EDP_AMOUNT=1500.00"
```

**Expected Response:**
```
OK
```

### 4. Simulate Idram Payment Confirmation (Server-to-Server)

Calculate checksum:
```
MD5(100000114:1500.00:your_secret_key:TEST_001:200000123:12345678901234:08/01/2026)
```

```bash
# You need to calculate the correct checksum based on your secret key
CHECKSUM="calculated_md5_hash_here"

curl -X POST http://localhost:5000/api/idram/result \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "EDP_BILL_NO=TEST_001&EDP_REC_ACCOUNT=100000114&EDP_PAYER_ACCOUNT=200000123&EDP_AMOUNT=1500.00&EDP_TRANS_ID=12345678901234&EDP_TRANS_DATE=08/01/2026&EDP_CHECKSUM=$CHECKSUM"
```

**Expected Response:**
```
OK
```

### 5. Check Updated Payment Status

```bash
curl -X GET http://localhost:5000/api/idram/status/TEST_001 \
  -H "Authorization: Bearer $TOKEN"
```

**Expected Response (after confirmation):**
```json
{
  "billNo": "TEST_001",
  "status": "Success",
  "amount": 1500.00,
  "description": "Test taxi ride payment",
  "transactionId": "12345678901234",
  "transactionDate": "08/01/2026",
  "createdAt": "2026-01-08T10:30:00Z",
  "completedAt": "2026-01-08T10:35:00Z"
}
```

## HTML Form Test

Create a test HTML file to simulate the payment form submission:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Idram Payment Test</title>
</head>
<body>
    <h1>Idram Payment Test</h1>
    <form action="https://banking.idram.am/Payment/GetPayment" method="POST">
        <input type="hidden" name="EDP_LANGUAGE" value="EN">
        <input type="hidden" name="EDP_REC_ACCOUNT" value="100000114">
        <input type="hidden" name="EDP_DESCRIPTION" value="Test taxi ride payment">
        <input type="hidden" name="EDP_AMOUNT" value="1500.00">
        <input type="hidden" name="EDP_BILL_NO" value="TEST_001">
        <input type="hidden" name="EDP_EMAIL" value="test@example.com">
        <button type="submit">Pay with Idram</button>
    </form>
</body>
</html>
```

## PowerShell Testing Script

```powershell
# Set variables
$BaseUrl = "http://localhost:5000"
$Token = "your_token_here"

# Create payment
$createPaymentBody = @{
    language = "EN"
    description = "PowerShell test payment"
    amount = 2000.00
    billNo = "PS_TEST_001"
    email = "test@example.com"
} | ConvertTo-Json

$headers = @{
    "Authorization" = "Bearer $Token"
    "Content-Type" = "application/json"
}

$createResponse = Invoke-RestMethod -Uri "$BaseUrl/api/idram/create-payment" `
    -Method Post `
    -Headers $headers `
    -Body $createPaymentBody

Write-Host "Payment created:"
$createResponse | ConvertTo-Json

# Check status
$statusResponse = Invoke-RestMethod -Uri "$BaseUrl/api/idram/status/PS_TEST_001" `
    -Method Get `
    -Headers $headers

Write-Host "Payment status:"
$statusResponse | ConvertTo-Json

# Create HTML form file
$formHtml = @"
<!DOCTYPE html>
<html>
<head><title>Idram Payment</title></head>
<body>
    <form id="paymentForm" action="$($createResponse.paymentUrl)" method="POST">
"@

foreach ($field in $createResponse.formFields.GetEnumerator()) {
    $formHtml += "`n        <input type='hidden' name='$($field.Key)' value='$($field.Value)'>"
}

$formHtml += @"

        <button type="submit">Pay with Idram</button>
    </form>
    <script>
        // Auto-submit form after 2 seconds
        setTimeout(function() {
            document.getElementById('paymentForm').submit();
        }, 2000);
    </script>
</body>
</html>
"@

$formHtml | Out-File -FilePath "idram_payment_form.html" -Encoding UTF8
Write-Host "Payment form saved to: idram_payment_form.html"
Write-Host "Open this file in a browser to complete the payment"
```

## Postman Collection

Import this collection into Postman:

```json
{
  "info": {
    "name": "Idram Payment API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "variable": [
    {
      "key": "baseUrl",
      "value": "http://localhost:5000"
    },
    {
      "key": "token",
      "value": ""
    }
  ],
  "item": [
    {
      "name": "Create Payment",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"language\": \"EN\",\n  \"description\": \"Postman test payment\",\n  \"amount\": 1800.00,\n  \"billNo\": \"POSTMAN_001\",\n  \"email\": \"test@example.com\"\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": {
          "raw": "{{baseUrl}}/api/idram/create-payment",
          "host": ["{{baseUrl}}"],
          "path": ["api", "idram", "create-payment"]
        }
      }
    },
    {
      "name": "Get Payment Status",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "url": {
          "raw": "{{baseUrl}}/api/idram/status/POSTMAN_001",
          "host": ["{{baseUrl}}"],
          "path": ["api", "idram", "status", "POSTMAN_001"]
        }
      }
    }
  ]
}
```

## Troubleshooting Commands

### Check if Idram service is registered:
```bash
curl http://localhost:5000/swagger/v1/swagger.json | grep -i idram
```

### View recent payments:
```bash
sqlite3 taxi.db "SELECT * FROM IdramPayments ORDER BY CreatedAt DESC LIMIT 10;"
```

### Clear test payments:
```bash
sqlite3 taxi.db "DELETE FROM IdramPayments WHERE BillNo LIKE 'TEST_%';"
```

### Check application logs:
```bash
# In PowerShell
Get-Content -Path "logs/application.log" -Tail 50 -Wait

# In Linux/Mac
tail -f logs/application.log
```

## Production Testing

Before going to production:

1. Test with ngrok:
```bash
ngrok http 5000
```

2. Update Idram merchant URLs to ngrok URL

3. Test complete flow:
   - Create payment
   - Complete payment in Idram wallet
   - Verify precheck is called
   - Verify payment confirmation is received
   - Verify checksum validation
   - Verify payment status updates
   - Verify success redirect works

4. Test edge cases:
   - Invalid checksum
   - Duplicate payment confirmation
   - Payment with non-existent bill number
   - Payment amount mismatch
