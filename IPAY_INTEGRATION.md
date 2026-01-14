# IPay (ArCa) Payment Integration

This document describes the IPay payment system integration in the Zeyro Taxi API.

## Overview

IPay is the payment gateway operated by Armenian Card (ArCa) that supports:
- One-stage and two-stage payments
- 3D Secure authentication
- Refunds and reversals
- Multiple currencies (AMD, RUB, USD, EUR)
- Card bindings for recurring payments

## Configuration

### 1. Update appsettings.json

Configure the following IPay settings in your `appsettings.json`:

```json
{
  "IPay": {
    "UserName": "your_username_here",
    "Password": "your_password_here",
    "Currency": "051",
    "Language": "en",
    "ReturnUrl": "http://localhost:5000/api/ipay/return",
    "ApiBaseUrl": "https://ipay.arca.am/payment/rest"
  }
}
```

**Configuration Parameters:**

- `UserName`: Your IPay merchant username (obtained from ArCa)
- `Password`: Your IPay merchant password
- `Currency`: ISO 4217 currency code
  - `051` = AMD (Armenian Dram)
  - `643` = RUB (Russian Ruble)
  - `840` = USD (US Dollar)
  - `978` = EUR (Euro)
- `Language`: Interface language (`en`, `ru`, `hy`)
- `ReturnUrl`: URL where users are redirected after payment
- `ApiBaseUrl`: IPay REST API base URL
  - Test: `https://ipaytest.arca.am:8445/payment/rest`
  - Production: `https://ipay.arca.am/payment/rest`

### 2. Environment Variables (Optional)

You can also configure via environment variables:

```bash
# PowerShell
$env:IPay__UserName = "your_username"
$env:IPay__Password = "your_password"
$env:IPay__Currency = "051"

# Linux/Mac
export IPay__UserName="your_username"
export IPay__Password="your_password"
export IPay__Currency="051"
```

### 3. Obtain Credentials

Contact Armenian Card to obtain:
- Test environment credentials for development
- Production credentials for live payments
- Access to the merchant admin panel

## Database Migration

The migration has been created and applied. The `IPayPayments` table includes:

- Order tracking
- Payment status
- Card information (masked PAN)
- Approval codes
- Action codes and descriptions

## API Endpoints

### 1. Create Payment

Create a new IPay payment.

**Endpoint:** `POST /api/ipay/create-payment`

**Headers:**
- `Authorization: Bearer {token}` (required)
- `Content-Type: application/json`

**Request Body:**
```json
{
  "orderNumber": "ORDER_12345",
  "amount": 5000.00,
  "description": "Taxi service payment",
  "language": "en"
}
```

**Response:**
```json
{
  "orderId": "70d7dcc5-c0d8-409a-a811-50f5cc1988f2",
  "formUrl": "https://ipay.arca.am/payment/merchants/xxx/payment_en.html?mdOrder=...",
  "status": "Created",
  "message": "Payment created successfully. Redirect user to FormUrl to complete payment."
}
```

**Usage:**
After receiving the response, redirect the user's browser to the `formUrl` to complete the payment.

### 2. Get Payment Status

Get the current status of a payment.

**Endpoint:** `GET /api/ipay/status/{orderId}`

**Headers:**
- `Authorization: Bearer {token}` (required)

**Response:**
```json
{
  "orderNumber": "ORDER_12345",
  "ipayOrderId": "70d7dcc5-c0d8-409a-a811-50f5cc1988f2",
  "status": "Deposited",
  "amount": 5000.00,
  "currency": "051",
  "description": "Taxi service payment",
  "pan": "411111******1111",
  "cardholderName": "John Doe",
  "approvalCode": "123456",
  "actionCode": 0,
  "actionCodeDescription": "Success",
  "createdAt": "2026-01-14T10:30:00Z",
  "completedAt": "2026-01-14T10:35:00Z"
}
```

### 3. Reverse (Cancel) Payment

Cancel a payment (available for a limited time after authorization).

**Endpoint:** `POST /api/ipay/reverse/{orderId}`

**Headers:**
- `Authorization: Bearer {token}` (required)

**Response:**
```json
{
  "message": "Payment reversed successfully"
}
```

### 4. Refund Payment

Refund a completed payment (full or partial).

**Endpoint:** `POST /api/ipay/refund`

**Headers:**
- `Authorization: Bearer {token}` (required)
- `Content-Type: application/json`

**Request Body:**
```json
{
  "orderId": "70d7dcc5-c0d8-409a-a811-50f5cc1988f2",
  "amount": 2500.00
}
```

**Response:**
```json
{
  "message": "Payment refunded successfully"
}
```

### 5. Return Callback (Internal)

This endpoint is called by IPay after payment completion.

**Endpoint:** `GET/POST /api/ipay/return?orderId={orderId}`

Users are automatically redirected to this URL after payment.

## Payment Flow

```
1. Client requests taxi
   ??> App creates IPay payment via API

2. API registers order with IPay
   ??> POST /api/ipay/create-payment
   ??> Returns orderId and formUrl

3. User is redirected to IPay payment page
   ??> User enters card details
   ??> 3D Secure authentication (if enabled)

4. IPay processes payment
   ??> Authorization
   ??> Capture (for one-stage payments)

5. User is redirected back to app
   ??> GET /api/ipay/return?orderId={orderId}
   ??> API fetches latest status from IPay
   ??> Updates database
   ??> Shows result to user

6. App confirms payment status
   ??> GET /api/ipay/status/{orderId}
   ??> Updates order status
```

## Payment States

IPay payments can be in the following states:

| State | Code | Description |
|-------|------|-------------|
| Created | 0 | Order registered but not paid |
| Approved | 1 | Pre-authorization successful (two-stage) |
| Deposited | 2 | Payment completed successfully |
| Reversed | 3 | Payment cancelled |
| Refunded | 4 | Payment refunded |
| ACS_Auth | 5 | 3D Secure authentication initiated |
| Declined | 6 | Payment declined |

## Security

### Best Practices

1. **Never expose credentials** in client-side code
2. **Use HTTPS** for all API calls in production
3. **Store credentials** in environment variables or secure configuration
4. **Validate amounts** before creating payments
5. **Check payment status** after return callback
6. **Implement idempotency** to prevent duplicate payments

### 3D Secure

- All payments automatically use 3D Secure when available
- Enhanced security for cardholder authentication
- Reduced fraud risk
- May fall back to SSL if 3DS not available

## Testing

### Test Environment

For testing, use the test API endpoint:

```
https://ipaytest.arca.am:8445/payment/rest
```

### Test Cards

Contact Armenian Card for test card numbers and credentials.

### Example Test Flow

```bash
# 1. Get authentication token
curl -X POST http://localhost:5000/api/auth/request-code \
  -H "Content-Type: application/json" \
  -d '{"phone": "+37412345678", "name": "Test User"}'

# 2. Create payment
curl -X POST http://localhost:5000/api/ipay/create-payment \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "orderNumber": "TEST_001",
    "amount": 1000.00,
    "description": "Test payment"
  }'

# 3. Open formUrl in browser to complete payment

# 4. Check status
curl -X GET http://localhost:5000/api/ipay/status/ORDER_ID \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Error Codes

Common IPay error codes:

| Code | Description |
|------|-------------|
| 0 | Success |
| 1 | Order already registered |
| 3 | Unknown currency |
| 4 | Missing required parameter |
| 5 | Invalid parameter value |
| 6 | Order not found |
| 7 | System error |

## Action Codes

Action codes indicate the transaction result:

| Code | Description |
|------|-------------|
| 0 | Approved |
| 100 | Declined |
| 101 | Expired card |
| 109 | Invalid merchant |
| 110 | Invalid amount |
| 116 | Insufficient funds |
| 120 | Transaction not permitted |

See the IPay documentation for a complete list of action codes.

## Advanced Features

### Two-Stage Payments

For two-stage (pre-authorization + capture) payments:

1. Register with `registerPreAuth.do` endpoint
2. Use `deposit.do` to complete the payment
3. Use `reverse.do` to cancel before completion

### Card Bindings

For recurring payments, IPay supports card bindings:

1. Pass `clientId` when creating payment
2. After successful payment, a binding is created
3. Use binding ID for future payments without card input

### Recurring Payments

For subscription-based services:

1. Create initial payment with `recurringInitialize`
2. Store `recurringId`
3. Create subsequent payments referencing `recurringId`

## Troubleshooting

### Payment Not Found

- Verify `orderId` is correct
- Check database for payment record
- Ensure payment was created successfully

### Return URL Not Called

- Verify `ReturnUrl` is accessible from internet
- Check firewall settings
- Use ngrok for local testing

### Status Not Updating

- Call `GET /api/ipay/status/{orderId}` to force refresh
- IPay updates may have slight delay
- Check IPay admin panel for transaction status

### Invalid Credentials

- Verify username and password in configuration
- Ensure using correct environment (test vs production)
- Contact Armenian Card for credential issues

## Production Checklist

Before going to production:

- [ ] Obtain production credentials from Armenian Card
- [ ] Update `ApiBaseUrl` to production endpoint
- [ ] Configure production `ReturnUrl` with HTTPS
- [ ] Test with real cards and small amounts
- [ ] Verify 3D Secure flow works correctly
- [ ] Test refund and reversal operations
- [ ] Set up monitoring and logging
- [ ] Review security best practices
- [ ] Document merchant agreement details

## Support

### IPay Support
- Website: https://www.arca.am
- Email: support@arca.am
- Phone: +374 10 59 22 22

### API Documentation
- Official IPay Merchant Manual (PDF)
- Admin Panel: https://ipay.arca.am/payment/admin

### Internal Support
- Check application logs for detailed error messages
- Review `IPayController.cs` for implementation details
- Consult `IPayPaymentService.cs` for API integration
