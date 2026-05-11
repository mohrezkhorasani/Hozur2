// Tools/ProtectedRoute.jsx
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '@/Tools/AuthProvider';

const ProtectedRoute = ({ requiredPermissions = [] }) => {
  const { user, loading, isAuthenticated } = useAuth();

  if (loading) {
    return <div>Loading...</div>; // Or your loading component
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Check for required permissions
  if (requiredPermissions.length > 0) {
    const hasPermissions = requiredPermissions.every(permission =>
      user?.permissions?.includes(permission)
    );
    
    if (!hasPermissions) {
      return <Navigate to="/rooms" replace />;
    }
  }

  return <Outlet />;
};

export default ProtectedRoute;