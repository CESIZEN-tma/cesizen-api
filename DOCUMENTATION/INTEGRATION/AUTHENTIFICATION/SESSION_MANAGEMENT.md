# Session Management

[< Back to Overview](./README.md) | [Login](./LOGIN.md)

---

## Overview

Session management allows users to view and revoke their active sessions across devices. Each login creates a new session, and users can manage these sessions for security.

---

## What is a Session?

A session represents an active login on a device. Each session has:

| Property | Description |
|----------|-------------|
| `id` | Unique session identifier |
| `createdAt` | When the session was created (login time) |
| `expiresAt` | When the session will expire |
| `isCurrentSession` | Whether this is the session making the request |

---

## API Endpoints

All session management endpoints require authentication.

### Get Active Sessions

Lists all active (non-expired, non-consumed) sessions.

```http
GET /user/sessions
Authorization: Bearer <access_token>
X-Refresh-Token: <refresh_token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "createdAt": "2024-01-10T08:00:00Z",
    "expiresAt": "2024-02-09T08:00:00Z",
    "isCurrentSession": true
  },
  {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "createdAt": "2024-01-08T14:30:00Z",
    "expiresAt": "2024-02-07T14:30:00Z",
    "isCurrentSession": false
  }
]
```

### Revoke Specific Session

Revokes a single session by ID.

```http
DELETE /user/sessions/{sessionId}
Authorization: Bearer <access_token>
```

**Response:** `200 OK`
```json
{
  "message": "Session revoked successfully."
}
```

**Error Response:** `404 Not Found`
```json
{
  "error": "Session not found."
}
```

### Revoke All Other Sessions

Revokes all sessions except the current one.

```http
DELETE /user/sessions
Authorization: Bearer <access_token>
X-Refresh-Token: <refresh_token>
```

**Response:** `200 OK`
```json
{
  "message": "All other sessions revoked successfully."
}
```

---

## Admin Endpoints

Same endpoints with `/admin` prefix and `[Authorize(Roles = "Administrator")]`:

```http
GET    /admin/sessions
DELETE /admin/sessions/{sessionId}
DELETE /admin/sessions
```

---

## The X-Refresh-Token Header

Some endpoints require the `X-Refresh-Token` header to identify the current session:

| Endpoint | X-Refresh-Token Required |
|----------|--------------------------|
| `GET /sessions` | Yes (to mark current session) |
| `DELETE /sessions/{id}` | No |
| `DELETE /sessions` | Yes (to exclude current session) |

---

## Frontend Implementation

### Session List Component
```typescript
interface Session {
  id: string;
  createdAt: string;
  expiresAt: string;
  isCurrentSession: boolean;
}

async function fetchSessions(): Promise<Session[]> {
  const response = await fetch('/user/sessions', {
    headers: {
      'Authorization': `Bearer ${getAccessToken()}`,
      'X-Refresh-Token': getRefreshToken(),
    },
  });

  if (response.ok) {
    return response.json();
  }

  throw new Error('Failed to fetch sessions');
}

function SessionList(): JSX.Element {
  const [sessions, setSessions] = useState<Session[]>([]);

  useEffect(() => {
    fetchSessions().then(setSessions);
  }, []);

  return (
    <div className="session-list">
      <h2>Active Sessions</h2>
      {sessions.map((session) => (
        <SessionItem
          key={session.id}
          session={session}
          onRevoke={() => handleRevoke(session.id)}
        />
      ))}
      <button onClick={handleRevokeAll}>
        Sign out from all other devices
      </button>
    </div>
  );
}
```

### Session Item Component
```typescript
function SessionItem({ session, onRevoke }: SessionItemProps): JSX.Element {
  const createdDate = new Date(session.createdAt).toLocaleDateString();
  const createdTime = new Date(session.createdAt).toLocaleTimeString();

  return (
    <div className={`session-item ${session.isCurrentSession ? 'current' : ''}`}>
      <div className="session-info">
        <span className="session-date">
          Logged in: {createdDate} at {createdTime}
        </span>
        {session.isCurrentSession && (
          <span className="current-badge">Current Session</span>
        )}
      </div>
      {!session.isCurrentSession && (
        <button onClick={onRevoke} className="revoke-btn">
          Revoke
        </button>
      )}
    </div>
  );
}
```

### Revoke Session
```typescript
async function revokeSession(sessionId: string): Promise<void> {
  const response = await fetch(`/user/sessions/${sessionId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${getAccessToken()}`,
    },
  });

  if (!response.ok) {
    throw new Error('Failed to revoke session');
  }
}

async function revokeAllOtherSessions(): Promise<void> {
  const response = await fetch('/user/sessions', {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${getAccessToken()}`,
      'X-Refresh-Token': getRefreshToken(),
    },
  });

  if (!response.ok) {
    throw new Error('Failed to revoke sessions');
  }
}
```

---

## Use Cases

### Security Audit
Allow users to review where they're logged in:
- See all active sessions
- Identify suspicious logins
- Revoke sessions from unknown devices

### Lost Device
If a user loses their phone/laptop:
1. Log in from another device
2. View sessions
3. Revoke the session from the lost device

### Password Change
When changing password, [all other sessions are automatically revoked](./CHANGE_PASSWORD.md) for security.

### Shared Computer
After using a shared computer:
1. Log in from personal device
2. Revoke all other sessions

---

## Session Lifecycle

```
LOGIN ──> Session Created ──> Active
                               │
              ┌────────────────┼────────────────┐
              │                │                │
           REFRESH          REVOKE           EXPIRE
              │                │                │
              ▼                ▼                ▼
         Old session       Consumed          Consumed
          consumed         (manual)        (automatic)
         New session
           created
```

---

## Related Documentation

- [Login & Token Management](./LOGIN.md)
- [Token Refresh](./TOKEN_REFRESH.md)
- [Change Password](./CHANGE_PASSWORD.md)
- [Logout](./LOGOUT.md)
