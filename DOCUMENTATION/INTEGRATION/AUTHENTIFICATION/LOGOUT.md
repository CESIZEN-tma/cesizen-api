# Logout

[< Back to Overview](./README.md) | [Login](./LOGIN.md)

---

## Overview

Logout invalidates the current session by consuming the refresh token. This prevents the token from being used again.

---

## API Endpoints

### User Logout
```http
POST /user/logout
Content-Type: application/json

{
  "refreshToken": "your-refresh-token-here"
}
```

### Admin Logout
```http
POST /admin/logout
Content-Type: application/json

{
  "refreshToken": "your-refresh-token-here"
}
```

---

## Response

**Status:** `200 OK`

```json
{
  "message": "Logged out successfully."
}
```

> **Note:** Logout always returns success, even if the token was already invalid. This is for security - it doesn't reveal session state.

---

## What Happens on Logout

1. **Session Consumed** - The session associated with the refresh token is marked as consumed
2. **Token Invalidated** - The refresh token can no longer be used
3. **Access Token Remains Valid** - Until it expires naturally (typically ~1 hour)

---

## Frontend Implementation

### Logout Function
```typescript
async function logout(): Promise<void> {
  const refreshToken = getStoredRefreshToken();

  // Always clear local state, even if API call fails
  try {
    if (refreshToken) {
      await fetch('/user/logout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken }),
      });
    }
  } catch (error) {
    // Log but don't block logout
    console.error('Logout API call failed:', error);
  } finally {
    // Clear tokens regardless of API response
    clearAllTokens();

    // Clear application state
    clearUserState();

    // Redirect to home or login page
    window.location.href = '/';
  }
}
```

### Clear Tokens
```typescript
function clearAllTokens(): void {
  // Clear memory
  authState.accessToken = null;
  authState.refreshToken = null;

  // Clear storage
  sessionStorage.removeItem('refreshToken');
  localStorage.removeItem('refreshToken');

  // Clear any cookies if used
  document.cookie = 'refreshToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
}

function clearUserState(): void {
  // Clear user data from state management
  userStore.reset();

  // Clear any cached data
  queryClient.clear(); // If using React Query
}
```

---

## Logout from All Devices

To log out from all devices, use the [Session Management](./SESSION_MANAGEMENT.md) endpoints:

```typescript
async function logoutFromAllDevices(): Promise<void> {
  const refreshToken = getStoredRefreshToken();

  // Revoke all other sessions first
  await fetch('/user/sessions', {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${getAccessToken()}`,
      'X-Refresh-Token': refreshToken,
    },
  });

  // Then logout current session
  await logout();
}
```

---

## Security Considerations

### Access Token Still Valid After Logout
The access token remains valid until it expires. For high-security applications:

1. **Use short access token lifetimes** (15-60 minutes)
2. **Implement token blacklisting** (server-side)
3. **Check session validity on critical operations**

### Always Clear Client-Side
Even if the API call fails:
- Clear local storage
- Clear session storage
- Clear cookies
- Reset application state

This ensures the user is logged out locally even if the network is unavailable.

---

## UI Considerations

### Logout Button Placement
- Always visible in navigation
- Confirm before logout (optional)
- Show loading state during logout

### Post-Logout Redirect
```typescript
// Option 1: Redirect to home
window.location.href = '/';

// Option 2: Redirect to login
window.location.href = '/login';

// Option 3: Redirect to login with return URL
const returnUrl = encodeURIComponent(window.location.pathname);
window.location.href = `/login?returnUrl=${returnUrl}`;
```

---

## Related Documentation

- [Login & Token Management](./LOGIN.md)
- [Session Management](./SESSION_MANAGEMENT.md)
- [Token Refresh](./TOKEN_REFRESH.md)
