# User vs Admin Authentication

[< Back to Overview](./README.md)

---

## Overview

The CesiZen API provides **two separate authentication systems**:

| Aspect | User Auth | Admin Auth |
|--------|-----------|------------|
| Base Route | `/user/*` | `/admin/*` |
| Purpose | End-user application access | Administrative panel access |
| JWT Claims | `sub` (user ID) | `sub` (admin ID) + `role: Administrator` |
| Session Storage | `Sessions` table | `AdminSessions` table |
| Authorization | `[Authorize]` | `[Authorize(Roles = "Administrator")]` |

---

## Route Comparison

### User Endpoints
```
POST   /user/register
PUT    /user/confirm-account/{token}
POST   /user/login
POST   /user/logout
POST   /user/refresh-token
POST   /user/forgot-password
POST   /user/reset-password
POST   /user/change-password
GET    /user/sessions
DELETE /user/sessions/{id}
DELETE /user/sessions
```

### Admin Endpoints
```
POST   /admin/register
PUT    /admin/confirm-account/{token}
POST   /admin/login
POST   /admin/logout
POST   /admin/refresh-token
POST   /admin/forgot-password
POST   /admin/reset-password
POST   /admin/change-password
GET    /admin/sessions
DELETE /admin/sessions/{id}
DELETE /admin/sessions
```

---

## JWT Token Differences

### User Token Payload
```json
{
  "sub": "user-guid-here",
  "exp": 1234567890,
  "iat": 1234567890
}
```

### Admin Token Payload
```json
{
  "sub": "admin-guid-here",
  "role": "Administrator",
  "exp": 1234567890,
  "iat": 1234567890
}
```

---

## Frontend Implementation

### Separate Auth Contexts

For applications with both user and admin interfaces, maintain **separate authentication contexts**:

```typescript
// User authentication
const userAuth = {
  login: (credentials) => fetch('/user/login', ...),
  logout: () => fetch('/user/logout', ...),
  refresh: () => fetch('/user/refresh-token', ...),
};

// Admin authentication
const adminAuth = {
  login: (credentials) => fetch('/admin/login', ...),
  logout: () => fetch('/admin/logout', ...),
  refresh: () => fetch('/admin/refresh-token', ...),
};
```

### Token Storage Separation

Store user and admin tokens separately to avoid conflicts:

```typescript
// User tokens
sessionStorage.setItem('user_access_token', token);

// Admin tokens
sessionStorage.setItem('admin_access_token', token);
```

---

## Authorization Headers

### User API Calls
```http
GET /api/user-resource
Authorization: Bearer <user_access_token>
```

### Admin API Calls
```http
GET /api/admin-resource
Authorization: Bearer <admin_access_token>
```

---

## Security Considerations

1. **Separate Sessions**: User and admin sessions are stored in different tables
2. **Role Verification**: Admin endpoints verify the `Administrator` role claim
3. **Independent Lockout**: User and admin accounts have separate lockout counters
4. **Separate Password Reset**: Each system has its own password reset tokens

---

## Related Documentation

- [Login & Token Management](./LOGIN.md)
- [Token Refresh](./TOKEN_REFRESH.md)
- [API Endpoints Summary](./API_ENDPOINTS.md)
