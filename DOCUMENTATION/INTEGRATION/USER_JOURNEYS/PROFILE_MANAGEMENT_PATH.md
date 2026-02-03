# Profile Management Path - Account Settings & Security

This guide documents how users manage their account settings, profile information, password, and sessions.

---

## Journey Overview

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  Dashboard   │───▶│    Profile   │───▶│  Edit Info   │
│  or Menu     │    │     Page     │    │   or          │
│              │    │              │    │  Security     │
└──────────────┘    └──────────────┘    └──────────────┘
```

**Authentication:** Required

---

## Access Profile Page

### User Action
From anywhere in the app, click:
- User avatar/profile picture
- "Profile" menu item
- Account settings icon

### Frontend Display
**Page:** `/profile`

**API Call on Load:**
```http
GET /api/users/profile
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

**Response (200 OK):**
```json
{
  "id": "user-guid",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "memberSince": "2025-01-20T10:00:00Z",
  "thumbnailUrl": "https://example.com/avatars/john.jpg",
  "accountActivated": true,
  "active": true
}
```

---

## Update Profile Information

### Frontend Display
Edit mode for profile fields:
- First Name
- Last Name
- Thumbnail URL (or file upload)

**API Call:**
```http
PUT /api/users/profile
Authorization: Bearer {access-token}
Content-Type: application/json
x-api-key: {your-api-key}

{
  "firstName": "John",
  "lastName": "Smith",
  "thumbnailUrl": "https://example.com/new-avatar.jpg"
}
```

**Success (200 OK):** Profile updated

---

## Change Password

**Related:** [Change Password Documentation](../AUTHENTIFICATION/CHANGE_PASSWORD.md)

---

## Related Documentation
- [Authentication Overview](../AUTHENTIFICATION/README.md)
- [Session Management](../AUTHENTIFICATION/SESSION_MANAGEMENT.md)
