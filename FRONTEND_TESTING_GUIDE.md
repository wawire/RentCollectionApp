# Frontend Testing Guide - Payment Portal System

This guide covers testing all the new frontend features for the Tenant Payment Portal and Landlord Payment Confirmation workflow.

## Prerequisites

### Start the Development Server

```bash
cd src/RentCollection.WebApp
npm install
npm run dev
```

The app will be available at `http://localhost:3000`

### Ensure Backend is Running

The .NET API must be running on `https://localhost:7001` (or your configured API URL).

```bash
cd src/RentCollection.API
dotnet run
```

## Test Accounts

### Tenant Accounts (For Tenant Portal Testing)

| Email | Password | Unit | Property | Monthly Rent |
|-------|----------|------|----------|--------------|
| peter.mwangi@gmail.com | Tenant@123 | B1 | Sunset Apartments | KSh 12,000 |
| grace.akinyi@yahoo.com | Tenant@123 | 1A | Sunset Apartments | KSh 18,000 |
| alice.wambui@gmail.com | Tenant@123 | K-2A | Kileleshwa Gardens | KSh 35,000 |
| james.kamau@gmail.com | Tenant@123 | W-3B | Westlands Towers | KSh 45,000 |

### Landlord Accounts (For Payment Confirmation Testing)

| Email | Password | Role | Properties |
|-------|----------|------|------------|
| john.landlord@example.com | Landlord@123 | Landlord | Sunset Apartments, Green Valley Estate |
| jane.smith@example.com | Landlord@123 | Landlord | Kileleshwa Gardens |
| bob.owner@example.com | Landlord@123 | Landlord | Westlands Towers, Parklands Homes |

### System Admin Account

| Email | Password | Role |
|-------|----------|------|
| admin@rentcollection.com | Admin@123 | SystemAdmin |

---

## Testing Scenarios

### 1. Tenant Portal - Complete Workflow

#### A. Login as Tenant

1. Navigate to `http://localhost:3000/login`
2. Enter credentials:
   - Email: `peter.mwangi@gmail.com`
   - Password: `Tenant@123`
3. Click **Login**
4. ✅ **Expected:** Auto-redirect to `/tenant-portal` dashboard

#### B. Tenant Portal Dashboard

**URL:** `/tenant-portal`

**Verify the following sections:**

1. **Header Section:**
   - ✅ Title: "Tenant Portal"
   - ✅ Welcome message: "Welcome, Peter Mwangi"

2. **Tenant Info Card:**
   - ✅ Unit: B1
   - ✅ Property: Sunset Apartments
   - ✅ Monthly Rent: KSh 12,000

3. **Statistics Cards:**
   - ✅ Pending Payments: Shows count
   - ✅ Last Payment: Shows amount and date (or N/A)
   - ✅ Total Payments: Shows count

4. **Quick Actions:**
   - ✅ Three action cards with links:
     - Payment Instructions
     - Record Payment
     - Payment History

5. **Recent Payments Section:**
   - ✅ Shows last 5 payments with status badges
   - ✅ Each payment shows:
     - Amount, date, method
     - Transaction reference
     - Status badge (green/yellow/red)

#### C. Payment Instructions Page

**URL:** `/tenant-portal/payment-instructions`

**Navigation:**
1. Click "Payment Instructions" from dashboard
2. OR click "View all →" in recent payments

**Verify:**

1. **Rent Details Section:**
   - ✅ Tenant Name: Peter Mwangi
   - ✅ Unit Number: B1
   - ✅ Property: Sunset Apartments
   - ✅ Monthly Rent: KSh 12,000
   - ✅ Landlord: John Landlord

2. **M-Pesa Paybill Section** (if account type is MPesaPaybill):
   - ✅ Green gradient background
   - ✅ Paybill Number with copy button
   - ✅ Account Number with copy button
   - ✅ Step-by-step instructions (7 steps)
   - ✅ Additional instructions (if any)
   - ✅ **Test Copy Button:** Click copy icon, verify "Copied" confirmation

3. **Bank Account Section** (if account type is BankAccount):
   - ✅ Blue gradient background
   - ✅ Bank Name
   - ✅ Account Number with copy button
   - ✅ Account Name
   - ✅ Branch and SWIFT Code
   - ✅ Additional instructions

4. **Next Steps Section:**
   - ✅ 3-step guide after payment
   - ✅ "Record Payment" button
   - ✅ Click button → redirects to `/tenant-portal/record-payment`

#### D. Record Payment Form

**URL:** `/tenant-portal/record-payment`

**Complete Form Test:**

1. **Fill the form:**
   - Amount: `12000`
   - Payment Date: Select today
   - Payment Method: Select `M-Pesa`
   - Transaction Reference: `SKG8N9Q2RT` (example M-Pesa code)
   - M-Pesa Phone Number: `0723870917`
   - Period Start: `2025-12-01`
   - Period End: `2025-12-31`
   - Notes: `December 2025 rent payment` (optional)

2. Click **Submit Payment**

3. ✅ **Expected Results:**
   - Loading state: Button shows "Submitting..."
   - Success screen appears:
     - Green checkmark icon
     - "Payment Recorded Successfully!"
     - "Your payment has been submitted and is awaiting landlord confirmation"
   - Auto-redirect to dashboard after 2 seconds

4. **Validation Tests:**
   - Try submitting without Amount → ✅ Should show validation error
   - Try submitting without Transaction Reference → ✅ Should show validation error
   - Try date after today → ✅ Should not allow future dates

#### E. Payment History Page

**URL:** `/tenant-portal/history`

**Navigation:**
1. Click "Payment History" from dashboard
2. OR click "View all →" in recent payments section

**Verify:**

1. **Summary Statistics:**
   - ✅ Total Payments: Count
   - ✅ Total Paid: Sum of completed payments (green)
   - ✅ Pending Amount: Sum of pending payments (yellow)

2. **Filter Buttons:**
   - ✅ All (shows count)
   - ✅ Completed (green)
   - ✅ Pending (yellow)
   - ✅ Rejected (red)
   - Click each filter → verify list updates

3. **Payment List:**
   - Each payment card shows:
     - ✅ Status icon (checkmark/clock/warning)
     - ✅ Amount in large text
     - ✅ Status badge (color-coded)
     - ✅ Payment Date
     - ✅ Payment Method
     - ✅ Transaction Reference
     - ✅ Payment Period (Dec 1 - Dec 31)
     - ✅ Confirmation details (if completed)
     - ✅ Tenant notes (if any)

4. **Empty State:**
   - Filter to a status with no payments
   - ✅ Should show "No [status] payments found"

---

### 2. Landlord Payment Confirmation - Complete Workflow

#### A. Login as Landlord

1. Navigate to `http://localhost:3000/login`
2. Enter credentials:
   - Email: `john.landlord@example.com`
   - Password: `Landlord@123`
3. Click **Login**
4. ✅ **Expected:** Auto-redirect to `/dashboard`

#### B. Navigate to Pending Payments

**Two ways to access:**

1. **Via Sidebar:**
   - ✅ Click "Pending Payments" in left sidebar
   - ✅ Icon: Clipboard with checkmark

2. **Direct URL:**
   - Navigate to `/payments/pending`

#### C. Pending Payments Page

**URL:** `/payments/pending`

**Verify:**

1. **Header:**
   - ✅ Title: "Pending Payments"
   - ✅ Subtitle: "Review and confirm tenant payment submissions"
   - ✅ Back link to "/payments"

2. **Summary Statistics:**
   - ✅ Pending Payments: Count
   - ✅ Total Amount: Sum (in yellow)
   - ✅ Awaiting Action: Count with clock icon

3. **Empty State** (if no pending payments):
   - ✅ Green checkmark icon
   - ✅ "No pending payments"
   - ✅ "All tenant payments have been reviewed"
   - ✅ "View All Payments" button

4. **Payment Cards** (if pending payments exist):
   Each card shows:
   - ✅ Yellow clock icon
   - ✅ Amount in large text
   - ✅ "Pending Review" badge (yellow)
   - ✅ Tenant name
   - ✅ Unit and Property
   - ✅ Payment Date (formatted nicely)
   - ✅ Payment Method
   - ✅ Transaction Reference (monospace font)
   - ✅ M-Pesa Phone Number (if M-Pesa)
   - ✅ Payment Period (blue info box)
   - ✅ Tenant Notes (gray box, if any)
   - ✅ Two action buttons:
     - **Confirm** (green)
     - **Reject** (red)

#### D. Confirm Payment Flow

1. **Click "Confirm" button** on any pending payment

2. **Confirm Modal Appears:**
   - ✅ Green checkmark icon
   - ✅ Title: "Confirm Payment"
   - ✅ Confirmation message with amount and tenant name
   - ✅ Optional "Notes" textarea
   - ✅ Two buttons:
     - Cancel (gray)
     - Confirm (green)

3. **Add Optional Notes:**
   - Type: `Payment verified via M-Pesa statement`

4. **Click "Confirm"**

5. ✅ **Expected Results:**
   - Button shows "Confirming..."
   - Modal closes
   - Green success alert: "Payment confirmed successfully!"
   - Payment disappears from pending list (auto-refresh)
   - Alert auto-dismisses after 3 seconds

#### E. Reject Payment Flow

1. **Click "Reject" button** on any pending payment

2. **Reject Modal Appears:**
   - ✅ Red X icon
   - ✅ Title: "Reject Payment"
   - ✅ Rejection message with amount and tenant name
   - ✅ Required "Reason for Rejection" textarea
   - ✅ Two buttons:
     - Cancel (gray)
     - Reject (red)

3. **Try Rejecting Without Reason:**
   - Click "Reject" with empty textarea
   - ✅ Should show error: "Please provide a reason for rejection"

4. **Add Rejection Reason:**
   - Type: `Transaction reference not found in M-Pesa statement`

5. **Click "Reject"**

6. ✅ **Expected Results:**
   - Button shows "Rejecting..."
   - Modal closes
   - Green success alert: "Payment rejected successfully"
   - Payment disappears from pending list
   - Alert auto-dismisses after 3 seconds

#### F. Verify Tenant Sees Status Update

1. **Logout from landlord account**
2. **Login as tenant** (peter.mwangi@gmail.com)
3. **Go to Payment History** (`/tenant-portal/history`)
4. **Find the payment you just confirmed/rejected**
5. ✅ **Verify:**
   - Confirmed payment:
     - Green checkmark icon
     - Status badge: "Completed" (green)
     - Shows "Confirmed: [date] by John Landlord"
     - Shows landlord notes (if any)
   - Rejected payment:
     - Red warning icon
     - Status badge: "Rejected" (red)
     - Shows rejection reason

---

### 3. Role-Based Navigation Testing

#### A. Tenant Navigation

**Login as:** peter.mwangi@gmail.com

**Verify Sidebar Shows:**
- ✅ Only "Tenant Portal" menu item
- ✅ Wallet icon
- ✅ Settings at bottom
- ✅ Does NOT show: Properties, Units, Dashboard, etc.

**Click "Tenant Portal":**
- ✅ Redirects to `/tenant-portal`
- ✅ Item highlighted as active

#### B. Landlord Navigation

**Login as:** john.landlord@example.com

**Verify Sidebar Shows:**
- ✅ Home
- ✅ Dashboard
- ✅ Properties
- ✅ Units
- ✅ Tenants
- ✅ Payments
- ✅ **Pending Payments** ⭐ (new)
- ✅ Reports
- ✅ SMS
- ✅ Settings

**Click Each Menu Item:**
- ✅ All items navigate correctly
- ✅ Active item is highlighted
- ✅ "Pending Payments" has clipboard-check icon

#### C. System Admin Navigation

**Login as:** admin@rentcollection.com

**Verify:**
- ✅ Same navigation as Landlord
- ✅ Can access all pages including Pending Payments

---

## UI/UX Verification Checklist

### Design Consistency

- ✅ All pages use consistent color scheme (primary colors)
- ✅ Font: Serif for headings, sans for body text
- ✅ Card styling: White background, border, shadow
- ✅ Buttons: Primary (blue), Success (green), Danger (red)
- ✅ Status badges: Color-coded consistently
  - Green: Completed
  - Yellow: Pending
  - Red: Rejected

### Responsive Design

**Test on different screen sizes:**

1. **Mobile (375px):**
   - ✅ Sidebar collapses to hamburger menu
   - ✅ Cards stack vertically
   - ✅ Forms are usable
   - ✅ Modals fit screen

2. **Tablet (768px):**
   - ✅ Sidebar appears
   - ✅ Grid layouts adjust (2 columns)

3. **Desktop (1280px+):**
   - ✅ Full layout with sidebar
   - ✅ Grid layouts use 3-4 columns

### Loading States

- ✅ Dashboard: Shows spinner while loading
- ✅ Payment Instructions: Shows spinner
- ✅ Payment History: Shows spinner
- ✅ Pending Payments: Shows spinner
- ✅ Form Submit: Button shows "Submitting..."
- ✅ Confirm/Reject: Button shows loading text

### Error Handling

**Test error scenarios:**

1. **Network Error:**
   - Disconnect from backend
   - Try loading any page
   - ✅ Should show error message
   - ✅ Should show "Try Again" or "Back" button

2. **API Error:**
   - Enter invalid data
   - ✅ Should show error alert
   - ✅ Error message should be user-friendly

3. **Validation Errors:**
   - Try submitting incomplete forms
   - ✅ Required fields should be highlighted
   - ✅ Error messages should be clear

---

## Integration Testing

### Full Payment Lifecycle

**Complete this flow from start to finish:**

1. ✅ **Tenant Login** → peter.mwangi@gmail.com
2. ✅ **View Instructions** → Get paybill/account details
3. ✅ **Record Payment** → Submit with transaction ref
4. ✅ **Verify Pending** → Check payment history (yellow badge)
5. ✅ **Logout**
6. ✅ **Landlord Login** → john.landlord@example.com
7. ✅ **View Pending** → See tenant's payment
8. ✅ **Confirm Payment** → Add notes, confirm
9. ✅ **Logout**
10. ✅ **Tenant Login** → peter.mwangi@gmail.com
11. ✅ **Verify Completed** → Check payment history (green badge)
12. ✅ **See Confirmation Details** → Landlord name, notes, date

**Time this process:** Should take < 2 minutes with all verifications.

---

## Browser Compatibility

Test on the following browsers:

- ✅ Chrome (latest)
- ✅ Firefox (latest)
- ✅ Safari (latest)
- ✅ Edge (latest)

**Verify:**
- All pages render correctly
- Modals work properly
- Copy-to-clipboard works
- Date pickers function
- Form submissions work

---

## Performance Checks

### Page Load Times

**Target: < 1 second for each page**

- ✅ `/tenant-portal` - Dashboard
- ✅ `/tenant-portal/payment-instructions`
- ✅ `/tenant-portal/record-payment`
- ✅ `/tenant-portal/history`
- ✅ `/payments/pending`

### API Response Times

**Target: < 500ms for most endpoints**

Open browser DevTools → Network tab:

- ✅ `GET /TenantPayments/instructions` - Should be fast
- ✅ `POST /TenantPayments/record` - Should be quick
- ✅ `GET /TenantPayments/history` - Should load fast
- ✅ `GET /LandlordPayments/pending` - Should be fast
- ✅ `POST /LandlordPayments/{id}/confirm` - Should be quick

---

## Accessibility Testing

### Keyboard Navigation

- ✅ Tab through forms - all fields reachable
- ✅ Enter key submits forms
- ✅ Escape key closes modals
- ✅ Focus indicators visible

### Screen Reader Compatibility

- ✅ All images have alt text
- ✅ Buttons have descriptive labels
- ✅ Form fields have proper labels
- ✅ Error messages are announced

---

## Common Issues & Troubleshooting

### Issue: "Cannot read properties of null (reading 'role')"

**Solution:** Make sure you're logged in. Check AuthContext has user data.

### Issue: "404 Not Found" on API calls

**Solution:**
1. Verify backend is running on correct port
2. Check `lib/services/api.ts` for correct base URL
3. Ensure CORS is configured in backend

### Issue: Empty payment list in tenant portal

**Solution:**
1. Check if tenant has any payment records in database
2. Run database seed data: `dotnet run --seed` (if available)
3. Manually record a payment first

### Issue: Pending payments page shows nothing

**Solution:**
1. Submit a payment as tenant first
2. Payment must have status "Pending"
3. Check landlord has access to the property

### Issue: Copy to clipboard not working

**Solution:**
1. Must be on HTTPS or localhost
2. Browser must support Clipboard API
3. Check browser permissions

---

## Automated Testing (Future Enhancement)

### Unit Tests

Create tests for:
- ✅ tenantPaymentService methods
- ✅ paymentService confirm/reject methods
- ✅ React component rendering
- ✅ Form validation logic

### E2E Tests (Cypress/Playwright)

Create flows for:
- ✅ Tenant payment submission
- ✅ Landlord payment confirmation
- ✅ Landlord payment rejection
- ✅ Role-based navigation

---

## Sign-Off Checklist

Before considering the feature complete:

### Tenant Portal
- ✅ Dashboard loads with correct data
- ✅ Payment instructions display correctly
- ✅ Record payment form submits successfully
- ✅ Payment history shows all payments with filters
- ✅ Status badges are color-coded correctly

### Landlord Confirmation
- ✅ Pending payments page loads
- ✅ Confirm modal works
- ✅ Reject modal works with validation
- ✅ Payments update in real-time
- ✅ Tenant sees status changes

### Navigation
- ✅ Tenants see only tenant menu
- ✅ Landlords see full menu
- ✅ Login redirects work correctly
- ✅ Active menu items highlight properly

### General
- ✅ No console errors
- ✅ No TypeScript errors
- ✅ Responsive on all screen sizes
- ✅ Loading states work
- ✅ Error states handled gracefully

---

## Support & Documentation

- **Backend API Docs:** See `TESTING_GUIDE.md` for API endpoints
- **Type Definitions:** See `lib/types/tenantPayment.types.ts` and `lib/types/payment.types.ts`
- **Service Layer:** See `lib/services/tenantPaymentService.ts` and `lib/services/paymentService.ts`

---

**Last Updated:** 2025-12-04
**Version:** 1.0.0
