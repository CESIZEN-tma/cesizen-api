# CesiZen API - Integration Documentation

Welcome to the CesiZen API integration documentation. This guide helps frontend developers understand how to integrate with the backend API and implement user flows.

---

## 📚 Documentation Structure

### 🎯 User Journeys
**Start here to understand how users navigate through the app**

[**User Journeys →**](./USER_JOURNEYS/README.md)

Step-by-step guides showing the complete user experience:
- [New User Path](./USER_JOURNEYS/NEW_USER_PATH.md) - First-time user from landing to first exercise
- [Returning User Path](./USER_JOURNEYS/RETURNING_USER_PATH.md) - Regular user workflows
- [Quiz to Exercise Path](./USER_JOURNEYS/QUIZ_TO_EXERCISE_PATH.md) - Core feature flow (detailed)
- [Profile Management Path](./USER_JOURNEYS/PROFILE_MANAGEMENT_PATH.md) - Account settings

### 👨‍💼 Admin Journeys
**Administrator workflows and management features**

[**Admin Journeys →**](./ADMIN_JOURNEYS/README.md)

- [User Management Path](./ADMIN_JOURNEYS/USER_MANAGEMENT_PATH.md) - Manage user accounts and sessions
- Content Creation Path *(coming soon)*
- Quiz Creation Path *(coming soon)*

### 🔐 Authentication
**Complete authentication system documentation**

[**Authentication →**](./AUTHENTIFICATION/README.md)

- Registration & Email Activation
- Login & Logout
- Token Management & Refresh
- Password Reset & Change
- Session Management
- Account Lockout Protection

### 🧩 Features Reference
**Detailed API documentation per feature**

[**Features →**](./FEATURES/README.md)

- Quizzes API
- Configurations API
- User Profile API
- Admin Features API

### 📖 Reference
**Quick lookup guides**

[**Reference →**](./REFERENCE/README.md)

- API Endpoints List
- Error Handling
- Data Models

---

## 🚀 Quick Start Guide

### For New Developers

1. **Understand Authentication**
   - Read: [Authentication Overview](./AUTHENTIFICATION/README.md)
   - Understand: [User vs Admin](./AUTHENTIFICATION/USER_VS_ADMIN.md)

2. **Learn the Core User Flow**
   - Follow: [New User Path](./USER_JOURNEYS/NEW_USER_PATH.md)
   - Deep dive: [Quiz to Exercise Path](./USER_JOURNEYS/QUIZ_TO_EXERCISE_PATH.md)

3. **Implement Features**
   - Browse: [Features Reference](./FEATURES/README.md)
   - Handle: [Errors](./REFERENCE/ERROR_HANDLING.md)

### For Returning Developers

- **Adding a new feature?** Check existing [User Journeys](./USER_JOURNEYS/README.md) for patterns
- **API endpoint question?** See [Features Reference](./FEATURES/README.md)
- **Authentication issue?** Review [Authentication docs](./AUTHENTIFICATION/README.md)

---

## 🎯 Key User Paths

Understanding these paths is essential for building the frontend:

| Path | Description | Documentation |
|------|-------------|---------------|
| **New User** | Registration → Activation → First Quiz → First Exercise | [View Path](./USER_JOURNEYS/NEW_USER_PATH.md) |
| **Returning User** | Login → Select Config → Start Exercise | [View Path](./USER_JOURNEYS/RETURNING_USER_PATH.md) |
| **Quiz Flow** | Browse → Answer Questions → Generate Config → Exercise | [View Path](./USER_JOURNEYS/QUIZ_TO_EXERCISE_PATH.md) |
| **Admin User Mgmt** | View Users → Enable/Disable → Manage Sessions | [View Path](./ADMIN_JOURNEYS/USER_MANAGEMENT_PATH.md) |

---

## 🔑 Key Concepts

### JWT Authentication
- **Access Token:** Short-lived, used for API authorization
- **Refresh Token:** Long-lived, used to get new access tokens
- **User Token:** No `role` claim
- **Admin Token:** Has `role: "Administrator"` claim

**Learn more:** [Authentication Overview](./AUTHENTIFICATION/README.md)

---

### Quiz → Configuration Flow
The core app feature:
1. User takes a quiz (answers questions)
2. Each answer has `ResponseOption` rules
3. Backend applies rules to generate breathing configuration
4. Configuration saved to user's library
5. User starts breathing exercise

**Learn more:** [Quiz to Exercise Path](./USER_JOURNEYS/QUIZ_TO_EXERCISE_PATH.md)

---

### API Headers
All requests require:
```http
x-api-key: {your-api-key}
```

Authenticated requests also require:
```http
Authorization: Bearer {access-token}
```

**Learn more:** [Authentication](./AUTHENTIFICATION/README.md)

---

## 📱 Implementation Priorities

### Phase 1: Core User Experience
1. ✅ Authentication (Register, Login, Activation)
2. ✅ Quiz browsing and taking
3. ✅ Configuration generation
4. ✅ Breathing exercise player

**Follow:** [New User Path](./USER_JOURNEYS/NEW_USER_PATH.md)

---

### Phase 2: User Features
1. Configuration management (CRUD)
2. Profile management
3. Session management
4. Password change

**Follow:** [Returning User Path](./USER_JOURNEYS/RETURNING_USER_PATH.md)

---

### Phase 3: Admin Features
1. User management (enable/disable, sessions)
2. Administrator management
3. Content management (pages, tags, menus)
4. Quiz creation and editing
5. Admin logs

**Follow:** [Admin Journeys](./ADMIN_JOURNEYS/README.md)

---

## 🎨 UI/UX Principles

When implementing these journeys:

1. **Progressive Disclosure** - Don't overwhelm new users
2. **Clear Navigation** - Always show where the user is
3. **Feedback & Confirmation** - Show loading states and confirm actions
4. **Graceful Error Handling** - Explain errors clearly
5. **Mobile-First** - Ensure responsive design

---

## 🔗 External Resources

- **API Base URL:** `/api/` (configured per environment)
- **Public Docs URL:** `/api/public/docs/` (this documentation served as HTML)

---

## 📝 Documentation Access

This documentation is available:

1. **As Markdown Files** - In `DOCUMENTATION/INTEGRATION/` folder
2. **As HTML via API** - Accessible at `/api/public/docs/integration/{path}`

Example:
```
GET /api/public/docs/integration/user_journeys
→ Displays USER_JOURNEYS/README.md as styled HTML

GET /api/public/docs/integration/user_journeys/new_user_path
→ Displays NEW_USER_PATH.md as styled HTML
```

---

## 🆘 Support

- **Questions about a specific journey?** → Read the journey documentation
- **API endpoint details?** → Check [Features Reference](./FEATURES/README.md)
- **Authentication issues?** → Review [Authentication docs](./AUTHENTIFICATION/README.md)
- **Error handling?** → See [Error Handling guide](./REFERENCE/ERROR_HANDLING.md)

---

## 🗺️ Documentation Map

```
INTEGRATION/
├── README.md (you are here)
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
│   ├── PASSWORD_RESET.md
│   └── ... (more auth docs)
│
├── FEATURES/
│   ├── README.md
│   ├── QUIZZES/
│   ├── CONFIGURATIONS/
│   ├── USER_PROFILE/
│   └── ADMIN_FEATURES/
│
└── REFERENCE/
    ├── README.md
    ├── API_ENDPOINTS.md
    └── ERROR_HANDLING.md
```

⭐ = Recommended starting points

---

**Ready to start?** Begin with the [New User Path](./USER_JOURNEYS/NEW_USER_PATH.md) to understand the complete first-time user experience.
