# Account Lockout (Brute Force Protection)

[< Back to Overview](./README.md) | [Login](./LOGIN.md)

---

## Overview

The account lockout system protects against brute force attacks by temporarily locking accounts after multiple failed login attempts.

---

## Lockout Parameters

| Parameter | Value |
|-----------|-------|
| Max Failed Attempts | 5 |
| Lockout Duration | 15 minutes |
| Counter Reset | On successful login |

---

## How It Works

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          LOCKOUT MECHANISM                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Attempt 1: Failed ──> FailedLoginAttempts = 1                         │
│  Attempt 2: Failed ──> FailedLoginAttempts = 2                         │
│  Attempt 3: Failed ──> FailedLoginAttempts = 3                         │
│  Attempt 4: Failed ──> FailedLoginAttempts = 4                         │
│  Attempt 5: Failed ──> FailedLoginAttempts = 5 ──> ACCOUNT LOCKED      │
│                                                    LockedUntil = now + 15min
│                                                                         │
│  During lockout:                                                        │
│  - All login attempts rejected                                          │
│  - Error shows remaining lockout time                                   │
│                                                                         │
│  After lockout expires OR successful login:                             │
│  - FailedLoginAttempts = 0                                              │
│  - LockedUntil = null                                                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Lockout Error Response

When an account is locked, login returns:

**Status:** `401 Unauthorized`

```json
{
  "error": "Account is locked. Please try again in 12 minute(s)."
}
```

The remaining time is calculated dynamically.

---

## Frontend Implementation

### Detecting Lockout
```typescript
async function login(credentials: LoginDto): Promise<LoginResult> {
  const response = await fetch('/user/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(credentials),
  });

  if (!response.ok) {
    const error = await response.json();

    // Check if account is locked
    if (error.error.includes('Account is locked')) {
      // Extract remaining minutes from error message
      const match = error.error.match(/(\d+) minute/);
      const remainingMinutes = match ? parseInt(match[1]) : 15;

      return {
        success: false,
        locked: true,
        lockoutMinutes: remainingMinutes,
      };
    }

    return {
      success: false,
      locked: false,
      error: error.error,
    };
  }

  return {
    success: true,
    tokens: await response.json(),
  };
}
```

### Lockout UI Component
```typescript
interface LockoutMessageProps {
  minutes: number;
}

function LockoutMessage({ minutes }: LockoutMessageProps): JSX.Element {
  const [remainingTime, setRemainingTime] = useState(minutes * 60);

  useEffect(() => {
    const timer = setInterval(() => {
      setRemainingTime((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [minutes]);

  const displayMinutes = Math.floor(remainingTime / 60);
  const displaySeconds = remainingTime % 60;

  if (remainingTime === 0) {
    return <p>You can try logging in again.</p>;
  }

  return (
    <div className="lockout-message">
      <h3>Account Temporarily Locked</h3>
      <p>Too many failed login attempts.</p>
      <p>
        Please wait <strong>{displayMinutes}:{displaySeconds.toString().padStart(2, '0')}</strong> before trying again.
      </p>
      <p>
        Or <a href="/forgot-password">reset your password</a> to unlock immediately.
      </p>
    </div>
  );
}
```

---

## Unlocking an Account

There are two ways to unlock an account:

### 1. Wait for Lockout to Expire
After 15 minutes, the lockout automatically expires and the user can attempt to log in again.

### 2. Reset Password
Using the [Password Reset](./PASSWORD_RESET.md) feature allows immediate access:

```
User locked out
      │
      ▼
Click "Forgot Password"
      │
      ▼
Receive reset email
      │
      ▼
Set new password
      │
      ▼
Login with new password ──> FailedLoginAttempts = 0
```

---

## Counter Reset Behavior

| Event | Failed Attempts | Locked Until |
|-------|-----------------|--------------|
| Failed login (< 5) | Incremented | Unchanged |
| Failed login (= 5) | Set to 5 | Set to now + 15min |
| Successful login | Reset to 0 | Cleared (null) |
| Password reset | Unchanged* | Unchanged* |
| Lockout expires | Unchanged | Naturally expired |

*Password reset doesn't directly reset the counter, but successful login after reset does.

---

## Security Notes

1. **Lockout persists across sessions** - Closing the browser doesn't reset the counter
2. **Email enumeration protection** - Same error message for wrong password and locked account would reveal if email exists. Current implementation shows lockout time only for valid accounts.
3. **DoS consideration** - Attackers could lock legitimate users. Password reset provides recovery.
4. **Admin accounts have separate lockout** - User lockout doesn't affect admin and vice versa

---

## Testing Lockout

To test the lockout functionality:

1. Attempt to login with wrong password 5 times
2. Verify lockout error message appears
3. Wait 15 minutes OR use password reset
4. Verify login works again

---

## Related Documentation

- [Login & Token Management](./LOGIN.md)
- [Password Reset](./PASSWORD_RESET.md)
- [Error Handling](./ERROR_HANDLING.md)
