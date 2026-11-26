

# Mini Lipila API
 
ASP.NET Core Web API for managing mobile money collections and disbursements via Lipila.
 
## Features
 
- ✅ ASP.NET Core Identity integration
- ✅ JWT authentication
- ✅ Role-based authorization (Admin, Merchant, Customer)
- ✅ Mobile money collections and disbursements via Lipila API
- ✅ Merchant and wallet management
- ✅ Customer KYC verification
- ✅ Payment tracking and status checking
## Tech Stack
 
- .NET 10.0
- ASP.NET Core Identity
- Entity Framework Core
- PostgreSQL
- FastEndpoints
- JWT Bearer Authentication
- Flurl.Http for API integration
## Setup
 
1. **Clone the repository**
   ```bash
   git clone https://github.com/Sam-sez/Mini_Lipila.git
   cd Mini_Lipila
   ```
 
2. **Configure User Secrets**
   ```bash
   dotnet user-secrets set "Jwt:Key" "your-secret-key-at-least-32-characters"
   dotnet user-secrets set "Lipila:ApiKey" "your-lipila-api-key"
   dotnet user-secrets set "Admin:Email" "admin@example.com"
   dotnet user-secrets set "Admin:Password" "SecurePass123!"
   ```
 
3. **Update Database Connection**
   - Edit `appsettings.json` with your PostgreSQL credentials
4. **Run Migrations**
   ```bash
   dotnet ef database update
   ```
 
5. **Run the Application**
   ```bash
   dotnet run
   ```
 
6. **Access Swagger UI**
   - Navigate to: `http://localhost:5000/swagger`
## API Endpoints
 
### Authentication
- `POST /auth/register` - Register new user
- `POST /auth/login` - Login and get JWT token
### Merchants (Admin only)
- `POST /merchants` - Create merchant
- `GET /merchants` - List all merchants
### Merchant Operations (Merchant + Admin)
- `GET /merchants/{id}` - Get merchant details
- `GET /merchants/{id}/balance` - Get merchant balance
- `POST /merchants/{id}/wallets` - Create wallet
- `GET /merchants/{id}/wallets` - List merchant wallets
### Payments (Merchant + Admin)
- `POST /collection` - Initiate mobile money collection
- `GET /collection/{id}/status` - Check collection status
- `POST /disbursements/mobile-money` - Initiate disbursement
- `GET /disbursements/{id}/status` - Check disbursement status
### Admin
- `POST /merchants/{merchantId}/users` - Create merchant user
- `POST /users/{id}/roles` - Assign role to user
- `GET /users/{id}/roles` - Get user roles
## Security Features
 
- Password hashing via ASP.NET Core Identity
- JWT token-based authentication
- Role-based authorization
- Merchant ownership verification
- Protected endpoints with proper access control
## Database
 
Uses PostgreSQL with Entity Framework Core. Main entities:
- ApplicationUser (Identity)
- Merchant
- Customer
- Wallet
- Payment
## Author
 
Developed as part of ASP.NET Core Identity integration task.
 

## Running with Docker

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running

### Setup
1. Create a `.env` file in the project root with your own values:
   ```env
   POSTGRES_DB=mini_lipila
   POSTGRES_USER=minilipila
   POSTGRES_PASSWORD=your-db-password
   JWT_KEY=your-secret-key-at-least-32-characters
   LIPILA_API_KEY=your-lipila-api-key
   ADMIN_EMAIL=admin@example.com
   ADMIN_PASSWORD=YourStrongPassword1
   ```
   Never commit this file. It is listed in `.gitignore`.

2. Build and start the API and database:
   ```bash
   docker compose up --build -d
   ```

3. Open Swagger: http://localhost:5001/swagger

Tables are created automatically on startup, and the admin account from
`ADMIN_EMAIL` / `ADMIN_PASSWORD` is seeded.