# SYSTEM REQUIREMENTS SPECIFICATION
## RentPro - Enterprise Property Management System

**Version:** 2.0
**Date:** December 17, 2025
**Target Market:** Kenya & East Africa (Primary), Global (Secondary)
**Competitive Benchmark:** Landlord Studio, Buildium, AppFolio

---

## TABLE OF CONTENTS
1. [Executive Overview](#executive-overview)
2. [Functional Requirements](#functional-requirements)
3. [Technical Requirements](#technical-requirements)
4. [Non-Functional Requirements](#non-functional-requirements)
5. [Integration Requirements](#integration-requirements)
6. [Security Requirements](#security-requirements)
7. [Performance Requirements](#performance-requirements)
8. [Compliance Requirements](#compliance-requirements)

---

## EXECUTIVE OVERVIEW

### System Vision
RentPro aims to be East Africa's leading property management platform, offering feature parity with global competitors (Landlord Studio, Buildium) while optimizing for the Kenyan market with M-Pesa integration and local compliance.

### Target Users
- **Primary**: Landlords managing 1-50 properties in Kenya
- **Secondary**: Property management companies
- **Tertiary**: Accountants, caretakers, tenants

### Core Value Propositions
1. **Kenya-First Design**: M-Pesa STK Push, SMS notifications, local payment methods
2. **Automation**: Reduce manual work by 70% through automated rent collection, reminders, and receipts
3. **Financial Intelligence**: Real-time cash flow, P&L statements, tax-ready reports
4. **Tenant Experience**: Self-service portal, instant payments, digital lease management

---

## FUNCTIONAL REQUIREMENTS

### FR-1: PROPERTY & UNIT MANAGEMENT

#### FR-1.1: Multi-Property Portfolio Management ✅ IMPLEMENTED
**Status:** Complete
**Priority:** P0 (Critical)

**Requirements:**
- [x] Add unlimited properties with details (name, location, type, description)
- [x] Upload multiple property images
- [x] Track property-level statistics (units, occupancy, revenue)
- [x] Grid and list view modes
- [x] Search and filter properties
- [x] Property-specific payment accounts

**Enhancement Needed:**
- [ ] Property performance comparison (revenue, occupancy trends)
- [ ] ROI calculation per property
- [ ] Property expense tracking and categorization
- [ ] Property valuation estimates

---

#### FR-1.2: Unit Management ✅ IMPLEMENTED
**Status:** Complete
**Priority:** P0 (Critical)

**Requirements:**
- [x] Create units with number, type, bedrooms, bathrooms, size
- [x] Set monthly rent per unit
- [x] Track unit status (Vacant, Occupied, Maintenance)
- [x] Amenities tracking (WiFi, Parking, Power Backup, Water, AC, Gas, Security)
- [x] Multiple unit images
- [x] Public vacancy listings

**Enhancement Needed:**
- [ ] Unit history (previous tenants, maintenance records)
- [ ] Rental price suggestions based on market data
- [ ] Virtual tours / 360° photos
- [ ] Floor plan uploads

---

### FR-2: TENANT MANAGEMENT

#### FR-2.1: Tenant Screening & Application Processing ❌ MISSING
**Status:** Not Implemented
**Priority:** P0 (Critical)
**Competitive Gap:** Landlord Studio has TransUnion integration

**Requirements:**
- [ ] **Online Application Forms**
  - Personal information (name, ID, contacts)
  - Employment details (employer, income, references)
  - Rental history (previous landlord contacts)
  - Upload ID/passport, employment letter, bank statements

- [ ] **Credit & Background Checks** (Phase 2)
  - Integration with credit bureaus (CRB Kenya)
  - Criminal background check API
  - Eviction history search
  - Reference verification workflow

- [ ] **Application Review Workflow**
  - Application status tracking (Submitted, Under Review, Approved, Rejected)
  - Landlord review dashboard
  - Automated scoring/ranking of applications
  - Email notifications at each stage

**Implementation Notes:**
- Start with manual review, add automated screening in Phase 2
- Integrate with Metropol CRB (Kenya's largest credit bureau)

---

#### FR-2.2: Tenant Onboarding ✅ IMPLEMENTED
**Status:** Complete
**Priority:** P0 (Critical)

**Requirements:**
- [x] Create tenant profile (name, email, phone, ID number)
- [x] Assign to unit
- [x] Set lease terms (start date, end date, monthly rent)
- [x] Configure security deposit
- [x] Set rent due day and late fee policy
- [x] Upload documents (ID copy, employment letter)

**Enhancement Needed:**
- [ ] Digital lease signing (e-signature)
- [ ] Move-in checklist with photos
- [ ] Initial unit condition report with tenant signature
- [ ] Automated welcome email with portal instructions

---

#### FR-2.3: Tenant Portal ✅ IMPLEMENTED
**Status:** Complete
**Priority:** P0 (Critical)

**Current Features:**
- [x] Dashboard with rent status and payment history
- [x] View lease information
- [x] Get payment instructions (M-Pesa Paybill, bank details)
- [x] Record payments made
- [x] Pay rent via M-Pesa STK Push
- [x] Pay security deposit via M-Pesa STK Push
- [x] Submit maintenance requests with photos
- [x] View/download documents
- [x] Respond to lease renewal offers
- [x] View payment history

**Enhancement Needed:**
- [ ] Auto-pay enrollment (recurring M-Pesa)
- [ ] Payment reminders (7 days, 3 days, day of, 1 day overdue)
- [ ] Split payment capability (pay rent in installments)
- [ ] Roommate management (split rent between multiple tenants)

---

### FR-3: PAYMENT & FINANCIAL MANAGEMENT

#### FR-3.1: Rent Collection ✅ PARTIAL
**Status:** 70% Complete
**Priority:** P0 (Critical)

**Current Implementation:**
- [x] M-Pesa STK Push for instant payment
- [x] Manual payment recording (M-Pesa, bank, cash)
- [x] Payment confirmation workflow for landlord
- [x] Late fee calculation (auto-calculated based on policy)
- [x] Payment history tracking
- [x] SMS notifications to tenants
- [x] PDF receipt generation

**Missing Critical Features:**
- [ ] **Automated Rent Reminders**
  - 7 days before due date
  - 3 days before due date
  - On due date
  - 1 day after due date (overdue notice)
  - Configurable reminder schedule per landlord

- [ ] **Auto-Pay / Recurring Payments**
  - Tenant enrolls in auto-pay
  - System automatically charges M-Pesa on due date
  - Fallback to manual reminder if auto-pay fails

- [ ] **Late Fees Automation**
  - Currently calculated but not auto-charged
  - Should auto-apply late fees after grace period
  - Send notification when late fee applied

- [ ] **Payment Plans**
  - Allow tenants to request payment plans
  - Split rent into multiple payments
  - Track payment plan compliance

**Implementation Priority:** Phase 1 (Q1 2026)

---

#### FR-3.2: Security Deposit Management ✅ PARTIAL
**Status:** 60% Complete
**Priority:** P1 (High)

**Current Implementation:**
- [x] Record initial security deposit amount
- [x] Track deposit balance
- [x] Tenant can view deposit balance
- [x] Tenant can pay deposit via M-Pesa STK Push
- [x] Record deductions and refunds
- [x] View transaction history

**Missing Features:**
- [ ] **Move-Out Inspection Workflow**
  - Schedule move-out inspection
  - Upload condition photos
  - Compare to move-in condition report
  - Calculate deductions automatically

- [ ] **Deduction Itemization**
  - Link deductions to specific damages/repairs
  - Attach photos as evidence
  - Generate itemized deduction report for tenant

- [ ] **Refund Processing**
  - Calculate refund amount (deposit - deductions)
  - Process M-Pesa refund or bank transfer
  - Track refund status
  - Generate refund receipt

**Implementation Priority:** Phase 2 (Q2 2026)

---

#### FR-3.3: Expense Tracking ❌ MISSING
**Status:** Not Implemented
**Priority:** P1 (High)
**Competitive Gap:** Landlord Studio has full expense management

**Requirements:**
- [ ] **Expense Recording**
  - Manual expense entry (category, amount, date, property, unit)
  - Receipt photo upload
  - Vendor/contractor assignment
  - Recurring expense setup (utilities, insurance)

- [ ] **Receipt OCR** (Phase 2)
  - Photo receipt → auto-extract amount, date, vendor
  - AI-powered categorization
  - Review and approve suggestions

- [ ] **Expense Categories**
  - Predefined categories (Repairs, Maintenance, Utilities, Insurance, Property Tax, Management Fees, Advertising, Legal Fees)
  - Custom category creation
  - Schedule E tax code mapping (US) / KRA T12 mapping (Kenya)

- [ ] **Bank Feed Integration** (Phase 3)
  - Connect bank accounts (via Plaid or similar)
  - Auto-import transactions
  - Smart categorization rules
  - Match expenses to properties

**Implementation Priority:** Phase 2 (Q2 2026)

---

#### FR-3.4: Financial Reporting ✅ PARTIAL
**Status:** 40% Complete
**Priority:** P1 (High)

**Current Implementation:**
- [x] Monthly rent collection report (PDF)
- [x] Tenant directory list (PDF)
- [x] Dashboard statistics (revenue, occupancy, collection rate)

**Missing Critical Reports:**
- [ ] **Profit & Loss (P&L) Statement**
  - Income: Rent, late fees, other income
  - Expenses: Categorized expenses
  - Net income calculation
  - Month-over-month comparison
  - Year-to-date summary
  - Export to Excel/PDF

- [ ] **Cash Flow Statement**
  - 12-month cash flow graph
  - Income vs expenses visualization
  - Trend analysis
  - Forecasting (next 3 months)

- [ ] **Expense Breakdown Report**
  - Pie chart by category
  - Bar chart by property
  - Top expense categories
  - Year-over-year comparison

- [ ] **Tax Reports**
  - Schedule E report (US landlords)
  - KRA rental income report (Kenya)
  - Deductible expenses summary
  - Depreciation calculations

- [ ] **Occupancy Report**
  - Vacancy rate by property
  - Average days vacant
  - Turnover rate
  - Lost revenue from vacancies

- [ ] **Rent Roll Report**
  - All units with tenant names
  - Rent amounts and due dates
  - Lease start/end dates
  - Security deposit amounts
  - Export to Excel

**Implementation Priority:** Phase 1 (Q1 2026) - P&L and Cash Flow

---

### FR-4: MAINTENANCE MANAGEMENT

#### FR-4.1: Maintenance Request System ✅ IMPLEMENTED
**Status:** Complete
**Priority:** P1 (High)

**Current Features:**
- [x] Tenant submits requests from portal
- [x] Upload photos of issue
- [x] Priority levels (Low, Medium, High, Emergency)
- [x] Status tracking (Pending, Assigned, In Progress, Completed, Cancelled)
- [x] Assign to caretaker
- [x] Track completion cost
- [x] Landlord dashboard to manage all requests

**Enhancement Needed:**
- [ ] **Contractor Management**
  - Contractor database (name, specialty, contact, rate)
  - Assign requests to external contractors
  - Track contractor performance
  - Contractor rating system

- [ ] **Job Export to PDF**
  - Generate job sheet with photos, description, property details
  - Share with contractor via email/SMS

- [ ] **Preventive Maintenance**
  - Schedule recurring maintenance (HVAC servicing, pest control)
  - Automated reminders for scheduled tasks
  - Maintenance calendar view

- [ ] **Expense Linking**
  - Link maintenance costs to expense tracking
  - Track ROI on repairs
  - Categorize maintenance expenses

**Implementation Priority:** Phase 2 (Q2 2026)

---

### FR-5: LEASE MANAGEMENT

#### FR-5.1: Lease Tracking ✅ PARTIAL
**Status:** 50% Complete
**Priority:** P1 (High)

**Current Implementation:**
- [x] Store lease start/end dates
- [x] Track lease status
- [x] Lease renewal offers
- [x] Tenant can view lease info
- [x] Upload lease documents

**Missing Features:**
- [ ] **Lease Templates**
  - Residential lease template (standard Kenya format)
  - Commercial lease template
  - Month-to-month agreement template
  - Customizable templates per landlord
  - Auto-populate with tenant/property data

- [ ] **Digital Lease Signing**
  - E-signature integration (Docusign, SignNow, or HelloSign)
  - Tenant signs lease digitally
  - Landlord signs lease digitally
  - Witness signatures (if required)
  - Signed document stored automatically

- [ ] **Lease Expiration Alerts**
  - Email to landlord 90 days before expiration
  - Email to landlord 60 days before expiration
  - Email to landlord 30 days before expiration
  - SMS reminder at 30 days
  - Dashboard alert for expiring leases

- [ ] **Rent Increase Notices**
  - Generate rent increase notice (compliant with local laws)
  - Send via email/SMS
  - Track tenant response
  - Update rent automatically on approval

**Implementation Priority:** Phase 1 (Q1 2026) - Templates & Alerts

---

### FR-6: COMMUNICATION & NOTIFICATIONS

#### FR-6.1: SMS Notifications ✅ PARTIAL
**Status:** 60% Complete
**Priority:** P1 (High)

**Current Implementation:**
- [x] SMS sent when payment recorded
- [x] SMS receipt with M-Pesa reference
- [x] Africa's Talking integration

**Missing Features:**
- [ ] **Automated Rent Reminders**
  - Configurable reminder schedule
  - Personalized message templates
  - Stop reminders after payment

- [ ] **Bulk SMS**
  - Send to all tenants
  - Send to tenants in specific property
  - Send to overdue tenants
  - SMS templates library

- [ ] **Notification Preferences**
  - Tenant opts in/out of SMS
  - Email vs SMS preference
  - Frequency settings

**Implementation Priority:** Phase 1 (Q1 2026)

---

#### FR-6.2: Email Notifications ❌ MISSING
**Status:** Not Implemented
**Priority:** P1 (High)

**Requirements:**
- [ ] **Transactional Emails**
  - Payment confirmation
  - Payment receipt attached
  - Lease expiration notices
  - Maintenance request updates

- [ ] **Email Templates**
  - Welcome email for new tenants
  - Rent reminder emails
  - Late rent notice
  - Lease renewal offer
  - Move-out instructions
  - Security deposit refund notice

- [ ] **Email Customization**
  - Landlord can edit templates
  - Add logo and branding
  - Custom footer with contact info

**Implementation Priority:** Phase 1 (Q1 2026)

---

### FR-7: DOCUMENT MANAGEMENT

#### FR-7.1: Document Storage ✅ IMPLEMENTED
**Status:** Complete
**Priority:** P1 (High)

**Current Features:**
- [x] Upload documents per tenant
- [x] Upload documents per property
- [x] Categorize documents (Lease Agreement, ID Copy, etc.)
- [x] Download documents
- [x] View document list

**Enhancement Needed:**
- [ ] **Document Templates Library**
  - Notice to enter property
  - Smoking policy notice
  - Pet policy violation
  - Lease violation notice
  - Intent to sell property
  - Rent increase notice
  - Move-out checklist
  - Security deposit deduction itemization

- [ ] **Document Generation**
  - Generate documents from templates
  - Auto-fill with tenant/property data
  - Save generated documents automatically

- [ ] **Certificate Tracking**
  - Safety certificates (electrical, gas, fire)
  - Upload certificate with expiry date
  - Automated expiry reminders

**Implementation Priority:** Phase 2 (Q2 2026)

---

### FR-8: LISTING & MARKETING

#### FR-8.1: Public Vacancy Listings ✅ IMPLEMENTED
**Status:** Complete
**Priority:** P1 (High)

**Current Features:**
- [x] Public listing page for vacant units
- [x] Unit details, images, amenities
- [x] Filter by property, rent range
- [x] Tenant application form

**Enhancement Needed:**
- [ ] **Multi-Platform Syndication** ❌ CRITICAL MISSING FEATURE
  - One-click publish to:
    - BuyRentKenya.com
    - Property24 Kenya
    - Jiji.co.ke
    - PigiaMe
    - Facebook Marketplace
    - Property Kenya (YouTube SEO)
  - Track listing performance (views, inquiries per platform)

- [ ] **Advanced Listing Features**
  - Virtual tours / video walk-throughs
  - 360° photos
  - Neighborhood highlights (schools, hospitals, malls nearby)
  - Map integration (Google Maps embed)
  - Share listing via WhatsApp/SMS

- [ ] **Lead Management**
  - Track inquiries from each listing
  - Automated response templates
  - Schedule viewing appointments
  - Convert inquiry to application

**Implementation Priority:** Phase 2 (Q2 2026) - Syndication is critical

---

### FR-9: ANALYTICS & INSIGHTS

#### FR-9.1: Dashboard Analytics ✅ PARTIAL
**Status:** 50% Complete
**Priority:** P1 (High)

**Current Implementation:**
- [x] Total properties, units, tenants
- [x] Occupancy percentage
- [x] Revenue total
- [x] Collection rate
- [x] Payment status breakdown (Paid, Pending, Overdue)

**Missing Features:**
- [ ] **Trend Analysis**
  - 12-month revenue trend (line graph)
  - Occupancy rate over time
  - Collection rate trends
  - Expense trends

- [ ] **Property Comparison**
  - Compare properties by revenue
  - Compare by occupancy rate
  - Compare by maintenance costs
  - Identify top/bottom performers

- [ ] **Forecasting**
  - Expected revenue next 3 months
  - Occupancy projections
  - Cash flow forecast

- [ ] **ROI Analysis**
  - Property purchase price input
  - Calculate net income
  - ROI percentage
  - Cash-on-cash return
  - Cap rate calculation

**Implementation Priority:** Phase 2 (Q2 2026)

---

## TECHNICAL REQUIREMENTS

### TR-1: BACKEND ARCHITECTURE

#### TR-1.1: Technology Stack ✅ IMPLEMENTED
- [x] .NET 8 (ASP.NET Core Web API)
- [x] Entity Framework Core 8
- [x] SQL Server (Azure SQL Database ready)
- [x] Clean Architecture (Domain, Application, Infrastructure, API layers)
- [x] CQRS pattern (optional, not fully implemented)
- [x] Repository pattern
- [x] Dependency injection

**Enhancement Needed:**
- [ ] Implement CQRS for complex queries (reports)
- [ ] Add MediatR for command/query handling
- [ ] Implement Unit of Work pattern
- [ ] Add Redis caching for frequently accessed data

---

#### TR-1.2: API Design ✅ IMPLEMENTED
- [x] RESTful API
- [x] JWT authentication
- [x] Role-based authorization
- [x] Swagger/OpenAPI documentation
- [x] CORS enabled
- [x] API versioning (v1)

**Enhancement Needed:**
- [ ] Rate limiting to prevent abuse
- [ ] API response caching (Redis)
- [ ] GraphQL endpoint for complex queries (optional)
- [ ] Webhook support for external integrations

---

### TR-2: FRONTEND ARCHITECTURE

#### TR-2.1: Technology Stack ✅ IMPLEMENTED
- [x] Next.js 15 (React 18)
- [x] TypeScript
- [x] Tailwind CSS
- [x] React Hook Form
- [x] Axios for API calls
- [x] JWT token management
- [x] Context API for state management

**Enhancement Needed:**
- [ ] Add Zustand or Redux for complex state
- [ ] Implement React Query for server state management
- [ ] Add Chart.js or Recharts for graphs
- [ ] PWA support (offline mode)
- [ ] Web push notifications

---

#### TR-2.2: UI/UX Requirements
**Current Status:** Good foundation, needs enhancements

**Required Improvements:**
- [ ] **Interactive Charts**
  - Line charts for revenue trends
  - Pie charts for expense breakdown
  - Bar charts for property comparison
  - Use Recharts or Chart.js

- [ ] **Calendar Views**
  - Maintenance schedule calendar
  - Lease expiration calendar
  - Payment due dates calendar
  - Use React Big Calendar or FullCalendar

- [ ] **Drag-and-Drop**
  - File uploads
  - Task reordering
  - Use React DnD

- [ ] **Real-Time Updates**
  - WebSocket for live payment notifications
  - Live dashboard updates
  - SignalR integration

- [ ] **Mobile Optimization**
  - Responsive design (already good)
  - Touch-friendly UI
  - Mobile-first navigation

- [ ] **Accessibility**
  - ARIA labels
  - Keyboard navigation
  - Screen reader support
  - WCAG 2.1 AA compliance

---

### TR-3: DATABASE DESIGN

#### TR-3.1: Current Schema ✅ IMPLEMENTED
**Core Tables:**
- Users
- Properties
- Units
- Tenants
- Payments
- LandlordPaymentAccounts
- MaintenanceRequests
- Documents
- LeaseRenewals
- SecurityDepositTransactions
- Notifications

**Enhancement Needed:**
- [ ] **New Tables Required:**
  - Expenses (amount, category, date, property, receipt_url)
  - ExpenseCategories (name, tax_code, parent_category)
  - Contractors (name, specialty, contact, rate, rating)
  - LeaseTemplates (name, content, variables)
  - TenantApplications (form data, screening results)
  - RentReminders (schedule, last_sent, next_send_date)
  - ListingSyndication (platform, listing_id, views, inquiries)
  - PropertyInspections (type, date, photos, findings)

- [ ] **Indexing Strategy**
  - Add indexes on foreign keys
  - Composite indexes on frequently queried columns
  - Full-text search indexes on property names, tenant names

- [ ] **Archival Strategy**
  - Move old payments to archive table after 2 years
  - Soft delete for tenants (keep history)
  - Audit log for all financial transactions

---

### TR-4: INTEGRATION REQUIREMENTS

#### TR-4.1: Payment Gateways ✅ PARTIAL
**Current Implementation:**
- [x] M-Pesa Daraja API (STK Push, B2C, Callbacks)

**Required Additions:**
- [ ] **Card Payments**
  - Stripe integration (international)
  - Flutterwave (Kenya, Africa)
  - PayPal (optional)

- [ ] **Bank Transfers**
  - Pesalink integration (Kenya)
  - RTGS/EFT tracking
  - Bank reconciliation

**Implementation Priority:** Phase 2 (Q2 2026)

---

#### TR-4.2: SMS/Email Providers ✅ PARTIAL
**Current Implementation:**
- [x] Africa's Talking SMS

**Required Additions:**
- [ ] **Email Service**
  - SendGrid integration (recommended)
  - Or AWS SES
  - Or Mailgun
  - Email template management

- [ ] **WhatsApp Business API** (Phase 3)
  - Send rent reminders via WhatsApp
  - Maintenance updates
  - Requires Meta Business verification

---

#### TR-4.3: Document Signing
**Status:** Not Implemented
**Priority:** P1 (High)

**Options:**
- [ ] DocuSign (most popular, expensive)
- [ ] HelloSign / Dropbox Sign (good balance)
- [ ] SignNow (affordable)
- [ ] Adobe Sign (enterprise)

**Implementation:** Phase 2 (Q2 2026)

---

#### TR-4.4: Tenant Screening
**Status:** Not Implemented
**Priority:** P1 (High)

**Kenya Options:**
- [ ] Metropol CRB (Credit Reference Bureau)
- [ ] TransUnion Kenya
- [ ] CreditInfo Kenya

**International Options:**
- [ ] TransUnion (US, global)
- [ ] Checkr (background checks)

**Implementation:** Phase 2 (Q2 2026)

---

#### TR-4.5: Accounting Software Integration
**Status:** Not Implemented
**Priority:** P2 (Medium)

**Target Integrations:**
- [ ] QuickBooks (most popular globally)
- [ ] Xero (popular in UK, AU, gaining in Kenya)
- [ ] Sage (enterprise)
- [ ] Wave (free, small landlords)

**Features:**
- Sync income/expenses
- Auto-categorization
- Two-way sync (changes reflect in both systems)

**Implementation:** Phase 3 (Q3 2026)

---

#### TR-4.6: Bank Feed Integration
**Status:** Not Implemented
**Priority:** P2 (Medium)

**Options:**
- [ ] Plaid (US, popular but expensive)
- [ ] Yodlee (enterprise)
- [ ] TrueLayer (UK, Europe)
- [ ] Open Banking APIs (manual integration per bank)

**Kenya-Specific:**
- [ ] Equity Bank API
- [ ] KCB API
- [ ] Co-operative Bank API
- [ ] M-Pesa Business API (statement downloads)

**Implementation:** Phase 3 (Q3 2026)

---

### TR-5: MOBILE APPLICATIONS

#### TR-5.1: Mobile App Requirements ❌ MISSING
**Status:** Not Implemented
**Priority:** P2 (Medium)
**Competitive Necessity:** Landlord Studio has excellent mobile apps

**Approach Options:**
1. **React Native** (Recommended)
   - Share code with Next.js (both use React)
   - Single codebase for iOS + Android
   - Faster development

2. **Native (Swift + Kotlin)**
   - Best performance
   - Platform-specific UX
   - Higher development cost

**Core Mobile Features:**
- [ ] Dashboard (stats, alerts)
- [ ] Payment notifications
- [ ] Record payment on-the-go
- [ ] Maintenance request submission
- [ ] Receipt scanning (OCR)
- [ ] Push notifications
- [ ] Offline mode (view cached data)

**Implementation:** Phase 3 (Q3 2026)

---

## NON-FUNCTIONAL REQUIREMENTS

### NFR-1: PERFORMANCE

#### NFR-1.1: Response Times
- API response time: < 200ms (p95)
- Page load time: < 2 seconds
- Dashboard rendering: < 3 seconds
- Report generation: < 5 seconds

#### NFR-1.2: Scalability
- Support 10,000 concurrent users
- Handle 1 million properties
- Process 100,000 payments per day
- Database: Vertical + horizontal scaling ready

#### NFR-1.3: Caching Strategy
- [ ] Implement Redis for frequently accessed data
- [ ] Cache dashboard stats (5-minute TTL)
- [ ] Cache property lists (10-minute TTL)
- [ ] API response caching (Etag/Last-Modified)

---

### NFR-2: SECURITY

#### NFR-2.1: Authentication & Authorization ✅ IMPLEMENTED
- [x] JWT token-based authentication
- [x] Role-based access control (RBAC)
- [x] Token expiration (24 hours)
- [x] Refresh token flow (recommended to add)

**Enhancement Needed:**
- [ ] Two-factor authentication (2FA)
- [ ] OAuth2 login (Google, Facebook)
- [ ] Password complexity requirements
- [ ] Account lockout after failed attempts
- [ ] Session management (logout all devices)

---

#### NFR-2.2: Data Security
- [ ] Encrypt sensitive data at rest (TDE on SQL Server)
- [ ] Encrypt data in transit (HTTPS/TLS 1.3)
- [ ] PII data anonymization in logs
- [ ] Secure file storage (Azure Blob with encryption)
- [ ] SQL injection prevention (parameterized queries) ✅
- [ ] XSS prevention (input sanitization) ✅

---

#### NFR-2.3: Compliance
**GDPR (if serving EU users):**
- [ ] User consent management
- [ ] Right to be forgotten (data deletion)
- [ ] Data export functionality
- [ ] Privacy policy and terms

**Kenya Data Protection Act 2019:**
- [ ] Data protection officer (DPO) assignment
- [ ] User consent for data processing
- [ ] Data breach notification procedures
- [ ] Secure data storage within Kenya (optional)

**Financial Compliance:**
- [ ] PCI-DSS (if storing card data - avoid by using Stripe/Flutterwave)
- [ ] M-Pesa security guidelines
- [ ] Receipt generation with tax compliance

---

### NFR-3: RELIABILITY

#### NFR-3.1: Availability
- Target uptime: 99.9% (43 minutes downtime per month)
- Implement health checks
- Automated failover
- Database replication (master-slave)

#### NFR-3.2: Backup & Recovery
- [ ] Daily automated database backups
- [ ] 30-day retention policy
- [ ] Point-in-time recovery (SQL Server)
- [ ] Disaster recovery plan (RTO: 4 hours, RPO: 1 hour)
- [ ] Regular backup restoration tests

#### NFR-3.3: Error Handling
- [ ] Centralized logging (Serilog to Azure Application Insights)
- [ ] Error tracking (Sentry or Raygun)
- [ ] User-friendly error messages
- [ ] Automated alerts for critical errors

---

### NFR-4: USABILITY

#### NFR-4.1: Accessibility
- [ ] WCAG 2.1 AA compliance
- [ ] Screen reader support
- [ ] Keyboard navigation
- [ ] Color contrast ratios (4.5:1 minimum)
- [ ] Text resizing support

#### NFR-4.2: Internationalization
- [ ] Multi-language support (English, Swahili initially)
- [ ] Multi-currency support (KES, USD, EUR)
- [ ] Date/time localization
- [ ] Number formatting per locale

#### NFR-4.3: User Experience
- [ ] Consistent design system
- [ ] Loading indicators for all async operations
- [ ] Optimistic UI updates
- [ ] Undo functionality for critical actions
- [ ] Contextual help and tooltips

---

## DEPLOYMENT & INFRASTRUCTURE

### INFR-1: Hosting Strategy

#### INFR-1.1: Cloud Provider (Recommended: Azure)
**Current Status:** Likely local dev environment

**Production Requirements:**
- [ ] **Backend API**
  - Azure App Service (or AWS Elastic Beanstalk)
  - Auto-scaling enabled
  - Load balancer
  - Custom domain with SSL

- [ ] **Frontend**
  - Vercel (Next.js optimized) - Recommended
  - Or Azure Static Web Apps
  - CDN for static assets
  - Global edge network

- [ ] **Database**
  - Azure SQL Database (or AWS RDS)
  - Geo-redundant backup
  - Automatic tuning enabled
  - Connection pooling

- [ ] **File Storage**
  - Azure Blob Storage (or AWS S3)
  - CDN integration
  - Lifecycle policies (move old files to cold storage)

- [ ] **Cache**
  - Azure Redis Cache (or AWS ElastiCache)
  - Multi-region replication

- [ ] **Monitoring**
  - Azure Application Insights
  - Real-time performance monitoring
  - Custom dashboards
  - Alerting rules

---

#### INFR-1.2: DevOps & CI/CD
- [ ] GitHub Actions (or Azure DevOps pipelines)
- [ ] Automated testing on PR
- [ ] Staging environment
- [ ] Blue-green deployment
- [ ] Automated rollback on failure
- [ ] Infrastructure as Code (Terraform or ARM templates)

---

#### INFR-1.3: Cost Estimation (Monthly, Kenya Market)
**Assumptions:** 1,000 properties, 5,000 units, 10,000 users

| Service | Provider | Cost (USD) |
|---------|----------|------------|
| Backend API | Azure App Service (P1v2) | $150 |
| Frontend | Vercel Pro | $20 |
| Database | Azure SQL (S3, 100 DTU) | $200 |
| File Storage | Azure Blob (1TB) | $25 |
| Redis Cache | Azure Redis (C1) | $75 |
| M-Pesa API | Safaricom | Transaction fees |
| SMS (10K/month) | Africa's Talking | $100 |
| Email (50K/month) | SendGrid | $20 |
| Monitoring | App Insights | $50 |
| **TOTAL** | | **~$640/month** |

**Revenue Model to Cover Costs:**
- Charge landlords $10-20/property/month
- Or 1% of rent collected
- Or tiered plans (Free: 3 properties, Pro: Unlimited)

---

## TESTING REQUIREMENTS

### TEST-1: Testing Strategy

#### TEST-1.1: Unit Testing
- [ ] Backend: xUnit tests for all services
- [ ] Target coverage: 80%+
- [ ] Test business logic, validation, calculations
- [ ] Mock external dependencies

#### TEST-1.2: Integration Testing
- [ ] API endpoint tests
- [ ] Database integration tests
- [ ] M-Pesa integration tests (sandbox)
- [ ] SMS service tests

#### TEST-1.3: End-to-End Testing
- [ ] Playwright or Cypress for frontend
- [ ] Critical user flows:
  - Tenant pays rent (end-to-end)
  - Landlord records payment
  - Tenant submits maintenance request
  - Landlord generates report

#### TEST-1.4: Performance Testing
- [ ] Load testing (JMeter or k6)
- [ ] Simulate 1000 concurrent users
- [ ] Identify bottlenecks
- [ ] Stress test payment processing

#### TEST-1.5: Security Testing
- [ ] OWASP Top 10 vulnerability scan
- [ ] Penetration testing
- [ ] Dependency vulnerability scanning (Dependabot)

---

## PHASED IMPLEMENTATION PLAN

### PHASE 1: CRITICAL GAPS (Q1 2026) - 3 Months
**Goal:** Achieve 80% feature parity with Landlord Studio

**Deliverables:**
1. Automated rent reminders (SMS + Email)
2. P&L and Cash Flow reports with charts
3. Expense tracking and categorization
4. Lease templates library
5. Lease expiration alerts
6. Email notification system
7. Enhanced dashboard with trend charts
8. Calendar view for all events

**Estimated Effort:** 400-500 hours

---

### PHASE 2: COMPETITIVE ADVANTAGE (Q2 2026) - 3 Months
**Goal:** Surpass competitors with local optimizations

**Deliverables:**
1. Tenant screening integration (Metropol CRB)
2. Multi-platform listing syndication (BuyRentKenya, Property24, Jiji)
3. Receipt OCR for expense capture
4. Contractor/vendor management
5. Digital lease signing (HelloSign)
6. Security deposit workflow (move-out, deductions, refunds)
7. Auto-pay enrollment for tenants
8. Property comparison analytics

**Estimated Effort:** 500-600 hours

---

### PHASE 3: SCALE & OPTIMIZE (Q3 2026) - 3 Months
**Goal:** Enterprise-ready, mobile-first, integrations

**Deliverables:**
1. React Native mobile apps (iOS + Android)
2. Bank feed integration (Equity, KCB, M-Pesa Business)
3. QuickBooks/Xero accounting integration
4. WhatsApp Business API for notifications
5. Advanced analytics and forecasting
6. Multi-language support (Swahili)
7. Payment plan functionality
8. Preventive maintenance scheduling

**Estimated Effort:** 600-700 hours

---

## SUCCESS METRICS

### User Adoption Metrics
- Monthly active users (MAU)
- Tenant portal adoption rate (target: 70%)
- Payment success rate (target: 95%)
- Average time to onboard new property (target: < 10 minutes)

### Financial Metrics
- Revenue per customer
- Customer acquisition cost (CAC)
- Customer lifetime value (LTV)
- Churn rate (target: < 5%/year)

### Performance Metrics
- Average API response time (target: < 200ms)
- Dashboard load time (target: < 2s)
- Report generation time (target: < 5s)
- System uptime (target: 99.9%)

### Satisfaction Metrics
- Net Promoter Score (NPS) (target: > 50)
- Customer satisfaction (CSAT) (target: > 4.5/5)
- Support ticket resolution time (target: < 24 hours)

---

## CONCLUSION

RentPro has a strong foundation with critical features already implemented (property management, payments, maintenance). To compete with Landlord Studio and become the leading platform in East Africa, focus on:

1. **Automation**: Rent reminders, late fees, auto-pay
2. **Financial Intelligence**: P&L, cash flow, expense tracking
3. **Communication**: Email templates, bulk SMS, WhatsApp
4. **Integrations**: Tenant screening, listing syndication, bank feeds
5. **Mobile-First**: Native apps for iOS/Android

By following this phased implementation plan, RentPro can achieve market leadership within 9-12 months.

---

**Document Version:** 2.0
**Last Updated:** December 17, 2025
**Next Review:** March 2026
