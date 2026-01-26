# COMPREHENSIVE SYSTEM AUDIT REPORT
## RentCollectionApp - Kenya Market Readiness and End-to-End Analysis

**Date:** January 25, 2026  
**Auditor:** Codex (AI Code Assistant)  
**Scope:** Backend, frontend, database, background jobs, security, payments, documents

---

## EXECUTIVE SUMMARY

### SYSTEM STATUS: 62% PRODUCTION READY

**Key Findings (Verified):**
- Core CRUD exists for properties, units, tenants, and payments; tenant isolation is present in several services but inconsistent across modules.
- M-Pesa STK push initiation and webhook handling exist; status query endpoints now exist with tenant and staff UI entry points.
- Notification and SMS sending now enforce role and tenant scoping, reducing cross-tenant messaging risk.
- Reports (arrears, occupancy, P&L) and owner statements exist; rent roll and receipt PDFs are now available.
- Bulk payment import now enforces tenant/org scoping and required payment fields but remains CSV-only.

**Primary Blockers (P1/P2):**
- Tenant isolation gaps remain in several modules (see matrix), especially where endpoints accept explicit IDs.
- Payment reconciliation still relies on manual workflows and limited status visibility.
- Webhook hardening (token/IP allowlist) is configuration-dependent and unverified in production.

---

## 0) PHASE 0 — ENVIRONMENT CHECK (READ-ONLY)

**dotnet --info:** SDK 8.0.417 installed; runtimes 6.0/8.0/10.0 present.  
**node --version:** v20.20.0  
**npm --version:** 10.8.2  
**TargetFramework mismatch:** None found (projects target net8.0; `global.json` pins 8.0.417).  
**Safest fix proposed:** Stay on .NET 8 LTS; no change required.

---

## 1) ARCHITECTURE AND STACK

**Backend:**
- Clean architecture layering with `src/RentCollection.Domain`, `src/RentCollection.Application`, `src/RentCollection.Infrastructure`, `src/RentCollection.API`.
- EF Core migrations in `src/RentCollection.Infrastructure/Migrations`.

**Frontend:**
- Next.js App Router under `src/RentCollection.WebApp/app`.

**Database:**
- EF Core code-first with configurations in `src/RentCollection.Infrastructure/Data/Configurations`.

**Authentication/RBAC:**
- JWT auth in `src/RentCollection.API/Program.cs`.
- Roles in `src/RentCollection.Domain/Enums/UserRole.cs`.
- 2FA endpoints in `src/RentCollection.API/Controllers/TwoFactorAuthController.cs`.

**Background Jobs:**
- Invoice generation: `src/RentCollection.Application/Services/InvoiceGenerationBackgroundService.cs`.
- Rent reminders: `src/RentCollection.Application/Services/RentReminderBackgroundService.cs`.
- M-Pesa reconciliation worker: `src/RentCollection.Application/Services/MpesaStkReconciliationBackgroundService.cs`.

**Integrations:**
- M-Pesa: `src/RentCollection.Infrastructure/Services/MPesaService.cs`, `src/RentCollection.API/Controllers/MPesaWebhookController.cs`.
- Africa's Talking SMS: `src/RentCollection.Infrastructure/Services/AfricasTalkingSmsService.cs`.
- Email: `src/RentCollection.Infrastructure/Services/EmailService.cs`.
- File storage: `src/RentCollection.Infrastructure/Services/LocalFileStorageService.cs`.
- PDF generation: `src/RentCollection.Infrastructure/Services/PdfGenerationService.cs`.

---

## 2) FEATURE VERIFICATION MATRIX

**Rule:** Implemented only if backend logic, API endpoint, frontend route, and authorization/tenant isolation are all verified.

| Feature | Status | Evidence | Risks/Notes |
|---|---|---|---|
| Authentication & Roles (Tenant, Landlord, Manager, Caretaker, Accountant, Admin) | Partial | `src/RentCollection.API/Controllers/AuthController.cs`, `src/RentCollection.API/Controllers/TwoFactorAuthController.cs`, `src/RentCollection.Domain/Enums/UserRole.cs`, `src/RentCollection.WebApp/app/(auth)` | Roles exist; RBAC enforcement is uneven outside auth flows. |
| Multi-tenancy & tenant isolation across ALL endpoints and UI | Partial | `src/RentCollection.API/Services/CurrentUserService.cs`, `src/RentCollection.Infrastructure/Services/PaymentService.cs`, `src/RentCollection.Infrastructure/Services/PropertyService.cs` | Gaps narrowed; remaining verification needed for endpoints that accept explicit IDs outside `/me`-style routes. |
| Properties CRUD + Units CRUD + occupancy | Partial | `src/RentCollection.API/Controllers/PropertiesController.cs`, `src/RentCollection.API/Controllers/UnitsController.cs`, `src/RentCollection.WebApp/app/properties`, `src/RentCollection.WebApp/app/units` | Occupancy inferred via unit/tenant links; full scoping verification incomplete. |
| Tenants CRUD + lease lifecycle + renewals | Partial | `src/RentCollection.API/Controllers/TenantsController.cs`, `src/RentCollection.API/Controllers/LeaseRenewalsController.cs`, `src/RentCollection.Infrastructure/Services/LeaseRenewalService.cs`, `src/RentCollection.WebApp/app/tenants` | Lease renewals exist; access/scoping requires full audit. |
| Tenant Portal (dashboard, lease info, documents, maintenance, payment history, settings) | Partial | `src/RentCollection.API/Controllers/TenantPortalController.cs`, `src/RentCollection.Infrastructure/Services/TenantPortalService.cs`, routes under `src/RentCollection.WebApp/app/tenant-portal` | Some features are data-only or lack full workflow (e.g., payment reconciliation). |
| Payments (STK push, callbacks, C2B, status, idempotency, fraud, manual, proof, pending flow, late fees, reconciliation) | Partial | `src/RentCollection.API/Controllers/TenantPaymentsController.cs`, `src/RentCollection.API/Controllers/MPesaWebhookController.cs`, `src/RentCollection.API/Controllers/PaymentsController.cs`, `src/RentCollection.API/Controllers/UnmatchedPaymentsController.cs`, UI `/tenant-portal/pay-now`, `/tenant-portal/pay-security-deposit`, `/tenant-portal/invoices/[id]`, `/dashboard/reconciliation` | Status query endpoints now exist with tenant/staff UI checks; DB idempotency constraint added; fraud controls and automated reconciliation remain partial; C2B/STK callbacks have no UI route. |
| Invoicing (entities, jobs, UI, allocation) | Partial | `src/RentCollection.Infrastructure/Services/InvoiceService.cs`, `src/RentCollection.Application/Services/InvoiceGenerationBackgroundService.cs`, `src/RentCollection.API/Controllers/InvoicesController.cs`, `src/RentCollection.WebApp/app/dashboard/invoices` | Idempotency and allocation coverage need verification. |
| Expenses (creation, categorization, attachments, approvals, reports) | Partial | `src/RentCollection.API/Controllers/ExpensesController.cs`, `src/RentCollection.Application/Services/ExpenseService.cs`, `src/RentCollection.WebApp/app/dashboard/expenses` | No approval workflow; attachments limited to `ReceiptUrl` (no upload endpoint). |
| Maintenance (ticket lifecycle, assignment, cost tracking, tenant visibility) | Partial | `src/RentCollection.API/Controllers/MaintenanceRequestsController.cs`, `src/RentCollection.Infrastructure/Services/MaintenanceRequestService.cs`, UI `/dashboard/maintenance`, `/tenant-portal/maintenance` | Workflow exists, but SLAs/approvals are limited. |
| Documents (upload, access control, verification, secure download) | Implemented | `src/RentCollection.API/Controllers/DocumentsController.cs`, `src/RentCollection.Infrastructure/Services/DocumentService.cs`, UI `/dashboard/documents`, `/tenant-portal/documents` | RBAC enforced and download endpoint present. |
| Inspections & Move-Out (checklists, photos, deductions, deposit settlement) | Partial | `src/RentCollection.API/Controllers/MoveOutInspectionsController.cs`, `src/RentCollection.Application/Services/MoveOutInspectionService.cs`, `src/RentCollection.API/Controllers/SecurityDepositsController.cs`, UI `/dashboard/move-out-inspections` | Settlement workflow exists but not fully end-to-end verified. |
| Utility Billing (meter readings, fixed/shared, billing logic) | Partial | `src/RentCollection.API/Controllers/MeterReadingsController.cs`, `src/RentCollection.Infrastructure/Services/UtilityBillingService.cs`, `src/RentCollection.Infrastructure/Services/UtilityConfigService.cs`, UI `/dashboard/utilities`, `/dashboard/meter-readings` | Automated billing job not present; configuration validation only. |
| Reports & PDFs (arrears, rent roll, occupancy, P&L, receipts, owner statements) | Implemented | `src/RentCollection.API/Controllers/ReportsController.cs`, `src/RentCollection.Infrastructure/Services/ReportsService.cs`, `src/RentCollection.Infrastructure/Services/PdfGenerationService.cs`, `src/RentCollection.API/Controllers/OwnerStatementsController.cs`, UI `/dashboard/reports`, `/dashboard/owner-statements`, `/payments/[id]` | Rent roll and receipt PDFs now available; verify report scope limits in production. |
| CSV Import & Export (bulk import, accounting exports) | Partial | `src/RentCollection.API/Controllers/BulkImportController.cs`, `src/RentCollection.Infrastructure/Services/BulkImportService.cs`, `src/RentCollection.API/Controllers/ExportsController.cs`, UI `/dashboard/bulk-import`, `/dashboard/exports` | Payments import now enforces tenant/org scoping and required fields; still CSV-only and lacks stronger validation feedback. |
| Logging & Observability (structured logs, audit logs, correlation IDs) | Partial | `src/RentCollection.API/Program.cs`, `src/RentCollection.Infrastructure/Services/AuditLogService.cs` | Correlation IDs are only added in M-Pesa webhooks. |
| Tests (unit, integration, money/security logic coverage) | Partial | `tests/RentCollection.UnitTests`, `tests/RentCollection.IntegrationTests` | Added coverage for STK idempotency, invoice generation idempotency, payment allocation, and tenant isolation; broader security/money logic still partial. |
| Deployment Readiness (config, secrets, HTTPS/CORS, webhooks, migrations) | Partial | `src/RentCollection.API/appsettings.json`, `src/RentCollection.API/Program.cs` | Placeholder secrets in config; webhook hardening needs production validation. |

---

## 3) SECURITY AUDIT

**Critical/High Findings:**
- None newly verified after remediation.

**Medium Findings:**
- **Webhook IP allowlist optional:** Empty allowlist allows any IP; token enforced only when configured. Evidence: `src/RentCollection.Infrastructure/Configuration/MPesaConfiguration.cs`, `src/RentCollection.API/Controllers/MPesaWebhookController.cs`.
- **Seed data uses real phone numbers:** Risk of accidental messaging in non-prod. Evidence: `src/RentCollection.Infrastructure/Data/ApplicationDbContextSeed.cs`.

**Low Findings:**
- Endpoints that accept tenant IDs require consistent `/me`-style scoping review (no proof of broad compromise yet).

**Resolved Since Previous Audit:**
- Bulk payment import now enforces tenant/org scoping and required payment fields. Evidence: `src/RentCollection.Infrastructure/Services/BulkImportService.cs`.
- Notification reminders now enforce tenant/landlord scoping. Evidence: `src/RentCollection.Infrastructure/Services/NotificationService.cs`.
- SMS endpoints are role-restricted. Evidence: `src/RentCollection.API/Controllers/SmsController.cs`.

---

## 4) DATA INTEGRITY AUDIT

**Risks:**
- Webhook idempotency and reconciliation are partial; no quarantine of invalid payloads beyond token/IP check. Evidence: `src/RentCollection.Infrastructure/Services/MPesaService.cs`.

**Resolved Since Previous Audit:**
- Payment import now sets required fields and enforces tenant/org scoping. Evidence: `src/RentCollection.Infrastructure/Services/BulkImportService.cs`.
- `Payment.TransactionReference` now has a unique filtered index. Evidence: `src/RentCollection.Infrastructure/Data/Configurations/PaymentConfiguration.cs`.

**Validation Coverage:**
- DTO validation exists for tenants and some payment inputs: `src/RentCollection.Application/Validators`.
- Phone normalization exists in SMS and MPesa services but is not centralized. Evidence: `src/RentCollection.Infrastructure/Services/AfricasTalkingSmsService.cs`, `src/RentCollection.Infrastructure/Services/MPesaService.cs`.

---

## 5) KENYA MARKET READINESS

**M-Pesa:**
- STK Push initiation: `src/RentCollection.Infrastructure/Services/MPesaService.cs`, `POST /api/tenantpayments/stk-push`.
- C2B validation/confirmation, STK callback, B2C callbacks: `src/RentCollection.API/Controllers/MPesaWebhookController.cs`.
- Status query endpoints exist: `GET /api/tenantpayments/stk-status`, `GET /api/payments/stk-status`.  
- Webhook token and optional IP allowlist configured; production enforcement depends on configuration.

**SMS:**
- Africa's Talking integration present: `src/RentCollection.Infrastructure/Services/AfricasTalkingSmsService.cs`.
- Sender ID configuration not verified (Unknown).

**KES formatting & phone normalization:**
- KES used in PDF rendering and templates. Evidence: `src/RentCollection.Infrastructure/Services/PdfGenerationService.cs`.
- Phone normalization exists in MPesa and SMS services. Evidence: `src/RentCollection.Infrastructure/Services/MPesaService.cs`, `src/RentCollection.Infrastructure/Services/AfricasTalkingSmsService.cs`.

**Caretaker workflows:**
- Caretaker role exists and is supported in RBAC on multiple endpoints, but tenant isolation is uneven. Evidence: `src/RentCollection.Domain/Enums/UserRole.cs`.

---

## 6) COMPETITIVE BENCHMARK (PHASE 1G)

### Competitive Feature Matrix (Yes / Partial / No / Unknown)

| Feature Category | RentCollectionApp | AppFolio | Buildium | Innago | Apartments.com | Kenya Platform (Bomahut) |
|---|---|---|---|---|---|---|
| Authentication & Roles | Partial | Unknown | Unknown | Unknown | Unknown | Unknown |
| Multi-tenancy & tenant isolation | Partial | Unknown | Unknown | Unknown | Unknown | Unknown |
| Properties & Units | Partial | Unknown | Unknown | Unknown | Unknown | Unknown |
| Tenants & Leases | Partial | Partial | Partial | Partial | Unknown | Unknown |
| Tenant Portal | Partial | Unknown | Unknown | Unknown | Unknown | Unknown |
| Payments | Partial | Unknown | Yes | Partial | Unknown | Yes |
| Invoicing | Partial | Unknown | Unknown | Unknown | Unknown | Yes |
| Expenses | Partial | Unknown | Unknown | Unknown | Unknown | Unknown |
| Maintenance | Partial | Unknown | Yes | Unknown | Unknown | Unknown |
| Documents | Implemented | Unknown | Unknown | Unknown | Unknown | Unknown |
| Inspections & Move-Out | Partial | Unknown | Unknown | Unknown | Unknown | Unknown |
| Utility Billing | Partial | Unknown | Unknown | Unknown | Unknown | Partial |
| Reports & PDFs | Partial | Partial | Unknown | Unknown | Unknown | Partial |
| CSV Import/Export | Partial | Unknown | Unknown | Unknown | Unknown | Unknown |
| Logging/Observability | Partial | Unknown | Unknown | Unknown | Unknown | Unknown |
| Tests | Partial | Unknown | Unknown | Unknown | Unknown | Unknown |
| Deployment Readiness | Partial | Unknown | Unknown | Unknown | Unknown | Unknown |

**Evidence Notes (public docs only):**
- AppFolio: FAQ mentions tenant management, accounting, reporting, analytics. Source: https://www.appfolio.com/property-management-software
- Buildium: feature links for accounting/payments, online rent payments, maintenance requests, online leasing, tenant screening, owner portal. Source: https://www.buildium.com/features/
- Innago: homepage describes collecting rent, signing leases, managing tenants online. Source: https://innago.com/
- Apartments.com: access blocked (HTTP 403). Source: https://www.apartments.com/rental-manager/
- Bomahut: homepage lists automated billing/receipts, integrated payments, arrears reports, meter-based invoices. Source: https://www.bomahut.com/

**Positioning Summary:**
- **Stronger:** Document RBAC/download and tenant portal coverage relative to early-stage Kenya platforms.
- **Weaker:** Payment idempotency, reconciliation, and tenant isolation consistency compared to mature platforms.
- **Strategic gaps:** End-to-end payments (status query + idempotency), operational reporting, and multi-tenant enforcement.

**Market Position Statement:**
- Direct competitors: Bomahut and other Kenya-first property management platforms.
- Non-direct competitors: AppFolio and Buildium (enterprise scale and US-centric features).
- Positioning: Africa-first opportunity exists if payments, SMS, and reconciliation are hardened.

---

## 7) ARCHITECTURAL JUDGMENT & RECOMMENDATIONS (PHASE 1H)

### 1. Enforce tenant scoping in bulk imports and notifications
- **Category:** Security
- **Evidence:** `src/RentCollection.Infrastructure/Services/BulkImportService.cs`, `src/RentCollection.Infrastructure/Services/NotificationService.cs`
- **Recommendation:** Implemented for bulk imports and notification sends; continue auditing other ID-based endpoints for consistent `/me` scoping.
- **Priority:** P2
- **Expected Impact:** Sustains tenant isolation and prevents regressions.
- **Effort:** S
- **Required for investors/sale:** Yes

### 2. Add database-level idempotency for payments
- **Category:** Data
- **Evidence:** `src/RentCollection.Infrastructure/Data/Configurations/PaymentConfiguration.cs`
- **Recommendation:** Implemented via unique filtered index; monitor for migration errors in existing data.
- **Priority:** P2
- **Expected Impact:** Prevents duplicate posting from webhooks.
- **Effort:** S
- **Required for investors/sale:** Yes

### 3. Close the M-Pesa lifecycle (status queries + reconciliation)
- **Category:** Payments
- **Evidence:** `src/RentCollection.Infrastructure/Services/MPesaService.cs`, `src/RentCollection.API/Controllers/TenantPaymentsController.cs`, `src/RentCollection.API/Controllers/PaymentsController.cs`
- **Recommendation:** Status query endpoints now exist; complete reconciliation UI and surface status checks in the tenant/staff dashboards.
- **Priority:** P1
- **Expected Impact:** Reduces unresolved payments and manual reconciliation.
- **Effort:** M
- **Required for investors/sale:** Yes

### 4. Tighten SMS sending permissions
- **Category:** Security
- **Evidence:** `src/RentCollection.API/Controllers/SmsController.cs`
- **Recommendation:** Implemented role restriction; add recipient scoping for SMS templates if custom sends are expanded.
- **Priority:** P2
- **Expected Impact:** Prevents abuse and compliance risk.
- **Effort:** S
- **Required for investors/sale:** Yes

### 5. Strengthen observability with correlation IDs
- **Category:** Operations
- **Evidence:** Correlation ID only in `MPesaWebhookController`
- **Recommendation:** Add global correlation ID middleware and structured logging scopes for all requests.
- **Priority:** P2
- **Expected Impact:** Improves incident response and payment traceability.
- **Effort:** M
- **Required for investors/sale:** No

---

## 8) PRIORITIZED GAP CLOSURE PLAN

| Priority | Issue | Effort | Risk | Impacted Modules |
|---|---|---|---|---|
| P2 | Global correlation IDs + structured logs | M | Low | API |
| P2 | Harden webhook validation (IP allowlist enforcement) | M | Medium | API, Infrastructure |
| P2 | Automated reconciliation workflows | M | Medium | API, WebApp |

---

## 9) CHANGES SINCE PREVIOUS AUDIT

- Implemented tenant/org scoping and required payment fields in bulk payment import. Evidence: `src/RentCollection.Infrastructure/Services/BulkImportService.cs`.
- Enforced tenant/landlord scoping for notification sending and restricted SMS endpoints to staff roles. Evidence: `src/RentCollection.Infrastructure/Services/NotificationService.cs`, `src/RentCollection.API/Controllers/SmsController.cs`.
- Added unique filtered index for `Payment.TransactionReference` with migration `20260125181334_AddUniqueTransactionReference.cs`.
- Added M-Pesa STK status query endpoints for tenant and staff APIs. Evidence: `src/RentCollection.API/Controllers/TenantPaymentsController.cs`, `src/RentCollection.API/Controllers/PaymentsController.cs`.
- Added tests for STK callback idempotency, invoice generation idempotency, payment allocation across invoices, and tenant isolation. Evidence: `tests/RentCollection.IntegrationTests/Payments/MPesaWebhookTests.cs`, `tests/RentCollection.IntegrationTests/Invoices/InvoiceGenerationTests.cs`, `tests/RentCollection.UnitTests/Payments/PaymentAllocationServiceTests.cs`, `tests/RentCollection.IntegrationTests/Payments/TenantIsolationTests.cs`.
- Added rent roll, tenant list, monthly report, and payment receipt PDF endpoints with UI links. Evidence: `src/RentCollection.API/Controllers/ReportsController.cs`, `src/RentCollection.Infrastructure/Services/PdfGenerationService.cs`, `src/RentCollection.WebApp/components/reports/PropertyReport.tsx`, `src/RentCollection.WebApp/lib/services/reportService.ts`, `src/RentCollection.WebApp/lib/services/paymentService.ts`.
- Added tenant and staff STK status checks in the tenant portal and reconciliation workflows. Evidence: `src/RentCollection.WebApp/app/tenant-portal/pay-now/page.tsx`, `src/RentCollection.WebApp/app/tenant-portal/pay-security-deposit/page.tsx`, `src/RentCollection.WebApp/app/tenant-portal/invoices/[id]/page.tsx`, `src/RentCollection.WebApp/app/dashboard/reconciliation/page.tsx`.

---

## 10) HOW TO TEST (CURRENT STATE)

1) Run tests: `dotnet test`
2) Validate tenant portal payments: `POST /api/tenantpayments/stk-push`, `GET /api/tenantpayments/history`, `GET /api/tenantpayments/stk-status?checkoutRequestId=...`
3) Validate M-Pesa callbacks: `POST /api/mpesa/c2b/validation`, `/api/mpesa/c2b/confirmation`, `/api/mpesa/stkpush/callback`
4) Validate staff payment queries: `GET /api/payments/stk-status?checkoutRequestId=...`
5) Verify document access control: `GET /api/documents/{id}` and `GET /api/documents/{id}/download`
6) Verify report endpoints: `GET /api/reports/arrears`, `GET /api/reports/occupancy`, `GET /api/reports/profit-loss`
7) Verify PDF endpoints: `GET /api/reports/monthly-report/{year}/{month}`, `GET /api/reports/tenant-list`, `GET /api/reports/rent-roll`, `GET /api/reports/payment-receipt/{paymentId}`

---

**Prepared by:** Codex (AI Code Assistant)  
**Review Date:** January 25, 2026  
**Next Review:** February 25, 2026
