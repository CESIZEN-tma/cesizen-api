# API Endpoints Summary

[< Back to Overview](./README.md)

---

## User Authentication Endpoints

| Method | Endpoint | Auth | Description | Documentation |
|--------|----------|------|-------------|---------------|
| POST | `/user/register` | No | Register new user | [Registration](./REGISTRATION.md) |
| PUT | `/user/confirm-account/{token}` | No | Activate account | [Registration](./REGISTRATION.md) |
| POST | `/user/login` | No | Login and get tokens | [Login](./LOGIN.md) |
| POST | `/user/logout` | No | Logout and invalidate session | [Logout](./LOGOUT.md) |
| POST | `/user/refresh-token` | No | Get new tokens | [Token Refresh](./TOKEN_REFRESH.md) |
| POST | `/user/forgot-password` | No | Request password reset | [Password Reset](./PASSWORD_RESET.md) |
| POST | `/user/reset-password` | No | Reset password with token | [Password Reset](./PASSWORD_RESET.md) |
| POST | `/user/change-password` | Yes | Change password (authenticated) | [Change Password](./CHANGE_PASSWORD.md) |
| GET | `/user/sessions` | Yes | List active sessions | [Session Management](./SESSION_MANAGEMENT.md) |
| DELETE | `/user/sessions/{id}` | Yes | Revoke specific session | [Session Management](./SESSION_MANAGEMENT.md) |
| DELETE | `/user/sessions` | Yes | Revoke all other sessions | [Session Management](./SESSION_MANAGEMENT.md) |

---

## Admin Authentication Endpoints

| Method | Endpoint | Auth | Description | Documentation |
|--------|----------|------|-------------|---------------|
| POST | `/admin/register` | No | Register new admin | [Registration](./REGISTRATION.md) |
| PUT | `/admin/confirm-account/{token}` | No | Activate account | [Registration](./REGISTRATION.md) |
| POST | `/admin/login` | No | Login and get tokens | [Login](./LOGIN.md) |
| POST | `/admin/logout` | No | Logout and invalidate session | [Logout](./LOGOUT.md) |
| POST | `/admin/refresh-token` | No | Get new tokens | [Token Refresh](./TOKEN_REFRESH.md) |
| POST | `/admin/forgot-password` | No | Request password reset | [Password Reset](./PASSWORD_RESET.md) |
| POST | `/admin/reset-password` | No | Reset password with token | [Password Reset](./PASSWORD_RESET.md) |
| POST | `/admin/change-password` | Admin | Change password (authenticated) | [Change Password](./CHANGE_PASSWORD.md) |
| GET | `/admin/sessions` | Admin | List active sessions | [Session Management](./SESSION_MANAGEMENT.md) |
| DELETE | `/admin/sessions/{id}` | Admin | Revoke specific session | [Session Management](./SESSION_MANAGEMENT.md) |
| DELETE | `/admin/sessions` | Admin | Revoke all other sessions | [Session Management](./SESSION_MANAGEMENT.md) |

---

## Request/Response Reference

### Registration

**Request:**
```json
{
  "email": "string",
  "password": "string (min 8 chars)",
  "confirmPassword": "string",
  "firstName": "string",
  "lastName": "string"
}
```

**Response:** `201 Created` (empty body)

---

### Login

**Request:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "accessTokenExpiration": "datetime",
  "refreshTokenExpiration": "datetime"
}
```

---

### Refresh Token

**Request:**
```json
{
  "refreshToken": "string"
}
```

**Response:** Same as Login

---

### Logout

**Request:**
```json
{
  "refreshToken": "string"
}
```

**Response:**
```json
{
  "message": "Logged out successfully."
}
```

---

### Forgot Password

**Request:**
```json
{
  "email": "string"
}
```

**Response:**
```json
{
  "message": "If an account with that email exists, a password reset link has been sent."
}
```

---

### Reset Password

**Request:**
```json
{
  "token": "string",
  "newPassword": "string (min 8 chars)",
  "confirmPassword": "string"
}
```

**Response:**
```json
{
  "message": "Password has been reset successfully. You can now login with your new password."
}
```

---

### Change Password

**Headers:**
```
Authorization: Bearer <access_token>
X-Refresh-Token: <refresh_token>
```

**Request:**
```json
{
  "currentPassword": "string",
  "newPassword": "string (min 8 chars)",
  "confirmPassword": "string"
}
```

**Response:**
```json
{
  "message": "Password changed successfully."
}
```

---

### Get Sessions

**Headers:**
```
Authorization: Bearer <access_token>
X-Refresh-Token: <refresh_token>
```

**Response:**
```json
[
  {
    "id": "guid",
    "createdAt": "datetime",
    "expiresAt": "datetime",
    "isCurrentSession": "boolean"
  }
]
```

---

### Revoke Session

**Headers:**
```
Authorization: Bearer <access_token>
```

**Response:**
```json
{
  "message": "Session revoked successfully."
}
```

---

### Revoke All Other Sessions

**Headers:**
```
Authorization: Bearer <access_token>
X-Refresh-Token: <refresh_token>
```

**Response:**
```json
{
  "message": "All other sessions revoked successfully."
}
```

---

## Authentication Headers

### Authorization Header
Used for protected endpoints:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### X-Refresh-Token Header
Used to identify current session:
```
X-Refresh-Token: dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...
```

---

## HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful request |
| 201 | Created | Resource created (registration) |
| 400 | Bad Request | Validation error |
| 401 | Unauthorized | Invalid credentials or token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |

---

## Related Documentation

- [Error Handling](./ERROR_HANDLING.md)
- [User vs Admin Authentication](./USER_VS_ADMIN.md)
