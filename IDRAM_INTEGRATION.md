# Idram Payment Integration

This document describes how to configure and use the Idram payment system integration in the Zeyro Taxi API.

## Configuration

### 1. Update appsettings.json

Configure the following Idram settings in your `appsettings.json` or environment variables:

```json
"Idram": {
  "ReceiverAccount": "100000114",
  "SecretKey": "your_secret_key_here",
  "Email": "merchant@example.com",
  "SuccessUrl": "http://localhost:5000/api/idram/success",
  "FailUrl": "http://localhost:5000/api/idram/fail",
  "ResultUrl": "http://localhost:5000/api/idram/result",
  "PaymentUrl": "https://banking.idram.am/Payment/GetPayment"
}
```

**Important:** Replace the following values with your actual Idram merchant credentials:
- `ReceiverAccount`: Your Idram merchant ID
- `SecretKey`: Your secret key provided by Idram
- `Email`: Your merchant email for payment notifications
- `SuccessUrl`, `FailUrl`, `ResultUrl`: Update these URLs to match your production domain

### 2. Environment Variables (Optional)

You can also configure these via environment variables:

```bash
# PowerShell
$env:Idram__ReceiverAccount = "100000114"
$env:Idram__SecretKey = "your_secret_key_here"
$env:Idram__Email = "merchant@example.com"

# Linux/Mac
export Idram__ReceiverAccount="100000114"
export Idram__SecretKey="your_secret_key_here"
export Idram__Email="merchant@example.com"
```

### 3. Configure Idram Merchant Portal

Contact Idram technical support to configure the following URLs in your merchant account:
- **SUCCESS_URL**: `https://yourdomain.com/api/idram/success`
- **FAIL_URL**: `https://yourdomain.com/api/idram/fail`
- **RESULT_URL**: `https://yourdomain.com/api/idram/result`

## Database Migration

After configuration, create and apply a migration for the IdramPayment model:

```bash
dotnet ef migrations add AddIdramPayments
dotnet ef database update
```

## API Endpoints

### 1. Create Payment
Create a new Idram payment and get form data to submit to Idram.

**Endpoint:** `POST /api/idram/create-payment`

**Headers:** 
- `Authorization: Bearer {token}` (required)

**Request Body:**
```json
{
  "language": "EN",
  "description": "Taxi service payment",
  "amount": 5000.00,
  "billNo": "ORDER_12345",
  "email": "customer@example.com"
}
```

**Response:**
```json
{
  "paymentUrl": "https://banking.idram.am/Payment/GetPayment",
  "formFields": {
    "EDP_LANGUAGE": "EN",
    "EDP_REC_ACCOUNT": "100000114",
    "EDP_DESCRIPTION": "Taxi service payment",
    "EDP_AMOUNT": "5000.00",
    "EDP_BILL_NO": "ORDER_12345",
    "EDP_EMAIL": "customer@example.com"
  }
}
```

### 2. Check Payment Status
Get the status of a payment by bill number.

**Endpoint:** `GET /api/idram/status/{billNo}`

**Headers:** 
- `Authorization: Bearer {token}` (required)

**Response:**
```json
{
  "billNo": "ORDER_12345",
  "status": "Success",
  "amount": 5000.00,
  "description": "Taxi service payment",
  "transactionId": "14DIGIT_TRANS_ID",
  "transactionDate": "08/01/2026",
  "createdAt": "2026-01-08T10:30:00Z",
  "completedAt": "2026-01-08T10:35:00Z"
}
```

### 3. Payment Callback (Internal)
This endpoint is called by Idram system for payment confirmation. **Do not call this endpoint directly.**

**Endpoint:** `POST /api/idram/result`

This endpoint handles:
- Preliminary order validation (precheck)
- Payment confirmation after successful transaction

## Client Integration

### Web Integration

After receiving the payment form data from `/api/idram/create-payment`, create an HTML form and submit it:

```html
<form id="idramPaymentForm" action="https://banking.idram.am/Payment/GetPayment" method="POST">
  <input type="hidden" name="EDP_LANGUAGE" value="EN">
  <input type="hidden" name="EDP_REC_ACCOUNT" value="100000114">
  <input type="hidden" name="EDP_DESCRIPTION" value="Taxi service payment">
  <input type="hidden" name="EDP_AMOUNT" value="5000.00">
  <input type="hidden" name="EDP_BILL_NO" value="ORDER_12345">
  <input type="hidden" name="EDP_EMAIL" value="customer@example.com">
</form>

<script>
  document.getElementById('idramPaymentForm').submit();
</script>
```

### Mobile Integration (iOS)

For iOS apps, implement `WKNavigationDelegate` to handle deep links:

```swift
func webView(_ webView: WKWebView, decidePolicyFor navigationAction: WKNavigationAction, 
             decisionHandler: @escaping (WKNavigationActionPolicy) -> Void) {
    if navigationAction.navigationType == WKNavigationType.linkActivated {
        if let url = navigationAction.request.url, url.absoluteString.contains("idramapp://") {
            if UIApplication.shared.canOpenURL(url) {
                UIApplication.shared.open(url)
            }
            decisionHandler(WKNavigationActionPolicy.cancel)
            return
        }
    }
    decisionHandler(WKNavigationActionPolicy.allow)
}
```

Add to `Info.plist`:
```xml
<key>LSApplicationQueriesSchemes</key>
<array>
    <string>idramapp</string>
</array>
```

### Mobile Integration (Android)

See Idram Android SDK: https://github.com/karapetyangevorg/IdramMerchantPayment-Android

## Payment Flow

1. **Client creates payment:**
   - Client calls `POST /api/idram/create-payment`
   - Server creates payment record in database with "Pending" status
   - Server returns Idram form data

2. **Client submits to Idram:**
   - Client submits form to `https://banking.idram.am/Payment/GetPayment`
   - User authenticates and completes payment in Idram wallet

3. **Idram validates order:**
   - Idram calls `POST /api/idram/result` with `EDP_PRECHECK=YES`
   - Server validates order exists and amount matches
   - Server responds with "OK" or "FAIL"

4. **Payment processing:**
   - If validation succeeds, Idram processes payment
   - Idram calls `POST /api/idram/result` with payment confirmation
   - Server validates checksum and updates payment status to "Success"

5. **User redirect:**
   - On success: User is redirected to `/api/idram/success`
   - On failure: User is redirected to `/api/idram/fail`

## Security

### Checksum Validation

All payment confirmations from Idram include a checksum calculated as:

```
MD5(EDP_REC_ACCOUNT:EDP_AMOUNT:SECRET_KEY:EDP_BILL_NO:EDP_PAYER_ACCOUNT:EDP_TRANS_ID:EDP_TRANS_DATE)
```

The server automatically validates this checksum to ensure payment authenticity.

### Important Security Notes

- **Never expose your SECRET_KEY** in client-side code
- Always validate the checksum before processing payments
- Use HTTPS for all callback URLs in production
- Store SECRET_KEY in secure environment variables, not in source code

## Testing

For testing in development:

1. Use ngrok or similar to expose your local server:
   ```bash
   ngrok http 5000
   ```

2. Update your Idram merchant URLs to use the ngrok URL:
   - `https://your-ngrok-url.ngrok.io/api/idram/result`
   - `https://your-ngrok-url.ngrok.io/api/idram/success`
   - `https://your-ngrok-url.ngrok.io/api/idram/fail`

3. Test the payment flow with a small amount

## Troubleshooting

### "OK" not received during precheck
- Verify `RESULT_URL` is correctly configured in Idram merchant portal
- Check server logs for validation errors
- Ensure payment exists in database with correct amount

### Checksum validation failed
- Verify `SECRET_KEY` matches the key provided by Idram
- Check that all parameters are in the correct order
- Ensure no extra whitespace in configuration values

### Payment stuck in "Pending" status
- Check if Idram callback reached your server (check logs)
- Verify `RESULT_URL` is accessible from Idram servers
- Ensure server responded with "OK" to payment confirmation

## Support

For issues specific to Idram integration:
- Contact Idram technical support
- Email: support@idram.am
- Documentation: https://idram.am/en/integration

For issues with this API implementation:
- Check server logs in `/app/logs` or console output
- Review the Idram controller at `Controllers/IdramController.cs`
