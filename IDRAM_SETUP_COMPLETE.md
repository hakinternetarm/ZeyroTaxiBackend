# Idram Payment Integration - Summary

## What Has Been Configured

The Idram payment system has been successfully integrated into your Zeyro Taxi API. Here's what was added:

### 1. Files Created

- **Services/IdramPaymentService.cs** - Core service for Idram payment handling including checksum validation
- **Controllers/IdramController.cs** - REST API endpoints for payment creation, callbacks, and status checking
- **Models/IdramPayment.cs** - Database model for tracking Idram payments
- **DTOs/IdramDTOs.cs** - Data transfer objects for Idram requests and responses
- **IDRAM_INTEGRATION.md** - Complete integration guide
- **IDRAM_ORDER_INTEGRATION.md** - Guide for integrating with taxi orders
- **IDRAM_TESTING.md** - Testing examples and troubleshooting

### 2. Files Modified

- **appsettings.json** - Added Idram configuration section
- **Program.cs** - Registered IdramPaymentService in DI container
- **Data/AppDbContext.cs** - Added IdramPayments DbSet and entity configuration

### 3. Configuration Added

```json
{
  "Idram": {
    "ReceiverAccount": "100000114",
    "SecretKey": "your_secret_key_here",
    "Email": "merchant@example.com",
    "SuccessUrl": "http://localhost:5000/api/idram/success",
    "FailUrl": "http://localhost:5000/api/idram/fail",
    "ResultUrl": "http://localhost:5000/api/idram/result",
    "PaymentUrl": "https://banking.idram.am/Payment/GetPayment"
  }
}
```

## Next Steps

### 1. Configure Your Idram Credentials

**IMPORTANT:** Update the following values in `appsettings.json` or environment variables:

```bash
# PowerShell
$env:Idram__ReceiverAccount = "YOUR_IDRAM_MERCHANT_ID"
$env:Idram__SecretKey = "YOUR_SECRET_KEY_FROM_IDRAM"
$env:Idram__Email = "your-merchant@email.com"

# Linux/Mac
export Idram__ReceiverAccount="YOUR_IDRAM_MERCHANT_ID"
export Idram__SecretKey="YOUR_SECRET_KEY_FROM_IDRAM"
export Idram__Email="your-merchant@email.com"
```

### 2. Create Database Migration

Run these commands to add the IdramPayments table to your database:

```bash
dotnet ef migrations add AddIdramPayments
dotnet ef database update
```

### 3. Configure Idram Merchant Portal

Contact Idram technical support to set up these URLs in your merchant account:

**For Production:**
- SUCCESS_URL: `https://yourdomain.com/api/idram/success`
- FAIL_URL: `https://yourdomain.com/api/idram/fail`
- RESULT_URL: `https://yourdomain.com/api/idram/result`

**For Development (using ngrok):**
```bash
# Start ngrok
ngrok http 5000

# Use the ngrok URL for testing
# SUCCESS_URL: https://your-id.ngrok.io/api/idram/success
# FAIL_URL: https://your-id.ngrok.io/api/idram/fail
# RESULT_URL: https://your-id.ngrok.io/api/idram/result
```

### 4. Test the Integration

1. **Start your API:**
   ```bash
   dotnet run
   ```

2. **Get an authentication token** (see IDRAM_TESTING.md for details)

3. **Create a test payment:**
   ```bash
   curl -X POST http://localhost:5000/api/idram/create-payment \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "language": "EN",
       "description": "Test payment",
       "amount": 100.00,
       "billNo": "TEST_001"
     }'
   ```

4. **Submit the returned form** to Idram's payment gateway

5. **Complete payment** in Idram wallet

6. **Check payment status:**
   ```bash
   curl -X GET http://localhost:5000/api/idram/status/TEST_001 \
     -H "Authorization: Bearer YOUR_TOKEN"
   ```

## API Endpoints Available

### Client Endpoints (Require Authentication)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/idram/create-payment` | Create a new payment and get form data |
| GET | `/api/idram/status/{billNo}` | Check payment status |

### Callback Endpoints (Called by Idram)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/idram/result` | Handles precheck and payment confirmation |
| GET/POST | `/api/idram/success` | Success redirect page |
| GET/POST | `/api/idram/fail` | Failure redirect page |

## Security Features Implemented

? **Checksum Validation** - All payment confirmations are validated using MD5 checksum  
? **Secret Key Protection** - Secret key is stored in configuration, never exposed to client  
? **Order Validation** - Precheck validates order exists and amount matches  
? **Transaction Tracking** - All payments are logged in database  
? **Authorization** - Client endpoints require JWT authentication  

## Payment Flow

```
Client App ? Create Payment ? API stores payment in DB
                              ?
                         Returns form data
                              ?
Client submits form ? Idram Payment Gateway
                              ?
                    User authenticates & pays
                              ?
Idram ? Precheck (validates order) ? API responds OK/FAIL
                              ?
         If OK, payment processes
                              ?
Idram ? Payment Confirmation ? API validates checksum
                              ?
                     Updates payment status
                              ?
            Redirects user to success/fail page
```

## Documentation

- **IDRAM_INTEGRATION.md** - Complete integration guide with configuration instructions
- **IDRAM_ORDER_INTEGRATION.md** - How to integrate payments with taxi orders
- **IDRAM_TESTING.md** - API testing examples with curl, PowerShell, and Postman

## Support

### Idram Documentation
- Official docs: https://idram.am/en/integration
- GitHub iOS SDK: https://github.com/karapetyangevorg/IdramMerchantPayment
- GitHub Android SDK: https://github.com/karapetyangevorg/IdramMerchantPayment-Android

### Getting Help
- Check server logs for detailed error messages
- Review IDRAM_TESTING.md for troubleshooting
- Contact Idram support: support@idram.am

## Important Notes

?? **Before Production:**
1. Replace default ReceiverAccount with your actual merchant ID
2. Set your SECRET_KEY (never commit to source control)
3. Update all URLs to use HTTPS and your production domain
4. Test the complete flow with small amounts
5. Verify checksum validation is working correctly
6. Ensure RESULT_URL is accessible from Idram servers

?? **Security:**
- Never expose your SECRET_KEY in client-side code
- Always use HTTPS in production
- Store sensitive configuration in environment variables
- Validate all callbacks using checksum before processing

## Example Usage in Mobile App

See IDRAM_ORDER_INTEGRATION.md for complete examples of:
- Creating payments for taxi orders
- Handling mobile deep links (iOS/Android)
- Processing payment callbacks
- Updating order status after payment

## Congratulations! ??

Your Zeyro Taxi API now supports Idram payments. Follow the next steps above to complete the configuration and start testing.
