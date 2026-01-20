# Error Handling

[< Back to Overview](./README.md) | [API Endpoints](./API_ENDPOINTS.md)

---

## Error Response Format

All errors follow this format:

```json
{
  "error": "Error message here"
}
```

---

## Error Codes by Endpoint

### Registration Errors

| Status | Error | Cause | User Action |
|--------|-------|-------|-------------|
| 400 | "Password must be identical." | Passwords don't match | Re-enter matching passwords |
| 400 | "Email already exists" | Email already registered | Use different email or login |

### Account Activation Errors

| Status | Error | Cause | User Action |
|--------|-------|-------|-------------|
| 400 | "Invalid token." | Token doesn't exist | Request new confirmation email |
| 400 | "Token already used." | Token was consumed | Login normally |
| 400 | "Token expired." | Token has expired | Request new confirmation email |
| 400 | "User not found." | User deleted | Register again |

### Login Errors

| Status | Error | Cause | User Action |
|--------|-------|-------|-------------|
| 401 | "Invalid credentials" | Wrong email/password | Check credentials |
| 401 | "Le compte doit être activé." | Account not activated | Check email for activation link |
| 401 | "Account is locked. Please try again in X minute(s)." | Too many failed attempts | Wait or reset password |

### Token Refresh Errors

| Status | Error | Cause | User Action |
|--------|-------|-------|-------------|
| 401 | "Invalid or expired refresh token." | Token invalid/consumed | Login again |
| 401 | "User not found." | User deleted | Register again |
| 401 | "Account is not activated." | Account deactivated | Contact support |

### Password Reset Errors

| Status | Error | Cause | User Action |
|--------|-------|-------|-------------|
| 400 | "Passwords must match." | Passwords don't match | Re-enter matching passwords |
| 400 | "Invalid or expired reset token." | Token doesn't exist | Request new reset email |
| 400 | "This reset link has already been used." | Token consumed | Request new reset email |
| 400 | "This reset link has expired. Please request a new one." | Token expired | Request new reset email |
| 400 | "User not found." | User deleted | Register again |
| 400 | "Failed to send reset email. Please try again later." | Email service error | Try again later |

### Change Password Errors

| Status | Error | Cause | User Action |
|--------|-------|-------|-------------|
| 400 | "New passwords must match." | Passwords don't match | Re-enter matching passwords |
| 400 | "User not found." | User deleted | Contact support |
| 400 | "Current password is incorrect." | Wrong current password | Enter correct password |

### Session Management Errors

| Status | Error | Cause | User Action |
|--------|-------|-------|-------------|
| 404 | "Session not found." | Invalid session ID | Refresh session list |
| 400 | "Current session not found." | Invalid refresh token | Re-login |

---

## Frontend Error Handling

### Generic Error Handler
```typescript
interface ApiError {
  error: string;
}

async function handleApiResponse<T>(response: Response): Promise<T> {
  if (response.ok) {
    // Some endpoints return empty body
    const text = await response.text();
    return text ? JSON.parse(text) : null;
  }

  const error: ApiError = await response.json();
  throw new ApiException(response.status, error.error);
}

class ApiException extends Error {
  constructor(
    public status: number,
    public error: string
  ) {
    super(error);
    this.name = 'ApiException';
  }
}
```

### Error Display Component
```typescript
interface ErrorMessageProps {
  error: string;
}

function ErrorMessage({ error }: ErrorMessageProps): JSX.Element {
  // Map technical errors to user-friendly messages
  const userMessage = mapErrorToUserMessage(error);

  return (
    <div className="error-message" role="alert">
      {userMessage}
    </div>
  );
}

function mapErrorToUserMessage(error: string): string {
  const errorMap: Record<string, string> = {
    'Invalid credentials': 'The email or password you entered is incorrect.',
    'Le compte doit être activé.': 'Please check your email to activate your account.',
    'Email already exists': 'An account with this email already exists. Try logging in instead.',
    'Password must be identical.': 'The passwords you entered do not match.',
    'Current password is incorrect.': 'The current password you entered is incorrect.',
    'Invalid token.': 'This link is invalid. Please request a new one.',
    'Token expired.': 'This link has expired. Please request a new one.',
  };

  // Check for lockout message
  if (error.includes('Account is locked')) {
    return error; // Already user-friendly
  }

  return errorMap[error] || 'An unexpected error occurred. Please try again.';
}
```

### Error Handling by Context
```typescript
async function handleLoginError(error: ApiException): void {
  switch (error.error) {
    case 'Invalid credentials':
      showError('Invalid email or password');
      break;

    case 'Le compte doit être activé.':
      showError('Please activate your account first');
      showResendActivationOption();
      break;

    default:
      if (error.error.includes('locked')) {
        showLockoutMessage(error.error);
      } else {
        showError('Login failed. Please try again.');
      }
  }
}

async function handlePasswordResetError(error: ApiException): void {
  switch (error.error) {
    case 'Invalid or expired reset token.':
    case 'This reset link has expired. Please request a new one.':
    case 'This reset link has already been used.':
      showError('This reset link is no longer valid.');
      showRequestNewResetOption();
      break;

    case 'Passwords must match.':
      showError('The passwords you entered do not match.');
      break;

    default:
      showError('Password reset failed. Please try again.');
  }
}
```

---

## HTTP Status Code Handling

```typescript
async function handleResponse(response: Response): Promise<void> {
  switch (response.status) {
    case 200:
    case 201:
      // Success
      break;

    case 400:
      // Validation error - show error message
      const badRequest = await response.json();
      showValidationError(badRequest.error);
      break;

    case 401:
      // Unauthorized - may need to refresh token or re-login
      if (isTokenExpiredError(response)) {
        const refreshed = await refreshTokens();
        if (!refreshed) {
          redirectToLogin();
        }
      } else {
        const unauthorized = await response.json();
        showError(unauthorized.error);
      }
      break;

    case 403:
      // Forbidden - insufficient permissions
      showError('You do not have permission to perform this action.');
      break;

    case 404:
      // Not found
      showError('The requested resource was not found.');
      break;

    case 500:
      // Server error
      showError('A server error occurred. Please try again later.');
      break;

    default:
      showError('An unexpected error occurred.');
  }
}
```

---

## Retry Logic

For transient errors (network issues, 500 errors):

```typescript
async function fetchWithRetry<T>(
  url: string,
  options: RequestInit,
  maxRetries: number = 3
): Promise<T> {
  let lastError: Error | null = null;

  for (let attempt = 0; attempt < maxRetries; attempt++) {
    try {
      const response = await fetch(url, options);

      // Don't retry client errors (4xx)
      if (response.status >= 400 && response.status < 500) {
        return handleApiResponse(response);
      }

      // Retry server errors (5xx)
      if (response.status >= 500) {
        throw new Error(`Server error: ${response.status}`);
      }

      return handleApiResponse(response);
    } catch (error) {
      lastError = error as Error;
      // Wait before retry (exponential backoff)
      await new Promise(resolve =>
        setTimeout(resolve, Math.pow(2, attempt) * 1000)
      );
    }
  }

  throw lastError;
}
```

---

## Related Documentation

- [API Endpoints Summary](./API_ENDPOINTS.md)
- [Login](./LOGIN.md)
- [Account Lockout](./ACCOUNT_LOCKOUT.md)
