# Project Progress & Handoff Log (CoreMe / Hozur2)

## 🌐 Project Context & Architecture
- **Domain**: Classrooms website of `Hekmateislami.com`
- **Backend Location**: `F:\DOTNET\New folder` (`CoreMe.sln` / ASP.NET Core API - **PERMANENT / NEVER DELETE**)
- **Frontend Location (Primary)**: `F:\jsb\pesteh\bbb` (Vite + React / Tailwind JS frontend - **PERMANENT / NEVER DELETE**)
- **Payment Frontend**: `F:\jsb\pesteh\payment` (Financial & Payment Tools frontend - **NEVER DELETE**)
- **BigBlueButton Plugins Location**: `F:\bbb` (BigBlueButton plugin folders - **PERMANENT / NEVER DELETE**)
- **BigBlueButton Complete MD Documentation Suite**: `F:\bbb\md` (Master Index: [README.md](file:///F:/bbb/md/README.md) - **PERMANENT / NEVER DELETE**)

---

## 📌 Workflow Rules
1. **Git Commits**: Every major task completion or logical milestone will be committed with a descriptive commit message.
2. **Progress Updates**: This file (`PROGRESS.md`) will be updated after significant changes or at the end of each session.
3. **Conciseness**: Keep this file concise and focused on top priorities. Summarize past logs if it gets too long.
4. **Mandatory Documentation Reading**: When working on BigBlueButton, plugins, data channels, or Greenlight integrations, **ALWAYS** consult the documentation files in `F:\bbb\md` (starting with `F:\bbb\md\README.md`). NEVER delete `F:\bbb\md` or any `.md` file inside it.
5. **Language Requirement**: **ALWAYS answer all user requests in English.** Regardless of the language in which the user prompts or asks questions (e.g. Persian / Farsi), the AI assistant must always respond, explain, and write documentation in English.

---

## 📂 Current Repository State
- **Backend Branch**: `CoreMe`
- **Backend Solution**: `CoreMe.sln`
- **Frontend Path**: `F:\jsb\pesteh\bbb`
- **Last Updated**: 2026-09-19
 
- **Najm System Meeting Synchronization (`POST /groups/addmeeting`) ([CommonMethods.cs](file:///F:/DOTNET/New%20folder/Application/General/CommonMethods.cs), [UserUseCase.cs](file:///F:/DOTNET/New%20folder/Application/Usecase/UserUseCase.cs))**:
  - **Dynamic Token Generation**: Requests a 120-minute token via `GET http://192.168.0.4:1940/api/token/generate?minutes=120` prior to dispatching each synchronization payload.
  - **Single-Character GroupID Validation**: Extracts and trims `room.NajmID` to a single character (`groupid`), skipping API dispatch if empty or null without disrupting meeting close operations.
  - **Meeting Roles & Attendance Mapping**:
    - **Boss**: Uses `NajmIDBoss` directly from `roomTBL` (`room.NajmIDBoss` or "0"), with BBB ID mapped to attendee record ID or "0".
    - **Presenter**: Primary meeting instructor (`MainEntity_Types.USER_TEACHER` or `meeting.Teacher`).
    - **Dabir**: Session secretary (`MainEntity_Types.USER_DABIR` or `RoomTBLPlanTBL.Dabir`).
    - **ListHozour**: Sends all `UserTBLToMeetingTBL` records for the meeting **except** teachers and dabir.
    - **Default Field Rules**: If `user.NajmID` is null/empty, sends `"0"`; if `NationalCode` is null/empty, sends `""`; for `bbbId`, sends the numeric ID of `UserTBLToMeetingTBL` (`um.ID.ToString()`).
    - **Dastor**: Composite Iranian meeting agenda combining session topic (`Title`) and detailed description/agenda (`Explain`).
  - **NajmID Persistence**: Automatically stores returned Najm meeting ID (`response.id`) into `MeetingTBL.NajmID` upon successful synchronization.
  - **Overloaded Entry Points**: Provided `SyncWithNajm(string meetId)` and backward-compatible `SyncWithNajm(MeetingTBL?, UserTBL?)` in `CommonMethods`.

- **Najm Boss Selection in Edit Room & Token Verification API ([RoomTBL.cs](file:///F:/DOTNET/New%20folder/Domain/Entity/RoomTBL.cs), [LogicalController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/LogicalController.cs), [EditRoomModal.jsx](file:///F:/jsb/pesteh/bbb/src/Modal/Admin/EditRoomModal.jsx), [AdminAPI.jsx](file:///F:/jsb/pesteh/bbb/src/api/AdminAPI.jsx))**:
  - **Entity & Migration (`db741`)**: Added `public string? NajmIDBoss { get; set; }` to `RoomTBL` entity and applied migration `20260917081628_db741` to the database. Also mapped `NajmID` and `NajmIDBoss` into `MiniRoomJSON` and `GetAllRoomsUseCase`.
  - **Token Generation & Bosses Fetch API (`GET /room/GetNajmBosses`)**: Retrieves token from `http://192.168.0.4:1940/api/token/generate?minutes=1` and fetches all bosses from `http://192.168.0.4:1940/api/groups/bosses` with header `X-Api-Token`, with retry and memory token management.
  - **Boss Validation & Assignment API (`POST /room/SetRoomNajmBoss`)**: Dedicated endpoint that takes `FriendlyID` and `NajmId`, generates a fresh 1-minute token to check against `http://192.168.0.4:1940/api/groups/bosses` to verify the boss exists in the Najm system, and updates `room.NajmIDBoss` and `room.NajmID` in the database.
  - **Frontend Searchable Dropdown**: Enhanced `EditRoomModal.jsx` with a real-time searchable dropdown filtering all 700+ Najm bosses by name or NajmId, displaying selected boss badge with remove/clear functionality, and submitting to `SetRoomNajmBoss`. Also integrated into `RoomManagement.jsx`.
  - **VIP Assembly Member Auto-Lock**: For verified members (`isOtpVerified` via OTP or logged-in VIP `user.isVIPMember`), the "اطلاعات گواهی حضور را تکمیل می‌نمایم" checkbox is automatically checked (`wantCertificate = true`), saved (`certInfoSaved = true`), and locked/disabled. The member's registered national code and phone are automatically mapped from the database/session without requiring redundant inputs, and edit capabilities are locked while verified. Toggling member mode off restores unlocked state.
  - **Non-Member Verification & SMS OTP API (`POST /user/VerifyCertificateUser` & `POST /user/ConfirmCertVerification`)**:
    - When an attendee checks certificate receipt and submits "✓ تایید اطلاعات گواهی", the API queries `UserTBLs` by normalized phone number or national code.
    - If user does not exist, checks candidate UUID from cookies (`bbbUserToken` or `bbbUserToken_random` starting with `userVc2-Rand`) to reuse the visitor's record without creating duplicate users; otherwise creates a new user record with that UUID.
    - Generates a 4-digit OTP code, stores hash/salt with 10-minute expiry and rate limit tracking, and dispatches SMS to the user's phone.
    - `ConfirmCertVerification` verifies the OTP code, updates user `PhoneNumber` and `NationalCode` in the database, and generates and returns the `vc2AuthNorm` JWT authentication token so the user is authenticated.
  - **Frontend Inline OTP Flow**: Integrated step-by-step OTP verification directly within the certificate section in `JoinForm.jsx` with phone masking, timer countdown, resend capability, cookie persistence (`vc2AuthNorm`, `certPhone`, `certNationalCode`), and token synchronization (`syncUserTokens`).

- **Unanswered Polls API & Duplicate Answer Prevention ([UserUseCase.cs](file:///F:/DOTNET/New%20folder/Application/Usecase/UserUseCase.cs), [BBBController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/BBBController.cs), [QuestionModal.jsx](file:///F:/bbb/bbb-plugin-get-certificate/src/plugin-generate-certificate/QuestionModal.jsx))**:
  - **New Unanswered Poll API (`GET /user/GetUnansweredPollList` & `GET /user/GetPoll`)**: Created a dedicated endpoint for attendee poll questions that excludes questions already answered by the current user. Retained existing `GET /user/GetPollList` without modification for meeting-wide moderator fetching.
  - **Dual-Layer Answer Detection**: `GetUnansweredPollListUseCase` consolidates sibling attendee records (`UserTBLToMeetingTBL`) by UUID, User ID, National Code, and Phone number. It identifies already-answered questions by querying both `MeetPollQuizAnsTBLToUserMeetTBLs` and attendance log entries in `UserTBLToMeetingTBLLogs` (`keyLog == "POLL_ATTENDANCE_VERIFIED"`).
  - **AnswerPollList Idempotency**: Enhanced `AnswerPollListUseCase` to check whether an answer option has already been recorded for that user-meeting session before inserting, preventing duplicate rows in the database.
  - **Certificate QuestionModal Integration**: Updated `QuestionModal.jsx` in `bbb-plugin-get-certificate` to call `/user/GetUnansweredPollList` with both `authbbb` and standard `Authorization` headers. If all questions have already been answered (e.g. during live attendance in `event-handler`), it automatically sets the poll completion flag in `localStorage`, fires `pollCompletedCallback()`, and closes the modal immediately so the user seamlessly receives their certificate.
  - **Production Bundle**: Rebuilt `dist/PluginGenerateCertification.js` (686 KiB) with 0 errors.

- **Poll-Based Synchronized Attendance Verification ([component.jsx](file:///F:/bbb/bbb-plugin-event-handler/src/plugin-hello-world/component.jsx), [manifest.json](file:///F:/bbb/bbb-plugin-event-handler/manifest.json), [02-event-handler.md](file:///F:/bbb/md/02-event-handler.md))**:
  - **Server-Compatible Data Channel Routing (`online_users_keysents`)**: Resolved `TypeError: pushEntry is not a function` ("function not exist") and inactive useEffect listeners by routing poll attendance sync through `online_users_keysents` with `subChannelName: "poll_attendance"`. Because `online_users_keysents` is already registered on BBB server manifests with full push permissions across all roles, BBB core reliably provides the push entry function without requiring server manifest updates or BBB server restarts.
  - **PushEntry Lifecycle Refs & Stale-Closure Defense**: Wrapped `pushEntry` in `setKeySentRef` and `setPollAttendanceChannelRef` with dynamic fallback helper `sendPollAttendance`. Eliminated stale-closure bugs in the interval timer and added asynchronous retry loops in `fetchPolls` so broadcasts never fail if BBB core is initializing.
  - **Unified Dual-Channel Event Dispatcher**: Connected `handleIncomingPollPayload` across both `pollAttendanceChannel` and `keysSent` listeners, guaranteeing that poll questions and `POLL_ATTENDANCE_ACTIVE` are immediately received and rendered on viewer clients regardless of network delivery channel.
  - **Single Poll Fetch for Meeting**: Moderator/presenter fetches all meeting polls via `GET https://core.hekmateislami.com/user/GetPollList` only once at session start and broadcasts them across the data channel (`DataChannelTypes.LATEST_ITEM`). Late joiners automatically sync the questions without making backend requests.
  - **Question Filtering**: Filters strictly for types `625` (scored) and `627` (unscored) with non-empty answer options (excluding `626` free-text).
  - **Synchronized Awareness Prompts**: Moderator/presenter broadcasts an active attendance event (`POLL_ATTENDANCE_ACTIVE`) with a unique `checkId` and a randomly picked question. All students simultaneously see and answer the exact same question.
  - **Attendance Logging (`POLL_ATTENDANCE_VERIFIED`)**: When a student responds, logs the answer to `POST /user/NewUserMeetingLog` with `keyLog: "POLL_ATTENDANCE_VERIFIED"`, submits the option to `POST /user/AnswerPollList`, and notifies moderators via toast.
  - **Fallback**: Retained the 20 Persian/Islamic Shia attendance themes as an automatic fallback if a meeting has no poll questions.
  - **Production Bundle**: Rebuilt `dist/EventHandler.js` bundle (254 KiB) with 0 errors.
- **Poll Report PDF Export (Questions and Results Only) ([SeeMeetingPollReport.jsx](file:///F:/jsb/pesteh/bbb/src/Pages/Profile/SeeMeetingPollReport.jsx))**:
  - Added **دانلود PDF** action button next to **خروجی Excel** on `/polls/:meetid/report`.
  - Implemented printable layout using `html2pdf.js` containing exclusively the questions and their results (vote counts, percentages, progress bars, scores, and user text responses).
  - Excluded all UI clutter (no navigation header, no statistics cards, no charts, no filter panels, and no pagination).
- **Admin Certificate Generation TokenJWT Fix & Multi-Row Synchronization ([MeetUseCase.cs](file:///F:/DOTNET/New%20folder/Application/Usecase/MeetUseCase.cs), [UserUseCase.cs](file:///F:/DOTNET/New%20folder/Application/Usecase/UserUseCase.cs), [CommonMethods.cs](file:///F:/DOTNET/New%20folder/Application/General/CommonMethods.cs))**:
  - **TokenJWT "creating..." Resolution**: In `GenerateCertificateWithUserMeetIDUseCase`, resolved issues where `TokenJWT` remained `"creating..."` by borrowing the real JWT from existing active session rows or generating a cryptographically valid JWT using `Statics.GenerateJWTUser`. Also updated `GetMeetingURLUseCase` to generate a real JWT upon initial join link request instead of storing `"creating..."`.
  - **Multi-Row Sibling Consolidation**: When an attendee has multiple entries in `UserTBLToMeetingTBL` for the same meeting (e.g. initial URL request vs active BBB session with duration), `GenerateCertificateWithUserMeetIDUseCase` discovers all sibling rows matching by `NationalCode`, `Phone`, `UUID`, or `UserTBL.ID`.
  - **Bidirectional Sync**: Prioritizes the active row with highest `TimeSpendSeconds` and valid `TokenJWT`. Simultaneously propagates `IsCertificated = true`, `CertificateFileURL`, and `CertificateTBL` records across all sibling records so both the active BBB plugin session and the Admin dashboard display certificate availability.
  - **Protected Certificate Flags**: In `CommonMethods.NewUserMeeting`, prevented resetting existing `IsCertificated = true` back to `false` and preserved recorded `TimeSpendSeconds`.
- **Admin Certificate Generation by Phone and National Code**:
  - **Backend Support ([MeetDTO.cs](file:///F:/DOTNET/New%20folder/Application/DTO/MeetDTO.cs), [MeetUseCase.cs](file:///F:/DOTNET/New%20folder/Application/Usecase/MeetUseCase.cs), [CommonMethods.cs](file:///F:/DOTNET/New%20folder/Application/General/CommonMethods.cs), [AdminVM.cs](file:///F:/DOTNET/New%20folder/CoreMe/vm/AdminVM.cs), [AdminController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/AdminController.cs))**:
    - Enhanced `GenerateCertificateWithUserMeetIDRequest` with `Phone`, `MeetID`, and `Name`.
    - Enhanced `GenerateCertificateWithUserMeetIDUseCase` to lookup/match participant by `ID` or by `MeetID` + `NationalCode`/`Phone`, bypass user duration restrictions, update user records with provided phone/national code, create/update `CertificateTBL`, generate official PDF certificate via `WordService`, upload to server via `IScpUploader`, and return direct PDF link.
    - Updated `AdminController` with `POST /room/GenerateCertificateWithUserMeetID` and query parameter support on `GET /room/GenerateCertificateWithUserMeetID/{umid}`.
    - Updated `CommonMethods.GetNationalCode` and `GetMembersOfMeetUseCase` to map `Phone`, `NationalCode`, and `CertificateURL`.
  - **Frontend Support ([GetRooms.jsx](file:///F:/jsb/pesteh/bbb/src/api/GetRooms.jsx), [MiniCalanderMeetingsModal.jsx](file:///F:/jsb/pesteh/bbb/src/Component/Home/MiniCalanderMeetingsModal.jsx))**:
    - Updated `GenerateCertificateWithUserMeetID` API in `GetRooms.jsx` to support POST payload with `nationalCode`, `phone`, `isMale`, `meetID`, and `name`.
    - In `MiniCalanderMeetingsModal.jsx`: Added **صدور گواهی** / **مدیریت گواهی** action buttons to each attendee row in the participant list table.
    - Added **+ صدور گواهینامه با کد ملی و موبایل** toolbar button to issue certificates for participants directly.
    - Added dedicated certificate generation modal with pre-filled attendee info, national code & phone validation, gender selector, live status updates, and copyable/clickable PDF download link.
    - Expanded search filter to match by national code and phone number.
- **BBB Plugins Cookie Optimization & SessionStorage/LocalStorage Migration**:
  - **JWT Claims Migration to `sessionStorage` ([cookieExpiremanager.jsx](file:///F:/bbb/bbb-plugin-new-meeting/src/plugin-hello-world/cookieExpiremanager.jsx), [component.jsx](file:///F:/bbb/bbb-plugin-new-meeting/src/plugin-hello-world/component.jsx), [jwtClaimsManager.jsx](file:///F:/bbb/bbb-plugin-show-timer/src/plugin-hello-world/jwtClaimsManager.jsx))**: Replaced storing JWT claims in 24-hour cookies with tab-scoped `sessionStorage` (`${meetId}~uauth~${claimKey}~${extId}`), freeing up browser cookie header payload space and auto-cleaning legacy cookies.
  - **Timer Keys Migration & 5-Hour Auto-Eviction in `show-timer` ([jwtClaimsManager.jsx](file:///F:/bbb/bbb-plugin-show-timer/src/plugin-hello-world/jwtClaimsManager.jsx), [component.jsx](file:///F:/bbb/bbb-plugin-show-timer/src/plugin-hello-world/component.jsx))**: Replaced `htimerpresenter` and `timerPresenter` cookies with `localStorage` keys containing `meetId` and creation timestamp `${meetId}~${extId}~(htimerPresenter|timerPresenter)~${Date.now()}`. Implemented 5-hour automatic lifecycle cleanup routines running on startup and periodically every 60s.
  - **National Code `_na` & Guarantee `_certificated` Storage in `get-certificate` ([component.jsx](file:///F:/bbb/bbb-plugin-get-certificate/src/plugin-generate-certificate/component.jsx))**: Migrated national code cache `${userToken}_na` and certificate guaranteed flag `${meetingId}_certificated` from cookies to `localStorage`. Implemented `cleanupMismatchedNaKeys` (removes cached national codes if user extId/token does not match) and `cleanupOldCertificatedKeys` (auto-removes expired certificated keys > 12h or foreign meetings).
  - **Production Builds**: Rebuilt bundles for `bbb-plugin-new-meeting` (`PluginNewMeeting.js`), `bbb-plugin-show-timer` (`ShowTimer.js`), and `bbb-plugin-get-certificate` (`PluginGenerateCertification.js`) with 0 errors.
- **BBB Presenter Panel Live User Attendance Timer & VIP/Registered Styling ([component.jsx](file:///F:/bbb/bbb-plugin-show-timer/src/plugin-hello-world/component.jsx), [jwtClaimsManager.jsx](file:///F:/bbb/bbb-plugin-show-timer/src/plugin-hello-world/jwtClaimsManager.jsx), [he.jsx](file:///F:/bbb/bbb-plugin-show-timer/src/plugin-hello-world/he.jsx))**:
  - **Fixed Name Overflow / Truncation**: Relocated the live timer badge to the subtext line (`.sc-gOqvNp`) beneath attendee names, giving the name element (`.sc-lmEHzG`) 100% full width so names are never hidden or truncated.
  - **Moderator-Only VIP & Registered Element Styling (Zero Extra Tags)**: Modifies existing DOM elements (row background gradient, accent border, avatar glow ring, and bold name text) visible only to moderators to uniquely highlight **VIP Members** (Gold/Amber glow) and **Registered Members** (Emerald glow), with VIP taking precedence.
  - **Fixed Timer Fast-Advance Acceleration**: Replaced raw increment counters with `Date.now()` timestamp delta calculations, preventing speedups caused by rapid `MutationObserver` triggers or React re-renders.
  - **Conditional Minutes vs Seconds Formatting**: Displays seconds for durations $< 1$ min (e.g. `۴۵ ثانیه`) and minutes/hours for durations $\ge 1$ min (e.g. `۵ دقیقه`, `۱ ساعت و ۱۰ دقیقه`) in Persian numerals.
- **BBB End Meeting Interception & Najm Sync Integration ([component.jsx](file:///F:/bbb/bbb-plugin-before-close-end-meeting/src/plugin-hello-world/component.jsx), [endMeetingConfirmModal.jsx](file:///F:/bbb/bbb-plugin-before-close-end-meeting/src/plugin-hello-world/endMeetingConfirmModal.jsx), [CommonMethods.cs](file:///F:/DOTNET/New%20folder/Application/General/CommonMethods.cs))**:
  - **`endMeetingButton` Override & Checkbox Sync**: Intercepted `#endMeetingButton` clicks, and passed the **ارسال اطلاعات به سامانه نجم** (`sendToNajm`) checkbox state directly to the API `/user/EndTheMeeting`.
  - **Backend Support ([UserVM.cs](file:///F:/DOTNET/New%20folder/CoreMe/vm/UserVM.cs), [UserDTO.cs](file:///F:/DOTNET/New%20folder/Application/DTO/UserDTO.cs), [UserUseCase.cs](file:///F:/DOTNET/New%20folder/Application/Usecase/UserUseCase.cs))**: Added `SendToNajm` boolean field to `EndTheMeetingViewModel` and `EndTheMeetingRequest`. In `EndTheMeetingUseCase`, when `SendToNajm` is checked, it invokes `_commonMethods.SyncWithNajm(meet, user)`.
  - **`CommonMethods.SyncWithNajm` Placeholder**: Created `SyncWithNajm` in `CommonMethods.cs` that logs the sync process to console/statics and returns a random HTTP status code (200, 201, 204, 400, 500) for future implementation.
  - **Plugin Bundle**: Rebuilt production bundle `PluginBeforeCloseEndMeeting.js`.
- **Join Room Page Redesign & National Code OTP Verification ([JoinForm.jsx](file:///F:/jsb/pesteh/bbb/src/Pages/JoinForm.jsx))**:
  - **National Code Lookup for Assembly Members (من از اعضاء مجمع هستم)**: Replaced raw phone number input with 10-digit National Code (کد ملی).
  - **Masked Phone Number Notice**: Upon sending OTP SMS via `SendOTPConfirmation`, the backend looks up the member's registered phone, sends the code, and returns the masked number (e.g. `0912*****49` / `912*****49`) displayed clearly to the user.
  - **Backend Support ([UserUseCase.cs](file:///F:/DOTNET/New%20folder/Application/Usecase/UserUseCase.cs), [UserDTO.cs](file:///F:/DOTNET/New%20folder/Application/DTO/UserDTO.cs), [BBBController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/BBBController.cs))**: Enhanced `SendOTPConfirmation` and `ConfirmOTP` to query by `NationalCode`, validate `IsMemberVIP`, and return `MaskedPhone`, `Fullname`, and auth token.
  - **UI/UX Modernization**: Redesigned the Join Room page with Islamic spiritual aesthetics, emerald & gold glassmorphism, responsive cards, live session indicators, reservation forms, and certificate management.
- **Member Presence Modal - Input Typing Fix, Pointer Events & Interactive Role Badges ([MemberModal.jsx](file:///F:/bbb/bbb-plugin-before-close-end-meeting/src/plugin-hello-world/memberModal.jsx), [EndMeetingConfirmModal.jsx](file:///F:/bbb/bbb-plugin-before-close-end-meeting/src/plugin-hello-world/endMeetingConfirmModal.jsx))**:
  - **`ReactDOM.createPortal` & Direct `document.body` Mounting**: Moved modal DOM rendering out of BBB's plugin host div directly into `document.body` with `z-[9999999]` and `pointerEvents: 'auto'`, completely bypassing parent plugin wrapper pointer event blocks.
  - **Capture-Phase Keyboard & Pointer Protection**: Attached native capture-phase event listeners (`keydown`, `keyup`, `keypress`, `mousedown`, `pointerdown`) to `modalRef` to prevent BigBlueButton's document/window level shortcut handlers from intercepting user keystrokes and focus.
  - **Input Focusability & Auto-Focus**: Added explicit `pointerEvents: 'auto'`, `userSelect: 'text'`, and auto-focus timeouts on `hozuriFirstname`, `hozuriLastName`, `hozuriNationalCode`, and `searchTerm`.
  - **Clickable Role Badge Toggle (⭐ عضو مجمع / مهمان / ✓ ثبت‌نام شده)**: Turned static badges into interactive toggle buttons that instantly switch member status between **⭐ عضو مجمع** (VIP Member) and **مهمان / ثبت‌نام شده** (Guest/Regular) with a single click, and passed `isMember: member.isVIPMember` to `/user/EditMemberList`.
  - **Virtual vs In-Person Tagging**: Added one-click toggle button on each attendee row to toggle between **🌐 مجازی (Virtual)** and **🏛️ حضوری (In-Person)**.
  - **Filter Tabs**: Added fast filter tabs: *همه (All)*, *🏛️ حضوری (In-Person)*, *🌐 مجازی (Virtual)*, and *⭐ اعضای مجمع (VIP)*.
  - **Add Hozuri User Form & Status 200 Verification**: Added `+ ثبت حاضرین حضوری` form with strict response validation. If `/user/AddHozuriMember` returns `data.status === 200`, the attendee is added and the form resets. If the API returns any non-200 status or error, fake local fallback addition is prevented and a visible error alert is presented to the user.
  - **Backend API Support**: Added `POST /user/AddHozuriMember` and enhanced `POST /user/EditMemberList` & `GET /user/GetTheMembers` in [LogicalController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/LogicalController.cs) and [UserUseCase.cs](file:///F:/DOTNET/New%20folder/Application/Usecase/UserUseCase.cs).
  - **Plugin Bundle**: Rebuilt `PluginBeforeCloseEndMeeting.js` production bundle.
- **BigBlueButton Plugins Documentation Suite (`F:\bbb\md`)**:
  - Created centralized `F:\bbb\md/` documentation directory with master index [README.md](file:///F:/bbb/md/README.md).
  - Documented all 13 BBB plugins, .NET Greenlight API, deployment scripts, data channels architecture matrix, and API contracts.
  - Updated individual plugin `README.md` files with direct cross-references.
- **Payment List Table Inline Cell Editing & PDF Bank/PaymentType Updates**:
  - **Backend Entity ([PaymentTBL.cs](file:///F:/DOTNET/New%20folder/Domain/Entity/PaymentTBL.cs))**: Added `paymentType` nullable property.
  - **Backend PDF Generator ([PDFGeneratorForPaymentTools.cs](file:///F:/DOTNET/New%20folder/CoreMe/PDFGeneratorForPaymentTools.cs))**: Formatted `documentNumberStr` as `"{documentNumberStr} - {bankName}"` and dynamically rendered `paymentType` ("نقد", "چک", "واریز") on PDF output line 360.
  - **Backend Controller ([TaskController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/TaskController.cs))**: Updated `EditPayment`, `AddNewPayment`, and `GetPDFOfPayment` endpoints to handle `paymentType` and recalculate `netAmount` when `amount` or `taxPercent` is updated.
  - **Frontend API ([GetRooms.jsx](file:///F:/jsb/pesteh/payment/src/api/GetRooms.jsx))**: Added exported `EditPayment(data)` API function.
  - **Frontend Add Payment Modal ([PaymentForm.jsx](file:///F:/jsb/pesteh/payment/src/Pages/tools/PaymentForm.jsx))**: Added **نحوه پرداخت** dropdown (نقد / چک / واریز).
  - **Frontend Table Page ([PaymentList.jsx](file:///F:/jsb/pesteh/payment/src/Pages/tools/PaymentList.jsx))**: Added **نام بانک** and **نحوه پرداخت** table columns, expanded floating edit popovers, hover pencil icons, and dropdown/datepicker cell inline editing.

---

## 🎯 Active Status & Completed Fixes
- [x] Live user presence timer badges in presenter/moderator user panel (`[data-test="userListContainer"]`) in [component.jsx](file:///F:/bbb/bbb-plugin-show-timer/src/plugin-hello-world/component.jsx) and [he.jsx](file:///F:/bbb/bbb-plugin-show-timer/src/plugin-hello-world/he.jsx) with `user_live_timer` data channel sync
- [x] Payment Method (`paymentType`) field added to Add Payment Modal, Payment Table, Backend API, and PDF generator
- [x] Expanded floating cell inline editor popup with larger inputs and action buttons in `/pay/list` ([PaymentList.jsx](file:///F:/jsb/pesteh/payment/src/Pages/tools/PaymentList.jsx))
- [x] Bank Name column & PDF generator formatting (`123456 - بانک سامان`) ([PDFGeneratorForPaymentTools.cs](file:///F:/DOTNET/New%20folder/CoreMe/PDFGeneratorForPaymentTools.cs))
- [x] Added Bank Name field to Add Payment Modal in `/pay/list` ([PaymentForm.jsx](file:///F:/jsb/pesteh/payment/src/Pages/tools/PaymentForm.jsx))
- [x] Updated backend PDF generator ([PDFGeneratorForPaymentTools.cs](file:///F:/DOTNET/New%20folder/CoreMe/PDFGeneratorForPaymentTools.cs)) and API controller ([TaskController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/TaskController.cs)) to append Bank Name after Document Number in generated PDF files
- [x] Added `bankName` property to [PaymentTBL.cs](file:///F:/DOTNET/New%20folder/Domain/Entity/PaymentTBL.cs)
- [x] Next Meeting / Upcoming Plans bold & shiny section in `/rooms/{roomid}` page ([RoomDetail.jsx](file:///F:/jsb/pesteh/bbb/src/Pages/RoomDetail.jsx))
- [x] Backend API endpoint `/room/GetUpcomingPlans` & updated `/room/getDetails` in [LogicalController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/LogicalController.cs)
- [x] Show avatar photo column in `/admin/users` user list using `avatarPath` API response data in [UserList.jsx](file:///F:/jsb/pesteh/bbb/src/Pages/Admin/UserList.jsx)
- [x] Exclude ignored meeting types (`MEET_IGNORE`) and deleted/disabled/canceled statuses from `GetTopTeachers` API in [LogicalController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/LogicalController.cs)
- [x] Fixed Edit User Modal backdrop, added Drag & Drop avatar file picker, and mapped `Status` field in `AdminGetUserList`
- [x] Edit User Info API endpoint, frontend API function, and `EditUserModal` component
- [x] Add role filter in `/adminlay/users` frontend (`UserList.jsx`)
- [x] Fix `GetTopTeachers` endpoint in `LogicalController.cs`
- [x] T-SQL queries for `MeetingTBL" cleanup (duration < 30m / null `EndDate` randomization)

---

## 📜 Commit History Summary
- `fcc2354` - docs: add conciseness guideline to PROGRESS.md
- `dede5f0` - docs: add PROGRESS.md handoff tracking file
- `0177225` - e2
