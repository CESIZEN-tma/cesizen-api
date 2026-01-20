# Authentication Flow

[< Back to Overview](./README.md)

---

## Complete Authentication Lifecycle

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         AUTHENTICATION LIFECYCLE                            │
└─────────────────────────────────────────────────────────────────────────────┘

    ┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
    │ REGISTER │────>│  EMAIL   │────>│ ACTIVATE │────>│  LOGIN   │
    └──────────┘     │  SENT    │     │ ACCOUNT  │     └────┬─────┘
                     └──────────┘     └──────────┘          │
                                                            ▼
    ┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
    │  LOGOUT  │<────│  REVOKE  │<────│  USE     │<────│ RECEIVE  │
    │          │     │ SESSIONS │     │  API     │     │ TOKENS   │
    └──────────┘     └──────────┘     └────┬─────┘     └──────────┘
                                           │
                                           ▼
                                      ┌──────────┐
                                      │ REFRESH  │
                                      │  TOKEN   │
                                      └──────────┘
```

---

## Step-by-Step Flow

### 1. Registration
See: [Registration & Account Activation](./REGISTRATION.md)

```
User submits registration form
        │
        ▼
    API creates account (inactive)
        │
        ▼
    Confirmation email sent
        │
        ▼
    User clicks email link
        │
        ▼
    Account activated ✓
```

### 2. Login
See: [Login & Token Management](./LOGIN.md)

```
User submits credentials
        │
        ▼
    ┌─────────────────┐
    │ Account locked? │──Yes──> Return lockout error
    └────────┬────────┘
             │ No
             ▼
    ┌─────────────────┐
    │ Valid password? │──No───> Increment failed attempts
    └────────┬────────┘         (Lock if >= 5)
             │ Yes
             ▼
    Reset failed attempts
             │
             ▼
    Generate tokens
             │
             ▼
    Create session
             │
             ▼
    Return tokens ✓
```

### 3. API Usage
```
Frontend makes API request
        │
        ▼
    Include Authorization header
    "Bearer <access_token>"
        │
        ▼
    ┌─────────────────┐
    │ Token valid?    │──No───> Return 401 Unauthorized
    └────────┬────────┘
             │ Yes
             ▼
    Process request ✓
```

### 4. Token Refresh
See: [Token Refresh](./TOKEN_REFRESH.md)

```
Access token expired
        │
        ▼
    Send refresh token
        │
        ▼
    ┌─────────────────┐
    │ Session valid?  │──No───> Return error (re-login required)
    └────────┬────────┘
             │ Yes
             ▼
    Consume old session
             │
             ▼
    Create new session
             │
             ▼
    Return new tokens ✓
```

### 5. Logout
See: [Logout](./LOGOUT.md)

```
User clicks logout
        │
        ▼
    Send refresh token
        │
        ▼
    Consume/invalidate session
        │
        ▼
    Clear local tokens ✓
```

---

## Token Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                        TOKEN TIMELINE                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  LOGIN                    REFRESH                    LOGOUT     │
│    │                         │                          │       │
│    ▼                         ▼                          ▼       │
│  ┌─────┐                   ┌─────┐                    ┌─────┐   │
│  │ AT1 │ ════════════════> │ AT2 │ ═══════════════>  │ END │   │
│  └─────┘   (1 hour)        └─────┘    (1 hour)       └─────┘   │
│                                                                 │
│  ┌─────┐                   ┌─────┐                              │
│  │ RT1 │ ─ ─ consumed ─ ─> │ RT2 │ ─ ─ consumed ─ ─>           │
│  └─────┘                   └─────┘                              │
│                                                                 │
│  AT = Access Token (short-lived)                                │
│  RT = Refresh Token (long-lived, single-use)                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Error Recovery Flows

### Expired Access Token
```
API returns 401
        │
        ▼
    Call refresh endpoint
        │
        ├──Success──> Retry original request with new token
        │
        └──Failure──> Redirect to login
```

### Locked Account
See: [Account Lockout](./ACCOUNT_LOCKOUT.md)

```
Login returns lockout error
        │
        ▼
    Display remaining lockout time
        │
        ▼
    Wait or use password reset
```

### Forgotten Password
See: [Password Reset](./PASSWORD_RESET.md)

```
User clicks "Forgot Password"
        │
        ▼
    Submit email
        │
        ▼
    Check email for reset link
        │
        ▼
    Set new password
        │
        ▼
    Login with new credentials
```

---

## Related Documentation

- [Registration & Account Activation](./REGISTRATION.md)
- [Login & Token Management](./LOGIN.md)
- [Token Refresh](./TOKEN_REFRESH.md)
- [Account Lockout](./ACCOUNT_LOCKOUT.md)
