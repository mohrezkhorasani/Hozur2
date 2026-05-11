// Tools/AuthContext.jsx
import { createContext, useContext, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { VerifyToken } from '@/api/StartNewMeeting';
import Cookies from 'js-cookie';

const AuthContext = createContext();

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const navigate = useNavigate();

  const verifyAndSetUser = async () => {
    try {
      const verify = await VerifyToken();
      // console.log(verify)
      if (verify.status === 200) {
        setUser(verify.data);
        setIsAuthenticated(true);
      } else {
        Cookies.remove("vc2AuthNorm");
        setUser(null);
        setIsAuthenticated(false);
      }
    } catch (error) {
      setUser(null);
      setIsAuthenticated(false);
    } finally {
      setLoading(false);
    }
  };
  useEffect(() => {
    verifyAndSetUser();
  }, []);

  const logout = () => {
    Cookies.remove("vc2AuthNorm");
    setUser(null);
    setIsAuthenticated(false);
    navigate('/login');
  };

  return (
    <AuthContext.Provider value={{
      user,
      loading,
      isAuthenticated,
      logout,
      refetchUser: verifyAndSetUser
    }}>
      {children}
    </AuthContext.Provider>
  );
};