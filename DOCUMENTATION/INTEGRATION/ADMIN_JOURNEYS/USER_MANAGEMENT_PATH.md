# Admin User Management Path

This guide documents how administrators manage user accounts, including enabling/disabling accounts and managing user sessions.

---

## Journey Overview

```
┌──────────────┐    ┌──────────────┐    ┌──────────────────────────┐
│    Admin     │───▶│  Users List  │───▶│  Choose Action:          │
│   Dashboard  │    │              │    │  • View user details     │
│              │    │              │    │  • Enable/Disable account│
└──────────────┘    └──────────────┘    │  • View/Revoke sessions  │
                                        └──────────────────────────┘
```

**Authentication:** Admin role required

---

## Step 1: View Users List

### Access
**Page:** `/admin/users`

**API Call:**
```http
GET /api/users
Authorization: Bearer {admin-access-token}
x-api-key: {your-api-key}
```

### Display
Table showing:
- User name
- Email
- Member since
- Account status (Active/Disabled badge)
- Actions (View, Enable/Disable, Sessions)

---

## Step 2: Enable/Disable User Account

### API Call
```http
PATCH /api/admin/users/{userId}/status
Authorization: Bearer {admin-access-token}
Content-Type: application/json
x-api-key: {your-api-key}

{
  "active": false  // or true to enable
}
```

**Success (200 OK):**
```json
{
  "message": "User disabled successfully"
}
```

**Error - Cannot disable own account (400):**
```json
{
  "error": "You cannot disable your own account"
}
```

### Frontend
- Show confirmation dialog before disabling
- Update status badge immediately on success
- Disabled users cannot login

---

## Step 3: View User Sessions

### API Call
```http
GET /api/admin/users/{userId}/sessions
Authorization: Bearer {admin-access-token}
x-api-key: {your-api-key}
```

**Success (200 OK):**
```json
[
  {
    "id": "session-guid",
    "createdAt": "2025-01-27T10:00:00Z",
    "expiresAt": "2025-01-28T10:00:00Z"
  }
]
```

### Display
Table showing:
- Session creation time
- Expiration time
- Actions (Revoke individual session)
- "Revoke All Sessions" button

---

## Step 4: Revoke User Sessions

### Revoke All Sessions
```http
DELETE /api/admin/users/{userId}/sessions
Authorization: Bearer {admin-access-token}
x-api-key: {your-api-key}
```

**Success (200 OK):**
```json
{
  "message": "All user sessions revoked successfully"
}
```

**Error - Trying to revoke own sessions (400):**
```json
{
  "error": "You are about to revoke your own sessions, which will log you out. Use the user authentication endpoint instead for self-management."
}
```

### Revoke Specific Session
```http
DELETE /api/admin/users/{userId}/sessions/{sessionId}
Authorization: Bearer {admin-access-token}
x-api-key: {your-api-key}
```

**Success (200 OK):**
```json
{
  "message": "User session revoked successfully"
}
```

### Frontend
- Show confirmation dialog
- Warn that user will be logged out
- Update sessions list after revocation
- All actions are automatically logged to Admin Logs

---

## Important Safeguards

### Cannot Disable Own Account
- Frontend should disable the toggle/button for current admin's account
- Backend enforces this rule (returns 400)

### Cannot Revoke Own Sessions via Admin Panel
- Backend returns 400 error
- Suggest using user profile page instead

### All Actions Logged
- Every user management action is automatically logged
- Visible in Admin Logs with:
  - Action code (USER_ENABLED, USER_DISABLED, USER_SESSION_REVOKED)
  - Target user ID
  - Timestamp
  - Performing administrator

---

## Related Documentation
- [Admin Logs](../../FEATURES/ADMIN_FEATURES/ADMIN_LOGS.md)
- [Session Management](../AUTHENTIFICATION/SESSION_MANAGEMENT.md)
