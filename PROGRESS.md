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

---

## 📂 Current Repository State
- **Backend Branch**: `CoreMe`
- **Backend Solution**: `CoreMe.sln`
- **Frontend Path**: `F:\jsb\pesteh\bbb`
- **Last Updated**: 2026-08-29
 
### Recent Active Work & Completed Tasks
- **BBB End Meeting Interception & Custom Modal with Najm Sync ([component.jsx](file:///F:/bbb/bbb-plugin-before-close-end-meeting/src/plugin-hello-world/component.jsx), [endMeetingConfirmModal.jsx](file:///F:/bbb/bbb-plugin-before-close-end-meeting/src/plugin-hello-world/endMeetingConfirmModal.jsx))**:
  - **`endMeetingButton` Override**: Intercepted `#endMeetingButton` clicks in the 3-dots dropdown menu to prevent BBB's default popup.
  - **Custom End Meeting Dialog**: Added modal with **لغو عملیات**, **بستن جلسه برای همه**, and **ارسال اطلاعات به سامانه نجم** checkbox (with automatic fallback to `MemberModal` if members unorganized, and trigger of `confirmEndMeeting` upon completion).
  - **Plugin Bundle**: Rebuilt production bundle `PluginBeforeCloseEndMeeting.js`.
- **Join Room Page Redesign & National Code OTP Verification ([JoinForm.jsx](file:///F:/jsb/pesteh/bbb/src/Pages/JoinForm.jsx))**:
  - **National Code Lookup for Assembly Members (من از اعضاء مجمع هستم)**: Replaced raw phone number input with 10-digit National Code (کد ملی).
  - **Masked Phone Number Notice**: Upon sending OTP SMS via `SendOTPConfirmation`, the backend looks up the member's registered phone, sends the code, and returns the masked number (e.g. `0912*****49` / `912*****49`) displayed clearly to the user.
  - **Backend Support ([UserUseCase.cs](file:///F:/DOTNET/New%20folder/Application/Usecase/UserUseCase.cs), [UserDTO.cs](file:///F:/DOTNET/New%20folder/Application/DTO/UserDTO.cs), [BBBController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/BBBController.cs))**: Enhanced `SendOTPConfirmation` and `ConfirmOTP` to query by `NationalCode`, validate `IsMemberVIP`, and return `MaskedPhone`, `Fullname`, and auth token.
  - **UI/UX Modernization**: Redesigned the Join Room page with Islamic spiritual aesthetics, emerald & gold glassmorphism, responsive cards, live session indicators, reservation forms, and certificate management.
- **Member Presence Modal - Virtual/In-Person Mode & Add Hozuri User ([MemberModal.jsx](file:///F:/bbb/bbb-plugin-before-close-end-meeting/src/plugin-hello-world/memberModal.jsx))**:
  - **Virtual vs In-Person Tagging**: Added one-click toggle button on each attendee row to toggle between **🌐 مجازی (Virtual)** and **🏛️ حضوری (In-Person)**.
  - **Filter Tabs**: Added fast filter tabs: *همه (All)*, *🏛️ حضوری (In-Person)*, *🌐 مجازی (Virtual)*, and *⭐ اعضای مجمع (VIP)*.
  - **Add Hozuri User Form**: Added `+ ثبت حاضرین حضوری` button and card form accepting **نام (First Name)**, **نام خانوادگی (Last Name)**, and **کد ملی (National Code)**.
  - **Backend API Support**: Added `POST /user/AddHozuriMember` and enhanced `POST /user/EditMemberList` & `GET /user/GetTheMembers` in [LogicalController.cs](file:///F:/DOTNET/New%20folder/CoreMe/Controllers/LogicalController.cs) and [UserUseCase.cs](file:///F:/DOTNET/New%20folder/Application/Usecase/UserUseCase.cs).
  - **Plugin Bundle**: Rebuilt `PluginBeforeCloseEndMeeting.js` bundle.
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
