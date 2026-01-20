# Email Notifications

[< Back to Overview](./README.md)

---

## Overview

The authentication system sends emails for various security-related events. These emails help users verify their identity and stay informed about account activity.

---

## Email Types

| Event | Email Sent | When |
|-------|------------|------|
| Registration | Account Confirmation | After successful registration |
| Password Reset Request | Reset Link | After forgot-password request |
| Password Reset Complete | Confirmation | After password is reset |
| Password Change | Confirmation | After password is changed |

---

## Email Details

### 1. Account Confirmation Email

**Trigger:** User or Admin registration

**Sent To:** Registered email address

**Subject:** "Confirmation de création de compte" (or admin variant)

**Contains:**
- Welcome message with user's name
- Confirmation link with unique token
- Instructions to activate account

**Token Expiration:** 24 hours

**User Action Required:** Click the confirmation link

```
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│  Subject: Confirmation de création de compte                 │
│                                                              │
│  ─────────────────────────────────────────────────────────  │
│                                                              │
│  Hello John Doe,                                             │
│                                                              │
│  Thank you for registering. Please click the link below      │
│  to activate your account:                                   │
│                                                              │
│  [Activate Account]                                          │
│                                                              │
│  This link will expire in 24 hours.                          │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

### 2. Password Reset Email

**Trigger:** Forgot password request (for existing, activated accounts)

**Sent To:** User's email address

**Subject:** "Réinitialisation de votre mot de passe"

**Contains:**
- Reset link with unique token
- Expiration warning (15 minutes)
- Security notice if not requested

**Token Expiration:** 15 minutes

**User Action Required:** Click the reset link and set new password

```
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│  Subject: Réinitialisation de votre mot de passe             │
│                                                              │
│  ─────────────────────────────────────────────────────────  │
│                                                              │
│  Hello John Doe,                                             │
│                                                              │
│  You have requested to reset your password.                  │
│  Click the link below to set a new password:                 │
│                                                              │
│  [Reset Password]                                            │
│                                                              │
│  This link will expire in 15 minutes.                        │
│                                                              │
│  If you did not request this, please ignore this email.      │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

### 3. Password Reset Confirmation Email

**Trigger:** Successful password reset OR password change

**Sent To:** User's email address

**Subject:** "Votre mot de passe a été modifié"

**Contains:**
- Confirmation that password was changed
- Security alert if not done by user
- Instructions if unauthorized

**User Action Required:** None (informational). Contact support if not initiated by user.

```
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│  Subject: Votre mot de passe a été modifié                   │
│                                                              │
│  ─────────────────────────────────────────────────────────  │
│                                                              │
│  Hello John Doe,                                             │
│                                                              │
│  Your password has been successfully changed.                │
│                                                              │
│  If you did not make this change, please contact support     │
│  immediately and reset your password.                        │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

## Email Timing

| Scenario | Email Sent | Delay |
|----------|-----------|-------|
| Registration | Immediately | < 1 minute |
| Forgot Password (valid email) | Immediately | < 1 minute |
| Forgot Password (invalid email) | Never | N/A |
| Password Reset | Immediately | < 1 minute |
| Password Change | Immediately | < 1 minute |

---

## Frontend Considerations

### After Registration
```typescript
function RegistrationSuccess(): JSX.Element {
  return (
    <div className="success-page">
      <h1>Registration Successful!</h1>
      <p>
        We've sent a confirmation email to your address.
        Please check your inbox and click the link to activate your account.
      </p>
      <p>
        <strong>Didn't receive the email?</strong>
        <ul>
          <li>Check your spam/junk folder</li>
          <li>Make sure you entered the correct email</li>
          <li>Wait a few minutes and try again</li>
        </ul>
      </p>
      <button onClick={resendConfirmation}>
        Resend Confirmation Email
      </button>
    </div>
  );
}
```

### After Forgot Password
```typescript
function ForgotPasswordSuccess(): JSX.Element {
  return (
    <div className="success-page">
      <h1>Check Your Email</h1>
      <p>
        If an account exists with that email address, you will receive
        a password reset link shortly.
      </p>
      <p>
        <strong>Important:</strong> The reset link expires in 15 minutes.
      </p>
      <a href="/login">Return to Login</a>
    </div>
  );
}
```

### Deep Link Handling

For mobile apps, configure deep links to handle email links:

```typescript
// React Native example
const linking = {
  prefixes: ['https://app.cesizen.com', 'cesizen://'],
  config: {
    screens: {
      ConfirmAccount: 'confirm-account/:token',
      ResetPassword: 'reset-password/:token',
    },
  },
};
```

---

## Security Considerations

### No Email Enumeration
The forgot-password endpoint always returns success, whether the email exists or not:
- Prevents attackers from discovering valid emails
- Same response time for existing/non-existing emails

### Email Delivery
Users should be instructed to:
1. Check spam/junk folders
2. Add sender to contacts/safe senders
3. Wait a few minutes before retrying

### Token Security
Email tokens should be:
- One-time use (consumed after use)
- Time-limited (15 min for reset, 24h for confirmation)
- Random and unpredictable
- Transmitted over HTTPS only

---

## User Communication

### Email Not Received
Provide clear guidance when users don't receive emails:

```typescript
function EmailTroubleshooting(): JSX.Element {
  return (
    <div className="troubleshooting">
      <h3>Didn't receive the email?</h3>
      <ol>
        <li>Wait 2-3 minutes (emails can be delayed)</li>
        <li>Check your spam/junk folder</li>
        <li>Search for emails from "noreply@cesizen.com"</li>
        <li>Verify you entered the correct email address</li>
        <li>Try requesting again</li>
        <li>Contact support if issues persist</li>
      </ol>
    </div>
  );
}
```

---

## Admin vs User Emails

Admin emails have slightly different wording but follow the same patterns:

| Email Type | User Subject | Admin Subject |
|------------|--------------|---------------|
| Confirmation | "Confirmation de création de compte" | "Confirmation de création de compte administrateur" |
| Password Reset | "Réinitialisation de votre mot de passe" | "Réinitialisation de votre mot de passe administrateur" |
| Password Changed | "Votre mot de passe a été modifié" | "Votre mot de passe administrateur a été modifié" |

---

## Related Documentation

- [Registration & Account Activation](./REGISTRATION.md)
- [Password Reset](./PASSWORD_RESET.md)
- [Change Password](./CHANGE_PASSWORD.md)
