// Tools/AuthContext.jsx
import { createContext, useContext, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { VerifyToken } from '@/api/StartNewMeeting';
import Cookies from 'js-cookie';
import {
  getOrCreateRandomUserToken,
  syncUserTokens,
  removeRegisteredUserToken,
  removeAuthToken,
  getRegisteredUserFromJwt
} from '@/api/apiNeed';

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
  const [VerifyTokenChecked, setVerifyTokenChecked] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const navigate = useNavigate();

  const verifyAndSetUser = async () => {
    try {
      const authCookie = Cookies.get("vc2AuthNorm");
      if (!authCookie) {
        removeRegisteredUserToken();
        getOrCreateRandomUserToken();
        setUser(null);
        setIsAuthenticated(false);
        return;
      }

      const verify = await VerifyToken();
      if (verify.status === 200 && verify.data) {
        setUser(verify.data);
        setIsAuthenticated(true);
        syncUserTokens(verify.data);
      } else {
        removeAuthToken();
        removeRegisteredUserToken();
        getOrCreateRandomUserToken();
        setUser(null);
        setIsAuthenticated(false);
      }
    } catch (error) {
      removeAuthToken();
      removeRegisteredUserToken();
      getOrCreateRandomUserToken();
      setUser(null);
      setIsAuthenticated(false);
    } finally {
      setLoading(false);
      setVerifyTokenChecked(true);
    }
  };

  useEffect(() => {
    // Check JWT claim synchronously on mount
    const jwtInfo = getRegisteredUserFromJwt();
    if (jwtInfo && jwtInfo.userId) {
      syncUserTokens();
    } else if (!Cookies.get("vc2AuthNorm")) {
      removeRegisteredUserToken();
      getOrCreateRandomUserToken();
    }
    verifyAndSetUser();
  }, []);

  useEffect(() => {
    if (!VerifyTokenChecked) return;

    if (user && (user.uid || user.id)) {
      // User is registered
      syncUserTokens(user);
    } else {
      // User is unregistered: remove bbbUserToken, keep bbbUserToken_random
      removeRegisteredUserToken();
      getOrCreateRandomUserToken();
    }
  }, [user, VerifyTokenChecked]);

  const logout = () => {
    removeAuthToken();
    removeRegisteredUserToken();
    // Note: bbbUserToken_random is kept and never deleted!
    getOrCreateRandomUserToken();
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