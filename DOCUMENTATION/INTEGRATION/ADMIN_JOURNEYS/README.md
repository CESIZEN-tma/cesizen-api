# Admin Journeys - Administrator Workflows

This section documents the paths administrators take when managing the CesiZen application. Admin users have elevated privileges to manage users, content, quizzes, and view system logs.

---

## Overview

Administrators can:
- Manage user accounts (enable/disable, view sessions)
- Manage other administrators (create, edit, delete)
- Create and manage content (pages, tags, menus)
- Create and manage quizzes
- View admin activity logs

---

## Available Admin Journeys

### User Management
- [**User Management Path**](./USER_MANAGEMENT_PATH.md) - Manage user accounts
  - View users → Enable/disable accounts → Manage sessions

### Content & Quiz Management
- **Content Creation Path** - Manage information pages, tags, and navigation
- **Quiz Creation Path** - Build quizzes with questions and response options

### System Administration
- **Admin Logs Path** - View and filter admin activity logs

---

## Quick Reference

| Journey | Purpose | Key Actions |
|---------|---------|-------------|
| **User Management** | Manage user accounts | Enable/Disable, View/Revoke sessions |
| **Content Creation** | Manage public content | Create pages, tags, menus |
| **Quiz Creation** | Build quizzes | Add questions, configure options |
| **Admin Logs** | Audit admin actions | View, filter, export logs |

---

## Admin Authentication

**Login Endpoint:** `POST /admin/login`

**Admin JWT Token includes role claim:**
```json
{
  "nameid": "admin-guid",
  "role": "Administrator",
  "exp": 1706356800
}
```

**Related:** [User vs Admin Authentication](../AUTHENTIFICATION/USER_VS_ADMIN.md)

---

## Next Steps

Choose an admin journey above to learn the detailed workflow.
