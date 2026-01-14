# Payment Gateway Comparison: Idram vs IPay

This document compares the two Armenian payment gateways integrated into the Zeyro Taxi API.

## Quick Comparison

| Feature | Idram | IPay (ArCa) |
|---------|-------|-------------|
| **Operator** | Idram LLC | Armenian Card (ArCa) |
| **Market Position** | Popular digital wallet | National card processing system |
| **Integration Type** | Form-based redirect | REST API with redirect |
| **Primary Use Case** | Idram wallet payments | Card payments (all types) |
| **3D Secure** | Supported | Supported |
| **Currencies** | AMD, USD, EUR, RUB | AMD, USD, EUR, RUB |
| **Two-stage Payments** | Not in basic integration | Supported (PreAuth + Deposit) |
| **Recurring Payments** | Via binding | Via binding + recurring API |
| **Mobile App Support** | Deep links (iOS/Android) | Web redirect |
| **Transaction Fees** | Contact Idram | Contact ArCa |

## Technical Comparison

### Idram

**Integration Method:**
- Form-based HTML submission
- User enters card on Idram's page
- Callback for validation and confirmation

**Pros:**
? Simple HTML form integration  
? Popular in Armenia (digital wallet)  
? Mobile app deep linking support  
? Idram-to-Idram instant transfers  
? Strong MD5 checksum validation  
? Pre-check validation before payment  

**Cons:**
? Form-based (less modern than REST API)  
? No direct card tokenization API  
? Limited to Idram ecosystem features  

**Best For:**
- Users with Idram wallets
- Simple payment flows
- Mobile app integrations
- Quick implementation

**Configuration:**
```json
{
  "Idram": {
    "ReceiverAccount": "100000114",
    "SecretKey": "your_secret_key",
    "Email": "merchant@example.com",
    "SuccessUrl": "http://localhost:5000/api/idram/success",
    "FailUrl": "http://localhost:5000/api/idram/fail",
    "ResultUrl": "http://localhost:5000/api/idram/result"
  }
}
```

### IPay (ArCa)

**Integration Method:**
- REST API for order registration
- User enters card on IPay's page
- Status polling after payment

**Pros:**
? Modern REST API  
? Full two-stage payment support  
? Comprehensive status information  
? ArCa network (all Armenian banks)  
? Advanced features (recurring, bindings)  
? Detailed transaction history  
? Refund and reversal support  

**Cons:**
? More complex initial setup  
? Requires API credentials management  
? No mobile deep linking (web only)  

**Best For:**
- Card payment processing
- Two-stage payment flows
- Recurring/subscription payments
- Enterprise integrations
- Detailed transaction tracking

**Configuration:**
```json
{
  "IPay": {
    "UserName": "your_username",
    "Password": "your_password",
    "Currency": "051",
    "Language": "en",
    "ReturnUrl": "http://localhost:5000/api/ipay/return",
    "ApiBaseUrl": "https://ipay.arca.am/payment/rest"
  }
}
```

## Feature Comparison

### Payment Lifecycle

**Idram:**
1. Register order ? Get bill number
2. Submit form with bill number
3. Idram pre-check (validation)
4. User pays on Idram page
5. Idram confirms payment (callback)
6. User redirected to success/fail

**IPay:**
1. Register order ? Get order ID + form URL
2. Redirect to form URL
3. User pays on IPay page
4. IPay processes payment
5. User redirected to return URL
6. Poll status for confirmation

### Two-Stage Payments

**Idram:**
- Not available in basic integration
- Use one-stage payments only

**IPay:**
- Full support with `registerPreAuth.do`
- Authorize (hold) ? Deposit (capture)
- Reverse (cancel) before deposit

### Refunds

**Idram:**
- Supported via callback
- Returns `EDP_TRANS_ID` for tracking

**IPay:**
- Supported via REST API
- `refund.do` endpoint
- Full and partial refunds
- Multiple refunds per transaction

### Card Bindings (Recurring)

**Idram:**
- Not explicitly documented
- Would require custom implementation

**IPay:**
- Full binding support
- Create binding on first payment
- Use binding for subsequent payments
- Activate/deactivate bindings

## API Endpoints

### Idram Endpoints

| Purpose | Endpoint |
|---------|----------|
| Create Payment | `POST /api/idram/create-payment` |
| Check Status | `GET /api/idram/status/{billNo}` |
| Success Page | `GET /api/idram/success` |
| Fail Page | `GET /api/idram/fail` |
| Result Callback | `POST /api/idram/result` |

### IPay Endpoints

| Purpose | Endpoint |
|---------|----------|
| Create Payment | `POST /api/ipay/create-payment` |
| Check Status | `GET /api/ipay/status/{orderId}` |
| Reverse Payment | `POST /api/ipay/reverse/{orderId}` |
| Refund Payment | `POST /api/ipay/refund` |
| Return Callback | `GET/POST /api/ipay/return` |

## Security Comparison

### Idram Security

- **Checksum:** MD5 hash validation
- **Secret Key:** Server-side validation
- **Pre-check:** Order validation before payment
- **Data:** Never stores card details

**Checksum Calculation:**
```
MD5(EDP_REC_ACCOUNT:EDP_AMOUNT:SECRET_KEY:EDP_BILL_NO:
    EDP_PAYER_ACCOUNT:EDP_TRANS_ID:EDP_TRANS_DATE)
```

### IPay Security

- **3D Secure:** Automatic enrollment
- **HTTPS:** All API calls encrypted
- **Credentials:** Username/password authentication
- **Data:** Only masked PAN stored

## Use Case Recommendations

### Use Idram When:

1. **Target audience has Idram wallets**
   - Popular in Armenia
   - Instant wallet transfers

2. **Simple payment flow needed**
   - One-time payments
   - Quick checkout

3. **Mobile app integration required**
   - Deep linking support
   - Native app payment flow

4. **Form-based integration preferred**
   - No API credentials management
   - Simple HTML forms

### Use IPay When:

1. **Card processing is primary method**
   - All Armenian bank cards
   - International cards via ArCa

2. **Two-stage payments needed**
   - Pre-authorization required
   - Delayed capture

3. **Advanced features required**
   - Recurring payments
   - Card bindings
   - Detailed reporting

4. **REST API integration preferred**
   - Programmatic control
   - Status polling

## Cost Considerations

Contact each provider for:
- Transaction fees (% + fixed)
- Monthly/annual fees
- Setup costs
- Volume discounts
- International transaction fees

**Typical Fee Structure:**
- Domestic cards: 1-3%
- International cards: 3-5%
- Setup fee: Varies
- Monthly minimum: May apply

## Implementation Complexity

### Idram: ??? (Moderate)

**Setup Time:** 2-4 hours

**Steps:**
1. Get credentials from Idram
2. Update configuration
3. Test with form submission
4. Implement callbacks

**Complexity:**
- Simple form-based integration
- Checksum validation required
- Two callback handlers

### IPay: ???? (Moderate-Advanced)

**Setup Time:** 4-8 hours

**Steps:**
1. Get credentials from ArCa
2. Update configuration
3. Test REST API integration
4. Implement status polling
5. Handle all payment states

**Complexity:**
- REST API integration
- Multiple endpoints
- State management
- Status polling

## Testing

### Idram Testing

**Test Environment:**
- Contact Idram for test credentials
- Use test SECRET_KEY
- Configure ngrok for callbacks

**Test Cards:**
- Provided by Idram support

### IPay Testing

**Test Environment:**
- URL: `https://ipaytest.arca.am:8445/payment/rest`
- Contact ArCa for test credentials

**Test Cards:**
- Provided by ArCa support

## Recommendation Matrix

| Scenario | Recommended | Reason |
|----------|-------------|--------|
| Startup/MVP | **Idram** | Simpler, faster integration |
| Enterprise | **IPay** | More features, better control |
| Mobile-first | **Idram** | Deep linking support |
| Subscriptions | **IPay** | Recurring payments, bindings |
| High volume | **IPay** | Better API, more control |
| Mixed payments | **Both** | Maximum user choice |

## Dual Integration Strategy

For maximum flexibility, you can use both:

```csharp
// Let user choose payment method
if (paymentMethod == "idram")
{
    // Use Idram for wallet users
    var idramPayment = await _idramService.CreatePayment(...);
    return idramPayment;
}
else if (paymentMethod == "card")
{
    // Use IPay for card users
    var ipayPayment = await _ipayService.CreatePayment(...);
    return ipayPayment;
}
```

**Benefits:**
- Maximum user choice
- Redundancy (failover)
- Optimize costs per transaction type
- Market coverage

## Migration Path

### From Stripe to Armenian Gateways

If migrating from Stripe:

1. **Keep Stripe** for international users
2. **Add Idram** for local wallet users
3. **Add IPay** for local card users
4. **Route based on** user preference/card BIN

### Phased Rollout

**Phase 1: Idram**
- Implement Idram first (simpler)
- Get Armenian market experience
- Learn callback patterns

**Phase 2: IPay**
- Add IPay for card processing
- Implement two-stage if needed
- Add recurring if needed

**Phase 3: Optimization**
- Route based on costs
- Analyze success rates
- Optimize user experience

## Support Resources

### Idram
- Website: https://idram.am
- Email: support@idram.am
- Documentation: Idram Merchant Interface PDF

### IPay (ArCa)
- Website: https://www.arca.am
- Email: support@arca.am
- Phone: +374 10 59 22 22
- Admin Panel: https://ipay.arca.am/payment/admin

## Conclusion

**Both payment gateways are now integrated and ready to use!**

Choose based on your specific needs:
- **Idram** for simplicity and wallet payments
- **IPay** for advanced features and card processing
- **Both** for maximum flexibility

Your Zeyro Taxi API now supports comprehensive payment processing for the Armenian market! ??
