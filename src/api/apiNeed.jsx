import Cookies from 'js-cookie';

// export const BaseURL = "https://core.hekmateislami.com/"
// export const BaseURL = "https://192.168.0.122:7087/"
export const BaseURL = "https://vc9.hekmateislami.com/"; // this is localhost for DEBUG
export const BaseURLFiles = "https://vcfile.hekmateislami.com/";
export const BaseURLSite = "https://vc2.hekmateislami.com/";

/**
 * Returns cookie domain if running under .hekmateislami.com, otherwise undefined (for localhost/IP).
 */
export const getCookieDomain = () => {
  if (typeof window !== 'undefined' && window.location && window.location.hostname) {
    const hostname = window.location.hostname;
    if (hostname.endsWith('hekmateislami.com')) {
      return '.hekmateislami.com';
    }
  }
  return undefined;
};

/**
 * Returns cookie options matching current environment (domain, path, secure, sameSite, expires).
 */
export const getCookieOptions = (days = 365 * 10) => {
  const domain = getCookieDomain();
  const isHttps = typeof window !== 'undefined' && window.location && window.location.protocol === 'https:';
  const options = {
    path: '/',
    expires: days
  };
  if (domain) {
    options.domain = domain;
  }
  if (isHttps) {
    options.secure = true;
    options.sameSite = 'None';
  } else {
    options.sameSite = 'Lax';
  }
  return options;
};

/**
 * Sets a cookie with domain/secure/sameSite attributes.
 */
export const setCookieWithConfig = (name, value, days = 365 * 10) => {
  const options = getCookieOptions(days);
  Cookies.set(name, value, options);
};

/**
 * Removes a cookie across all possible domain permutations (both with and without domain).
 */
export const removeCookie = (name) => {
  const domain = getCookieDomain();
  Cookies.remove(name, { path: '/' });
  Cookies.remove(name);
  if (domain) {
    Cookies.remove(name, { path: '/', domain });
    Cookies.remove(name, { domain });
  }
};

/**
 * Safely removes registered user token (bbbUserToken) only.
 * Never touches bbbUserToken_random!
 */
export const removeRegisteredUserToken = () => {
  removeCookie('bbbUserToken');
};

/**
 * Safely removes auth JWT token (vc2AuthNorm).
 */
export const removeAuthToken = () => {
  removeCookie('vc2AuthNorm');
};

/**
 * Generates a random user token for visitors.
 */
export const generateRandomToken = () => {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) {
    return `userVc2-Rand_${crypto.randomUUID()}`;
  }
  return `userVc2-Rand_${Date.now()}_${Math.random().toString(36).substring(2, 11)}`;
};

/**
 * Retrieves existing bbbUserToken_random or creates it on the first visit.
 * This cookie is set with a 10-year expiration and must NEVER be deleted on logout.
 */
export const getOrCreateRandomUserToken = () => {
  let randToken = Cookies.get('bbbUserToken_random');
  if (!randToken || randToken === 'null' || randToken === 'undefined') {
    // Check if legacy code stored a random token in bbbUserToken; migrate it so visitor keeps their identity
    const legacyToken = Cookies.get('bbbUserToken');
    if (legacyToken && legacyToken.startsWith('userVc2-Rand_')) {
      randToken = legacyToken;
    } else {
      randToken = generateRandomToken();
    }
    setCookieWithConfig('bbbUserToken_random', randToken, 365 * 10);
  }
  return randToken;
};

/**
 * Decodes JWT payload without external libraries.
 */
export const parseJwt = (token) => {
  if (!token || typeof token !== 'string') return null;
  const parts = token.split('.');
  if (parts.length !== 3) return null;
  try {
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    try {
      return JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
    } catch {
      return null;
    }
  }
};

/**
 * Checks JWT from parameter or vc2AuthNorm cookie to extract registered user info.
 * Returns { userId, payload } if valid & registered, or null if unregistered/expired.
 */
export const getRegisteredUserFromJwt = (jwtToken) => {
  const token = jwtToken || Cookies.get('vc2AuthNorm');
  if (!token) return null;

  const payload = parseJwt(token);
  if (!payload) return null;

  // Check expiration
  if (payload.exp && payload.exp * 1000 < Date.now()) {
    return null; // Expired
  }

  // Check if token explicitly marks user as unregistered guest
  if (payload['um-registered'] === 'False' || payload['um-registered'] === false) {
    return null;
  }

  // Extract user ID (UserID, uuid, uid, UserId, sub, id)
  const userId = payload.UserID || payload.uuid || payload.uid || payload.UserId || payload.sub || payload.id;
  if (!userId) return null;

  return {
    userId: String(userId),
    payload
  };
};

/**
 * Synchronizes user tokens:
 * - If user is registered: sets bbbUserToken cookie to their registered ID.
 * - If user is NOT registered: clears bbbUserToken cookie (keeps bbbUserToken_random).
 * - Always ensures bbbUserToken_random exists.
 */
export const syncUserTokens = (verifiedUser = null) => {
  // 1. Ensure permanent random visitor token exists
  const randToken = getOrCreateRandomUserToken();

  // 2. Determine if registered
  let registeredUserId = null;
  if (verifiedUser && (verifiedUser.uid || verifiedUser.id)) {
    registeredUserId = verifiedUser.uid ? String(verifiedUser.uid) : `userVc2-ID_${verifiedUser.id}`;
  } else {
    const jwtInfo = getRegisteredUserFromJwt();
    if (jwtInfo && jwtInfo.userId) {
      registeredUserId = jwtInfo.userId;
    }
  }

  // 3. Set or remove bbbUserToken based on registration
  if (registeredUserId) {
    setCookieWithConfig('bbbUserToken', registeredUserId, 365 * 10);
    return { isRegistered: true, userToken: registeredUserId, randomToken: randToken };
  } else {
    // Unregistered: remove bbbUserToken, keep bbbUserToken_random intact
    removeRegisteredUserToken();
    return { isRegistered: false, userToken: null, randomToken: randToken };
  }
};

/**
 * Returns bbbUserToken if present, else returns bbbUserToken_random.
 */
export const getEffectiveUserToken = () => {
  const userToken = Cookies.get('bbbUserToken');
  if (userToken && userToken !== 'null' && userToken !== 'undefined') {
    return userToken;
  }
  return getOrCreateRandomUserToken();
};

/**
 * HTTP headers for API calls.
 * Uses dynamic getters so:
 * - Direct property access: NormalHeaderAPI.bbbusertoken2
 * - Object spread: { ...NormalHeaderAPI }
 * - Direct fetch header: fetch(url, { headers: NormalHeaderAPI })
 * always retrieve current dynamic values:
 * - vc2AuthNorm from Cookies.get("vc2AuthNorm")
 * - bbbusertoken2 from bbbUserToken (if registered) or bbbUserToken_random (if guest)
 */
export const NormalHeaderAPI = {
  "accept": "*/*",
  get "vc2AuthNorm"() {
    return Cookies.get("vc2AuthNorm") || "";
  },
  set "vc2AuthNorm"(val) {
    // Allow assignment without throwing TypeError
  },
  get "bbbusertoken2"() {
    return getEffectiveUserToken();
  },
  set "bbbusertoken2"(val) {
    // Allow assignment without throwing TypeError
  }
};

/**
 * Helper returning fresh plain object with current headers.
 */
export const getNormalHeaders = () => ({
  "accept": "*/*",
  "vc2AuthNorm": Cookies.get("vc2AuthNorm") || "",
  "bbbusertoken2": getEffectiveUserToken()
});

// Auto-initialize random token and verify registration state in browser
if (typeof window !== 'undefined') {
  try {
    getOrCreateRandomUserToken();
    // If no vc2AuthNorm cookie, make sure bbbUserToken is not lingering from old random generation
    if (!Cookies.get("vc2AuthNorm")) {
      const legacyToken = Cookies.get("bbbUserToken");
      if (legacyToken && legacyToken.startsWith("userVc2-Rand_")) {
        removeRegisteredUserToken();
      }
    }
  } catch (e) {
    // Ignore during SSR or initial execution
  }
}