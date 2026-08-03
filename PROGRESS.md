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
- **Edit User Info Feature**:
  - **Backend (`LogicalController.cs`)**: Added `[HttpPost("/user/EditUserInfo")]` API endpoint and `EditUserInfoVM` model to update `Firstname`, `LastName`, `IsHekmatShobe`, `IsMemberVIP`, `Status`, and `AvatarPath`.
  - **Frontend API (`AdminAPI.jsx`)**: Added `EditUserInfo` fetch function.
  - **Frontend Modal (`EditUserModal.jsx`)**: Created modal component for editing user details.
  - **Frontend Page (`UserList.jsx`)**: Integrated `EditUserModal` with edit button icon in table actions column.
- **Frontend Role Filter (`UserList.jsx`)**: Added role filter dropdown UI control and filtering logic to `/adminlay/users`.
- **Backend `GetTopTeachers` (`LogicalController.cs`)**: Refactored `GetTopTeachers` endpoint to exclude `MEET_IGNORE` meetings, fix missing `RoomFriendlyID` filter, and deduplicate merged teacher counts.
- **SQL Scripts**: Created T-SQL queries for updating short/null meetings to `MEET_IGNORE` (-99) and randomizing `EndDate` for meetings with >2 participants.

---

## 🎯 Active Status & Completed Fixes
- [x] Edit User Info API endpoint, frontend API function, and `EditUserModal` component
- [x] Add role filter in `/adminlay/users` frontend (`UserList.jsx`)
- [x] Fix `GetTopTeachers` endpoint in `LogicalController.cs`
- [x] T-SQL queries for `MeetingTBL` cleanup (duration < 30m / null `EndDate` randomization)

---

## 📜 Commit History Summary
- `fcc2354` - docs: add conciseness guideline to PROGRESS.md
- `dede5f0` - docs: add PROGRESS.md handoff tracking file
- `0177225` - e2
