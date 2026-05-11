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

function App() {
  const [count, setCount] = useState(0)

  return (
    <Router>

      <AuthProvider>
        <Routes>

          {/* صفحات عمومی */}
          <Route element={<BaseLayout />}>

            <Route element={<LoginForm />} path='/login' />
            <Route element={<SignUpForm />} path='/signup' />
            <Route path="/rooms/:Mid/join" element={<JoinForm />} />
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
            </Route>
          </Route>
          {/* Protected routes with AccountLayout and specific permissions */}
          <Route element={<ProtectedRoute requiredPermissions={['manage_admins']} />}>
            {/* <Route path="/me" element={<AccountLayout />}>
                <Route path="my-account" element={<DashboardPage />} />
                <Route path="orders" element={<ProfileOrders />} />
              </Route> */}
          </Route>
          <Route element={<ProtectedRoute requiredPermissions={['manage_users']} />}>
            <Route element={<BaseLayout />}>
              <Route path="/admin" element={<AdminPanel />}>
                <Route path="users" element={<UsersList />} />
              </Route>
            </Route>
          </Route>
          {/* صفحه لاگین (مثال) */}
          {/* <Route path="/login" element={<LoginPage />} /> */}



          <Route path="*" element={<NotFoundPage />} />
        </Routes>
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
