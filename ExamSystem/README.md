# 📘 Exam System

A complete exam management system built with **.NET 8 Web API** backend and **React** frontend, featuring role-based authentication, exam management, and student assessment capabilities.

---

## ✨ Features

- 🔑 **Role-based Authentication**: Admin and Student roles with JWT tokens
- 📝 **Exam Management**: Create, update, delete exams with questions and answers
- 🎓 **Student Assessment**: Take exams with automatic scoring and cooldown periods
- 📡 **Comprehensive API**: RESTful endpoints with Swagger documentation
- ✅ **Test Coverage**: Unit tests and E2E integration tests included

---

## 🛠 Tech Stack

- **Backend**: .NET 8 Web API, Entity Framework Core, SQL Server  
- **Frontend**: React 18, TypeScript, Axios  
- **Authentication**: JWT Bearer tokens  
- **Testing**: xUnit, MSTest  
- **Documentation**: Swagger/OpenAPI  

---

## 🚀 Quick Start

### 🔧 Prerequisites
- .NET 8 SDK  
- Node.js 18+  
- SQL Server (LocalDB or full instance)  

---

### 1️⃣ Clone Repository
```bash
git clone https://github.com/your-username/ExamSystem.git
cd ExamSystem
```

### 2️⃣ Setup Backend
```bash
cd ExamSystem.API

# Restore packages
dotnet restore

# Update database
dotnet ef database update

# Run API (https://localhost:7001)
dotnet run
```

### 3️⃣ Setup Frontend
```bash
cd exam-system-frontend

# Install dependencies
npm install

# Start dev server (http://localhost:3000)
npm start
```

---

## 📖 API Documentation

- Swagger UI: [`https://localhost:7001/swagger`](https://localhost:7001/swagger)

### 🔐 Authentication

The system uses **JWT Bearer tokens**.  

Test credentials:  

**Admin User**  
- Email: `admin@example.com`  
- Password: `Admin123!`  

**Student User**  
- Email: `student@example.com`  
- Password: `Student123!`  

---

### 📌 Key Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST   | `/api/auth/login`        | User authentication |
| GET    | `/api/exams`             | Get all exams (Admin) |
| POST   | `/api/exams`             | Create exam (Admin) |
| GET    | `/api/exams/student`     | Get available exams (Student) |
| POST   | `/api/exams/{id}/start`  | Start exam attempt (Student) |
| POST   | `/api/exams/{id}/submit` | Submit exam answers (Student) |

---

## 🧑‍💻 Usage Guide

### 👩‍💼 For Administrators
1. Login with admin credentials  
2. Create an exam:  

```json
POST /api/exams
{
  "title": "Math Quiz",
  "description": "Basic mathematics test",
  "duration": 30,
  "cooldownMinutes": 60,
  "questions": [
    {
      "text": "What is 2+2?",
      "options": ["3", "4", "5", "6"],
      "correctAnswer": 1
    }
  ]
}
```
3. Manage existing exams (view, update, delete)

---

### 👨‍🎓 For Students
1. Login with student credentials  
2. View available exams (respects cooldown periods)  
3. Start an exam attempt  
4. Submit answers  
5. View results (score + correct answers)  

---

### ⏳ Cooldown Logic
- Students must wait between exam attempts  
- Cooldown = **attempt start time + cooldown minutes**  
- Applies to all attempts (completed or incomplete)  

---

## 🧪 Testing

### Run Unit Tests
```bash
cd ExamSystem.Tests
dotnet test
```

### Run E2E Tests
```bash
cd ExamSystem.E2ETests
dotnet test
```

✅ **Coverage**: 34 unit tests + 5 E2E tests (all passing)  

---

## 📂 Project Structure

```
ExamSystem/
├── ExamSystem.API/           # Web API backend
│   ├── Controllers/          # API controllers
│   ├── Services/             # Business logic
│   ├── Models/               # Data models
│   └── Data/                 # EF Core context
├── ExamSystem.Tests/         # Unit tests
├── ExamSystem.E2ETests/      # Integration tests
└── exam-system-frontend/     # React frontend
    ├── src/
    │   ├── components/       # React components
    │   └── services/         # API services
    └── public/
```

---

## ⚙️ Configuration

### 🔗 Database Connection
Update `appsettings.json` in **ExamSystem.API**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=ExamSystemDb;Trusted_Connection=true;"
  }
}
```

### 🌐 Frontend API URL
Update `src/services/api.ts` in frontend:
```ts
const API_BASE_URL = 'https://localhost:7001/api';
```

---

## 🏗 Development

### Adding New Features
1. **Backend** → Add controllers in `Controllers/`, logic in `Services/`  
2. **Frontend** → Add components in `src/components/`, API calls in `src/services/`  
3. **Tests** → Add unit tests in `ExamSystem.Tests/`, E2E tests in `ExamSystem.E2ETests/`  

### Database Migrations
```bash
cd ExamSystem.API
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

---

## 🛠 Troubleshooting

### Common Issues
1. **CORS Errors** → Ensure frontend URL is added in CORS policy (`Program.cs`)  
2. **Database Connection** → Verify SQL Server is running and connection string is correct  
3. **Port Conflicts** → Ensure 7001 (API) and 3000 (frontend) are free  

### Logs
- API → Console output (`dotnet run`)  
- Frontend → Browser developer console  

---

## 📜 License
This project is licensed under the MIT License.
