// src/config.js
const envApiUrl = typeof import.meta !== 'undefined' ? import.meta?.env?.VITE_API_BASE_URL : undefined;
const envSiteUrl = typeof import.meta !== 'undefined' ? import.meta?.env?.VITE_BASE_URL_SITE : undefined;

const config = {
  development: {
    API_BASE_URL: envApiUrl || "https://vc9.hekmateislami.com",
    REACT_BASE_URL: envSiteUrl || "http://localhost:3000",
  },
  production: {
    API_BASE_URL: envApiUrl || "https://core.hekmateislami.com",
    REACT_BASE_URL: envSiteUrl || "https://vc2.hekmateislami.com",
  },
};

// Safe way to get environment
const getEnvironment = () => {
  try {
    // Check if process and process.env exist
    if (typeof process !== 'undefined' && process?.env?.NODE_ENV) {
      return process.env.NODE_ENV;
    }
    // Check for import.meta.env (Vite)
    if (typeof import.meta !== 'undefined' && import.meta?.env?.MODE) {
      return import.meta.env.MODE;
    }
    // Default to development
    return "development";
  } catch (error) {
    console.warn("Error detecting environment, defaulting to development:", error);
    return "development";
  }
};

const environment = getEnvironment();
const currentConfig = config[environment] || config.development;

export default currentConfig;
