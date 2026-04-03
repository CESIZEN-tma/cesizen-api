# Password Reset

[< Back to Overview](./README.md) | [Account Lockout](./ACCOUNT_LOCKOUT.md)

---

## Overview

Password reset allows users who have forgotten their password to set a new one via email verification. This is also the recommended way to unlock a locked account.

---

## Password Reset Flow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Request   │────>│   Email     │────>│   Click     │────>│   Set New   │
│   Reset     │     │   Sent      │     │   Link      │     │   Password  │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                                                   │
                                                                   ▼
                                                            ┌─────────────┐
                                                            │   Login     │
                                                            │   Normally  │
                                                            └─────────────┘
```

---

## API Endpoints

### Step 1: Request Password Reset

```http
POST /user/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Response:** `200 OK`
```json
{
  "message": "If an account with that email exists, a password reset link has been sent."
}
```

> **Security Note:** The response is always the same whether the email exists or not. This prevents email enumeration attacks.

### Step 2: Reset Password with Token

```http
POST /user/reset-password
Content-Type: application/json

{
  "token": "reset-token-from-email",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

**Success Response:** `200 OK`
```json
{
  "message": "Password has been reset successfully. You can now login with your new password."
}
```

**Error Responses:**

| Status | Error | Cause |
|--------|-------|-------|
| 400 | "Passwords must match." | Passwords don't match |
| 400 | "Invalid or expired reset token." | Token doesn't exist |
| 400 | "This reset link has already been used." | Token consumed |
| 400 | "This reset link has expired. Please request a new one." | Token expired |

---

## Admin Endpoints

```http
POST /admin/forgot-password
POST /admin/reset-password
```

Same request/response format as user endpoints.

---

## Token Characteristics

| Property | Value |
|----------|-------|
| Expiration | 15 minutes |
| Single use | Yes (consumed after use) |
| Delivery | Email link |

---

## Email Sent

When a password reset is requested for a valid account:

1. **Reset Email** - Contains reset link with token
2. **Confirmation Email** - Sent after password is successfully changed

See: [Email Notifications](./EMAIL_NOTIFICATIONS.md)

---

## Frontend Implementation

### Forgot Password Form
```typescript
async function requestPasswordReset(email: string): Promise<void> {
  const response = await fetch('/user/forgot-password', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email }),
  });

  if (response.ok) {
    // Always show success message (don't reveal if email exists)
    showMessage('If an account with that email exists, you will receive a password reset link.');
  } else {
    showError('An error occurred. Please try again.');
  }
}
```

### Reset Password Page
```typescript
// URL: /reset-password?token=xxx

function ResetPasswordPage(): JSX.Element {
  const token = useQueryParam('token');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    try {
      const response = await fetch('/user/reset-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          token,
          newPassword: password,
          confirmPassword,
        }),
      });

      if (response.ok) {
        showMessage('Password reset successfully!');
        redirect('/login');
      } else {
        const data = await response.json();
        setError(data.error);
      }
    } catch {
      setError('An error occurred. Please try again.');
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <h1>Reset Your Password</h1>
      {error && <div className="error">{error}</div>}
      <input
        type="password"
        placeholder="New Password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        minLength={8}
        required
      />
      <input
        type="password"
        placeholder="Confirm Password"
        value={confirmPassword}
        onChange={(e) => setConfirmPassword(e.target.value)}
        required
      />
      <button type="submit">Reset Password</button>
    </form>
  );
}
```

### Deep Link Handling
```typescript
// Parse reset token from URL
function getResetToken(): string | null {
  const params = new URLSearchParams(window.location.search);
  return params.get('token');
}

// Validate token exists on page load
useEffect(() => {
  const token = getResetToken();
  if (!token) {
    redirect('/forgot-password');
  }
}, []);
```

---

## Security Considerations

### Token Expiration
Reset tokens expire after 15 minutes. If the user takes longer:
1. Show expiration error
2. Provide link to request new reset email

### Rate Limiting (Recommended)
Consider implementing rate limiting on the forgot-password endpoint to prevent abuse:
- Limit requests per email per hour
- Limit requests per IP per hour

### Token in URL
The token is passed in the URL. Consider:
- Using HTTPS (mandatory)
- Not logging URLs with tokens
- Clearing browser history after reset

---

## Unlocking Locked Accounts

Password reset can be used to unlock accounts that were locked due to failed login attempts:

```
Account Locked (5 failed attempts)
           │
           ▼
Request Password Reset
           │
           ▼
Set New Password
           │
           ▼
Login with New Password ──> Failed attempts reset to 0
```

---

## Related Documentation

- [Account Lockout](./ACCOUNT_LOCKOUT.md)
- [Change Password](./CHANGE_PASSWORD.md)
- [Email Notifications](./EMAIL_NOTIFICATIONS.md)
- [Login](./LOGIN.md)
