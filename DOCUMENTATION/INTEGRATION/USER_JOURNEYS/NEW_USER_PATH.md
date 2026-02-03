# New User Journey - First Time Experience

This guide documents the complete path a new user takes from discovering the app to completing their first breathing exercise.

---

## Journey Overview

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Landing   │────▶│  Register   │────▶│   Activate  │────▶│    Login    │
│     Page    │     │   Account   │     │   via Email │     │             │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                                                     │
                                                                     ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Complete  │◀────│  Take First │◀────│   Browse    │◀────│  Dashboard  │
│  Breathing  │     │    Quiz     │     │   Quizzes   │     │             │
│  Exercise   │     │             │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

**Total Steps:** 9
**Estimated Time:** 5-10 minutes
**Authentication Required:** From step 5 onwards

---

## Step 1: Landing Page (Public Access)

### User Action
First-time visitor arrives at the application URL

### Frontend Display
**Page:** `/` (Landing Page)

**Elements:**
- Hero section with app tagline
  - "Master Your Breath, Master Your Mind"
  - Description of breathing exercises
- Call-to-action buttons:
  - "Get Started" (primary)
  - "Browse Quizzes"
- Navigation menu (fetched from API)
- Featured content preview
- Footer with info links

### API Calls
```http
GET /api/content/menus
x-api-key: {your-api-key}
```

**Response (200 OK):**
```json
[
  {
    "id": "menu-guid",
    "label": "About",
    "url": "/content/about",
    "position": 1
  },
  {
    "id": "menu-guid-2",
    "label": "Features",
    "url": "/content/features",
    "position": 2
  }
]
```

### User Options
1. Click "Get Started" → Navigate to Step 2 (Registration)
2. Click "Browse Quizzes" → Navigate to Step 2.5 (View Quizzes - Public)
3. Browse public content → Information pages

### Implementation Notes
- No authentication required
- Cache menu items for performance
- Render navigation dynamically from API response

---

## Step 2 (Optional): Browse Quizzes Before Registration

### User Action
Clicks "Browse Quizzes" to explore without registering

### Frontend Display
**Page:** `/quizzes` (Quiz Listing)

**Elements:**
- List of available quizzes
- Quiz cards showing:
  - Title
  - Description (short)
  - Question count estimate
  - "Start Quiz" button

### API Calls
```http
GET /api/quizzes
x-api-key: {your-api-key}
```

**Response (200 OK):**
```json
[
  {
    "id": "quiz-guid",
    "nom": "Breathing Style Quiz",
    "active": true,
    "creationTime": "2025-01-27T10:00:00Z"
  }
]
```

### User Action
Clicks "Start Quiz" on any quiz card

### Frontend Response
**Show modal/banner:**
- Message: "Please create an account to take quizzes"
- Buttons:
  - "Register" (primary) → Step 3
  - "Login" (secondary) → Skip to Step 5
  - "Cancel" → Stay on quiz list

### Purpose
- Allow exploration before commitment
- Create motivation to register
- Show value proposition

---

## Step 3: Registration

### User Action
Clicks "Register" button (from landing page or quiz prompt)

### Frontend Display
**Page:** `/register` or modal overlay

**Form Fields:**
- Email (required, email validation)
- Password (required, strength indicator)
- Confirm Password (required, must match)
- First Name (required)
- Last Name (required)

**Validation Rules:**
- Email: Valid format, not already in use
- Password: Minimum 8 characters, at least 1 uppercase, 1 number, 1 special character
- Confirm Password: Must match password exactly
- Names: Non-empty, reasonable length

### API Call
```http
POST /user/register
Content-Type: application/json
x-api-key: {your-api-key}

{
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### Success Response (201 Created)
```json
{
  "message": "Registration successful. Please check your email to activate your account.",
  "email": "john.doe@example.com"
}
```

### Frontend Action on Success
1. Show success message:
   - "Account created successfully!"
   - "We've sent a confirmation email to john.doe@example.com"
   - "Please check your inbox and click the activation link"
2. Display email icon/illustration
3. Provide option: "Didn't receive email? Resend"
4. Disable form, show "Check Your Email" screen

### Error Handling

**400 Bad Request - Email Exists:**
```json
{
  "error": "Email already registered"
}
```
**Frontend:** Show error on email field, suggest login

**400 Bad Request - Password Weak:**
```json
{
  "error": "Password does not meet requirements"
}
```
**Frontend:** Show password requirements clearly

**422 Validation Error:**
```json
{
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 8 characters"]
  }
}
```
**Frontend:** Show field-specific errors

### Implementation Notes
- Use real-time validation for better UX
- Show password strength indicator as user types
- Disable submit button until form is valid
- Show loading state during API call

**Related Documentation:**
- [Registration API Details](../AUTHENTIFICATION/REGISTRATION.md)

---

## Step 4: Email Activation

### User Action
1. Opens email client
2. Finds confirmation email from CesiZen
3. Clicks activation link

### Email Content
**Subject:** "Activate Your CesiZen Account"

**Body:**
- Welcome message
- Activation link (valid for 24 hours)
- Fallback: "If button doesn't work, copy this link: {url}"

### Activation Link Format
```
https://yourdomain.com/activate?token={activation-token}
```

### Frontend Display
**Page:** `/activate` (with token query parameter)

**On Page Load:**
1. Extract token from URL
2. Immediately call activation API
3. Show loading spinner

### API Call
```http
GET /user/confirm-email?token={activation-token}
x-api-key: {your-api-key}
```

### Success Response (200 OK)
```json
{
  "message": "Email confirmed successfully. You can now log in."
}
```

### Frontend Action on Success
1. Show success screen:
   - ✅ "Account Activated!"
   - "Your account is now active"
   - "You can now login and start using CesiZen"
2. Auto-redirect to login after 3 seconds
3. Or provide "Login Now" button

### Error Handling

**400 Bad Request - Invalid Token:**
```json
{
  "error": "Invalid or expired confirmation token"
}
```
**Frontend:**
- Show error message
- Offer to resend confirmation email
- Provide support contact

**400 Bad Request - Already Confirmed:**
```json
{
  "error": "Email already confirmed"
}
```
**Frontend:**
- Show info message: "Your account is already activated"
- Redirect to login

### Implementation Notes
- Token validation happens on backend
- No need to store token in frontend
- Handle expired tokens gracefully

**Related Documentation:**
- [Email Notifications](../AUTHENTIFICATION/EMAIL_NOTIFICATIONS.md)

---

## Step 5: First Login

### User Action
Enters email and password on login page

### Frontend Display
**Page:** `/login`

**Form Fields:**
- Email (required)
- Password (required)
- "Remember Me" checkbox (optional)
- "Forgot Password?" link

### API Call
```http
POST /user/login
Content-Type: application/json
x-api-key: {your-api-key}

{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

### Success Response (200 OK)
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
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

### Frontend Actions on Success

**1. Store Tokens:**
```javascript
// Store access token in memory (React state, Vuex, etc.)
setAccessToken(response.accessToken);

// Store refresh token securely
// Option A: HttpOnly cookie (handled by backend)
// Option B: Secure storage (mobile apps)
localStorage.setItem('refreshToken', response.refreshToken); // Only for demo!
```

**2. Decode Token to Get User Info:**
```javascript
import jwt_decode from 'jwt-decode';

const decoded = jwt_decode(response.accessToken);
console.log(decoded);
// {
//   "nameid": "user-guid",
//   "exp": 1706356800
// }

// Check if user is admin
const isAdmin = decoded.role === "Administrator"; // false for regular users
```

**3. Store User Profile:**
```javascript
setCurrentUser(response.user);
```

**4. Redirect to Dashboard:**
```javascript
// For new users, show onboarding tips
if (isFirstLogin) {
  navigate('/dashboard?welcome=true');
} else {
  navigate('/dashboard');
}
```

### Error Handling

**400 Bad Request - Invalid Credentials:**
```json
{
  "error": "Invalid email or password"
}
```
**Frontend:**
- Show generic error (don't reveal which is wrong)
- Clear password field
- Keep email filled

**400 Bad Request - Account Not Activated:**
```json
{
  "error": "Please confirm your email before logging in"
}
```
**Frontend:**
- Show message with email resend option
- Link to resend confirmation email

**400 Bad Request - Account Locked:**
```json
{
  "error": "Account locked due to too many failed login attempts. Try again in 15 minutes."
}
```
**Frontend:**
- Show lockout message
- Display time remaining
- Offer password reset option

**Account Disabled (Admin Action):**
```json
{
  "error": "Your account has been disabled. Please contact support."
}
```
**Frontend:**
- Show support contact information
- No retry option

### Implementation Notes
- Never store tokens in localStorage in production
- Use httpOnly cookies for refresh tokens (most secure)
- Implement token refresh before showing dashboard
- Track failed login attempts (handled by backend)

**Related Documentation:**
- [Login Flow](../AUTHENTIFICATION/LOGIN.md)
- [Account Lockout](../AUTHENTIFICATION/ACCOUNT_LOCKOUT.md)
- [Token Management](../AUTHENTIFICATION/TOKEN_REFRESH.md)

---

## Step 6: User Dashboard (First Visit)

### Frontend Display
**Page:** `/dashboard`

**Elements:**

**Welcome Banner (First Time Only):**
- "Welcome to CesiZen, John! 👋"
- "Let's get started with your first breathing exercise"
- Brief tutorial or tour option

**Quick Stats:**
- Saved Configurations: 0
- Quizzes Completed: 0
- Total Sessions: 0

**Main Call-to-Action:**
- Large button: "Take Your First Quiz"
- Secondary button: "Create Custom Configuration"

**Empty State for Configurations:**
- Illustration (empty state graphic)
- Message: "You don't have any saved configurations yet"
- Subtext: "Complete a quiz or create a custom breathing pattern"

**Navigation:**
- Sidebar or top nav with:
  - Dashboard (current)
  - My Configurations
  - Browse Quizzes
  - Profile

### API Calls on Dashboard Load

**1. Get User Profile:**
```http
GET /api/users/profile
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

**Response (200 OK):**
```json
{
  "id": "user-guid",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "memberSince": "2025-01-27T10:00:00Z",
  "thumbnailUrl": null,
  "accountActivated": true,
  "active": true
}
```

**2. Get Saved Configurations:**
```http
GET /api/user-saved-configurations
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

**Response (200 OK):** (Empty for new user)
```json
[]
```

### User Action
Clicks "Take Your First Quiz"

### Frontend Response
Navigate to Quiz Listing page (`/quizzes`)

---

## Step 7: Browse and Select Quiz

### Frontend Display
**Page:** `/quizzes`

**Elements:**
- Page title: "Choose a Quiz"
- Subtitle: "Answer questions to generate a personalized breathing pattern"
- Quiz cards grid

**Each Quiz Card:**
- Quiz title
- Description
- Estimated time: "~5 minutes"
- Difficulty indicator (if available)
- "Start Quiz" button

### API Call
```http
GET /api/quizzes
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

**Response (200 OK):**
```json
[
  {
    "id": "quiz-1-guid",
    "nom": "Breathing Style Quiz",
    "active": true,
    "creationTime": "2025-01-27T10:00:00Z"
  },
  {
    "id": "quiz-2-guid",
    "nom": "Stress Relief Quiz",
    "active": true,
    "creationTime": "2025-01-27T11:00:00Z"
  }
]
```

### User Action
Clicks "Start Quiz" on "Breathing Style Quiz"

### Frontend Action
1. Fetch full quiz with questions:

```http
GET /api/quizzes/quiz-1-guid
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

**Response (200 OK):**
```json
{
  "id": "quiz-1-guid",
  "nom": "Breathing Style Quiz",
  "active": true,
  "questions": [
    {
      "id": "q1-guid",
      "text": "How stressed do you feel right now?",
      "position": 1,
      "responsesOptions": [
        {
          "id": "opt1-guid",
          "label": "Very stressed",
          "position": 1,
          "targetedField": "Difficulty",
          "operation": "SET",
          "value": "3"
        },
        {
          "id": "opt2-guid",
          "label": "Moderately stressed",
          "position": 2,
          "targetedField": "Difficulty",
          "operation": "SET",
          "value": "2"
        },
        {
          "id": "opt3-guid",
          "label": "Not stressed",
          "position": 3,
          "targetedField": "Difficulty",
          "operation": "SET",
          "value": "1"
        }
      ]
    },
    {
      "id": "q2-guid",
      "text": "What is your primary goal?",
      "position": 2,
      "responsesOptions": [
        {
          "id": "opt4-guid",
          "label": "Relaxation",
          "position": 1,
          "targetedField": "Objective",
          "operation": "SET",
          "value": "Relaxation"
        },
        {
          "id": "opt5-guid",
          "label": "Focus and concentration",
          "position": 2,
          "targetedField": "Objective",
          "operation": "SET",
          "value": "Focus"
        },
        {
          "id": "opt6-guid",
          "label": "Energy boost",
          "position": 3,
          "targetedField": "Objective",
          "operation": "SET",
          "value": "Energy"
        }
      ]
    }
  ]
}
```

2. Navigate to Quiz Taking page: `/quizzes/quiz-1-guid/take`

---

## Step 8: Complete Quiz

**Detailed quiz-taking flow is documented in:**
[Quiz to Exercise Path](./QUIZ_TO_EXERCISE_PATH.md#quiz-taking-detailed-flow)

### Quick Summary

**Frontend State Management:**
```javascript
{
  quizId: "quiz-1-guid",
  currentQuestionIndex: 0,
  totalQuestions: 2,
  responses: [
    { questionId: "q1-guid", selectedOptionId: "opt2-guid" },
    { questionId: "q2-guid", selectedOptionId: "opt4-guid" }
  ]
}
```

**User Flow:**
1. View question 1
2. Select an option
3. Click "Next"
4. View question 2
5. Select an option
6. Click "Submit" (on last question)

### Submit Quiz

**API Call:**
```http
POST /api/user-saved-configurations/from-quiz
Authorization: Bearer {access-token}
Content-Type: application/json
x-api-key: {your-api-key}

{
  "quizId": "quiz-1-guid",
  "responses": [
    {
      "questionId": "q1-guid",
      "selectedOptionId": "opt2-guid"
    },
    {
      "questionId": "q2-guid",
      "selectedOptionId": "opt4-guid"
    }
  ]
}
```

**Success Response (201 Created):**
```json
{
  "id": "config-guid",
  "name": "Generated from Breathing Style Quiz - 2025-01-27 15:30:00",
  "inhalation": 4,
  "retention1": 7,
  "exhalation": 8,
  "retention2": 0,
  "durationMinutes": 10,
  "difficulty": 2,
  "objective": "Relaxation",
  "guidanceType": "Visual",
  "creationTime": "2025-01-27T15:30:00Z",
  "updateTime": null
}
```

### Frontend Action
Navigate to Configuration Result page: `/configurations/config-guid/result`

---

## Step 9: View Generated Configuration & Start First Exercise

### Frontend Display
**Page:** `/configurations/config-guid/result`

**Elements:**

**Success Banner:**
- ✅ "Configuration Created!"
- "Based on your answers, we've created a personalized breathing pattern"

**Configuration Details Card:**
- **Name:** "Generated from Breathing Style Quiz"
- **Breathing Pattern:**
  - 🫁 Inhale: 4 seconds
  - ⏸️ Hold: 7 seconds
  - 🌬️ Exhale: 8 seconds
  - ⏸️ Hold: 0 seconds
- **Duration:** 10 minutes
- **Difficulty:** 2/5 (⭐⭐☆☆☆)
- **Objective:** Relaxation
- **Guidance:** Visual

**Breathing Pattern Visualization:**
- Animated circle preview
- Shows expand (inhale) → hold → contract (exhale) → hold cycle

**Action Buttons:**
- **"Start Exercise"** (primary, large) → Navigate to exercise player
- "Edit Configuration" (secondary) → Customize settings
- "Retake Quiz" (tertiary) → Start quiz over
- "View All Configurations" (link) → Go to configurations list

### User Action
Clicks "Start Exercise"

### Frontend Action
Navigate to Breathing Exercise Player: `/configurations/config-guid/exercise`

---

## Step 10: First Breathing Exercise

### Frontend Display
**Page:** `/configurations/config-guid/exercise`

**Layout:**
- Full-screen mode (optional)
- Clean, distraction-free interface
- Dark or calming background

**Central Elements:**

**Breathing Animation:**
- Large circle in center
- Expands during inhale (4 seconds)
- Stays large during hold (7 seconds)
- Contracts during exhale (8 seconds)
- Stays small during hold (0 seconds in this case)

**Phase Indicator:**
- Large text showing current phase:
  - "Inhale" (during inhalation)
  - "Hold" (during retention)
  - "Exhale" (during exhalation)

**Timers:**
- **Current Phase Timer:** "3s" (counts down)
- **Total Session Timer:** "2:30 / 10:00"
- **Progress Bar:** Visual indicator of session completion

**Controls:**
- ⏯️ Play/Pause button
- ⏹️ Stop button (exit exercise)
- 🔊 Volume control (if audio guidance enabled)
- ⛶ Fullscreen toggle

### Exercise Flow

**1. Session Start:**
- Animation begins automatically
- Soft sound (if audio enabled)
- User follows visual cues

**2. During Session:**
- Circle expands smoothly → "Inhale" (4s)
- Circle stays large → "Hold" (7s)
- Circle contracts smoothly → "Exhale" (8s)
- (No second hold in this config)
- Repeat cycle

**3. Session Progress:**
- Total duration: 10 minutes
- Progress bar fills gradually
- Timer counts down

**4. User Can:**
- Pause at any time → Animation stops
- Resume → Animation continues from current phase
- Stop → Confirmation: "Are you sure? Progress will be lost"

**5. Session Complete:**
- Animation stops
- Show completion screen:
  - 🎉 "Session Complete! Well done!"
  - "You completed 10 minutes of Relaxation breathing"
  - Statistics (if tracked):
    - Total cycles: ~40
    - Total breaths: ~80

**Action Buttons:**
- "Do Another Session" → Restart same exercise
- "Back to Configurations" → Navigate to `/configurations`
- "Dashboard" → Navigate to `/dashboard`

### Frontend State
```javascript
{
  configurationId: "config-guid",
  isPlaying: true,
  currentPhase: "inhale", // "inhale" | "retention1" | "exhale" | "retention2"
  phaseTimeRemaining: 4,
  totalTimeElapsed: 150, // seconds
  totalDuration: 600, // 10 minutes = 600 seconds
  cycleCount: 15
}
```

### Implementation Notes
- Use smooth CSS animations or Canvas for circle
- Implement precise timing (avoid drift)
- Handle tab visibility (pause when tab hidden?)
- Save session stats (optional feature)
- Allow early exit with confirmation

---

## Journey Complete! 🎉

### What the User Has Achieved

At this point, the new user has successfully:

✅ **Created an account** - Registered and activated via email
✅ **Logged in** - Authenticated with JWT tokens
✅ **Completed their first quiz** - Answered questions
✅ **Generated a personalized configuration** - Received tailored breathing pattern
✅ **Completed their first breathing exercise** - Experienced the core app feature

### Next Steps for the User

The user can now:

1. **Explore More Quizzes** → [Quiz to Exercise Path](./QUIZ_TO_EXERCISE_PATH.md)
2. **Create Custom Configurations** → Create manual breathing patterns
3. **Manage Saved Configurations** → Edit, delete, organize
4. **Customize Profile** → [Profile Management Path](./PROFILE_MANAGEMENT_PATH.md)
5. **Track Progress** → View statistics and history (if implemented)

### What to Build Next (Frontend Developers)

1. **Implement Returning User Flow** → [Returning User Path](./RETURNING_USER_PATH.md)
2. **Add Configuration Management** → Edit/delete configurations
3. **Enhance Exercise Player** → Audio guidance, themes, statistics
4. **Build Profile Features** → Password change, account settings
5. **Add Social Features** → Share configurations, community

---

## Related Documentation

### Authentication
- [Complete Authentication Flow](../AUTHENTIFICATION/AUTH_FLOW.md)
- [Registration Details](../AUTHENTIFICATION/REGISTRATION.md)
- [Login Details](../AUTHENTIFICATION/LOGIN.md)
- [Token Refresh](../AUTHENTIFICATION/TOKEN_REFRESH.md)

### Features
- [Quiz to Exercise Path (Detailed)](./QUIZ_TO_EXERCISE_PATH.md)
- [Returning User Path](./RETURNING_USER_PATH.md)
- [Profile Management](./PROFILE_MANAGEMENT_PATH.md)

### API Reference
- [Quiz API](../../FEATURES/QUIZZES/README.md)
- [Configuration API](../../FEATURES/CONFIGURATIONS/README.md)
- [Error Handling](../../REFERENCE/ERROR_HANDLING.md)

---

## Developer Checklist

Use this checklist when implementing the new user journey:

### Authentication Phase
- [ ] Landing page with CTAs
- [ ] Public quiz browsing
- [ ] Registration form with validation
- [ ] Email confirmation flow
- [ ] Login with token management
- [ ] Token storage (secure)

### First-Time Experience
- [ ] Welcome dashboard with onboarding
- [ ] Empty state for configurations
- [ ] "Take First Quiz" CTA prominent
- [ ] Quiz listing page
- [ ] Quiz selection and start

### Quiz & Configuration
- [ ] Quiz taking interface
- [ ] Question navigation (prev/next)
- [ ] Response selection tracking
- [ ] Quiz submission
- [ ] Configuration result page
- [ ] Breathing pattern visualization

### Exercise Player
- [ ] Full-screen breathing animation
- [ ] Phase indicators (Inhale/Hold/Exhale)
- [ ] Timers (current phase + total)
- [ ] Play/Pause/Stop controls
- [ ] Session completion screen
- [ ] Progress tracking

### Error Handling
- [ ] Invalid credentials
- [ ] Expired tokens
- [ ] Network errors
- [ ] Validation errors
- [ ] Account locked/disabled

### Polish
- [ ] Loading states
- [ ] Success confirmations
- [ ] Error messages
- [ ] Smooth transitions
- [ ] Responsive design

---

**Ready to implement returning users?** → [Returning User Path](./RETURNING_USER_PATH.md)
