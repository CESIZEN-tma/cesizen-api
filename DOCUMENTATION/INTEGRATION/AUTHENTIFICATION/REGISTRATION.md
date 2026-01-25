# Registration & Account Activation

[< Back to Overview](./README.md) | [Authentication Flow](./AUTH_FLOW.md)

---

## Overview

New accounts require email verification before they can log in. This prevents fake account creation and ensures email ownership.

---

## Registration Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Submit     │────>│   Account    │────>│    Email     │────>│   Account    │
│   Form       │     │   Created    │     │    Sent      │     │   Active     │
│              │     │  (inactive)  │     │              │     │              │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
```

---

## API Endpoints

### Register User
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

**Success Response:** `201 Created`

**Error Responses:**
| Status | Error | Cause |
|--------|-------|-------|
| 400 | "Password must be identical." | Passwords don't match |
| 400 | "Email already exists" | Email already registered |

### Register Admin
```http
POST /admin/register
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "firstName": "Admin",
  "lastName": "User"
}
```

---

## Account Activation

After registration, an email is sent with a confirmation link.

### Confirm Account
```http
PUT /user/confirm-account/{token}
```

**Success Response:** `200 OK`
```json
{
  "message": "Account activated successfully."
}
```

**Error Responses:**
| Status | Error | Cause |
|--------|-------|-------|
| 400 | "Invalid token." | Token doesn't exist |
| 400 | "Token already used." | Token was already consumed |
| 400 | "Token expired." | Token has expired |

---

## Email Sent

When registration is successful, an email is sent containing:
- Welcome message with user's name
- Confirmation link with unique token
- Token expiration information

See: [Email Notifications](./EMAIL_NOTIFICATIONS.md)

---

## Frontend Implementation

### Registration Form
```typescript
interface RegisterDto {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
}

async function register(data: RegisterDto): Promise<void> {
  const response = await fetch('/user/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });

  if (response.status === 201) {
    // Show success message: "Check your email to activate your account"
    showMessage('Registration successful! Please check your email.');
  } else {
    const error = await response.json();
    showError(error.error);
  }
}
```

### Activation Page
```typescript
async function activateAccount(token: string): Promise<void> {
  const response = await fetch(`/user/confirm-account/${token}`, {
    method: 'PUT',
  });

  if (response.ok) {
    // Redirect to login page
    showMessage('Account activated! You can now log in.');
    redirect('/login');
  } else {
    const error = await response.json();
    showError(error.error);
  }
}
```

---

## Password Requirements

| Requirement | Value |
|-------------|-------|
| Minimum length | 8 characters |
| Confirmation | Must match password field |

---

## Important Notes

1. **Account is inactive until confirmed** - Login attempts will fail with "Le compte doit être activé."
2. **Tokens are single-use** - Once used, the token cannot be reused
3. **Tokens expire** - Users must request a new confirmation email if the token expires
4. **Email uniqueness** - Each email can only be registered once

---

## Related Documentation

- [Login & Token Management](./LOGIN.md)
- [Email Notifications](./EMAIL_NOTIFICATIONS.md)
- [User vs Admin Authentication](./USER_VS_ADMIN.md)
