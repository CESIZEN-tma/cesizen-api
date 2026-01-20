# Token Refresh

[< Back to Overview](./README.md) | [Login](./LOGIN.md)

---

## Overview

Access tokens are short-lived for security. When they expire, use the refresh token to obtain new tokens without requiring the user to log in again.

---

## How It Works

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Access Token   │────>│  Send Refresh   │────>│  Receive New    │
│    Expired      │     │     Token       │     │     Tokens      │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  Old Session    │
                        │   Consumed      │
                        │  New Session    │
                        │   Created       │
                        └─────────────────┘
```

---

## API Endpoints

### User Token Refresh
```http
POST /user/refresh-token
Content-Type: application/json

{
  "refreshToken": "your-refresh-token-here"
}
```

### Admin Token Refresh
```http
POST /admin/refresh-token
Content-Type: application/json

{
  "refreshToken": "your-refresh-token-here"
}
```

---

## Success Response

**Status:** `200 OK`

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "bmV3IHJlZnJlc2ggdG9rZW4...",
  "accessTokenExpiration": "2024-01-15T13:00:00Z",
  "refreshTokenExpiration": "2024-02-14T12:00:00Z"
}
```

> **Important:** You receive a **new refresh token**. The old one is invalidated.

---

## Error Responses

| Status | Error | Cause | Action |
|--------|-------|-------|--------|
| 401 | "Invalid or expired refresh token." | Token invalid/expired/consumed | Redirect to login |
| 401 | "User not found." | User deleted | Redirect to login |
| 401 | "Account is not activated." | Account deactivated | Contact support |

---

## Frontend Implementation

### Refresh Function
```typescript
async function refreshTokens(): Promise<boolean> {
  const refreshToken = getStoredRefreshToken();

  if (!refreshToken) {
    return false;
  }

  try {
    const response = await fetch('/user/refresh-token', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });

    if (response.ok) {
      const tokens = await response.json();

      // Store the NEW tokens (important: refresh token changes!)
      storeTokens(tokens);

      return true;
    } else {
      // Refresh failed - clear tokens and redirect to login
      clearTokens();
      return false;
    }
  } catch (error) {
    console.error('Token refresh failed:', error);
    return false;
  }
}
```

### Automatic Token Refresh
```typescript
// Option 1: Refresh before expiration
function scheduleTokenRefresh(expirationTime: Date): void {
  const now = new Date();
  const timeUntilExpiry = expirationTime.getTime() - now.getTime();

  // Refresh 5 minutes before expiration
  const refreshTime = timeUntilExpiry - (5 * 60 * 1000);

  if (refreshTime > 0) {
    setTimeout(async () => {
      await refreshTokens();
      // Schedule next refresh
      scheduleTokenRefresh(authState.accessTokenExpiration);
    }, refreshTime);
  }
}

// Option 2: Refresh on 401 response (shown in LOGIN.md)
```

### Interceptor Pattern (Axios example)
```typescript
import axios from 'axios';

const api = axios.create({ baseURL: '/api' });

// Request interceptor - add token
api.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor - handle 401
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      const refreshed = await refreshTokens();

      if (refreshed) {
        // Retry with new token
        originalRequest.headers.Authorization = `Bearer ${getAccessToken()}`;
        return api(originalRequest);
      }
    }

    return Promise.reject(error);
  }
);
```

---

## Token Rotation Security

Each refresh creates a **new session** and **consumes the old one**:

```
Login:     RT1 created (Session 1)
Refresh 1: RT1 consumed, RT2 created (Session 2)
Refresh 2: RT2 consumed, RT3 created (Session 3)
...
```

This prevents:
- **Token reuse attacks** - Stolen tokens become invalid after first use
- **Session hijacking** - Attacker cannot maintain access indefinitely

---

## Edge Cases

### Concurrent Refresh Requests
If two requests try to refresh the same token:
- First request succeeds
- Second request fails (token already consumed)

**Solution:** Implement a refresh lock:
```typescript
let isRefreshing = false;
let refreshPromise: Promise<boolean> | null = null;

async function safeRefreshTokens(): Promise<boolean> {
  if (isRefreshing) {
    // Wait for ongoing refresh
    return refreshPromise!;
  }

  isRefreshing = true;
  refreshPromise = refreshTokens();

  try {
    return await refreshPromise;
  } finally {
    isRefreshing = false;
    refreshPromise = null;
  }
}
```

### Offline Handling
If refresh fails due to network issues:
1. Queue the original request
2. Retry when back online
3. If token expired while offline, redirect to login

---

## When Refresh Fails

If refresh returns an error, the user must log in again:

```typescript
async function handleRefreshFailure(): void {
  // Clear all stored tokens
  clearTokens();

  // Clear application state
  clearUserState();

  // Redirect to login with return URL
  const returnUrl = encodeURIComponent(window.location.pathname);
  window.location.href = `/login?returnUrl=${returnUrl}`;
}
```

---

## Related Documentation

- [Login & Token Management](./LOGIN.md)
- [Session Management](./SESSION_MANAGEMENT.md)
- [Logout](./LOGOUT.md)
