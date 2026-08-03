# Project Progress & Handoff Log (CoreMe / Hozur2)

## 🌐 Project Context & Architecture
- **Domain**: Classrooms website of `Hekmateislami.com`
- **Backend Location**: `F:\DOTNET\New folder` (`CoreMe.sln` / ASP.NET Core API)
- **Frontend Location**: `F:\jsb\pesteh\bbb` (Vite + Tailwind JS frontend)

---

## 📌 Workflow Rules
1. **Git Commits**: Every major task completion or logical milestone will be committed with a descriptive commit message.
2. **Progress Updates**: This file (`PROGRESS.md`) will be updated after significant changes or at the end of each session.
3. **Conciseness**: Keep this file concise and focused on top priorities. Summarize past logs if it gets too long.

---

## 📂 Current Repository State
- **Backend Branch**: `CoreMe`
- **Backend Solution**: `CoreMe.sln`
- **Frontend Path**: `F:\jsb\pesteh\bbb`
- **Last Updated**: 2026-08-03

### Recent Active Work & Completed Tasks
- **Edit User Info & Avatar Upload**:
  - **Backend (`AdminUseCase.cs` & `SharedObject.cs`)**: Added `Status`, `Firstname`, `LastName`, `IsHekmatShobe`, `IsVIPMember`, `AvatarPath`, and `IsValid` status mapping to `AdminGetUserList` API response (`MiniUserJSON`).
  - **Backend (`LogicalController.cs`)**: Updated `/user/EditUserInfo` to accept `[FromForm]` with `IFormFile? AvatarFile` and save image files to `/wwwroot/uploads/avatars/`.
  - **Frontend (`AdminAPI.jsx`)**: Updated `EditUserInfo` helper function to transmit `FormData` supporting file uploads.
  - **Frontend Modal (`EditUserModal.jsx`)**: Replaced pitch-black backdrop with semi-transparent blurred overlay (`bg-black/40 backdrop-blur-sm`). Replaced URL input with **Drag & Drop image dropzone + File Picker**. Added status selector dropdown.
  - **Frontend User List (`UserList.jsx`)**: Added user status code badge to table rows alongside active/disabled status button.

---

## 🎯 Active Status & Completed Fixes
- [x] Fixed Edit User Modal backdrop, added Drag & Drop avatar file picker, and mapped `Status` field in `AdminGetUserList`
- [x] Edit User Info API endpoint, frontend API function, and `EditUserModal` component
- [x] Add role filter in `/adminlay/users` frontend (`UserList.jsx`)
- [x] Fix `GetTopTeachers` endpoint in `LogicalController.cs`
- [x] T-SQL queries for `MeetingTBL` cleanup (duration < 30m / null `EndDate` randomization)

---

## 📜 Commit History Summary
- `fcc2354` - docs: add conciseness guideline to PROGRESS.md
- `dede5f0` - docs: add PROGRESS.md handoff tracking file
- `0177225` - e2
