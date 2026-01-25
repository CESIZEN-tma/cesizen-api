# Change Password

[< Back to Overview](./README.md) | [Session Management](./SESSION_MANAGEMENT.md)

---

## Overview

Change password allows authenticated users to update their password. Unlike [Password Reset](./PASSWORD_RESET.md), this requires knowing the current password.

---

## Security Features

When password is changed:
1. **Current password verified** - Prevents unauthorized changes
2. **All other sessions revoked** - Logs out from all other devices
3. **Confirmation email sent** - Alerts user of the change

---

## API Endpoint

### User Change Password

```http
POST /user/change-password
Authorization: Bearer <access_token>
X-Refresh-Token: <refresh_token>
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

### Admin Change Password

```http
POST /admin/change-password
Authorization: Bearer <access_token>
X-Refresh-Token: <refresh_token>
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

---

## Response

**Success:** `200 OK`
```json
{
  "message": "Password changed successfully."
}
```

**Error Responses:**

| Status | Error | Cause |
|--------|-------|-------|
| 400 | "New passwords must match." | Passwords don't match |
| 400 | "User not found." | User deleted |
| 400 | "Current password is incorrect." | Wrong current password |

---

## What Happens

```
User submits change password request
              │
              ▼
    ┌─────────────────────┐
    │ Verify current      │──No──> "Current password is incorrect"
    │ password            │
    └──────────┬──────────┘
               │ Yes
               ▼
    Hash and save new password
               │
               ▼
    Revoke all other sessions
               │
               ▼
    Send confirmation email
               │
               ▼
    Return success ✓
```

---

## Frontend Implementation

### Change Password Form
```typescript
interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

async function changePassword(data: ChangePasswordDto): Promise<boolean> {
  const response = await fetch('/user/change-password', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${getAccessToken()}`,
      'X-Refresh-Token': getRefreshToken(),
    },
    body: JSON.stringify(data),
  });

  if (response.ok) {
    return true;
  }

  const error = await response.json();
  throw new Error(error.error);
}
```

### Change Password Component
```typescript
function ChangePasswordForm(): JSX.Element {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');

    // Client-side validation
    if (newPassword !== confirmPassword) {
      setError('New passwords do not match');
      return;
    }

    if (newPassword.length < 8) {
      setError('Password must be at least 8 characters');
      return;
    }

    if (newPassword === currentPassword) {
      setError('New password must be different from current password');
      return;
    }

    try {
      await changePassword({
        currentPassword,
        newPassword,
        confirmPassword,
      });

      setSuccess(true);
      // Note: Other sessions are now revoked
      // Current session remains valid
    } catch (err) {
      setError(err.message);
    }
  };

  if (success) {
    return (
      <div className="success-message">
        <h2>Password Changed Successfully</h2>
        <p>Your password has been updated.</p>
        <p>You have been logged out from all other devices.</p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2>Change Password</h2>

      {error && <div className="error">{error}</div>}

      <div className="form-group">
        <label>Current Password</label>
        <input
          type="password"
          value={currentPassword}
          onChange={(e) => setCurrentPassword(e.target.value)}
          required
        />
      </div>

      <div className="form-group">
        <label>New Password</label>
        <input
          type="password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          minLength={8}
          required
        />
      </div>

      <div className="form-group">
        <label>Confirm New Password</label>
        <input
          type="password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          required
        />
      </div>

      <button type="submit">Change Password</button>

      <p className="info">
        <strong>Note:</strong> Changing your password will log you out from all other devices.
      </p>
    </form>
  );
}
```

---

## Password Requirements

| Requirement | Value |
|-------------|-------|
| Minimum length | 8 characters |
| Must match | confirmPassword must equal newPassword |

---

## Session Behavior

After successful password change:

| Session | Status |
|---------|--------|
| Current session | Remains valid |
| All other sessions | Revoked |

This means:
- User stays logged in on current device
- User is logged out everywhere else
- Other devices will need to re-authenticate

---

## Security Considerations

### Why Require Current Password?
Prevents unauthorized password changes if:
- Device is stolen but unlocked
- Session token is compromised
- Someone has temporary access to the device

### Why Revoke Other Sessions?
If password was changed due to suspected compromise:
- Attacker's sessions are invalidated
- User retains access on trusted device
- Clean security slate

### Confirmation Email
A confirmation email is sent to alert the user. If they didn't change the password, they should:
1. Change password again immediately
2. Review active sessions
3. Contact support if needed

---

## Change Password vs Reset Password

| Feature | Change Password | Reset Password |
|---------|-----------------|----------------|
| Requires authentication | Yes | No |
| Requires current password | Yes | No |
| Requires email verification | No | Yes |
| Revokes other sessions | Yes | No |
| Can unlock account | No | Yes |

---

## Related Documentation

- [Password Reset](./PASSWORD_RESET.md)
- [Session Management](./SESSION_MANAGEMENT.md)
- [Email Notifications](./EMAIL_NOTIFICATIONS.md)
- [Login](./LOGIN.md)
