# Returning User Journey - Regular User Workflow

This guide documents the typical path a returning user takes when using the CesiZen application. Unlike new users, returning users already have an account and know how to navigate the app.

---

## Journey Overview

```
┌─────────────┐     ┌─────────────┐     ┌─────────────────────────────────┐
│    Login    │────▶│  Dashboard  │────▶│  Choose Action:                 │
│             │     │             │     │  • Use existing configuration   │
│             │     │             │     │  • Take new quiz                │
└─────────────┘     └─────────────┘     │  • Create custom configuration  │
                                        └──────┬──────────────────────────┘
                                               │
           ┌───────────────────────────────────┴───────────────┐
           │                                                   │
           ▼                                                   ▼
    ┌─────────────┐                                    ┌─────────────┐
    │   Select    │                                    │  Take Quiz  │
    │   Saved     │                                    │     OR      │
    │   Config    │                                    │   Create    │
    └──────┬──────┘                                    │   Custom    │
           │                                           └──────┬──────┘
           │                                                  │
           │                                                  │
           ▼                                                  ▼
    ┌─────────────┐                                    ┌─────────────┐
    │   Start     │◀───────────────────────────────────│    Save     │
    │  Breathing  │                                    │    New      │
    │  Exercise   │                                    │   Config    │
    └─────────────┘                                    └─────────────┘
```

**Typical Session Time:** 2-15 minutes
**Authentication:** Required from start

---

## Path A: Quick Exercise with Existing Configuration

This is the fastest path for users who want to quickly start a breathing exercise with a previously saved configuration.

### Step 1: Login

**Frontend Display:** `/login`

**API Call:**
```http
POST /user/login
Content-Type: application/json
x-api-key: {your-api-key}

{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Success Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "eyJhbGciOi...",
  "user": {
    "id": "user-guid",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "accountActivated": true,
    "active": true
  }
}
```

**Frontend Actions:**
1. Store access token (memory/state)
2. Store refresh token (httpOnly cookie preferred)
3. Decode token to get user ID
4. Navigate to Dashboard

---

### Step 2: Dashboard - Quick Access

**Frontend Display:** `/dashboard`

**API Calls on Load:**

**1. Get User Profile (Optional - if not cached):**
```http
GET /api/users/profile
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

**2. Get Saved Configurations:**
```http
GET /api/user-saved-configurations
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

**Response (200 OK):**
```json
[
  {
    "id": "config-1-guid",
    "name": "Morning Energizer",
    "inhalation": 4,
    "retention1": 4,
    "exhalation": 4,
    "retention2": 4,
    "durationMinutes": 5,
    "difficulty": 1,
    "objective": "Energy",
    "guidanceType": "Visual",
    "creationTime": "2025-01-20T08:00:00Z",
    "updateTime": null
  },
  {
    "id": "config-2-guid",
    "name": "Evening Relaxation",
    "inhalation": 4,
    "retention1": 7,
    "exhalation": 8,
    "retention2": 0,
    "durationMinutes": 10,
    "difficulty": 2,
    "objective": "Relaxation",
    "guidanceType": "Audio",
    "creationTime": "2025-01-22T20:00:00Z",
    "updateTime": "2025-01-25T20:15:00Z"
  },
  {
    "id": "config-3-guid",
    "name": "Focus Session",
    "inhalation": 5,
    "retention1": 5,
    "exhalation": 5,
    "retention2": 5,
    "durationMinutes": 15,
    "difficulty": 3,
    "objective": "Focus",
    "guidanceType": "Both",
    "creationTime": "2025-01-24T14:00:00Z",
    "updateTime": null
  }
]
```

**Dashboard Elements:**

**Welcome Banner:**
- "Welcome back, John! 👋"
- Current time-based greeting: "Good morning" / "Good afternoon" / "Good evening"

**Quick Stats:**
- Saved Configurations: 3
- Quizzes Completed: 5
- Total Sessions This Week: 12
- Streak: 7 days 🔥

**Favorite/Recent Configurations (Quick Access):**
- Display 2-3 most recently used or favorited configs
- Each card shows:
  - Configuration name
  - Breathing pattern summary: "4-4-4-4" (inhale-hold-exhale-hold)
  - Duration badge: "5 min"
  - Difficulty: ⭐⭐☆☆☆
  - **Large "Start" button**

**Example Card:**
```
┌───────────────────────────────────┐
│  🌅 Morning Energizer             │
│                                   │
│  Pattern: 4-4-4-4                 │
│  Duration: 5 minutes              │
│  Difficulty: ⭐☆☆☆☆               │
│                                   │
│  [    Start Exercise    ]         │
│  Edit | Delete                    │
└───────────────────────────────────┘
```

**Other Action Buttons:**
- "View All Configurations" → Navigate to `/configurations`
- "Take a Quiz" → Navigate to `/quizzes`
- "Create Custom" → Navigate to `/configurations/create`

---

### Step 3: Quick Start Exercise

**User Action:** Clicks "Start" on "Morning Energizer" card

**Frontend Action:**
Navigate directly to Exercise Player: `/configurations/config-1-guid/exercise`

**No Additional API Calls Needed** - Configuration data already loaded

**Page Display:** Breathing Exercise Player (see [Exercise Player Details](#breathing-exercise-player-details) below)

---

## Path B: Browse and Select from All Configurations

For users who want to see all their saved configurations before choosing.

### Step 1-2: Login & Dashboard
(Same as Path A)

### Step 3: View All Configurations

**User Action:** Clicks "View All Configurations" from dashboard

**Frontend Display:** `/configurations`

**Elements:**

**Page Header:**
- Title: "My Configurations"
- Subtitle: "Manage your saved breathing patterns"
- "Create New" button (primary)

**Filters & Search:**
- Search by name
- Filter by:
  - Objective (All / Relaxation / Focus / Energy)
  - Difficulty (All / 1-5)
- Sort by:
  - Recently Used
  - Recently Created
  - Name (A-Z)
  - Duration

**Configuration Cards Grid:**
Each card displays:
- Configuration name
- Breathing pattern: "4-7-8-0"
- Duration, Difficulty, Objective
- Last used: "2 hours ago"
- Actions:
  - **"Start"** (primary button)
  - "Edit" (icon)
  - "Duplicate" (icon)
  - "Delete" (icon)

**Example Layout:**
```
My Configurations                          [+ Create New]

Search: [____________]  Objective: [All ▼]  Sort: [Recently Used ▼]

┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│  Morning        │  │  Evening        │  │  Focus          │
│  Energizer      │  │  Relaxation     │  │  Session        │
│                 │  │                 │  │                 │
│  4-4-4-4        │  │  4-7-8-0        │  │  5-5-5-5        │
│  5 min | ⭐     │  │  10 min | ⭐⭐  │  │  15 min | ⭐⭐⭐ │
│  Energy         │  │  Relaxation     │  │  Focus          │
│                 │  │                 │  │                 │
│  Used 2h ago    │  │  Used yesterday │  │  Used 3 days ago│
│                 │  │                 │  │                 │
│  [Start]        │  │  [Start]        │  │  [Start]        │
│  ✏️ 📋 🗑️       │  │  ✏️ 📋 🗑️       │  │  ✏️ 📋 🗑️       │
└─────────────────┘  └─────────────────┘  └─────────────────┘
```

**User Actions:**
- Click "Start" → Exercise Player
- Click "Edit" → Edit Configuration form
- Click "Duplicate" → Pre-filled create form
- Click "Delete" → Confirmation dialog

### Step 4: Start Selected Exercise

**User Action:** Clicks "Start" on any configuration

**Frontend Action:** Navigate to `/configurations/{id}/exercise`

---

## Path C: Take a New Quiz

For users who want to create a new configuration by taking a quiz.

### Step 1-2: Login & Dashboard
(Same as Path A)

### Step 3: Browse Quizzes

**User Action:** Clicks "Take a Quiz" from dashboard

**Frontend Display:** `/quizzes`

**API Call:**
```http
GET /api/quizzes
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

**Response:** List of available quizzes (same format as new user)

**User Sees:**
- Quiz cards with titles, descriptions
- "Start Quiz" buttons
- Indication of previously completed quizzes (if tracked)

### Step 4: Complete Quiz & Generate Configuration

**Full details:** [Quiz to Exercise Path](./QUIZ_TO_EXERCISE_PATH.md)

**Quick Summary:**
1. Select quiz
2. Answer all questions
3. Submit responses
4. Receive generated configuration
5. Configuration automatically added to saved list

### Step 5: Start Exercise with New Configuration

Navigate to Exercise Player with newly generated config

---

## Path D: Create Custom Configuration

For advanced users who want to manually create a breathing pattern.

### Step 1-2: Login & Dashboard
(Same as Path A)

### Step 3: Create Custom Configuration

**User Action:** Clicks "Create Custom" from dashboard or configurations page

**Frontend Display:** `/configurations/create`

**Form Elements:**

**Basic Information:**
- Configuration Name (required)
  - Placeholder: "e.g., Morning Routine"
  - Validation: Max 100 characters

**Breathing Pattern:**
- Inhalation Duration (seconds)
  - Number input, min: 1, max: 30
  - Default: 4
- Retention 1 / Hold After Inhale (seconds)
  - Number input, min: 0, max: 30
  - Default: 4
- Exhalation Duration (seconds)
  - Number input, min: 1, max: 30
  - Default: 4
- Retention 2 / Hold After Exhale (seconds)
  - Number input, min: 0, max: 30
  - Default: 0

**Session Settings:**
- Duration (minutes)
  - Number input, min: 1, max: 60
  - Default: 10
- Difficulty (1-5)
  - Slider or radio buttons
  - Default: 2

**Preferences:**
- Objective (dropdown)
  - Options: Relaxation, Focus, Energy, Sleep, Custom
  - Default: Relaxation
- Guidance Type (radio buttons)
  - Options: Visual, Audio, Both, None
  - Default: Visual

**Live Preview:**
- Animated breathing circle showing the pattern
- Updates in real-time as user adjusts values
- Shows total cycle time: "17 seconds per cycle"

**Example Form:**
```
Create Custom Configuration

Name: [Morning Energizer_________________]

Breathing Pattern
  Inhale:  [4] seconds    ━━━━━━━━━━━━━━
  Hold:    [4] seconds    ━━━━━━━━━━━━━━
  Exhale:  [4] seconds    ━━━━━━━━━━━━━━
  Hold:    [4] seconds    ━━━━━━━━━━━━━━

Session Duration: [5] minutes

Difficulty: ●━━━━ (1 - Easy)

Objective: [Energy        ▼]
Guidance:  ○ Visual  ○ Audio  ● Both  ○ None

┌─────────────────────┐
│   LIVE PREVIEW      │
│                     │
│        ●            │  Pattern: 4-4-4-4
│       ╱ ╲           │  Cycle: 16 seconds
│      ╱   ╲          │  ~19 cycles in 5 min
│     ●─────●         │
│                     │
└─────────────────────┘

[Cancel]  [Save Configuration]
```

**API Call on Save:**
```http
POST /api/user-saved-configurations
Authorization: Bearer {access-token}
Content-Type: application/json
x-api-key: {your-api-key}

{
  "name": "Morning Energizer",
  "inhalation": 4,
  "retention1": 4,
  "exhalation": 4,
  "retention2": 4,
  "durationMinutes": 5,
  "difficulty": 1,
  "objective": "Energy",
  "guidanceType": "Both"
}
```

**Success Response (201 Created):**
```json
{
  "id": "new-config-guid",
  "name": "Morning Energizer",
  "inhalation": 4,
  "retention1": 4,
  "exhalation": 4,
  "retention2": 4,
  "durationMinutes": 5,
  "difficulty": 1,
  "objective": "Energy",
  "guidanceType": "Both",
  "creationTime": "2025-01-27T16:00:00Z",
  "updateTime": null
}
```

**Frontend Action on Success:**
1. Show success toast: "Configuration saved!"
2. Options:
   - "Start Exercise Now" → Exercise Player
   - "View All Configurations" → Configurations list
   - "Create Another" → Clear form

---

## Breathing Exercise Player Details

Regardless of which path the user took (A, B, C, or D), they end up at the Breathing Exercise Player.

**Frontend Display:** `/configurations/{config-id}/exercise`

**Layout & Elements:**

**Central Breathing Animation:**
- Large animated circle (or chosen visualization)
- Smooth transitions between phases
- Color changes based on phase (optional):
  - Blue → Inhale
  - Light blue → Hold
  - Green → Exhale
  - Light green → Hold

**Phase Indicator:**
- Large text showing current action:
  - "Breathe In" / "Inhale"
  - "Hold"
  - "Breathe Out" / "Exhale"
  - "Hold" (if retention2 > 0)

**Timing Information:**
- **Phase Timer:** "3s" (countdown for current phase)
- **Total Progress:** "3:45 / 10:00"
- **Progress Bar:** Visual indicator at bottom/top

**Controls:**
- ⏯️ Play / Pause button
- ⏹️ Stop button (with confirmation)
- 🔊 Volume slider (if audio guidance)
- ⛶ Fullscreen toggle
- ⚙️ Settings (optional):
  - Change animation style
  - Toggle sounds
  - Adjust guidance voice

**Session State:**
```javascript
{
  configId: "config-1-guid",
  isPlaying: true,
  currentPhase: "inhale",       // "inhale" | "retention1" | "exhale" | "retention2"
  phaseTimeRemaining: 3,         // seconds
  totalTimeElapsed: 225,         // 3:45 in seconds
  totalDuration: 600,            // 10:00 in seconds
  currentCycle: 14,
  estimatedTotalCycles: 37
}
```

**Animation Logic:**
1. **Inhale Phase:**
   - Duration: 4 seconds (from config)
   - Circle expands from small to large
   - Text: "Breathe In"
   - Timer counts down: 4, 3, 2, 1

2. **Hold Phase (Retention 1):**
   - Duration: 4 seconds
   - Circle stays large
   - Text: "Hold"
   - Timer counts down: 4, 3, 2, 1

3. **Exhale Phase:**
   - Duration: 4 seconds
   - Circle contracts from large to small
   - Text: "Breathe Out"
   - Timer counts down: 4, 3, 2, 1

4. **Hold Phase (Retention 2):**
   - Duration: 4 seconds (or 0 if not configured)
   - Circle stays small
   - Text: "Hold"
   - Timer counts down: 4, 3, 2, 1

5. **Repeat:** Loop back to step 1

**User Interactions:**

**Click Pause:**
- Animation freezes at current state
- Button changes to "Resume"
- Timer stops
- Option: Show "Resume" overlay on animation

**Click Stop:**
- Show confirmation dialog:
  - "Stop session?"
  - "Your progress: 3:45 / 10:00"
  - "This will not be saved"
  - [Continue Session] [Stop]
- If confirmed:
  - Exit to configurations list or dashboard
  - Optional: Save partial session stats

**Session Complete:**
- Animation stops
- Show completion screen:
  ```
  ┌─────────────────────────────────┐
  │                                 │
  │         🎉                      │
  │    Session Complete!            │
  │                                 │
  │  You completed 10 minutes of    │
  │  Energy breathing               │
  │                                 │
  │  Total Cycles: 37               │
  │  Total Breaths: 148             │
  │                                 │
  │  [Do Another Session]           │
  │  [Back to Dashboard]            │
  │  [View All Configurations]      │
  │                                 │
  └─────────────────────────────────┘
  ```

**Optional Features:**
- **Statistics Tracking:** Save session completion to database
- **Achievements:** "5 sessions in a row!"
- **Streak Counter:** "7 day streak 🔥"
- **Session History:** View past sessions

---

## Common User Actions

### Edit Existing Configuration

**From:** Configurations list
**Action:** Click "Edit" icon on config card

**Frontend Display:** `/configurations/{config-id}/edit`

**Form:** Same as create form, pre-populated with current values

**API Call on Save:**
```http
PUT /api/user-saved-configurations/{config-id}
Authorization: Bearer {access-token}
Content-Type: application/json
x-api-key: {your-api-key}

{
  "name": "Morning Energizer (Updated)",
  "inhalation": 5,
  "retention1": 5,
  "exhalation": 5,
  "retention2": 0,
  "durationMinutes": 7,
  "difficulty": 2,
  "objective": "Energy",
  "guidanceType": "Visual"
}
```

**Success:** Update configurations list, show toast

---

### Delete Configuration

**From:** Configurations list
**Action:** Click "Delete" icon

**Confirmation Dialog:**
```
Delete Configuration?

Are you sure you want to delete "Morning Energizer"?
This action cannot be undone.

[Cancel]  [Delete]
```

**API Call on Confirm:**
```http
DELETE /api/user-saved-configurations/{config-id}
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

**Success (204 No Content):**
- Remove from list
- Show toast: "Configuration deleted"

---

### Duplicate Configuration

**From:** Configurations list
**Action:** Click "Duplicate" icon

**Frontend Action:**
1. Copy configuration data
2. Append " (Copy)" to name
3. Navigate to create form with pre-filled data
4. User can modify and save as new configuration

---

## Token Refresh Flow (Automatic)

Returning users may have expired access tokens. Implement automatic refresh.

**Scenario:** Access token expires during session

**Detection:** API returns 401 Unauthorized

**Frontend Action:**
1. Detect 401 response
2. Attempt token refresh:

```http
POST /user/refresh-token
Content-Type: application/json
x-api-key: {your-api-key}

{
  "refreshToken": "{stored-refresh-token}"
}
```

**Success (200 OK):**
```json
{
  "accessToken": "new-access-token",
  "refreshToken": "new-refresh-token"
}
```

**Frontend:**
1. Store new tokens
2. Retry original failed request
3. Continue seamlessly (user doesn't notice)

**Failure (401 - Refresh token also expired):**
1. Clear all tokens
2. Show message: "Your session has expired. Please log in again."
3. Redirect to login
4. Remember intended destination
5. After successful login, redirect back

**Related:** [Token Refresh Documentation](../AUTHENTIFICATION/TOKEN_REFRESH.md)

---

## Quick Reference: Returning User Paths

| Path | User Goal | Key Steps | Time |
|------|-----------|-----------|------|
| **A - Quick Start** | Start exercise ASAP | Login → Dashboard → Click "Start" on recent config | ~30 sec |
| **B - Browse Configs** | Choose from all saved | Login → Dashboard → View All → Select → Start | ~1-2 min |
| **C - New Quiz** | Get new configuration | Login → Dashboard → Take Quiz → Submit → Start | ~5-7 min |
| **D - Custom Create** | Make custom pattern | Login → Dashboard → Create Custom → Save → Start | ~2-3 min |

---

## Related Documentation

### User Journeys
- [New User Path](./NEW_USER_PATH.md) - First-time experience
- [Quiz to Exercise Path](./QUIZ_TO_EXERCISE_PATH.md) - Detailed quiz flow
- [Profile Management Path](./PROFILE_MANAGEMENT_PATH.md) - Account settings

### Authentication
- [Login Flow](../AUTHENTIFICATION/LOGIN.md)
- [Token Refresh](../AUTHENTIFICATION/TOKEN_REFRESH.md)
- [Session Management](../AUTHENTIFICATION/SESSION_MANAGEMENT.md)

### API Reference
- [Configurations API](../../FEATURES/CONFIGURATIONS/README.md)
- [Quizzes API](../../FEATURES/QUIZZES/README.md)

---

## Developer Implementation Checklist

### Dashboard
- [ ] Welcome message with user name
- [ ] Quick stats (configs, sessions, streak)
- [ ] Recent/favorite configurations (quick access)
- [ ] "Start" buttons on config cards
- [ ] Action buttons (View All, Take Quiz, Create Custom)

### Configurations List
- [ ] Grid/list view of all configurations
- [ ] Search and filter functionality
- [ ] Sort options
- [ ] Config cards with actions (Start, Edit, Duplicate, Delete)
- [ ] Empty state handling

### Exercise Player
- [ ] Breathing animation (smooth, accurate)
- [ ] Phase indicators (Inhale, Hold, Exhale)
- [ ] Timers (phase countdown, total progress)
- [ ] Play/Pause/Stop controls
- [ ] Fullscreen mode
- [ ] Session completion screen
- [ ] Audio guidance (if configured)

### Configuration Management
- [ ] Create form with validation
- [ ] Edit form (pre-populated)
- [ ] Delete confirmation dialog
- [ ] Duplicate functionality
- [ ] Live preview of breathing pattern

### Token Management
- [ ] Automatic token refresh on 401
- [ ] Retry failed requests after refresh
- [ ] Handle refresh token expiration
- [ ] Seamless user experience

---

**Next:** Explore the detailed [Quiz to Exercise Path](./QUIZ_TO_EXERCISE_PATH.md) for in-depth quiz mechanics.
