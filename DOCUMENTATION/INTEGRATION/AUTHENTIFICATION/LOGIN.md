# Login & Token Management

[< Back to Overview](./README.md) | [Authentication Flow](./AUTH_FLOW.md)

---

## Overview

Login authenticates users and returns JWT tokens for API access. The system implements brute force protection through [Account Lockout](./ACCOUNT_LOCKOUT.md).

---

## Login Flow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Submit    │────>│   Verify    │────>│   Generate  │────>│   Return    │
│ Credentials │     │   Account   │     │   Tokens    │     │   Tokens    │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                           │
                    ┌──────┴──────┐
                    │   Checks:   │
                    │ - Locked?   │
                    │ - Activated?│
                    │ - Password? │
                    └─────────────┘
```

---

## API Endpoints

### User Login
```http
POST /user/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

### Admin Login
```http
POST /admin/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "SecurePassword123!"
}
```

---

## Success Response

**Status:** `200 OK`

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
  "accessTokenExpiration": "2024-01-15T12:00:00Z",
  "refreshTokenExpiration": "2024-02-14T11:00:00Z"
}
```

| Field | Description | Typical Lifetime |
|-------|-------------|------------------|
| `accessToken` | JWT for API authorization | ~1 hour |
| `refreshToken` | Token to get new access token | ~30 days |
| `accessTokenExpiration` | When access token expires | - |
| `refreshTokenExpiration` | When refresh token expires | - |

---

## Error Responses

| Status | Error | Cause | Action |
|--------|-------|-------|--------|
| 401 | "Invalid credentials" | Wrong email or password | Check credentials |
| 401 | "Le compte doit être activé." | Account not activated | Check email for activation link |
| 401 | "Account is locked. Please try again in X minute(s)." | Too many failed attempts | Wait or reset password |

---

## Frontend Implementation

### Login Function
```typescript
interface LoginDto {
  email: string;
  password: string;
}

interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiration: string;
  refreshTokenExpiration: string;
}

async function login(credentials: LoginDto): Promise<AuthTokens | null> {
  const response = await fetch('/user/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(credentials),
  });

  if (response.ok) {
    const tokens: AuthTokens = await response.json();

    // Store tokens securely
    storeTokens(tokens);

    return tokens;
  } else {
    const error = await response.json();

    // Handle specific errors
    if (error.error.includes('locked')) {
      // Extract remaining time and show lockout message
      showLockoutMessage(error.error);
    } else if (error.error.includes('activé')) {
      showActivationRequired();
    } else {
      showError('Invalid email or password');
    }

    return null;
  }
}
```

### Token Storage
```typescript
function storeTokens(tokens: AuthTokens): void {
  // Option 1: Memory (most secure for access token)
  authState.accessToken = tokens.accessToken;

  // Option 2: Session storage (cleared when tab closes)
  sessionStorage.setItem('refreshToken', tokens.refreshToken);

  // Store expiration times for refresh scheduling
  authState.accessTokenExpiration = new Date(tokens.accessTokenExpiration);
  authState.refreshTokenExpiration = new Date(tokens.refreshTokenExpiration);
}
```

### API Request with Token
```typescript
async function authenticatedFetch(url: string, options: RequestInit = {}): Promise<Response> {
  const accessToken = getAccessToken();

  const response = await fetch(url, {
    ...options,
    headers: {
      ...options.headers,
      'Authorization': `Bearer ${accessToken}`,
    },
  });

  // If unauthorized, try to refresh token
  if (response.status === 401) {
    const refreshed = await refreshTokens();
    if (refreshed) {
      // Retry with new token
      return authenticatedFetch(url, options);
    } else {
      // Redirect to login
      redirectToLogin();
    }
  }

  return response;
}
```

---

## What Happens on Login

1. **Account Lockout Check** - If `LockedUntil > now`, login is rejected
2. **Account Activation Check** - Unactivated accounts cannot log in
3. **Password Verification** - Password is verified against stored hash
4. **Failed Attempt Handling** - On failure, counter increments (locks at 5)
5. **Success Reset** - On success, failed attempts reset to 0
6. **Session Creation** - A new session is created with the refresh token
7. **Token Generation** - Access and refresh tokens are generated

---

## Security Notes

1. **Failed attempts persist** - Even after closing the browser
2. **Lockout is time-based** - 15 minutes by default
3. **Refresh tokens are single-use** - Each refresh consumes the token
4. **Sessions are tracked** - Each login creates a new session record

---

## Related Documentation

- [Token Refresh](./TOKEN_REFRESH.md)
- [Account Lockout](./ACCOUNT_LOCKOUT.md)
- [Session Management](./SESSION_MANAGEMENT.md)
- [Logout](./LOGOUT.md)
