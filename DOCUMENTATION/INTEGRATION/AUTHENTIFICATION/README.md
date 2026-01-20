# Authentication Integration Guide

This documentation covers the complete authentication system integration for frontend applications.

## Table of Contents

### Getting Started
- [Overview](#overview)
- [User vs Admin Authentication](./USER_VS_ADMIN.md)
- [Authentication Flow](./AUTH_FLOW.md)

### Core Features
- [Registration & Account Activation](./REGISTRATION.md)
- [Login & Token Management](./LOGIN.md)
- [Token Refresh](./TOKEN_REFRESH.md)
- [Logout](./LOGOUT.md)

### Security Features
- [Account Lockout (Brute Force Protection)](./ACCOUNT_LOCKOUT.md)
- [Session Management](./SESSION_MANAGEMENT.md)
- [Password Reset](./PASSWORD_RESET.md)
- [Change Password](./CHANGE_PASSWORD.md)

### Reference
- [API Endpoints Summary](./API_ENDPOINTS.md)
- [Error Handling](./ERROR_HANDLING.md)
- [Email Notifications](./EMAIL_NOTIFICATIONS.md)

---

## Overview

The CesiZen API provides a complete JWT-based authentication system with the following features:

| Feature | Description |
|---------|-------------|
| JWT Authentication | Access tokens for API authorization |
| Refresh Tokens | Long-lived tokens for session continuity |
| Account Lockout | Brute force protection after 5 failed attempts |
| Session Management | View and revoke active sessions |
| Password Reset | Email-based password recovery |
| Change Password | Authenticated password change |
| Email Verification | Account activation via email |

## Quick Start

### 1. Register a new user
```http
POST /user/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### 2. Activate account via email link

### 3. Login to get tokens
```http
POST /user/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

### 4. Use access token for API calls
```http
GET /api/protected-resource
Authorization: Bearer <access_token>
```

### 5. Refresh token when expired
```http
POST /user/refresh-token
Content-Type: application/json

{
  "refreshToken": "<refresh_token>"
}
```

---

## Token Storage Recommendations

| Token Type | Recommended Storage | Reason |
|------------|---------------------|--------|
| Access Token | Memory (variable) | Short-lived, reduces XSS risk |
| Refresh Token | HttpOnly Cookie or Secure Storage | Long-lived, needs protection |

> **Security Note**: Never store tokens in `localStorage` for production applications. Use `httpOnly` cookies or secure native storage.

---

## Next Steps

- [Understand the difference between User and Admin auth](./USER_VS_ADMIN.md)
- [Learn the complete authentication flow](./AUTH_FLOW.md)
- [View all API endpoints](./API_ENDPOINTS.md)
