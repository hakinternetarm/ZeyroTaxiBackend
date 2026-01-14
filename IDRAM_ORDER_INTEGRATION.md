# Integrating Idram with Orders

This document shows how to integrate Idram payments with the taxi ordering flow.

## Example: Paying for an Order with Idram

### 1. Update Order Model (Optional)

Add Idram payment tracking to the Order model if needed:

```csharp
public string? IdramBillNo { get; set; }
public string? IdramTransactionId { get; set; }
public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Idram
```

### 2. Create Payment for Order

In your client application, when user selects Idram as payment method:

```javascript
// Step 1: Create order
const orderResponse = await fetch('/api/orders/request', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    pickupLat: 40.1776,
    pickupLng: 44.5126,
    destLat: 40.1872,
    destLng: 44.5152,
    paymentMethod: 'Idram',
    // ... other order fields
  })
});

const order = await orderResponse.json();

// Step 2: Create Idram payment for this order
const paymentResponse = await fetch('/api/idram/create-payment', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    language: 'EN',
    description: `Taxi Order #${order.id}`,
    amount: order.estimatedFare,
    billNo: `ORDER_${order.id}`,
    email: 'customer@example.com'
  })
});

const paymentData = await paymentResponse.json();

// Step 3: Submit form to Idram
const form = document.createElement('form');
form.method = 'POST';
form.action = paymentData.paymentUrl;

for (const [key, value] of Object.entries(paymentData.formFields)) {
  const input = document.createElement('input');
  input.type = 'hidden';
  input.name = key;
  input.value = value;
  form.appendChild(input);
}

document.body.appendChild(form);
form.submit();
```

### 3. Update Order After Payment

Add a webhook or polling mechanism to update the order status after payment:

```csharp
// In IdramController.cs HandleResult method, after successful payment:

// Find related order
var billNo = confirmation.EDP_BILL_NO;
if (billNo.StartsWith("ORDER_"))
{
    var orderIdStr = billNo.Replace("ORDER_", "");
    if (Guid.TryParse(orderIdStr, out var orderId))
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.PaymentStatus = "Paid";
            order.IdramTransactionId = confirmation.EDP_TRANS_ID;
            await _db.SaveChangesAsync();
            
            _logger.LogInformation("Order {orderId} payment confirmed via Idram", orderId);
        }
    }
}
```

### 4. Success/Fail Redirect Handling

Update the success and fail endpoints to redirect to your app:

```csharp
[HttpGet("success")]
[HttpPost("success")]
public IActionResult Success([FromQuery] string? billNo)
{
    _logger.LogInformation("Idram payment success redirect: billNo={billNo}", billNo);
    
    // Extract order ID from bill number
    var orderId = billNo?.Replace("ORDER_", "");
    
    // Redirect to your app with success
    return Redirect($"yourapp://payment/success?orderId={orderId}");
}

[HttpGet("fail")]
[HttpPost("fail")]
public IActionResult Fail([FromQuery] string? billNo)
{
    _logger.LogInformation("Idram payment failed redirect: billNo={billNo}", billNo);
    
    // Extract order ID from bill number
    var orderId = billNo?.Replace("ORDER_", "");
    
    // Redirect to your app with failure
    return Redirect($"yourapp://payment/fail?orderId={orderId}");
}
```

### 5. Mobile Deep Link Setup

For mobile apps, configure deep links to handle the redirect:

**iOS (Info.plist):**
```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>yourapp</string>
        </array>
    </dict>
</array>
```

**Android (AndroidManifest.xml):**
```xml
<intent-filter>
    <action android:name="android.intent.action.VIEW" />
    <category android:name="android.intent.category.DEFAULT" />
    <category android:name="android.intent.category.BROWSABLE" />
    <data android:scheme="yourapp" />
</intent-filter>
```

## Complete Flow Example

```
1. User requests taxi
   ??> POST /api/orders/request (creates order with PaymentMethod=Idram)

2. App creates Idram payment
   ??> POST /api/idram/create-payment (billNo=ORDER_{orderId})
   
3. User is redirected to Idram wallet
   ??> https://banking.idram.am/Payment/GetPayment
   
4. Idram validates order (precheck)
   ??> POST /api/idram/result (EDP_PRECHECK=YES)
   ??> Server validates order exists and amount
   ??> Server responds "OK"
   
5. User completes payment in Idram wallet

6. Idram confirms payment
   ??> POST /api/idram/result (with EDP_TRANS_ID)
   ??> Server validates checksum
   ??> Server updates IdramPayment status to "Success"
   ??> Server updates Order.PaymentStatus to "Paid"
   ??> Server responds "OK"
   
7. User is redirected back to app
   ??> GET /api/idram/success?billNo=ORDER_{orderId}
   ??> Redirect to yourapp://payment/success?orderId={orderId}
   
8. App shows success and assigns driver to order
```

## Testing Checklist

- [ ] Idram credentials configured in appsettings.json
- [ ] Database migration applied
- [ ] RESULT_URL accessible from internet (use ngrok for local testing)
- [ ] Deep links configured in mobile app
- [ ] Payment precheck validates correctly
- [ ] Checksum validation works
- [ ] Order status updates after payment
- [ ] Success/fail redirects work correctly
- [ ] Test with small amount (e.g., 100 AMD)
