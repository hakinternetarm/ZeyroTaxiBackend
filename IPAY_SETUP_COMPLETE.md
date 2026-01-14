# IPay Payment Integration - Setup Complete

## What Has Been Configured

The IPay (ArCa) payment system has been successfully integrated into your Zeyro Taxi API.

### Files Created

1. **Services/IPayPaymentService.cs** - Core service for IPay payment operations
2. **Controllers/IPayController.cs** - REST API endpoints for payment management
3. **Models/IPayPayment.cs** - Database model for tracking IPay payments
4. **DTOs/IPayDTOs.cs** - Data transfer objects for requests and responses
5. **IPAY_INTEGRATION.md** - Complete integration documentation

### Files Modified

1. **appsettings.json** - Added IPay configuration section
2. **Program.cs** - Registered IPayPaymentService and HttpClient in DI
3. **Data/AppDbContext.cs** - Added IPayPayments DbSet and entity configuration

### Database Migration

? **Migration Created**: `AddIPayPayments`
? **Migration Applied**: Database updated successfully

**IPayPayments Table Schema:**
- Id (Primary Key)
- UserId (Foreign Key to Users)
- OrderNumber (Unique)
- IPayOrderId (IPay's order identifier)
- Description
- Amount
- Currency
- Status
- Pan (masked card number)
- CardholderName
- ApprovalCode
- ActionCode
- ActionCodeDescription
- CreatedAt
- CompletedAt

## Configuration Required

### 1. Update appsettings.json

**IMPORTANT:** Replace these values with your actual IPay credentials:

```json
{
  "IPay": {
    "UserName": "YOUR_IPAY_USERNAME",
    "Password": "YOUR_IPAY_PASSWORD",
    "Currency": "051",
    "Language": "en",
    "ReturnUrl": "https://yourdomain.com/api/ipay/return",
    "ApiBaseUrl": "https://ipay.arca.am/payment/rest"
  }
}
```

Or use environment variables:

```bash
# PowerShell
$env:IPay__UserName = "YOUR_IPAY_USERNAME"
$env:IPay__Password = "YOUR_IPAY_PASSWORD"

# Linux/Mac
export IPay__UserName="YOUR_IPAY_USERNAME"
export IPay__Password="YOUR_IPAY_PASSWORD"
```

### 2. Currency Codes

Choose the appropriate currency:
- `051` = AMD (Armenian Dram) - Default
- `643` = RUB (Russian Ruble)
- `840` = USD (US Dollar)
- `978` = EUR (Euro)

### 3. Environment URLs

**For Testing:**
```
ApiBaseUrl: https://ipaytest.arca.am:8445/payment/rest
```

**For Production:**
```
ApiBaseUrl: https://ipay.arca.am/payment/rest
```

## API Endpoints Available

### Client Endpoints (Require Authentication)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/ipay/create-payment` | Create new payment and get redirect URL |
| GET | `/api/ipay/status/{orderId}` | Get payment status by IPay order ID |
| POST | `/api/ipay/reverse/{orderId}` | Cancel/reverse a payment |
| POST | `/api/ipay/refund` | Refund a completed payment |

### Callback Endpoint

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET/POST | `/api/ipay/return` | IPay redirect after payment (automatic) |

## Quick Start Testing

### 1. Start the API

```bash
dotnet run
```

### 2. Get Authentication Token

```bash
curl -X POST http://localhost:5000/api/auth/request-code \
  -H "Content-Type: application/json" \
  -d '{"phone": "+37412345678", "name": "Test User"}'
```

### 3. Create Payment

```bash
curl -X POST http://localhost:5000/api/ipay/create-payment \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "orderNumber": "TEST_001",
    "amount": 1000.00,
    "description": "Test taxi payment"
  }'
```

**Response:**
```json
{
  "orderId": "70d7dcc5-c0d8-409a-a811-50f5cc1988f2",
  "formUrl": "https://ipay.arca.am/payment/...",
  "status": "Created",
  "message": "Payment created successfully..."
}
```

### 4. Complete Payment

Redirect user's browser to the `formUrl` from step 3 to complete payment on IPay's secure page.

### 5. Check Status

```bash
curl -X GET http://localhost:5000/api/ipay/status/ORDER_ID \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Payment Flow

```
???????????????
?   Client    ?
?  Creates    ?
?   Order     ?
???????????????
       ?
       ?
???????????????????????????????????
? API: POST /api/ipay/create-     ?
?      payment                     ?
? - Registers order with IPay      ?
? - Stores in database             ?
? - Returns formUrl                ?
???????????????????????????????????
       ?
       ?
???????????????????????????????????
? Redirect to IPay Payment Page   ?
? - User enters card details       ?
? - 3D Secure authentication       ?
? - IPay processes payment         ?
???????????????????????????????????
       ?
       ?
???????????????????????????????????
? Redirect to /api/ipay/return    ?
? - Fetches status from IPay       ?
? - Updates database               ?
? - Shows result page              ?
???????????????????????????????????
```

## Payment States

| State | Description |
|-------|-------------|
| **Created** | Order registered, awaiting payment |
| **Approved** | Pre-authorized (two-stage only) |
| **Deposited** | ? Payment completed successfully |
| **Reversed** | Payment cancelled |
| **Refunded** | Payment refunded to customer |
| **Declined** | ? Payment declined |

## Features Implemented

? **One-stage payments** - Direct payment capture  
? **Payment status tracking** - Real-time status updates  
? **Refunds** - Full and partial refunds  
? **Reversals** - Cancel authorized payments  
? **3D Secure** - Automatic 3DS authentication  
? **Database persistence** - All transactions logged  
? **Multi-currency support** - AMD, RUB, USD, EUR  
? **Multi-language** - English, Russian, Armenian  

## Security Features

- ? JWT authentication required for all endpoints
- ? Credentials stored in configuration (not code)
- ? HTTPS for all IPay communications
- ? Card data never stored (only masked PAN)
- ? Automatic 3D Secure authentication
- ? Transaction logging and audit trail

## Integration with Orders

To integrate IPay with your taxi orders, see example in `IPAY_INTEGRATION.md` or follow this pattern:

```csharp
// In OrdersController or similar
var ipayRequest = new IPayPaymentRequest
{
    OrderNumber = $"ORDER_{order.Id}",
    Amount = order.EstimatedFare,
    Description = $"Taxi ride from {order.PickupAddress} to {order.DestinationAddress}"
};

var ipayResponse = await _ipayService.CreatePayment(ipayRequest);

// Redirect user to ipayResponse.FormUrl
```

## Testing Checklist

Before production deployment:

- [ ] Update configuration with production credentials
- [ ] Change `ApiBaseUrl` to production endpoint
- [ ] Update `ReturnUrl` to production HTTPS URL
- [ ] Test with real test cards from Armenian Card
- [ ] Verify 3D Secure authentication works
- [ ] Test refund and reversal operations
- [ ] Test with different currencies (if applicable)
- [ ] Verify callback URL is accessible from internet
- [ ] Test error handling scenarios
- [ ] Review and test timeout handling

## Documentation

Complete documentation available in:
- **IPAY_INTEGRATION.md** - Full integration guide
- **IPay Merchant Manual.pdf** - Official IPay documentation

## Support Contacts

### Armenian Card (ArCa)
- Website: https://www.arca.am
- Email: support@arca.am
- Phone: +374 10 59 22 22
- Admin Panel: https://ipay.arca.am/payment/admin

### Troubleshooting

**Common Issues:**

1. **"Payment not found"**
   - Check orderId is correct
   - Verify payment was created successfully

2. **"Invalid credentials"**
   - Verify username and password in config
   - Check environment (test vs production)

3. **"Return URL not called"**
   - Ensure URL is publicly accessible
   - Use ngrok for local testing
   - Check firewall settings

4. **"Status not updating"**
   - Call status endpoint to force refresh
   - Check IPay admin panel
   - Allow time for processing

## Next Steps

1. **Obtain Credentials**: Contact Armenian Card for test/production credentials
2. **Update Configuration**: Add your credentials to `appsettings.json`
3. **Test Integration**: Use test cards to verify flow
4. **Integrate with Orders**: Connect IPay to your order system
5. **Deploy to Production**: Update URLs and credentials for live environment

## Congratulations! ??

Your Zeyro Taxi API now supports both Idram and IPay payment systems, covering the major Armenian payment gateways!

### Summary of Payment Integrations

- ? **Stripe** - International cards (existing)
- ? **Idram** - Armenian payment gateway
- ? **IPay (ArCa)** - Armenian Card payment gateway

Your API is now ready to accept payments through multiple payment providers, giving your users maximum flexibility and convenience.
