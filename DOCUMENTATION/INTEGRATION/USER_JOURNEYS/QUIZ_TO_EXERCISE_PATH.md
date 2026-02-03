# Quiz to Exercise Path - Core Feature Flow

This guide provides detailed documentation of the most important feature in CesiZen: taking a quiz to generate a personalized breathing configuration and starting a breathing exercise.

This is the **core value proposition** of the application.

---

## Journey Overview

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│    Browse    │───▶│   View Quiz  │───▶│  Start Quiz  │───▶│   Question   │
│   Quizzes    │    │    Details   │    │              │    │      #1      │
└──────────────┘    └──────────────┘    └──────────────┘    └──────┬───────┘
                                                                     │
                                                                     ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   Complete   │◀───│  Submit Quiz │◀───│  Question    │◀───│  Question    │
│   Breathing  │    │  & Generate  │    │     #N       │    │     #2...    │
│   Exercise   │    │    Config    │    │  (Last)      │    │              │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
        ▲                   │
        │                   ▼
        │           ┌──────────────┐
        └───────────│  View Result │
                    │ Configuration│
                    └──────────────┘
```

**Total Steps:** 7 main steps
**Estimated Time:** 5-10 minutes
**Authentication:** Required

---

## Step 1: Browse Available Quizzes

### User Location
Can arrive from:
- Dashboard → "Take a Quiz" button
- Navigation menu → "Quizzes"
- Direct link: `/quizzes`

### Frontend Display
**Page:** `/quizzes`

**Elements:**

**Page Header:**
- Title: "Discover Your Perfect Breathing Pattern"
- Subtitle: "Answer a few questions to get a personalized breathing configuration"

**Quiz Cards Grid:**
Display all active quizzes in a responsive grid (2-3 columns on desktop, 1 on mobile)

**Each Quiz Card Shows:**
- Quiz title (large, prominent)
- Short description
- Estimated time: "~5 minutes"
- Number of questions: "8 questions"
- Difficulty indicator (if applicable)
- Visual icon or illustration
- "Start Quiz" button (primary)
- Preview button (optional): "Preview Questions"

**Example Card:**
```
┌─────────────────────────────────────────┐
│  🫁  Breathing Style Quiz                │
│                                         │
│  Discover the perfect breathing         │
│  pattern based on your stress level     │
│  and goals                              │
│                                         │
│  ⏱️  ~5 minutes  •  📋 8 questions       │
│                                         │
│  [      Start Quiz      ]               │
│  Preview Questions                      │
└─────────────────────────────────────────┘
```

### API Call
```http
GET /api/quizzes
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

### Response (200 OK)
```json
[
  {
    "id": "quiz-1-guid",
    "nom": "Breathing Style Quiz",
    "active": true,
    "creationTime": "2025-01-20T10:00:00Z"
  },
  {
    "id": "quiz-2-guid",
    "nom": "Stress Relief Assessment",
    "active": true,
    "creationTime": "2025-01-22T14:00:00Z"
  },
  {
    "id": "quiz-3-guid",
    "nom": "Focus Enhancement Quiz",
    "active": true,
    "creationTime": "2025-01-24T09:00:00Z"
  }
]
```

### Frontend Implementation Notes
- Cache quiz list for performance
- Show loading skeleton while fetching
- Handle empty state: "No quizzes available yet"
- Sort by: Featured → Most Recent → Alphabetical
- Optional: Show "Completed" badge if user has taken quiz before

### User Actions
- Click "Start Quiz" → Navigate to Step 2
- Click "Preview Questions" (optional) → Show modal with question list
- Back to Dashboard → Navigate to `/dashboard`

---

## Step 2: View Quiz Details (Optional but Recommended)

### Frontend Display
**Page:** `/quizzes/{quiz-id}` or Modal Overlay

**Purpose:** Give user context before committing to quiz

**Elements:**

**Quiz Header:**
- Quiz title (large)
- Description (full text)
- Metadata:
  - Estimated time
  - Number of questions
  - What they'll get: "A personalized breathing configuration"

**What to Expect:**
- "You'll answer {N} questions about:"
  - Your current stress level
  - Your breathing goals
  - Your experience level
  - Your preferences

**How It Works:**
1. Answer all questions honestly
2. Select the option that best fits you
3. Submit your responses
4. Receive a custom breathing pattern
5. Start your first exercise

**Example Questions Preview (Optional):**
- "How stressed do you feel?"
- "What's your primary goal?"
- "How much time do you have?"

**Action Buttons:**
- **"Start Quiz"** (large, primary)
- "Back to Quizzes" (secondary)

### API Call (if full quiz details needed)
```http
GET /api/quizzes/{quiz-id}
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

### Response (200 OK)
```json
{
  "id": "quiz-1-guid",
  "nom": "Breathing Style Quiz",
  "active": true,
  "creationTime": "2025-01-20T10:00:00Z",
  "questions": [
    {
      "id": "q1-guid",
      "text": "How stressed do you feel right now?",
      "position": 1,
      "responsesOptions": [...]
    },
    // ... more questions
  ]
}
```

**Note:** You can delay loading full quiz data until user clicks "Start Quiz" to optimize performance.

---

## Step 3: Start Quiz - Initialize Quiz State

### User Action
Clicks "Start Quiz" button

### Frontend Actions

**1. Fetch Full Quiz Data (if not already loaded):**
```http
GET /api/quizzes/{quiz-id}
Authorization: Bearer {access-token}
x-api-key: {your-api-key}
```

**2. Initialize Quiz State:**
```javascript
const quizState = {
  quizId: "quiz-1-guid",
  quizName: "Breathing Style Quiz",
  questions: [...], // Array of question objects from API
  totalQuestions: 8,
  currentQuestionIndex: 0,
  responses: [],    // Will be populated as user answers
  isSubmitting: false,
  error: null
};
```

**3. Navigate to Quiz Taking Interface:**
Route: `/quizzes/{quiz-id}/take`

---

## Step 4: Quiz Taking Interface - Question by Question

### Frontend Display
**Page:** `/quizzes/{quiz-id}/take`

**Layout:**

**Top Bar:**
- Quiz name (small, for context)
- Progress indicator: "Question 1 of 8"
- Progress bar: Visual representation (12.5% complete)
- Exit button (X) with confirmation

**Main Content Area:**

**Question Section:**
- Question text (large, easy to read)
  - Example: "How stressed do you feel right now?"
- Optional: Question description or context

**Response Options:**
- Radio buttons (single choice)
- Options displayed clearly
- Each option shows:
  - Label text
  - Optional: Icon or emoji
  - Visual feedback when selected (highlighted, checkmark)

**Navigation Buttons:**
- **"Previous"** button (left) - Disabled on first question
- **"Next"** button (right) - Enabled when option selected
- On last question: **"Submit Quiz"** instead of "Next"

**Example UI:**
```
────────────────────────────────────────────────────
Breathing Style Quiz                         [X]

Question 1 of 8                    ▓▓░░░░░░░░ 12%

────────────────────────────────────────────────────

How stressed do you feel right now?

Choose the option that best describes your current state


○  Very stressed
   Feeling overwhelmed, anxious, or tense

●  Moderately stressed                        ✓
   Some tension, but manageable

○  Not stressed
   Feeling calm and relaxed


────────────────────────────────────────────────────

                    [  Previous  ]  [    Next    ]

────────────────────────────────────────────────────
```

### Quiz State Management

**Data Structure:**
```javascript
{
  quizId: "quiz-1-guid",
  currentQuestionIndex: 0,  // 0-based index
  totalQuestions: 8,
  questions: [
    {
      id: "q1-guid",
      text: "How stressed do you feel right now?",
      position: 1,
      responsesOptions: [
        {
          id: "opt1-guid",
          label: "Very stressed",
          position: 1,
          targetedField: "Difficulty",
          operation: "SET",
          value: "3"
        },
        {
          id: "opt2-guid",
          label: "Moderately stressed",
          position: 2,
          targetedField: "Difficulty",
          operation: "SET",
          value: "2"
        },
        {
          id: "opt3-guid",
          label: "Not stressed",
          position: 3,
          targetedField: "Difficulty",
          operation: "SET",
          value: "1"
        }
      ]
    },
    // ... more questions
  ],
  responses: [
    {
      questionId: "q1-guid",
      selectedOptionId: "opt2-guid"  // User selected "Moderately stressed"
    }
    // ... more responses added as user progresses
  ]
}
```

### User Interactions

#### Select an Option
**User Action:** Clicks on a response option

**Frontend:**
1. Highlight selected option
2. Deselect any previously selected option for this question
3. Update state:
```javascript
// Remove existing response for this question (if any)
responses = responses.filter(r => r.questionId !== currentQuestion.id);

// Add new response
responses.push({
  questionId: currentQuestion.id,
  selectedOptionId: selectedOption.id
});
```
4. Enable "Next" button

---

#### Click "Next"
**User Action:** Clicks "Next" button

**Validation:**
- Ensure an option is selected
- If not: Show error "Please select an option"

**Frontend:**
1. Save current response to state (already done on selection)
2. Increment `currentQuestionIndex`
3. Render next question
4. Pre-select option if user previously answered this question (navigating back)
5. Update progress bar
6. Scroll to top

---

#### Click "Previous"
**User Action:** Clicks "Previous" button

**Frontend:**
1. Decrement `currentQuestionIndex`
2. Render previous question
3. Pre-select the option user previously chose
4. Update progress bar
5. Disable "Previous" if now on first question

---

#### Last Question - "Submit Quiz"

**Condition:** `currentQuestionIndex === totalQuestions - 1`

**Frontend:**
- "Next" button changes to "Submit Quiz"
- Optional: Change button color to indicate finality
- On click → Proceed to Step 5

---

### Advanced Features (Optional)

**Auto-save Progress:**
- Save to localStorage every time user answers
- On page refresh, restore progress
- Show notification: "Progress restored"

**Question Validation:**
- Mark required questions
- Allow skip for optional questions
- Show visual indicator of answered vs unanswered

**Keyboard Navigation:**
- Number keys (1-9) to select options
- Arrow keys to navigate options
- Enter to confirm and go to next
- Backspace to go to previous

---

## Step 5: Submit Quiz & Generate Configuration

### User Action
Clicks "Submit Quiz" on last question

### Frontend Validation

**Pre-submission Checks:**
1. All required questions answered?
2. Responses array complete?
3. Each response valid?

**If Validation Fails:**
- Show error message: "Please answer all questions"
- Highlight unanswered questions
- Navigate to first unanswered question
- Do NOT submit to API

**Validation Code Example:**
```javascript
function validateQuizCompletion(questions, responses) {
  // Check all questions have responses
  const answeredQuestionIds = responses.map(r => r.questionId);
  const unansweredQuestions = questions.filter(
    q => !answeredQuestionIds.includes(q.id)
  );

  if (unansweredQuestions.length > 0) {
    return {
      valid: false,
      error: "Please answer all questions",
      unansweredQuestions
    };
  }

  return { valid: true };
}
```

### API Call: Submit Quiz Responses

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
    },
    {
      "questionId": "q3-guid",
      "selectedOptionId": "opt7-guid"
    },
    // ... all 8 responses
  ]
}
```

### Frontend During Submission

**Show Loading State:**
1. Disable "Submit Quiz" button
2. Show loading spinner
3. Display message: "Generating your personalized configuration..."
4. Optional: Show interesting facts or tips while waiting

**Example Loading UI:**
```
┌─────────────────────────────────────┐
│                                     │
│         ⏳                          │
│                                     │
│  Generating Your Configuration...  │
│                                     │
│  Analyzing your responses...        │
│  Calculating breathing pattern...   │
│  Optimizing for your goals...       │
│                                     │
│  ━━━━━━━━━━━━━━━━━━ 75%            │
│                                     │
└─────────────────────────────────────┘
```

### Success Response (201 Created)

```json
{
  "id": "new-config-guid",
  "name": "Generated from Breathing Style Quiz - 2025-01-27 16:30:00",
  "inhalation": 4,
  "retention1": 7,
  "exhalation": 8,
  "retention2": 0,
  "durationMinutes": 10,
  "difficulty": 2,
  "objective": "Relaxation",
  "guidanceType": "Visual",
  "creationTime": "2025-01-27T16:30:00Z",
  "updateTime": null
}
```

### Understanding the Backend Logic

The backend uses the `ResponseOption` rules to generate the configuration:

**ResponseOption Structure:**
```json
{
  "targetedField": "Difficulty",    // Which config field to modify
  "operation": "SET",                // How to apply: SET, ADD, MULTIPLY
  "value": "2"                       // Value to apply
}
```

**Example Processing:**
```
Base Configuration (default values):
{
  inhalation: 4,
  retention1: 4,
  exhalation: 4,
  retention2: 4,
  durationMinutes: 10,
  difficulty: 1,
  objective: "General",
  guidanceType: "Visual"
}

User's Response #1:
  Question: "How stressed?"
  Selected: "Moderately stressed"
  Option rule: SET Difficulty = 2

Result: difficulty = 2

User's Response #2:
  Question: "Primary goal?"
  Selected: "Relaxation"
  Option rule: SET Objective = "Relaxation"

Result: objective = "Relaxation"

User's Response #3:
  Question: "Preferred breathing speed?"
  Selected: "Slower"
  Option rule: ADD 3 to Retention1

Result: retention1 = 4 + 3 = 7

User's Response #4:
  Question: "Longer exhale?"
  Selected: "Yes, for relaxation"
  Option rule: MULTIPLY Exhalation by 2

Result: exhalation = 4 * 2 = 8

... and so on for all questions
```

**Final Generated Configuration:**
```json
{
  "inhalation": 4,      // Default
  "retention1": 7,      // Modified by ADD operation
  "exhalation": 8,      // Modified by MULTIPLY operation
  "retention2": 0,      // Modified by SET to 0
  "durationMinutes": 10, // Default
  "difficulty": 2,      // SET operation
  "objective": "Relaxation", // SET operation
  "guidanceType": "Visual" // Default
}
```

### Error Handling

#### 400 Bad Request - Invalid Quiz
```json
{
  "error": "Quiz not found"
}
```
**Frontend:**
- Show error message
- Offer to go back to quiz list
- Log error for debugging

---

#### 400 Bad Request - Invalid Question
```json
{
  "error": "Question {question-guid} does not belong to quiz {quiz-guid}"
}
```
**Frontend:**
- This shouldn't happen if using data from API correctly
- Show generic error: "Something went wrong. Please try again."
- Offer to restart quiz
- Log error with full state for debugging

---

#### 400 Bad Request - Invalid Option
```json
{
  "error": "Option {option-guid} does not belong to question {question-guid}"
}
```
**Frontend:**
- Similar to above
- Indicate data integrity issue
- Offer to restart quiz

---

#### Network Error / Timeout
**Frontend:**
- Show retry button
- Preserve quiz responses (don't lose data!)
- Allow user to retry submission
- Message: "Connection error. Your responses are saved. Click Retry."

---

### Frontend Action on Success

1. **Store Configuration:** Save to state/context
2. **Show Success Animation:** Brief celebration (✓ checkmark, confetti)
3. **Navigate to Result Page:** `/configurations/{new-config-guid}/result`

---

## Step 6: View Configuration Result

### Frontend Display
**Page:** `/configurations/{config-id}/result`

**Layout:**

**Success Header:**
- ✅ Large checkmark icon
- "Your Personalized Configuration is Ready!"
- Subtext: "Based on your quiz responses"

**Configuration Card:**

**Name & Metadata:**
- Configuration name (editable inline, optional)
- Created: "Just now"
- Source: "Generated from Breathing Style Quiz"

**Breathing Pattern Details:**
```
┌─────────────────────────────────────────────┐
│  Breathing Pattern                          │
│                                             │
│  🫁 Inhale:  4 seconds                      │
│  ⏸️  Hold:    7 seconds                      │
│  🌬️  Exhale:  8 seconds                      │
│  ⏸️  Hold:    0 seconds                      │
│                                             │
│  Total cycle: 19 seconds                    │
│  Cycles in 10 min: ~31                      │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│  Session Details                            │
│                                             │
│  ⏱️  Duration: 10 minutes                    │
│  ⭐ Difficulty: ⭐⭐☆☆☆ (2/5)               │
│  🎯 Objective: Relaxation                   │
│  🎨 Guidance: Visual                        │
└─────────────────────────────────────────────┘
```

**Live Preview:**
- Animated breathing circle
- Shows the actual pattern: 4-7-8-0
- User can watch before committing to exercise
- Play/Pause preview

**Action Buttons (Prominent):**
- **"Start Exercise"** (large, primary CTA)
- "Save & Customize" (secondary) - Edit configuration
- "Retake Quiz" (tertiary) - Start quiz over
- "View All Configurations" (link) - Go to configurations list

**Why This Pattern?**
- Explanation section (optional):
  - "Your answers indicated you want relaxation"
  - "The extended hold (7s) and longer exhale (8s) activate parasympathetic nervous system"
  - "This pattern is ideal for reducing stress"

### User Actions

#### Click "Start Exercise"
**Immediate:** Navigate to Exercise Player (`/configurations/{config-id}/exercise`)

---

#### Click "Save & Customize"
**Action:** Navigate to Edit Configuration page
**Pre-filled:** With current configuration
**User Can:** Adjust any values
**On Save:** Updates configuration, returns to result page

---

#### Click "Retake Quiz"
**Confirmation:** "Retake quiz? Current configuration will be kept."
**Action:** Navigate back to quiz start (`/quizzes/{quiz-id}`)
**Note:** Current configuration is already saved, won't be lost

---

#### Click "View All Configurations"
**Action:** Navigate to configurations list (`/configurations`)
**Note:** New configuration already added to list

---

## Step 7: Complete Breathing Exercise

### Frontend Display
**Page:** `/configurations/{config-id}/exercise`

This is the final step where the user actually uses their personalized configuration.

**Full details available in:**
- [New User Path - Step 10](./NEW_USER_PATH.md#step-10-first-breathing-exercise)
- [Returning User Path - Exercise Player](./RETURNING_USER_PATH.md#breathing-exercise-player-details)

### Quick Summary

**Elements:**
- Full-screen breathing animation
- Phase indicator (Inhale/Hold/Exhale)
- Timers (current phase + total session)
- Progress bar
- Play/Pause/Stop controls

**Flow:**
1. Animation starts automatically
2. User follows visual cues:
   - Circle expands → "Breathe In" (4 seconds)
   - Circle stays large → "Hold" (7 seconds)
   - Circle contracts → "Breathe Out" (8 seconds)
   - (No second hold in this example)
3. Pattern repeats for 10 minutes
4. Session completes
5. Show completion screen with statistics

**Completion:**
- User has successfully completed entire quiz-to-exercise flow
- Configuration saved to their library
- Can repeat exercise anytime from configurations list

---

## Complete Flow Diagram

```
User Journey: Quiz to Exercise
═══════════════════════════════════════════════════════════

1. Browse Quizzes
   ├─ GET /api/quizzes
   └─ Display quiz cards
         │
         ▼
2. View Quiz Details (optional)
   ├─ GET /api/quizzes/{id}
   └─ Show quiz info & questions preview
         │
         ▼
3. Start Quiz
   ├─ Initialize quiz state
   └─ Navigate to quiz-taking UI
         │
         ▼
4. Answer Questions (Loop)
   ├─ Display question
   ├─ User selects option
   ├─ Save response
   ├─ Progress to next question
   └─ Repeat for all questions
         │
         ▼
5. Submit Quiz
   ├─ Validate all questions answered
   ├─ POST /api/user-saved-configurations/from-quiz
   ├─ Backend generates configuration
   └─ Receive new configuration
         │
         ▼
6. View Result
   ├─ Display configuration details
   ├─ Show breathing pattern
   ├─ Animated preview
   └─ Provide action options
         │
         ▼
7. Start Exercise
   ├─ Navigate to exercise player
   ├─ Run breathing animation
   ├─ Follow breathing pattern
   └─ Complete session
```

---

## State Management Example

**React Example:**
```javascript
// Quiz state
const [quizState, setQuizState] = useState({
  quizId: null,
  quizName: '',
  questions: [],
  currentQuestionIndex: 0,
  responses: [],
  isSubmitting: false,
  generatedConfig: null,
  error: null
});

// Functions
function selectOption(questionId, optionId) {
  setQuizState(prev => ({
    ...prev,
    responses: [
      ...prev.responses.filter(r => r.questionId !== questionId),
      { questionId, selectedOptionId: optionId }
    ]
  }));
}

function nextQuestion() {
  if (quizState.currentQuestionIndex < quizState.questions.length - 1) {
    setQuizState(prev => ({
      ...prev,
      currentQuestionIndex: prev.currentQuestionIndex + 1
    }));
  }
}

function previousQuestion() {
  if (quizState.currentQuestionIndex > 0) {
    setQuizState(prev => ({
      ...prev,
      currentQuestionIndex: prev.currentQuestionIndex - 1
    }));
  }
}

async function submitQuiz() {
  setQuizState(prev => ({ ...prev, isSubmitting: true, error: null }));

  try {
    const response = await fetch('/api/user-saved-configurations/from-quiz', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'application/json',
        'x-api-key': apiKey
      },
      body: JSON.stringify({
        quizId: quizState.quizId,
        responses: quizState.responses
      })
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error);
    }

    const config = await response.json();
    setQuizState(prev => ({ ...prev, generatedConfig: config, isSubmitting: false }));
    navigate(`/configurations/${config.id}/result`);

  } catch (error) {
    setQuizState(prev => ({ ...prev, error: error.message, isSubmitting: false }));
  }
}
```

---

## UI/UX Best Practices

### Quiz Taking Interface

**Do:**
- ✅ Show clear progress indicator
- ✅ Allow navigation back to previous questions
- ✅ Pre-select previously chosen options
- ✅ Use large, easy-to-tap/click options
- ✅ Provide visual feedback on selection
- ✅ Save progress automatically
- ✅ Validate before submission
- ✅ Show loading state during generation

**Don't:**
- ❌ Allow submission with unanswered questions
- ❌ Lose user progress on refresh
- ❌ Hide progress indicator
- ❌ Use confusing navigation
- ❌ Show too many options per question (max 5-6)

---

### Configuration Result Page

**Do:**
- ✅ Make "Start Exercise" button prominent
- ✅ Show breathing pattern visually
- ✅ Provide preview animation
- ✅ Explain why this pattern was chosen
- ✅ Allow customization
- ✅ Save configuration automatically

**Don't:**
- ❌ Hide the breathing pattern details
- ❌ Force immediate exercise start
- ❌ Prevent editing
- ❌ Lose the configuration if user navigates away

---

## Related Documentation

### User Journeys
- [New User Path](./NEW_USER_PATH.md) - Complete first-time experience
- [Returning User Path](./RETURNING_USER_PATH.md) - Regular user workflow

### API Reference
- [Quizzes API](../../FEATURES/QUIZZES/README.md)
- [Configurations API](../../FEATURES/CONFIGURATIONS/README.md)
- [Error Handling](../../REFERENCE/ERROR_HANDLING.md)

---

## Developer Checklist

### Quiz Browsing
- [ ] Quiz list page with cards
- [ ] Quiz details page/modal
- [ ] Loading states
- [ ] Empty state (no quizzes)

### Quiz Taking
- [ ] Question display interface
- [ ] Single-choice selection (radio buttons)
- [ ] Progress indicator (question X of N)
- [ ] Progress bar (visual percentage)
- [ ] Previous/Next navigation
- [ ] Response state management
- [ ] Pre-select on navigation back
- [ ] Exit confirmation

### Validation & Submission
- [ ] Validate all questions answered
- [ ] Show unanswered questions
- [ ] Loading state during submission
- [ ] Error handling (400, 404, 500, network)
- [ ] Retry mechanism

### Result Display
- [ ] Configuration details card
- [ ] Breathing pattern visualization
- [ ] Animated preview
- [ ] Action buttons (Start, Edit, Retake, View All)
- [ ] Success messaging

### Exercise Player
- [ ] Breathing animation
- [ ] Phase indicators
- [ ] Timers
- [ ] Controls (Play/Pause/Stop)
- [ ] Session completion

### State Management
- [ ] Quiz state (questions, responses, current index)
- [ ] Response tracking
- [ ] Configuration result storage
- [ ] Auto-save progress (localStorage)

---

**This completes the core quiz-to-exercise flow!** Users can now discover, create, and use personalized breathing configurations.
