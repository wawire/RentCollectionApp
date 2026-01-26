# RentPro - Modern Property Management Platform

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Next.js](https://img.shields.io/badge/Next.js-15-black.svg)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-blue.svg)

**The all-in-one property management solution built for Kenya and East Africa.**

RentPro simplifies rental property management with automated rent collection via M-Pesa, comprehensive financial reporting, maintenance tracking, and a self-service tenant portal. Built with modern technology and designed for landlords managing 1-100 properties.

---

## 🌟 Key Features

### 💰 Payment Management
- **M-Pesa STK Push Integration** - Tenants pay rent instantly via mobile money
- **Multiple Payment Methods** - M-Pesa, bank transfer, cash tracking
- **Payment Confirmation Workflow** - Landlord approves pending payments
- **Security Deposit Management** - Track deposits, deductions, and refunds
- **Automated Late Fees** - Configured per tenant with grace periods
- **Payment History** - Complete audit trail for all transactions

### 🏢 Property & Unit Management
- **Multi-Property Portfolio** - Manage unlimited properties from one dashboard
- **Detailed Unit Tracking** - Bedrooms, bathrooms, amenities, rent pricing
- **Multiple Images** - Upload property and unit photos
- **Occupancy Dashboard** - Real-time vacancy and occupancy metrics
- **Public Vacancy Listings** - Showcase available units to prospective tenants

### 👥 Tenant Management
- **Comprehensive Tenant Profiles** - Contact info, lease terms, documents
- **Tenant Portal** - Self-service dashboard for payments and requests
- **Lease Tracking** - Start/end dates, renewal management
- **Tenant Application System** - Online application forms for vacant units
- **Document Storage** - Store IDs, employment letters, lease agreements

### 🔧 Maintenance Management
- **Request Submission** - Tenants submit requests with photos from portal
- **Priority Levels** - Emergency, High, Medium, Low
- **Status Tracking** - Pending, Assigned, In Progress, Completed
- **Caretaker Assignment** - Assign tasks to property caretakers
- **Cost Tracking** - Record maintenance expenses per request

### 📊 Reports & Analytics
- **Dashboard Analytics** - Properties, units, tenants, revenue, occupancy
- **Monthly Reports** - Rent collection summaries (PDF export)
- **Tenant Directory** - Complete tenant list with contact info
- **Payment Status** - Real-time view of paid, pending, and overdue units
- **Property Performance** - Track collection rates and revenue

### 💬 Communication
- **SMS Notifications** - Africa's Talking integration for payment receipts
- **Payment Reminders** - Notify tenants of upcoming due dates
- **Maintenance Updates** - Status change notifications

### 🔐 Security & Access Control
- **Role-Based Access** - System Admin, Landlord, Caretaker, Accountant, Tenant
- **JWT Authentication** - Secure token-based authentication
- **Multi-Tenancy** - Complete data isolation per landlord
- **Document Security** - Secure file storage and access control

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and npm
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/wawire/RentCollectionApp.git
cd RentCollectionApp
```

2. **Set up the database**
```bash
cd src/RentCollection.API
dotnet ef database update
```

This will create the database and seed it with demo data.

3. **Start the backend API**
```bash
dotnet run
```

The API will be available at `http://localhost:5000` (Swagger at `http://localhost:5000/swagger`)

4. **Start the frontend (in a new terminal)**
```bash
cd src/RentCollection.WebApp
npm install
npm run dev
```

The web app will be available at `http://localhost:3000`

---

## 🔑 Demo Credentials

| Role | Email | Password | Access |
|------|-------|----------|--------|
| **System Admin** | admin@rentcollection.com | Admin@123 | Full system access |
| **Landlord** | landlord@example.com | Landlord@123 | Manage properties, tenants, payments |
| **Caretaker** | caretaker@example.com | Caretaker@123 | Manage assigned property |
| **Tenant** | peter.mwangi@gmail.com | Tenant@123 | Tenant portal access |

---

## 🏗️ Tech Stack

### Backend
- **.NET 8** - Latest long-term support version
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core** - ORM with code-first migrations
- **SQL Server** - Relational database
- **Clean Architecture** - Domain-Driven Design (DDD) principles
- **JWT Authentication** - Secure token-based auth
- **M-Pesa Daraja API** - Payment integration
- **Africa's Talking** - SMS service

### Frontend
- **Next.js 15** - React framework with App Router
- **React 18** - Modern React with hooks
- **TypeScript** - Type-safe JavaScript
- **Tailwind CSS** - Utility-first styling
- **Axios** - HTTP client for API calls
- **React Hook Form** - Form validation
- **React Icons** - Icon library

### DevOps & Infrastructure
- **Git** - Version control
- **Azure** - Cloud hosting (recommended)
- **Docker** - Containerization (optional)
- **GitHub Actions** - CI/CD pipelines

---

## 📁 Project Structure

```
RentCollectionApp/
├── src/
│   ├── RentCollection.Domain/          # Core business entities
│   │   ├── Entities/                   # User, Property, Unit, Tenant, Payment
│   │   ├── Enums/                      # UserRole, PaymentStatus, etc.
│   │   └── Interfaces/                 # Repository interfaces
│   │
│   ├── RentCollection.Application/     # Business logic layer
│   │   ├── DTOs/                       # Data transfer objects
│   │   ├── Services/                   # Business services
│   │   └── Interfaces/                 # Service interfaces
│   │
│   ├── RentCollection.Infrastructure/  # External integrations
│   │   ├── Data/                       # EF Core DbContext, migrations
│   │   ├── Repositories/               # Data access repositories
│   │   ├── Services/                   # External APIs (M-Pesa, SMS)
│   │   └── Security/                   # JWT token generation
│   │
│   ├── RentCollection.API/             # REST API controllers
│   │   ├── Controllers/                # API endpoints
│   │   ├── Middleware/                 # Error handling, auth
│   │   └── Program.cs                  # Startup configuration
│   │
│   └── RentCollection.WebApp/          # Next.js frontend
│       ├── app/                        # App Router pages
│       │   ├── dashboard/              # Landlord dashboard
│       │   ├── properties/             # Property management
│       │   ├── units/                  # Unit management
│       │   ├── tenants/                # Tenant management
│       │   ├── payments/               # Payment management
│       │   ├── reports/                # Reports & analytics
│       │   └── tenant-portal/          # Tenant self-service portal
│       ├── components/                 # React components
│       ├── lib/                        # Utilities, services, types
│       └── contexts/                   # React Context (Auth)
│
└── docs/                               # Documentation
    ├── ARCHITECTURE.md                 # System design & database schema
    ├── API.md                          # API endpoint documentation
    ├── DEVELOPMENT.md                  # Development setup & guidelines
    ├── SYSTEM_REQUIREMENTS.md          # Detailed requirements specification
    ├── FEATURE_ROADMAP.md              # Product roadmap and priorities
    └── COMPETITIVE_ANALYSIS.md         # Market analysis vs competitors
```

---

## 🎯 User Roles & Permissions

### System Admin
- Full access to all landlords' properties
- User management (create landlords, caretakers, accountants)
- System configuration
- Global analytics

### Landlord
- Manage own properties, units, tenants
- View/confirm/reject payments
- Generate reports
- Configure payment accounts
- Manage maintenance requests
- Upload documents

### Caretaker
- Manage assigned property only
- View tenants and units
- Manage maintenance requests
- View payment status (read-only)

### Accountant
- Read-only access to financial data
- Generate reports
- View payment history
- Export financial data

### Tenant
- View lease information
- See payment instructions
- Pay rent via M-Pesa STK Push
- Record manual payments
- Submit maintenance requests
- View payment history
- Download documents

---

## 🔄 Payment Flow

### 1. Tenant Initiates Payment
```
Tenant Portal → Pay Now → M-Pesa STK Push
  ↓
  Tenant's phone receives M-Pesa prompt
  ↓
  Tenant enters PIN and confirms
  ↓
  M-Pesa callback to API
```

### 2. Landlord Confirms Payment
```
Payment recorded as "Pending"
  ↓
  Landlord receives notification
  ↓
  Landlord reviews payment in dashboard
  ↓
  Landlord confirms/rejects
  ↓
  Payment marked as "Confirmed" or "Rejected"
  ↓
  Tenant receives SMS receipt (if confirmed)
```

### 3. Alternative: Manual Payment Recording
```
Tenant pays via Paybill/bank/cash
  ↓
  Tenant records payment in portal
  ↓
  Uploads M-Pesa receipt screenshot
  ↓
  Landlord confirms payment
  ↓
  Payment confirmed, SMS sent
```

---

## 📚 Documentation

- **[Architecture](docs/ARCHITECTURE.md)** - System design, database schema, payment flows
- **[API Reference](docs/API.md)** - Complete API endpoint documentation
- **[Development Guide](docs/DEVELOPMENT.md)** - Setup instructions, testing, contributing
- **[System Requirements](docs/SYSTEM_REQUIREMENTS.md)** - Detailed functional & technical requirements
- **[Feature Roadmap](docs/FEATURE_ROADMAP.md)** - Product roadmap & upcoming features
- **[Competitive Analysis](docs/COMPETITIVE_ANALYSIS.md)** - Market position vs competitors

---

## 🗺️ Roadmap

### ✅ Current Features (v1.0)
- Multi-property & unit management
- Tenant management with portal
- M-Pesa STK Push payment integration
- Security deposit tracking
- Maintenance request system
- SMS notifications
- Basic reporting

### 🚧 Coming Soon (v2.0 - Q1 2026)
- **Automated rent reminders** - SMS/Email 7 days before due date
- **Expense tracking** - Record and categorize property expenses
- **P&L Reports** - Profit & Loss statements with charts
- **Cash Flow Dashboard** - 12-month income vs expense visualization
- **Email notifications** - Professional email templates
- **Lease templates** - Generate leases from templates
- **Lease expiration alerts** - Automated reminders 90/60/30 days before

### 🔮 Future (v3.0 - Q2 2026)
- **Tenant screening** - Credit checks via Metropol CRB
- **Listing syndication** - Publish to BuyRentKenya, Property24, Jiji
- **Receipt OCR** - Photo → auto-categorized expense
- **Digital lease signing** - E-signature integration
- **Contractor management** - Vendor database and tracking
- **Mobile apps** - iOS and Android native apps

### 🚀 Long-term Vision
- Bank feed integration (auto-import transactions)
- Accounting software integration (QuickBooks, Xero)
- WhatsApp Business API notifications
- Advanced analytics & forecasting
- Multi-language support (Swahili)
- Expand to Tanzania, Uganda, Nigeria

See [FEATURE_ROADMAP.md](docs/FEATURE_ROADMAP.md) for detailed roadmap.

---

## 🆚 Competitive Advantages

### vs Landlord Studio
✅ **Superior Kenya market fit** - M-Pesa as primary payment method
✅ **Better maintenance workflow** - Full assignment and tracking
✅ **Local support** - Nairobi-based team
✅ **Competitive pricing** - Same price, better local experience

### vs Buildium/AppFolio
✅ **10x more affordable** - $15/month vs $200+/month
✅ **Simpler to use** - No complexity, intuitive UI
✅ **Kenya-optimized** - M-Pesa, SMS, local payment methods
✅ **Faster time to value** - Get started in 10 minutes

### vs TenantCloud
✅ **Better features** - Advanced maintenance, security deposits
✅ **Kenya-first design** - Built for African landlords
✅ **Superior UX** - Modern, fast, intuitive

See [COMPETITIVE_ANALYSIS.md](docs/COMPETITIVE_ANALYSIS.md) for detailed comparison.

---

## 💡 Use Cases

### For Landlords
- Collect rent online with M-Pesa (reduce trips to the bank)
- Track which tenants have paid and who is overdue
- Generate monthly financial reports for tax filing
- Manage maintenance requests from tenants
- Store all tenant documents in one place
- Send bulk SMS reminders to tenants

### For Property Managers
- Manage multiple properties for different owners
- Give property owners read-only access to view reports
- Assign caretakers to specific properties
- Track expenses per property
- Generate owner statements

### For Tenants
- View rent balance and payment history
- Pay rent instantly via M-Pesa from their phone
- Submit maintenance requests with photos
- Download lease agreements and receipts
- Get SMS reminders for upcoming rent

---

## 🛠️ Configuration

### M-Pesa Setup
1. Register for M-Pesa Daraja API at [developer.safaricom.co.ke](https://developer.safaricom.co.ke)
2. Set up Paybill number
3. Configure callback base URL and STK settings in `appsettings.json`:

```json
{
  "MPesa": {
    "UseSandbox": true,
    "CallbackBaseUrl": "https://yourdomain.com",
    "StkPushTimeout": 60,
    "EnableDetailedLogging": false
  }
}
```

Landlord-specific credentials (consumer key/secret, shortcode, passkey, B2C initiator/security credential) are stored per `LandlordPaymentAccount` via the API/UI.

### SMS Configuration
1. Sign up at [AfricasTalking.com](https://africastalking.com/)
2. Get API key and username
3. Configure in `appsettings.json`:

```json
{
  "AfricasTalking": {
    "ApiKey": "your_api_key",
    "Username": "your_username",
    "SenderId": "RENTPAY"
  }
}
```

### Database Connection
Update connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RentCollectionDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

---

## 🧪 Testing

### Run Backend Tests
```bash
cd src/RentCollection.API
dotnet test
```

### Run Frontend Tests
```bash
cd src/RentCollection.WebApp
npm test
```

---

## 🚀 Deployment

### Backend (Azure App Service)
1. Create Azure App Service (.NET 8)
2. Create Azure SQL Database
3. Configure connection string in App Service settings
4. Deploy via GitHub Actions or Azure CLI

### Frontend (Vercel)
1. Connect GitHub repo to Vercel
2. Configure build settings:
   - Framework: Next.js
   - Build Command: `npm run build`
   - Output Directory: `.next`
3. Add environment variable: `NEXT_PUBLIC_API_URL=https://your-api.azurewebsites.net`
4. Deploy

---

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

See [DEVELOPMENT.md](docs/DEVELOPMENT.md) for detailed guidelines.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📞 Support

- **Email:** support@rentpro.co.ke
- **Documentation:** [docs/](docs/)
- **GitHub Issues:** [github.com/wawire/RentCollectionApp/issues](https://github.com/wawire/RentCollectionApp/issues)

---

## 🙏 Acknowledgments

- **M-Pesa Daraja API** - Safaricom for payment integration
- **Africa's Talking** - SMS service provider
- **Next.js Team** - Excellent framework
- **Microsoft .NET Team** - Powerful backend platform
- **Open Source Community** - For all the amazing libraries

---

## 📊 Project Stats

![GitHub stars](https://img.shields.io/github/stars/wawire/RentCollectionApp?style=social)
![GitHub forks](https://img.shields.io/github/forks/wawire/RentCollectionApp?style=social)
![GitHub watchers](https://img.shields.io/github/watchers/wawire/RentCollectionApp?style=social)

---

## 🌍 Made in Kenya 🇰🇪

Built with ❤️ for landlords and property managers across East Africa.

**RentPro** - Simplifying property management, one rental at a time.

---

**Version:** 1.0
**Last Updated:** December 17, 2025
