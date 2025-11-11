# Missing Features for Production-Ready System

**Last Updated:** 2025-11-11
**Priority:** Features ranked by importance for MVP and beyond

---

## 🔴 CRITICAL - Required for Production MVP

### 1. Payment Gateway Integration
**Current State:** Manual payment recording only
**Impact:** HIGH - Without this, tenants can't pay online
**Effort:** 1-2 weeks per integration

#### A. M-Pesa Integration (ESSENTIAL for Kenya)
**Why:** 80%+ of Kenyans use M-Pesa for payments

**Implementation:**
- **Option 1: Safaricom Daraja API** (Official)
  - STK Push (Lipa Na M-Pesa)
  - C2B (Customer to Business)
  - Transaction status queries
  - Callbacks for payment confirmation

- **Option 2: Flutterwave** (Easier integration)
  - Supports M-Pesa + Cards + Bank
  - Single API for multiple payment methods
  - Better documentation

**Files to Create:**
```
Infrastructure/Services/
  - IMpesaService.cs
  - MpesaService.cs (Daraja API)
  - or IPaymentGatewayService.cs (for Flutterwave)
  - PaymentCallbackService.cs

API/Controllers/
  - PaymentGatewayController.cs
  - MpesaCallbackController.cs

Application/DTOs/Payments/
  - InitiatePaymentDto.cs
  - PaymentCallbackDto.cs
  - MpesaStkPushDto.cs
```

**Features:**
- [ ] STK Push to tenant phone
- [ ] Payment callback handling
- [ ] Automatic payment record creation
- [ ] Transaction reconciliation
- [ ] Failed payment retry
- [ ] Payment status webhooks

**Configuration Needed:**
```json
"Mpesa": {
  "ConsumerKey": "your-key",
  "ConsumerSecret": "your-secret",
  "ShortCode": "174379",
  "PassKey": "your-passkey",
  "CallbackUrl": "https://yourdomain.com/api/mpesa/callback"
}
```

#### B. Card Payments (Visa/Mastercard)
**Why:** Expats and corporate clients prefer card payments

**Recommended Providers:**
1. **Flutterwave** (Best for Africa)
   - Supports all African cards
   - PCI compliant
   - Good UX

2. **Stripe** (International standard)
   - Better for international tenants
   - More expensive in Kenya

3. **PayStack** (Alternative)
   - Good rates
   - African focus

**Files to Create:**
```
Infrastructure/Services/
  - ICardPaymentService.cs
  - FlutterwaveService.cs
  - StripeService.cs (alternative)

Application/DTOs/Payments/
  - CardPaymentDto.cs
  - PaymentIntentDto.cs
```

**Features:**
- [ ] Tokenized card storage
- [ ] Recurring payments (auto-debit monthly rent)
- [ ] 3D Secure support
- [ ] Refund handling
- [ ] Partial payments

#### C. Bank Transfer Integration
**Why:** Some tenants prefer direct bank transfers

**Options:**
- Manual: Just record bank reference number (already supported)
- Automated: Bank API integration for verification

---

### 2. Authentication & Authorization 🔴
**Current State:** NO AUTH - Wide open!
**Impact:** CRITICAL SECURITY ISSUE
**Effort:** 3-5 days

**What's Missing:**
- [ ] User management (Admin, PropertyManager, Tenant roles)
- [ ] JWT token authentication
- [ ] Role-based authorization
- [ ] Login/Register endpoints
- [ ] Password hashing
- [ ] Refresh tokens
- [ ] Email verification

**Files to Create:**
```
Domain/Entities/
  - User.cs
  - Role.cs
  - UserRole.cs

Application/Services/
  - IAuthService.cs
  - AuthService.cs
  - IJwtTokenService.cs
  - JwtTokenService.cs

API/Controllers/
  - AuthController.cs
  - UsersController.cs

Application/DTOs/Auth/
  - LoginDto.cs
  - RegisterDto.cs
  - TokenResponseDto.cs
  - UserDto.cs
```

**Roles to Implement:**
```csharp
public enum UserRole
{
    SuperAdmin,      // Full system access
    PropertyManager, // Manage assigned properties
    Tenant,          // View own data + pay rent
    Accountant       // View reports only
}
```

**Permissions:**
- SuperAdmin: Everything
- PropertyManager: CRUD on assigned properties/units/tenants
- Tenant: View own info, make payments, download receipts
- Accountant: Read-only access to reports

---

### 3. Automated Payment Reminders 🟡
**Current State:** Manual SMS sending only
**Impact:** MEDIUM - High manual effort
**Effort:** 2-3 days

**What's Needed:**
- [ ] Background job scheduler (Hangfire or Quartz.NET)
- [ ] Scheduled rent reminder jobs
- [ ] Overdue payment notifications
- [ ] Escalation (Day 1, 3, 7, 14 overdue)

**Files to Create:**
```
Infrastructure/Jobs/
  - RentReminderJob.cs
  - OverduePaymentJob.cs
  - LeaseExpiryReminderJob.cs

Infrastructure/Services/
  - IScheduledJobService.cs
  - ScheduledJobService.cs
```

**Schedule:**
- 5 days before rent due: Reminder SMS
- 1 day before: Final reminder
- Day 1 overdue: First overdue notice
- Day 3: Second notice
- Day 7: Escalation notice
- Day 14: Final warning

**Configuration:**
```json
"JobScheduler": {
  "RentReminderDays": [5, 1],
  "OverdueEscalationDays": [1, 3, 7, 14]
}
```

---

## 🟡 IMPORTANT - Should Have for Better UX

### 4. Email Notifications
**Why:** SMS is expensive; Email is free and professional

**Features:**
- [ ] Email service (SendGrid/AWS SES/SMTP)
- [ ] Email templates (matching SMS templates)
- [ ] Welcome email for new tenants
- [ ] Monthly rent receipt via email
- [ ] Payment confirmation emails
- [ ] Lease expiry reminders

**Files to Create:**
```
Infrastructure/Services/
  - IEmailService.cs
  - EmailService.cs (SendGrid)
  - EmailTemplates.cs

Application/DTOs/Email/
  - SendEmailDto.cs
```

### 5. Late Fee Calculation
**Why:** Automate penalty charges for late payments

**Features:**
- [ ] Configurable late fee rules (% or fixed amount)
- [ ] Grace period configuration
- [ ] Automatic late fee posting
- [ ] Late fee waivers (admin approval)

**Files to Create:**
```
Domain/Entities/
  - LateFee.cs
  - LateFeeConfiguration.cs

Application/Services/
  - ILateFeeService.cs
  - LateFeeService.cs
```

**Configuration:**
```csharp
public class LateFeeConfiguration
{
    public int GracePeriodDays { get; set; } = 3;
    public decimal LateFeePercentage { get; set; } = 5m; // 5% of rent
    public decimal MaxLateFeeAmount { get; set; } = 5000m;
}
```

### 6. Tenant Portal Features
**Why:** Reduce property manager workload

**Features:**
- [ ] Tenant self-service dashboard
- [ ] View payment history
- [ ] Download receipts
- [ ] Submit maintenance requests
- [ ] View lease details
- [ ] Update contact information

### 7. Multi-Currency Support
**Why:** International properties or expats

**Features:**
- [ ] Currency entity and configuration
- [ ] Exchange rate service
- [ ] Convert amounts for reports
- [ ] Multi-currency payment recording

---

## 🟢 NICE TO HAVE - Future Enhancements

### 8. Maintenance Request System
**Features:**
- [ ] Tenants submit maintenance requests
- [ ] Upload photos of issues
- [ ] Priority levels (Low, Medium, High, Emergency)
- [ ] Assignment to maintenance staff
- [ ] Status tracking (Pending, In Progress, Completed)
- [ ] Cost tracking

**Files to Create:**
```
Domain/Entities/
  - MaintenanceRequest.cs
  - MaintenanceRequestStatus.cs

Controllers/
  - MaintenanceRequestsController.cs
```

### 9. Document Management
**Features:**
- [ ] Upload lease agreements (PDF)
- [ ] Store tenant ID documents
- [ ] Property images
- [ ] Inspection reports
- [ ] Digital signatures

**Technology:**
- Azure Blob Storage or AWS S3
- Or local file system for small deployments

### 10. Communication System
**Features:**
- [ ] In-app messaging between landlord/tenant
- [ ] Broadcast announcements
- [ ] Read receipts
- [ ] Message history

### 11. Expense Tracking
**Features:**
- [ ] Record property expenses (utilities, maintenance, taxes)
- [ ] Expense categories
- [ ] Profit/Loss reports
- [ ] Tax reporting

### 12. Utility Bill Management
**Features:**
- [ ] Record utility readings (water, electricity)
- [ ] Calculate utility charges
- [ ] Split utilities among tenants
- [ ] Add to rent invoices

### 13. Tenant Screening
**Features:**
- [ ] Credit check integration
- [ ] Reference verification
- [ ] Background checks
- [ ] Application forms

### 14. Vacancy Management
**Features:**
- [ ] Advertise vacant units
- [ ] Application tracking
- [ ] Viewing appointments
- [ ] Application approval workflow

### 15. Lease Renewal Automation
**Features:**
- [ ] Auto-detect expiring leases (30/60/90 days)
- [ ] Generate renewal offers
- [ ] Track renewal status
- [ ] Auto-archive expired leases

### 16. Multi-Tenancy (SaaS Mode)
**Why:** Sell your system to multiple property managers

**Features:**
- [ ] Separate data per organization
- [ ] Subscription billing
- [ ] Feature flags per plan
- [ ] White-labeling

### 17. Mobile App
**Why:** Better UX for tenants

**Options:**
- React Native
- Flutter
- .NET MAUI

**Features:**
- [ ] Push notifications
- [ ] Biometric login
- [ ] Quick rent payment
- [ ] Offline mode

### 18. Analytics & Reporting
**Enhanced beyond current dashboard:**
- [ ] Occupancy trends over time
- [ ] Revenue forecasting
- [ ] Tenant retention rates
- [ ] Property performance comparison
- [ ] Export to Excel
- [ ] Interactive charts (Chart.js)

### 19. Audit Logging
**Why:** Track who changed what and when

**Features:**
- [ ] Log all CRUD operations
- [ ] User action history
- [ ] Change tracking
- [ ] Compliance reporting

**Files:**
```
Domain/Entities/
  - AuditLog.cs

Infrastructure/Interceptors/
  - AuditInterceptor.cs (EF Core)
```

### 20. Integration APIs
**Features:**
- [ ] Accounting software integration (QuickBooks, Xero)
- [ ] Property listing sites (PigiaMe, BuyRentKenya)
- [ ] Calendar sync (Google Calendar for viewings)
- [ ] WhatsApp Business API

---

## 📊 PRIORITY ROADMAP

### Phase 1: Production MVP (Must Have) - 2-3 weeks
1. ✅ Core functionality (DONE!)
2. 🔴 M-Pesa integration (1 week)
3. 🔴 Authentication & Authorization (3-5 days)
4. 🔴 Automated payment reminders (2-3 days)
5. 🟡 Email notifications (2 days)
6. Frontend implementation (2 weeks)

### Phase 2: Enhanced MVP - 2 weeks
7. Card payment integration (Flutterwave)
8. Late fee calculation
9. Tenant portal features
10. Maintenance requests

### Phase 3: Professional System - 3-4 weeks
11. Document management
12. Expense tracking
13. Utility bill management
14. Advanced analytics
15. Audit logging

### Phase 4: SaaS/Enterprise - 4-6 weeks
16. Multi-tenancy
17. Mobile app
18. Lease renewal automation
19. Tenant screening
20. Third-party integrations

---

## 💰 ESTIMATED COSTS (Monthly for 100 tenants)

**Payment Gateways:**
- M-Pesa (Daraja): Free + 1-3% transaction fee
- Flutterwave: 3.8% + KES 10 per transaction
- Stripe: 3.9% + KES 30 per transaction

**SMS:**
- Africa's Talking: ~KES 0.80 per SMS
- 100 tenants × 3 SMS/month = KES 240/month

**Email:**
- SendGrid: Free up to 100 emails/day
- AWS SES: $0.10 per 1000 emails

**Hosting (Azure/AWS):**
- Database: $20-50/month
- App Service: $50-100/month
- Storage: $5-10/month
- Total: ~$75-160/month

**Total Monthly Cost:** ~$80-200/month for 100 tenants

---

## 🎯 RECOMMENDATIONS

### For Immediate Production Launch:
**MUST IMPLEMENT:**
1. M-Pesa integration (can't launch without online payments in Kenya)
2. Authentication & Authorization (security critical)
3. Email notifications (cheaper than SMS)

**CAN WAIT:**
- Card payments (add later when you get international tenants)
- Maintenance requests (can use WhatsApp/calls initially)
- Advanced analytics (current dashboard is sufficient)

### For 6-Month Roadmap:
1. Launch with M-Pesa + Auth + Email (Phase 1)
2. Add card payments after 3 months (Phase 2)
3. Add maintenance & expenses after 6 months (Phase 3)

### For SaaS Product:
- Implement multi-tenancy from the start
- Add subscription billing (Stripe)
- Focus on mobile app for better tenant UX
- Build marketplace integrations

---

## 💡 QUICK WINS (Easy to add, high value)

1. **Email notifications** - 1-2 days, huge cost savings vs SMS
2. **Late fee automation** - 1 day, automatic revenue
3. **Payment reminders** - 2 days, reduce late payments
4. **Audit logging** - 1 day, better security
5. **Excel export** - 1 day, accountants love this

---

## 🚀 NEXT STEPS

**This Week:**
1. Run migrations and test current backend
2. Decide on payment gateway (recommend Flutterwave for M-Pesa + Cards)
3. Get Flutterwave account and API keys

**Next Week:**
1. Implement M-Pesa payment integration
2. Add authentication & authorization
3. Start frontend development

**Within Month:**
1. Complete MVP with payments + auth
2. Deploy to production
3. Onboard first 10 beta users

Would you like me to implement any of these features? I recommend starting with M-Pesa integration!
