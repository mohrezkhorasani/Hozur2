import { useState } from 'react'
import './App.css'
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import HomePage from './Pages/Home';
import Product from './Pages/JoinForm';
import DashboardPage from './Pages/Dashboard';
import BaseLayout from "./Layout/BaseLayout"
import AccountLayout from './Layout/AccountLayout';
import ProfileOrders from './Pages/Profile/ProfileOrders';
import JoinForm from './Pages/JoinForm';
import RoomSubRequest from './Pages/RoomSubRequest';
import HomeNormalUser from './Pages/HomeNormalUser';
import LoginForm from './Pages/Profile/login';
import HomeGuest from './Pages/HomeGuest';
import SignUpForm from './Pages/Profile/Signup';
import ProtectedRoute from './Tools/AuthWrapper';
import { AuthProvider } from './Tools/AuthProvider';
import ContactPage from './Pages/Contactus';
import NotFoundPage from './Pages/Error/404';
import ProfilePage from './Pages/RoomPerUser';
import AdminPanel from './Pages/Admin';
import UsersList from './Pages/Admin/UserList';
import MeetingEnded from './Pages/MeetingEnded';
import PollManagement from './Pages/PollManagement';
import AdminDashboard from './Pages/AdminDashboard';
import AdminLayout from './Pages/AdminLayout';
import SlideshowList from './Pages/Admin/SlideList';
import RoomsManagement from './Pages/Adminlay/RoomManagement';
import WebhooksManagement from './Pages/Adminlay/HookManagement';
import MeetingManagement from './Pages/Adminlay/MeetingManagement';
import ParticipantManagement from './Pages/Adminlay/ParticipantManagement';
import PollReportPage from './Pages/Profile/SeeMeetingPollReport';
import FileManagerPage from './Pages/Dashboard/FTPManagement';
import PaymentForm from './Pages/tools/PaymentForm';
import PaymentManagement from './Pages/tools/PaymentManagement';
import UserMeetingLogsList from './Pages/Profile/UserMeetingLogsList';
import RoomDetail from './Pages/RoomDetail';
import CacheManagement from './Pages/Adminlay/CacheManagement';
import { ToastProvider } from './Tools/ToastProvider';

function App() {
  const [count, setCount] = useState(0)

  return (
    <Router>

      <AuthProvider>
        <ToastProvider>

          <Routes>
            {/* صفحات عمومی */}
            <Route element={<BaseLayout />}>

              <Route element={<LoginForm />} path='/login' />
              <Route element={<SignUpForm />} path='/signup' />
              <Route path="/rooms/:Mid/join" element={<JoinForm />} />
              <Route path="/rooms/:Mid/sub/request" element={<RoomSubRequest />} />
              <Route path="/rooms/:roomId" element={<RoomDetail />} />
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/" element={<HomePage />} />
              <Route path="/rooms" element={<HomeGuest />} />
              <Route path="/norm" element={<HomeNormalUser />} />
              <Route path="/contact-form" element={<ContactPage />} />
              <Route path="/a/:Uid" element={<ProfilePage />} />
              <Route path="/a" element={<HomePage />} />
              <Route path="/meeting/ended" element={<MeetingEnded />} />

            </Route>
            <Route path="/bale">
              <Route element={<LoginForm />} path='/bale/login' />
              <Route element={<SignUpForm />} path='/bale/signup' />
              <Route path="/bale/rooms/:Mid/join" element={<JoinForm />} />
              <Route path="/bale/rooms/:Mid/sub/request" element={<RoomSubRequest />} />
              <Route path="/bale/rooms/:roomId" element={<RoomDetail />} />
              <Route path="/bale/dashboard" element={<DashboardPage />} />
              <Route path="/bale/" element={<HomePage />} />
              <Route path="/bale/rooms" element={<HomeGuest />} />
              <Route path="/bale/norm" element={<HomeNormalUser />} />
              <Route path="/bale/contact-form" element={<ContactPage />} />
              <Route path="/bale/a/:Username" element={<ProfilePage />} />

            </Route>
            {/* صفحات نیازمند احراز هویت */}
            <Route element={<ProtectedRoute />}>
              <Route path="/me" element={<AccountLayout />}>
                <Route path="my-account" element={<DashboardPage />} />
                <Route path="orders" element={<ProfileOrders />} />
              </Route>
            </Route>
            {/* Protected routes with AccountLayout and specific permissions */}
            <Route element={<ProtectedRoute requiredPermissions={['create_meeting', 'edit_meeting', 'delete_meeting']} />}>
              <Route element={<BaseLayout />}>
                <Route path="/" element={<HomePage />} />
                <Route path="/usermeet/logs/:umid" element={<UserMeetingLogsList />} />

              </Route>
            </Route>
            {/* Protected routes with AccountLayout and specific permissions */}
            <Route element={<ProtectedRoute requiredPermissions={['manage_admins']} />}>
              <Route element={<BaseLayout />}>
                <Route path="/adminlay" element={<AdminLayout />}>
                  <Route path="cache" element={<CacheManagement />} />
                </Route>
                <Route path="/admin" element={<AdminLayout />}>
                  <Route path="cache" element={<CacheManagement />} />
                </Route>
              </Route>
            </Route>
            <Route element={<ProtectedRoute requiredPermissions={['ftp']} />}>
              <Route path="/dashboard" element={<BaseLayout />}>
                <Route path="ftp-manage" element={<FileManagerPage />} />
              </Route>
            </Route>
            <Route element={<ProtectedRoute requiredPermissions={['manage_users']} />}>
              <Route element={<BaseLayout />}>
                <Route path="/admin" element={<AdminPanel />}>
                  <Route path="users" element={<UsersList />} />
                </Route>
              </Route>
            </Route>
            <Route element={<ProtectedRoute requiredPermissions={['view_poll_reports']} />}>
              <Route element={<BaseLayout />}>
                {/* <Route path="/admin" element={<AdminPanel />}> */}
                <Route path="polls/:meetid/report" element={<PollReportPage />} />
                {/* </Route> */}
              </Route>
            </Route>
            <Route element={<ProtectedRoute requiredPermissions={['pay_request']} />} >
              <Route element={<PaymentManagement />} path='/tools-off/paymentmgmt/list' />
            </Route>
            {/* دسترسی به مدیریت جلسات برای همه کاربران وارد شده دارای نقش/اتاق */}
            <Route element={<ProtectedRoute />}>
              <Route element={<BaseLayout />}>
                <Route path="/admin" element={<AdminLayout />}>
                  <Route path="meetings" element={<MeetingManagement />} />
                  <Route path="sessions" element={<MeetingManagement />} />
                  <Route path="participants" element={<ParticipantManagement />} />
                </Route>
                <Route path="/adminlay" element={<AdminLayout />}>
                  <Route path="meetings" element={<MeetingManagement />} />
                  <Route path="sessions" element={<MeetingManagement />} />
                  <Route path="participants" element={<ParticipantManagement />} />
                </Route>
              </Route>
            </Route>

            <Route element={<ProtectedRoute requiredPermissions={['edit_poll']} />}>
              <Route element={<BaseLayout />}>
                <Route path="/adminlay" element={<AdminLayout />}>
                  <Route index element={null} /> {/* صفحه اصلی داشبورد */}
                  <Route path="polls" element={<PollManagement />} />
                  <Route path="users" element={<UsersList />} />
                  <Route path="slideshow" element={<SlideshowList />} />
                  <Route path="rooms" element={<RoomsManagement />} />
                  <Route path="webhooks" element={<WebhooksManagement />} />
                </Route>
                <Route path="/admin" element={<AdminLayout />}>
                  <Route path="slideshow" element={<SlideshowList />} />
                  <Route path="rooms" element={<RoomsManagement />} />
                  <Route path="polls" element={<PollManagement />} />
                  <Route path="users" element={<UsersList />} />
                  <Route path="webhooks" element={<WebhooksManagement />} />
                </Route>
              </Route>
            </Route>
            <Route element={<BaseLayout />}>
              <Route path="/admin/a2a2" element={<AdminDashboard />} />
            </Route>

            {/* صفحه لاگین (مثال) */}
            {/* <Route path="/login" element={<LoginPage />} /> */}



            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </ToastProvider>
      </AuthProvider>
    </Router>

  )
}

export default App


// import { useState } from 'react'
// import './App.css'
// import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
// import { AuthProvider } from './Tools/AuthProvider';
// import JWTDecoder from './Pages/Admin/JWTDecoder';

// function App() {
//   const [count, setCount] = useState(0)

//   return (
//     <Router>

//       <AuthProvider>
//         <Routes>
//           <Route element={<JWTDecoder />} path='/' />

//         </Routes>
//       </AuthProvider>
//     </Router>

//   )
// }

// export default App
