import React, { useState, useEffect } from 'react';

const JWTDecoder = () => {
  const [token, setToken] = useState('');
  const [decodedHeader, setDecodedHeader] = useState(null);
  const [decodedPayload, setDecodedPayload] = useState(null);
  const [signature, setSignature] = useState('');
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState('json');

  // دیکود خودکار با هر تغییر در توکن
  useEffect(() => {
    if (token.trim()) {
      decodeJWT(token);
    } else {
      setDecodedHeader(null);
      setDecodedPayload(null);
      setSignature('');
      setError('');
    }
  }, [token]);

  const decodeJWT = (token) => {
    try {
      const cleanToken = token.trim();
      
      if (!cleanToken) {
        setDecodedHeader(null);
        setDecodedPayload(null);
        setSignature('');
        setError('');
        return;
      }

      const parts = cleanToken.split('.');
      if (parts.length !== 3) {
        setError('فرمت توکن نامعتبر است. توکن JWT باید شامل 3 بخش باشد.');
        setDecodedHeader(null);
        setDecodedPayload(null);
        setSignature('');
        return;
      }

      // دیکود Header
      const headerBase64 = parts[0];
      let headerDecoded = decodeBase64Url(headerBase64);
      const headerJson = JSON.parse(headerDecoded);
      setDecodedHeader(headerJson);

      // دیکود Payload
      const payloadBase64 = parts[1];
      let payloadDecoded = decodeBase64Url(payloadBase64);
      const payloadJson = JSON.parse(payloadDecoded);
      setDecodedPayload(payloadJson);

      // ذخیره Signature
      setSignature(parts[2]);

      setError('');
    } catch (err) {
      setError('خطا در دیکد کردن توکن: ' + err.message);
      setDecodedHeader(null);
      setDecodedPayload(null);
      setSignature('');
    }
  };

  const decodeBase64Url = (base64Url) => {
    let base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    while (base64.length % 4) {
      base64 += '=';
    }
    return atob(base64);
  };

  const handleClear = () => {
    setToken('');
    setDecodedHeader(null);
    setDecodedPayload(null);
    setSignature('');
    setError('');
  };

  const copyToClipboard = (text, section) => {
    navigator.clipboard.writeText(text);
    alert(`${section} کپی شد!`);
  };

  // تابع برای نمایش رنگی توکن در textarea
  const renderColoredToken = () => {
    if (!token) return null;
    
    const parts = token.split('.');
    if (parts.length !== 3) return token;
    
    return (
      <div className="relative">
        <div 
          className="absolute inset-0 p-4 font-mono text-sm pointer-events-none overflow-hidden whitespace-pre-wrap break-all"
          style={{ fontFamily: 'monospace' }}
        >
          <span className="text-blue-400">{parts[0]}</span>
          <span className="text-gray-500">.</span>
          <span className="text-green-400">{parts[1]}</span>
          <span className="text-gray-500">.</span>
          <span className="text-purple-400">{parts[2]}</span>
        </div>
        <textarea
          className="w-full h-96 p-4 font-mono text-sm bg-gray-900 text-transparent border-2 border-gray-600 rounded-lg focus:outline-none focus:border-green-500 focus:ring-2 focus:ring-green-500/20 transition-all resize-none caret-white"
          placeholder="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
          value={token}
          onChange={(e) => setToken(e.target.value)}
          style={{ caretColor: 'white' }}
        />
      </div>
    );
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 to-gray-800 p-6">
      <div className="max-w-7xl mx-auto">
        {/* هدر */}
        <div className="text-center mb-8">
          <h1 className="text-5xl font-bold bg-gradient-to-r from-green-400 via-blue-500 to-purple-600 bg-clip-text text-transparent mb-3">
            🔐 JWT Token Decoder
          </h1>
          <p className="text-gray-300 text-lg">
            تجزیه و تحلیل کامل توکن JWT - هر سه بخش Header, Payload, Signature
          </p>
        </div>

        {/* دو ستونه */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          
          {/* ستون چپ - ورودی توکن با رنگ‌بندی */}
          <div className="bg-gray-800 rounded-xl shadow-2xl border border-gray-700 p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-bold text-gray-200 flex items-center gap-2">
                <span className="text-2xl">📝</span>
                ورودی توکن JWT
              </h2>
              <button
                onClick={handleClear}
                className="text-red-400 hover:text-red-300 text-sm flex items-center gap-1 transition-colors"
              >
                <span>🗑️</span>
                پاک کردن
              </button>
            </div>
            
            {/* اینپوت رنگی */}
            <div className="relative">
              <div 
                className="absolute inset-0 p-4 font-mono text-sm pointer-events-none overflow-auto whitespace-pre-wrap break-all"
                style={{ fontFamily: 'monospace', lineHeight: '1.5' }}
              >
                {token && token.split('.').length === 3 ? (
                  <>
                    <span className="text-blue-400">{token.split('.')[0]}</span>
                    <span className="text-gray-500">.</span>
                    <span className="text-green-400">{token.split('.')[1]}</span>
                    <span className="text-gray-500">.</span>
                    <span className="text-purple-400">{token.split('.')[2]}</span>
                  </>
                ) : (
                  <span className="text-gray-500">{token || ' '}</span>
                )}
              </div>
              <textarea
                className="w-full h-96 p-4 font-mono text-sm bg-transparent text-transparent border-2 border-gray-600 rounded-lg focus:outline-none focus:border-green-500 focus:ring-2 focus:ring-green-500/20 transition-all resize-none"
                placeholder="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
                value={token}
                onChange={(e) => setToken(e.target.value)}
                style={{ caretColor: 'white' }}
              />
            </div>

            {/* راهنمای رنگ‌ها */}
            <div className="mt-4 flex justify-center gap-6 text-xs">
              <div className="flex items-center gap-1">
                <span className="w-3 h-3 bg-blue-500 rounded-full"></span>
                <span className="text-gray-400">Header</span>
              </div>
              <div className="flex items-center gap-1">
                <span className="w-3 h-3 bg-green-500 rounded-full"></span>
                <span className="text-gray-400">Payload</span>
              </div>
              <div className="flex items-center gap-1">
                <span className="w-3 h-3 bg-purple-500 rounded-full"></span>
                <span className="text-gray-400">Signature</span>
              </div>
              <div className="flex items-center gap-1">
                <span className="text-gray-500 text-lg">.</span>
                <span className="text-gray-400">Separator</span>
              </div>
            </div>

            {error && (
              <div className="mt-4 p-3 bg-red-900/50 border border-red-700 rounded-lg flex items-start gap-2">
                <span className="text-red-400 text-lg">⚠️</span>
                <span className="text-red-300 text-sm">{error}</span>
              </div>
            )}

            {!token && !error && (
              <div className="mt-4 p-3 bg-blue-900/50 border border-blue-700 rounded-lg">
                <p className="text-blue-300 text-sm flex items-center gap-2">
                  <span>💡</span>
                  توکن JWT خود را در کادر بالا جایگذاری کنید - دیکد کردن به صورت خودکار انجام می‌شود
                </p>
              </div>
            )}
          </div>

          {/* ستون راست - نمایش اطلاعات */}
          <div className="bg-gray-800 rounded-xl shadow-2xl border border-gray-700 overflow-hidden">
            {/* تب‌ها */}
            <div className="flex border-b border-gray-700 bg-gray-900">
              <button
                onClick={() => setActiveTab('json')}
                className={`flex-1 px-6 py-3 text-center font-semibold transition-all ${
                  activeTab === 'json'
                    ? 'bg-gray-800 text-green-400 border-b-2 border-green-500'
                    : 'text-gray-400 hover:text-gray-200 hover:bg-gray-800/50'
                }`}
              >
                {`{ }`} نمایش JSON (پیش‌فرض)
              </button>
              <button
                onClick={() => setActiveTab('table')}
                className={`flex-1 px-6 py-3 text-center font-semibold transition-all ${
                  activeTab === 'table'
                    ? 'bg-gray-800 text-green-400 border-b-2 border-green-500'
                    : 'text-gray-400 hover:text-gray-200 hover:bg-gray-800/50'
                }`}
              >
                📊 نمایش جدول
              </button>
            </div>

            {/* محتوای تب‌ها */}
            <div className="p-6 min-h-[500px] max-h-[600px] overflow-auto">
              {!decodedHeader && !decodedPayload && !error && (
                <div className="flex flex-col items-center justify-center h-96 text-center">
                  <div className="text-6xl mb-4">🔍</div>
                  <h3 className="text-xl font-semibold text-gray-300 mb-2">هنوز توکنی وارد نشده</h3>
                  <p className="text-gray-400">توکن JWT خود را در سمت چپ وارد کنید</p>
                  <p className="text-gray-500 text-sm mt-2">هر سه بخش توکن به صورت خودکار نمایش داده می‌شود</p>
                </div>
              )}

              {error && (
                <div className="flex flex-col items-center justify-center h-96 text-center">
                  <div className="text-6xl mb-4">❌</div>
                  <h3 className="text-xl font-semibold text-red-400 mb-2">خطا در دیکد کردن</h3>
                  <p className="text-gray-400">{error}</p>
                </div>
              )}

              {decodedHeader && decodedPayload && activeTab === 'json' && (
                <div className="space-y-4">
                  {/* بخش Header - آبی */}
                  <div className="bg-blue-900/20 border border-blue-500/30 rounded-lg overflow-hidden">
                    <div className="bg-blue-900/50 px-4 py-2 flex justify-between items-center">
                      <div className="flex items-center gap-2">
                        <span className="text-blue-400 font-mono text-sm font-bold">HEADER</span>
                        <span className="text-blue-300 text-xs">(الگوریتم و نوع توکن)</span>
                      </div>
                      <button
                        onClick={() => copyToClipboard(JSON.stringify(decodedHeader, null, 2), 'Header')}
                        className="text-blue-400 hover:text-blue-300 text-xs transition-colors"
                      >
                        📋 کپی
                      </button>
                    </div>
                    <pre className="p-4 text-blue-300 font-mono text-sm overflow-x-auto">
                      {JSON.stringify(decodedHeader, null, 2)}
                    </pre>
                  </div>

                  {/* بخش Payload - سبز */}
                  <div className="bg-green-900/20 border border-green-500/30 rounded-lg overflow-hidden">
                    <div className="bg-green-900/50 px-4 py-2 flex justify-between items-center">
                      <div className="flex items-center gap-2">
                        <span className="text-green-400 font-mono text-sm font-bold">PAYLOAD</span>
                        <span className="text-green-300 text-xs">(داده‌های اصلی توکن)</span>
                      </div>
                      <button
                        onClick={() => copyToClipboard(JSON.stringify(decodedPayload, null, 2), 'Payload')}
                        className="text-green-400 hover:text-green-300 text-xs transition-colors"
                      >
                        📋 کپی
                      </button>
                    </div>
                    <pre className="p-4 text-green-300 font-mono text-sm overflow-x-auto">
                      {JSON.stringify(decodedPayload, null, 2)}
                    </pre>
                  </div>

                  {/* بخش Signature - بنفش */}
                  <div className="bg-purple-900/20 border border-purple-500/30 rounded-lg overflow-hidden">
                    <div className="bg-purple-900/50 px-4 py-2 flex justify-between items-center">
                      <div className="flex items-center gap-2">
                        <span className="text-purple-400 font-mono text-sm font-bold">SIGNATURE</span>
                        <span className="text-purple-300 text-xs">(امضای دیجیتال)</span>
                      </div>
                      <button
                        onClick={() => copyToClipboard(signature, 'Signature')}
                        className="text-purple-400 hover:text-purple-300 text-xs transition-colors"
                      >
                        📋 کپی
                      </button>
                    </div>
                    <div className="p-4">
                      <code className="text-purple-300 font-mono text-xs break-all bg-purple-950/30 p-2 rounded block">
                        {signature}
                      </code>
                      <p className="text-purple-400/70 text-xs mt-2">
                        ⚠️ Signature معمولاً به صورت Base64Url encoded است و قابل دیکد کردن به متن ساده نمی‌باشد
                      </p>
                    </div>
                  </div>
                </div>
              )}

              {decodedHeader && decodedPayload && activeTab === 'table' && (
                <div className="space-y-6">
                  {/* جدول Header */}
                  <div>
                    <h3 className="text-blue-400 font-mono font-bold mb-3 flex items-center gap-2">
                      <span className="w-3 h-3 bg-blue-500 rounded-full"></span>
                      HEADER
                    </h3>
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm border-collapse">
                        <tbody>
                          {Object.entries(decodedHeader).map(([key, value], index) => (
                            <tr key={index} className="border-b border-blue-500/20">
                              <td className="p-2 font-mono font-bold text-blue-300">{key}</td>
                              <td className="p-2 font-mono text-blue-200">{String(value)}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>

                  {/* جدول Payload */}
                  <div>
                    <h3 className="text-green-400 font-mono font-bold mb-3 flex items-center gap-2">
                      <span className="w-3 h-3 bg-green-500 rounded-full"></span>
                      PAYLOAD
                    </h3>
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm border-collapse">
                        <tbody>
                          {Object.entries(decodedPayload).map(([key, value], index) => (
                            <tr key={index} className="border-b border-green-500/20">
                              <td className="p-2 font-mono font-bold text-green-300 align-top">{key}</td>
                              <td className="p-2 font-mono text-green-200 break-all">
                                {typeof value === 'object' ? (
                                  <pre className="text-xs mt-1">{JSON.stringify(value, null, 2)}</pre>
                                ) : (
                                  String(value)
                                )}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>

                  {/* نمایش Signature */}
                  <div>
                    <h3 className="text-purple-400 font-mono font-bold mb-3 flex items-center gap-2">
                      <span className="w-3 h-3 bg-purple-500 rounded-full"></span>
                      SIGNATURE
                    </h3>
                    <div className="bg-purple-950/30 p-3 rounded-lg">
                      <code className="text-purple-300 font-mono text-xs break-all">
                        {signature}
                      </code>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* توضیحات پایین */}
        <div className="mt-6 text-center text-gray-400 text-sm">
          <span className="inline-flex items-center gap-3">
            <span className="flex items-center gap-1"><span className="w-2 h-2 bg-blue-500 rounded-full"></span> Header</span>
            <span className="flex items-center gap-1"><span className="w-2 h-2 bg-green-500 rounded-full"></span> Payload</span>
            <span className="flex items-center gap-1"><span className="w-2 h-2 bg-purple-500 rounded-full"></span> Signature</span>
            <span className="w-2 h-2 bg-green-500 rounded-full animate-pulse ml-2"></span>
            دیکد کردن خودکار با هر تغییر
          </span>
        </div>
      </div>
    </div>
  );
};

export default JWTDecoder;