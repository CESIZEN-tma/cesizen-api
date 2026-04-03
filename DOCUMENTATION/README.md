# CesiZen API Documentation

Welcome to the CesiZen API documentation hub. This documentation helps you integrate with the backend API and understand how users navigate through the application.

---

## 📑 Documentation Sections

### 🎯 [Integration Guides](./INTEGRATION/README.md)

**Complete frontend integration documentation**

Start here for comprehensive guides on implementing the CesiZen frontend application.

**What's Inside:**
- 👥 [User Journeys](./INTEGRATION/USER_JOURNEYS/README.md) - Step-by-step user paths through the app
- 👨‍💼 [Admin Journeys](./INTEGRATION/ADMIN_JOURNEYS/README.md) - Administrator workflows
- 🔐 [Authentication](./INTEGRATION/AUTHENTIFICATION/README.md) - Complete auth system docs
- 🧩 [Features Reference](./INTEGRATION/FEATURES/README.md) - API documentation per feature
- 📖 [Reference](./INTEGRATION/REFERENCE/README.md) - Quick lookup guides

---

## 🚀 Quick Start

### New to CesiZen API?

Follow this path to get started:

1. **Understand Authentication**
   - Read: [Authentication Overview](./INTEGRATION/AUTHENTIFICATION/README.md)
   - Learn: [User vs Admin Authentication](./INTEGRATION/AUTHENTIFICATION/USER_VS_ADMIN.md)

2. **Learn the Core User Experience**
   - Follow: [New User Journey](./INTEGRATION/USER_JOURNEYS/NEW_USER_PATH.md)
   - Deep dive: [Quiz to Exercise Flow](./INTEGRATION/USER_JOURNEYS/QUIZ_TO_EXERCISE_PATH.md)

3. **Implement Features**
   - Browse: [Features Reference](./INTEGRATION/FEATURES/README.md)
   - Reference: [API Endpoints](./INTEGRATION/REFERENCE/README.md)

---

## 🎯 Popular User Paths

These are the most important user flows to understand:

| Path | Description | Documentation |
|------|-------------|---------------|
| **New User** | Registration → Quiz → First Exercise | [View Path →](./INTEGRATION/USER_JOURNEYS/NEW_USER_PATH.md) |
| **Returning User** | Login → Select Config → Exercise | [View Path →](./INTEGRATION/USER_JOURNEYS/RETURNING_USER_PATH.md) |
| **Core Feature** | Quiz → Generate Config → Exercise | [View Path →](./INTEGRATION/USER_JOURNEYS/QUIZ_TO_EXERCISE_PATH.md) |
| **Admin** | Manage Users & Content | [View Path →](./INTEGRATION/ADMIN_JOURNEYS/README.md) |

---

## 📚 Key Documentation

### User Experience

**[User Journeys](./INTEGRATION/USER_JOURNEYS/README.md)** - Complete user flows
- [New User Path](./INTEGRATION/USER_JOURNEYS/NEW_USER_PATH.md) - First-time experience (10 steps)
- [Returning User Path](./INTEGRATION/USER_JOURNEYS/RETURNING_USER_PATH.md) - Regular workflows (4 paths)
- [Quiz to Exercise Path](./INTEGRATION/USER_JOURNEYS/QUIZ_TO_EXERCISE_PATH.md) - Core feature (7 steps, detailed)
- [Profile Management](./INTEGRATION/USER_JOURNEYS/PROFILE_MANAGEMENT_PATH.md) - Account settings

**Purpose:** Understand exactly what users see and do at each step

---

### Administrator Experience

**[Admin Journeys](./INTEGRATION/ADMIN_JOURNEYS/README.md)** - Admin workflows
- [User Management](./INTEGRATION/ADMIN_JOURNEYS/USER_MANAGEMENT_PATH.md) - Enable/disable users, manage sessions
- Content Management *(coming soon)*
- Quiz Creation *(coming soon)*

**Purpose:** Learn how admins manage the system

---

### Authentication System

**[Authentication](./INTEGRATION/AUTHENTIFICATION/README.md)** - Complete auth docs

**Core Features:**
- [Registration](./INTEGRATION/AUTHENTIFICATION/REGISTRATION.md) - User signup & activation
- [Login](./INTEGRATION/AUTHENTIFICATION/LOGIN.md) - Authentication flow
- [Token Refresh](./INTEGRATION/AUTHENTIFICATION/TOKEN_REFRESH.md) - Session continuity
- [Password Reset](./INTEGRATION/AUTHENTIFICATION/PASSWORD_RESET.md) - Recovery flow
- [Session Management](./INTEGRATION/AUTHENTIFICATION/SESSION_MANAGEMENT.md) - Active sessions

**Purpose:** Implement secure JWT-based authentication

---

### API Features Reference

**[Features](./INTEGRATION/FEATURES/README.md)** - Detailed API docs

**Available Features:**
- Quizzes API
- Configurations API
- User Profile API
- Admin Features API

**Purpose:** Quick API endpoint reference

---

## 🔑 Key Concepts

### The CesiZen App

**What is CesiZen?**
A breathing exercise application that helps users discover and practice personalized breathing patterns.

**Core Flow:**
```
User takes Quiz → Answers questions →
Backend generates personalized breathing configuration →
User practices breathing exercise
```

---

### JWT Authentication

**Two User Types:**

**Regular User:**
```json
{
  "nameid": "user-guid",
  "exp": 1706356800
}
```
No `role` claim

**Administrator:**
```json
{
  "nameid": "admin-guid",
  "role": "Administrator",
  "exp": 1706356800
}
```
Has `role: "Administrator"` claim

**Learn more:** [User vs Admin](./INTEGRATION/AUTHENTIFICATION/USER_VS_ADMIN.md)

---

### Quiz → Configuration Generation

**How it works:**

1. Admin creates quiz with questions
2. Each answer option has `ResponseOption` rules:
   ```json
   {
     "targetedField": "Difficulty",
     "operation": "SET",
     "value": "2"
   }
   ```
3. User answers questions
4. Backend applies rules to generate breathing configuration
5. Configuration saved to user's library

**Learn more:** [Quiz to Exercise Path](./INTEGRATION/USER_JOURNEYS/QUIZ_TO_EXERCISE_PATH.md)

---

## 🛠️ Implementation Phases

### Phase 1: Core User Experience ⭐
**Priority: HIGH**

- ✅ Authentication (Register, Login, Activation)
- ✅ Quiz browsing and taking
- ✅ Configuration generation
- ✅ Breathing exercise player

**Follow:** [New User Path](./INTEGRATION/USER_JOURNEYS/NEW_USER_PATH.md)

---

### Phase 2: User Features
**Priority: MEDIUM**

- Configuration management (CRUD)
- Profile management
- Session management
- Password change

**Follow:** [Returning User Path](./INTEGRATION/USER_JOURNEYS/RETURNING_USER_PATH.md)

---

### Phase 3: Admin Features
**Priority: LOW**

- User management
- Content management
- Quiz creation
- Admin logs

**Follow:** [Admin Journeys](./INTEGRATION/ADMIN_JOURNEYS/README.md)

---

## 📡 API Access

### Base URL
```
/api/
```

### Required Headers

**All requests:**
```http
x-api-key: {your-api-key}
```

**Authenticated requests:**
```http
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

---

## 🗺️ Documentation Structure

```
DOCUMENTATION/
├── README.md (you are here)
│
└── INTEGRATION/
    ├── README.md
    │
    ├── USER_JOURNEYS/
    │   ├── README.md
    │   ├── NEW_USER_PATH.md ⭐
    │   ├── RETURNING_USER_PATH.md
    │   ├── QUIZ_TO_EXERCISE_PATH.md ⭐
    │   └── PROFILE_MANAGEMENT_PATH.md
    │
    ├── ADMIN_JOURNEYS/
    │   ├── README.md
    │   └── USER_MANAGEMENT_PATH.md
    │
    ├── AUTHENTIFICATION/
    │   ├── README.md ⭐
    │   ├── REGISTRATION.md
    │   ├── LOGIN.md
    │   ├── TOKEN_REFRESH.md
    │   └── ... (14 docs total)
    │
    ├── FEATURES/
    │   ├── README.md
    │   ├── QUIZZES/
    │   ├── CONFIGURATIONS/
    │   └── USER_PROFILE/
    │
    └── REFERENCE/
        ├── README.md
        ├── API_ENDPOINTS.md
        └── ERROR_HANDLING.md
```

⭐ = Recommended starting points

---

## 🌐 Access Documentation

### Via API (HTML)

This documentation is accessible as styled HTML via the API:

```
GET /api/public/docs
→ This main page

GET /api/public/docs/integration
→ Integration guide index

GET /api/public/docs/integration/user_journeys/new_user_path
→ New user journey

GET /api/public/docs/integration/authentification
→ Authentication overview
```

### Via Files (Markdown)

All documentation is also available as markdown files in the `DOCUMENTATION/` folder.

---

## 🎯 Where to Go Next

### For Frontend Developers
1. [Integration Guide](./INTEGRATION/README.md) - Start here
2. [New User Path](./INTEGRATION/USER_JOURNEYS/NEW_USER_PATH.md) - Understand the flow
3. [Authentication](./INTEGRATION/AUTHENTIFICATION/README.md) - Implement auth

### For Backend Developers
- Authentication system is fully implemented
- Quiz configuration generation is automatic
- Admin logging happens automatically

### For Product/Design Teams
- [User Journeys](./INTEGRATION/USER_JOURNEYS/README.md) - See complete user flows
- [Quiz to Exercise Path](./INTEGRATION/USER_JOURNEYS/QUIZ_TO_EXERCISE_PATH.md) - Understand core feature

---

## 📞 Support

**Questions about a specific topic?**
- Authentication → [Auth docs](./INTEGRATION/AUTHENTIFICATION/README.md)
- User flows → [User Journeys](./INTEGRATION/USER_JOURNEYS/README.md)
- API endpoints → [Features Reference](./INTEGRATION/FEATURES/README.md)
- Admin features → [Admin Journeys](./INTEGRATION/ADMIN_JOURNEYS/README.md)

**Can't find what you're looking for?**
- Check the [Integration guide index](./INTEGRATION/README.md)
- Browse the documentation structure above

---

## 📈 Documentation Statistics

- **24+ documentation files**
- **4 major sections** (User Journeys, Admin Journeys, Authentication, Features)
- **14 authentication guides**
- **4 detailed user journeys**
- **Complete API integration examples**

---

**Ready to start building?** → [Go to Integration Guide](./INTEGRATION/README.md)
