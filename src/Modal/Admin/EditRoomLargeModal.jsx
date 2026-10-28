import React, { useState, useEffect } from 'react';
import { GetRoomFullDetails, EditRoomFull, GetRoomWaitingMembers, ChangeRoomMemberStatus } from '../../api/AdminAPI';
import { UploadFiles } from '../../api/FTP';
import { FaEdit, FaImage, FaSlidersH, FaUsers, FaLock, FaSave, FaTimes, FaUserPlus, FaTrash, FaInfoCircle, FaUpload, FaSpinner } from 'react-icons/fa';

const EditRoomLargeModal = ({ isOpen, onClose, roomId, onUpdated }) => {
  const [activeTab, setActiveTab] = useState('basic');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [uploadingField, setUploadingField] = useState(null);
  const [roomData, setRoomData] = useState(null);
  
  const [formData, setFormData] = useState({
    title: '',
    explain: '',
    bio: '',
    ttrUrl: '',
    avatar: '',
    logo: '',
    presentationPath: '',
    coverImage: '',
    requiredAdminAcceptToView: false,
    requiredLoginToView: false,
    recordRoom: true,
    isPublicRoom: false,
    isSuperPrivateRoom: false,
    isPrivateRoom: false,
    isSubscribed: false,
    isShowMemberVIPBeforeEndTheMeeting: false,
    price: 0,
    oldprice: 0,
    support: '',
    isRegistrationEnabled: false,
  });

  const [addAdminInput, setAddAdminInput] = useState('');
  const [addAdminList, setAddAdminList] = useState([]);
  const [removeAdminList, setRemoveAdminList] = useState([]);

  const [waitingMembers, setWaitingMembers] = useState([]);
  const [waitingSearch, setWaitingSearch] = useState('');
  const [loadingWaiting, setLoadingWaiting] = useState(false);

  useEffect(() => {
    if (activeTab === 'waitingList' && roomId) {
      fetchWaitingMembers();
    }
  }, [activeTab, roomId]);

  const fetchWaitingMembers = async () => {
    setLoadingWaiting(true);
    try {
      const res = await GetRoomWaitingMembers(roomId);
      if (res && res.status === 200) {
        setWaitingMembers(res.members || []);
      } else {
        setWaitingMembers([]);
      }
    } catch (err) {
      console.error(err);
      setWaitingMembers([]);
    } finally {
      setLoadingWaiting(false);
    }
  };

  const handleChangeMemberStatus = async (userId, status) => {
    try {
      const res = await ChangeRoomMemberStatus(roomId, userId, status);
      if (res && res.status === 200) {
        setWaitingMembers(prev => prev.filter(m => m.userId !== userId));
        // alert(status === 100 ? 'کاربر تایید شد' : 'کاربر رد شد');
      } else {
        alert('خطا در تغییر وضعیت');
      }
    } catch (err) {
      console.error(err);
      alert('خطا در تغییر وضعیت');
    }
  };

  const handleFileUpload = async (e, targetField) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploadingField(targetField);
    try {
      const res = await UploadFiles([file]);
      // Determine response format (urls array or data array/object)
      let fileUrl = '';
      if (res && res.urls && res.urls.length > 0) {
        fileUrl = res.urls[0];
      } else if (Array.isArray(res) && res.length > 0) {
        fileUrl = typeof res[0] === 'string' ? res[0] : res[0].url || res[0].path;
      } else if (res && res.url) {
        fileUrl = res.url;
      } else if (res && res.data) {
        fileUrl = Array.isArray(res.data) ? res.data[0] : res.data;
      }

      if (fileUrl) {
        handleInputChange(targetField, fileUrl);
      } else {
        alert('فایل آپلود شد اما آدرس آن یافت نشد.');
      }
    } catch (err) {
      console.error('File upload error:', err);
      alert('خطا در آپلود فایل');
    } finally {
      setUploadingField(null);
    }
  };

  useEffect(() => {
    if (isOpen && roomId) {
      fetchDetails();
    }
  }, [isOpen, roomId]);

  const fetchDetails = async () => {
    setLoading(true);
    try {
      const res = await GetRoomFullDetails(roomId);
      if (res.status === 200 && res) {
        setRoomData(res);
        setFormData({
          title: res.title || '',
          explain: res.explain || '',
          bio: res.bio || '',
          ttrUrl: res.ttR_URL || res.ttr_URL || res.ttrUrl || '',
          avatar: res.avatar || '',
          logo: res.logo || '',
          presentationPath: res.presentationPath || '',
          coverImage: res.coverImage || '',
          requiredAdminAcceptToView: !!res.requiredAdminAcceptToView,
          requiredLoginToView: !!res.requiredLoginToView,
          recordRoom: res.recordRoom !== undefined ? !!res.recordRoom : true,
          isPublicRoom: !!res.isPublicRoom,
          isSuperPrivateRoom: !!res.isSuperPrivateRoom,
          isPrivateRoom: !!res.isPrivateRoom,
          isSubscribed: !!res.isSubscribed,
          isShowMemberVIPBeforeEndTheMeeting: !!res.isShowMemberVIPBeforeEndTheMeeting,
          price: res.metadata?.price || 0,
          oldprice: res.metadata?.oldprice || 0,
          support: res.metadata?.support || '',
          isRegistrationEnabled: res.metadata?.isRegistrationEnabled || false,
        });
        setAddAdminList([]);
        setRemoveAdminList([]);
      }
    } catch (err) {
      console.error('Error fetching room details:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (field, value) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleAddAdmin = () => {
    if (!addAdminInput.trim()) return;
    if (!addAdminList.includes(addAdminInput.trim())) {
      setAddAdminList(prev => [...prev, addAdminInput.trim()]);
    }
    setAddAdminInput('');
  };

  const handleRemoveAddAdmin = (username) => {
    setAddAdminList(prev => prev.filter(u => u !== username));
  };

  const handleRemoveExistingAdmin = (uuid) => {
    if (!removeAdminList.includes(uuid)) {
      setRemoveAdminList(prev => [...prev, uuid]);
    }
  };

  const handleSave = async (e) => {
    if (e) e.preventDefault();
    if (!roomData?.canEdit) {
      alert('شما اجازه ویرایش این اتاق را ندارید.');
      return;
    }
    setSaving(true);
    try {
      const payload = {
        friendlyID: roomId,
        title: formData.title,
        explain: formData.explain,
        bio: formData.bio,
        ttr_URL: formData.ttrUrl,
        avatar: formData.avatar,
        logo: formData.logo,
        presentationPath: formData.presentationPath,
        coverImage: formData.coverImage,
        requiredAdminAcceptToView: formData.requiredAdminAcceptToView,
        requiredLoginToView: formData.requiredLoginToView,
        recordRoom: formData.recordRoom,
        isSuperPrivateRoom: formData.isSuperPrivateRoom,
        isPrivateRoom: formData.isPrivateRoom,
        isSubscribed: formData.isSubscribed,
        isShowMemberVIPBeforeEndTheMeeting: formData.isShowMemberVIPBeforeEndTheMeeting,
        addAdminUUIDs: addAdminList,
        removeAdminUUIDs: removeAdminList
      };

      const res = await EditRoomFull(payload);
      
      try {
        await fetch('/admin/SetRoomMetadata', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            roomId,
            price: Number(formData.price),
            oldprice: Number(formData.oldprice),
            support: formData.support,
            isRegistrationEnabled: formData.isRegistrationEnabled
          })
        });
      } catch (metaErr) {
        console.error('Error saving metadata', metaErr);
      }

      if (res.status === 200) {
        alert('تنظیمات اتاق با موفقیت به‌روزرسانی شد.');
        if (onUpdated) onUpdated();
        onClose();
      } else {
        alert(res.message || 'خطا در ذخیره اطلاعات');
      }
    } catch (err) {
      console.error(err);
      alert('خطا در برقراری ارتباط با سرور');
    } finally {
      setSaving(false);
    }
  };

  if (!isOpen) return null;

  const canEdit = roomData?.canEdit ?? true;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-md p-4">
      <div className="bg-white rounded-3xl shadow-2xl w-11/12 max-w-6xl h-[88vh] flex flex-col overflow-hidden border border-gray-100 dir-rtl">
        {/* Header */}
        <div className="bg-linear-to-r from-[#1f2937] to-[#111827] text-white px-8 py-5 flex items-center justify-between shadow-md">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-[#30d4cc]/20 rounded-2xl border border-[#30d4cc]/30">
              <FaEdit className="text-[#30d4cc] text-xl" />
            </div>
            <div>
              <h2 className="text-xl font-bold">مدیریت و ویرایش کامل اتاق</h2>
              <p className="text-xs text-gray-300 mt-0.5">
                {roomData ? roomData.title : 'در حال دریافت اطلاعات...'} ({roomId})
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 text-gray-400 hover:text-white hover:bg-white/10 rounded-full transition-all"
          >
            <FaTimes className="text-xl" />
          </button>
        </div>

        {/* Read-Only Notice for Admins */}
        {roomData && !roomData.canEdit && (
          <div className="bg-amber-50 border-b border-amber-200 px-8 py-3 flex items-center gap-3 text-amber-800 text-sm">
            <FaInfoCircle className="text-amber-600 shrink-0 text-lg" />
            <span>
              شما به عنوان <strong>مدیر اتاق</strong> دسترسی مشاهده تنظیمات را دارید. امکان ویرایش و ذخیره فقط برای <strong>مالک اصلی اتاق</strong> فعال است.
            </span>
          </div>
        )}

        {/* Modal Main Body (Right Sidebar Tabs + Content Area) */}
        {loading ? (
          <div className="flex-1 flex items-center justify-center text-gray-500">
            <div className="flex flex-col items-center gap-3">
              <div className="w-10 h-10 border-4 border-[#30d4cc] border-t-transparent rounded-full animate-spin"></div>
              <span>در حال دریافت کامل اطلاعات اتاق...</span>
            </div>
          </div>
        ) : (
          <div className="flex-1 flex overflow-hidden">
            {/* Right Sidebar Nav Tabs */}
            <div className="w-64 bg-gray-50 border-l border-gray-200 p-4 space-y-2 flex shrink-0 flex-col">
              <button
                type="button"
                onClick={() => setActiveTab('basic')}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-2xl font-semibold text-sm transition-all text-right ${
                  activeTab === 'basic'
                    ? 'bg-[#30d4cc] text-gray-900 shadow-lg shadow-[#30d4cc]/30 font-bold'
                    : 'text-gray-600 hover:bg-gray-200/70 hover:text-gray-900'
                }`}
              >
                <FaImage className="text-lg" />
                <span>اطلاعات پایه و کاور</span>
              </button>

              <button
                type="button"
                onClick={() => setActiveTab('settings')}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-2xl font-semibold text-sm transition-all text-right ${
                  activeTab === 'settings'
                    ? 'bg-[#30d4cc] text-gray-900 shadow-lg shadow-[#30d4cc]/30 font-bold'
                    : 'text-gray-600 hover:bg-gray-200/70 hover:text-gray-900'
                }`}
              >
                <FaSlidersH className="text-lg" />
                <span>تنظیمات و دسترسی‌ها</span>
              </button>

              <button
                type="button"
                onClick={() => setActiveTab('members')}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-2xl font-semibold text-sm transition-all text-right ${
                  activeTab === 'members'
                    ? 'bg-[#30d4cc] text-gray-900 shadow-lg shadow-[#30d4cc]/30 font-bold'
                    : 'text-gray-600 hover:bg-gray-200/70 hover:text-gray-900'
                }`}
              >
                <FaUsers className="text-lg" />
                <span>مدیران و اعضا</span>
              </button>

              <button
                type="button"
                onClick={() => setActiveTab('waitingList')}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-2xl font-semibold text-sm transition-all text-right ${
                  activeTab === 'waitingList'
                    ? 'bg-[#30d4cc] text-gray-900 shadow-lg shadow-[#30d4cc]/30 font-bold'
                    : 'text-gray-600 hover:bg-gray-200/70 hover:text-gray-900'
                }`}
              >
                <FaUsers className="text-lg" />
                <span>لیست انتظار</span>
              </button>
            </div>

            {/* Tab Content Area */}
            <div className="flex-1 p-8 overflow-y-auto bg-white">
              {/* TAB 1: BASIC INFORMATION & COVER */}
              {activeTab === 'basic' && (
                <div className="space-y-6 max-w-4xl">
                  <h3 className="text-lg font-bold text-gray-800 border-b pb-3 flex items-center gap-2">
                    <FaImage className="text-[#30d4cc]" />
                    اطلاعات اصلی و تصاویر اتاق
                  </h3>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                        عنوان اتاق <span className="text-red-500">*</span>
                      </label>
                      <input
                        type="text"
                        disabled={!canEdit}
                        value={formData.title}
                        onChange={e => handleInputChange('title', e.target.value)}
                        className="w-full px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100"
                        placeholder="عنوان اتاق"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                        آدرس TTR (لینک اختصاصی)
                      </label>
                      <input
                        type="text"
                        disabled={!canEdit}
                        value={formData.ttrUrl}
                        onChange={e => handleInputChange('ttrUrl', e.target.value)}
                        className="w-full px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100"
                        placeholder="https://ttr.link/..."
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                      بیو و معرفی کوتاه (Bio)
                    </label>
                    <input
                      type="text"
                      disabled={!canEdit}
                      value={formData.bio}
                      onChange={e => handleInputChange('bio', e.target.value)}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100"
                      placeholder="خلاصه توضیحات یا شعار اتاق..."
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                      توضیحات جامع (Explain)
                    </label>
                    <textarea
                      rows={3}
                      disabled={!canEdit}
                      value={formData.explain}
                      onChange={e => handleInputChange('explain', e.target.value)}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100"
                      placeholder="توضیحات کامل درباره اهداف و محتوای اتاق..."
                    />
                  </div>

                  <hr className="my-6 border-gray-200" />

                  {/* Images & Media Section */}
                  <h4 className="text-base font-bold text-gray-800 mb-4">تصاویر و فایل پیش‌فرض</h4>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                        تصویر کاور (image_cover متاداده)
                      </label>
                      <div className="flex gap-2">
                        <input
                          type="text"
                          disabled={!canEdit}
                          value={formData.coverImage}
                          onChange={e => handleInputChange('coverImage', e.target.value)}
                          className="flex-1 px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100 text-sm"
                          placeholder="آدرس تصویر کاور..."
                        />
                        {canEdit && (
                          <label className="cursor-pointer bg-gray-100 hover:bg-gray-200 border border-gray-300 text-gray-700 px-3 py-2.5 rounded-xl flex items-center gap-1.5 transition-all text-xs font-semibold shrink-0">
                            {uploadingField === 'coverImage' ? (
                              <FaSpinner className="animate-spin text-[#30d4cc]" />
                            ) : (
                              <FaUpload className="text-gray-500" />
                            )}
                            <span>آپلود</span>
                            <input
                              type="file"
                              accept="image/*"
                              className="hidden"
                              disabled={uploadingField === 'coverImage'}
                              onChange={e => handleFileUpload(e, 'coverImage')}
                            />
                          </label>
                        )}
                      </div>
                      {formData.coverImage && (
                        <div className="mt-2 rounded-xl overflow-hidden border border-gray-200 h-28 bg-gray-100 flex items-center justify-center">
                          <img src={formData.coverImage} alt="Cover Preview" className="h-full w-full object-cover" onError={e => e.target.style.display='none'} />
                        </div>
                      )}
                    </div>

                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                        لوگو / آیکون اختصاصی اتاق (Logo)
                      </label>
                      <div className="flex gap-2">
                        <input
                          type="text"
                          disabled={!canEdit}
                          value={formData.logo}
                          onChange={e => handleInputChange('logo', e.target.value)}
                          className="flex-1 px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100 text-sm"
                          placeholder="آدرس لوگو..."
                        />
                        {canEdit && (
                          <label className="cursor-pointer bg-gray-100 hover:bg-gray-200 border border-gray-300 text-gray-700 px-3 py-2.5 rounded-xl flex items-center gap-1.5 transition-all text-xs font-semibold shrink-0">
                            {uploadingField === 'logo' ? (
                              <FaSpinner className="animate-spin text-[#30d4cc]" />
                            ) : (
                              <FaUpload className="text-gray-500" />
                            )}
                            <span>آپلود</span>
                            <input
                              type="file"
                              accept="image/*"
                              className="hidden"
                              disabled={uploadingField === 'logo'}
                              onChange={e => handleFileUpload(e, 'logo')}
                            />
                          </label>
                        )}
                      </div>
                    </div>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                        تصویر آواتار (Avatar)
                      </label>
                      <div className="flex gap-2">
                        <input
                          type="text"
                          disabled={!canEdit}
                          value={formData.avatar}
                          onChange={e => handleInputChange('avatar', e.target.value)}
                          className="flex-1 px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100 text-sm"
                          placeholder="آدرس آواتار..."
                        />
                        {canEdit && (
                          <label className="cursor-pointer bg-gray-100 hover:bg-gray-200 border border-gray-300 text-gray-700 px-3 py-2.5 rounded-xl flex items-center gap-1.5 transition-all text-xs font-semibold shrink-0">
                            {uploadingField === 'avatar' ? (
                              <FaSpinner className="animate-spin text-[#30d4cc]" />
                            ) : (
                              <FaUpload className="text-gray-500" />
                            )}
                            <span>آپلود</span>
                            <input
                              type="file"
                              accept="image/*"
                              className="hidden"
                              disabled={uploadingField === 'avatar'}
                              onChange={e => handleFileUpload(e, 'avatar')}
                            />
                          </label>
                        )}
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                        مسیر اسلاید / پرزنتیشن پیش‌فرض (PresentationPath)
                      </label>
                      <div className="flex gap-2">
                        <input
                          type="text"
                          disabled={!canEdit}
                          value={formData.presentationPath}
                          onChange={e => handleInputChange('presentationPath', e.target.value)}
                          className="flex-1 px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100 text-sm"
                          placeholder="آدرس پرزنتیشن یا اسلاید..."
                        />
                        {canEdit && (
                          <label className="cursor-pointer bg-gray-100 hover:bg-gray-200 border border-gray-300 text-gray-700 px-3 py-2.5 rounded-xl flex items-center gap-1.5 transition-all text-xs font-semibold shrink-0">
                            {uploadingField === 'presentationPath' ? (
                              <FaSpinner className="animate-spin text-[#30d4cc]" />
                            ) : (
                              <FaUpload className="text-gray-500" />
                            )}
                            <span>آپلود</span>
                            <input
                              type="file"
                              className="hidden"
                              disabled={uploadingField === 'presentationPath'}
                              onChange={e => handleFileUpload(e, 'presentationPath')}
                            />
                          </label>
                        )}
                      </div>
                    </div>
                  </div>

                  {/* Metadata / Extra General Info */}
                  <hr className="my-6 border-gray-200" />
                  <h4 className="text-base font-bold text-gray-800 mb-4">اطلاعات فروش و پشتیبانی</h4>
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                        قیمت (تومان)
                      </label>
                      <input
                        type="number"
                        disabled={!canEdit}
                        value={formData.price}
                        onChange={e => handleInputChange('price', e.target.value)}
                        className="w-full px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                        قیمت قبلی (تومان)
                      </label>
                      <input
                        type="number"
                        disabled={!canEdit}
                        value={formData.oldprice}
                        onChange={e => handleInputChange('oldprice', e.target.value)}
                        className="w-full px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100"
                      />
                    </div>
                  </div>

                  <div className="mt-5">
                    <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                      اطلاعات پشتیبانی
                    </label>
                    <textarea
                      rows={2}
                      disabled={!canEdit}
                      value={formData.support}
                      onChange={e => handleInputChange('support', e.target.value)}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] disabled:bg-gray-100"
                      placeholder="اطلاعات تماس یا پیام پشتیبانی..."
                    />
                  </div>

                  <div className="mt-5 flex items-center justify-between p-4 bg-gray-50 rounded-2xl border border-gray-200">
                    <div>
                      <h4 className="font-bold text-gray-800 text-sm">امکان ثبت‌نام فعال باشد</h4>
                      <p className="text-xs text-gray-500 mt-0.5">کاربران بتوانند در این اتاق ثبت‌نام کنند</p>
                    </div>
                    <input
                      type="checkbox"
                      disabled={!canEdit}
                      checked={formData.isRegistrationEnabled}
                      onChange={e => handleInputChange('isRegistrationEnabled', e.target.checked)}
                      className="w-5 h-5 accent-[#30d4cc] rounded cursor-pointer"
                    />
                  </div>
                </div>
              )}

              {/* TAB 2: SETTINGS & ACCESS SWITCHES */}
              {activeTab === 'settings' && (
                <div className="space-y-6 max-w-4xl">
                  <h3 className="text-lg font-bold text-gray-800 border-b pb-3 flex items-center gap-2">
                    <FaSlidersH className="text-[#30d4cc]" />
                    تنظیمات امنیتی، ضبط و دسترسی‌های اتاق
                  </h3>

                  <div className="space-y-4">
                    {/* Switch 1 */}
                    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl border border-gray-200">
                      <div>
                        <h4 className="font-bold text-gray-800 text-sm">قبل از پیوستن نیاز به تأیید مدیر باشد</h4>
                        <p className="text-xs text-gray-500 mt-0.5">کاربران پس از ورود باید منتظر قبول توسط مدیر باشند (RequiredAdminAcceptToView)</p>
                      </div>
                      <input
                        type="checkbox"
                        disabled={!canEdit}
                        checked={formData.requiredAdminAcceptToView}
                        onChange={e => handleInputChange('requiredAdminAcceptToView', e.target.checked)}
                        className="w-5 h-5 accent-[#30d4cc] rounded cursor-pointer"
                      />
                    </div>

                    {/* Switch 2 */}
                    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl border border-gray-200">
                      <div>
                        <h4 className="font-bold text-gray-800 text-sm">نیازمند ورود به سیستم باشد</h4>
                        <p className="text-xs text-gray-500 mt-0.5">کاربران حتماً باید قبل از ورود در سامانه لاگین کرده باشند (RequiredLoginToView)</p>
                      </div>
                      <input
                        type="checkbox"
                        disabled={!canEdit}
                        checked={formData.requiredLoginToView}
                        onChange={e => handleInputChange('requiredLoginToView', e.target.checked)}
                        className="w-5 h-5 accent-[#30d4cc] rounded cursor-pointer"
                      />
                    </div>

                    {/* Switch 3 */}
                    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl border border-gray-200">
                      <div>
                        <h4 className="font-bold text-gray-800 text-sm">اجازه ضبط جلسه</h4>
                        <p className="text-xs text-gray-500 mt-0.5">امکان ریکورد و ضبط جلسات این اتاق فعال باشد (RecordRoom)</p>
                      </div>
                      <input
                        type="checkbox"
                        disabled={!canEdit}
                        checked={formData.recordRoom}
                        onChange={e => handleInputChange('recordRoom', e.target.checked)}
                        className="w-5 h-5 accent-[#30d4cc] rounded cursor-pointer"
                      />
                    </div>

                    {/* Switch 4 */}
                    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl border border-gray-200">
                      <div>
                        <h4 className="font-bold text-gray-800 text-sm">نمایش عمومی در صفحه اصلی (IsPublicRoom)</h4>
                        <p className="text-xs text-amber-600 mt-0.5">توجه: این فیلد توسط مدیران کل سامانه قابل تنظیم است.</p>
                      </div>
                      <input
                        type="checkbox"
                        disabled={true}
                        checked={formData.isPublicRoom}
                        className="w-5 h-5 accent-gray-400 rounded"
                      />
                    </div>

                    {/* Switch 5 */}
                    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl border border-gray-200">
                      <div>
                        <h4 className="font-bold text-gray-800 text-sm">اتاق فوق‌العاده خصوصی (IsSuperPrivateRoom)</h4>
                        <p className="text-xs text-gray-500 mt-0.5">اتاق حتی در پروفایل عمومی کاربر نیز نمایش داده نمی‌شود</p>
                      </div>
                      <input
                        type="checkbox"
                        disabled={!canEdit}
                        checked={formData.isSuperPrivateRoom}
                        onChange={e => handleInputChange('isSuperPrivateRoom', e.target.checked)}
                        className="w-5 h-5 accent-[#30d4cc] rounded cursor-pointer"
                      />
                    </div>

                    {/* Switch 6 */}
                    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl border border-gray-200">
                      <div>
                        <h4 className="font-bold text-gray-800 text-sm">اتاق خصوصی با الزام ثبت‌نام (IsPrivateRoom)</h4>
                        <p className="text-xs text-gray-500 mt-0.5">صفحه روم باز می‌شود اما ورود مستلزم تایید ثبت‌نام است</p>
                      </div>
                      <input
                        type="checkbox"
                        disabled={!canEdit}
                        checked={formData.isPrivateRoom}
                        onChange={e => handleInputChange('isPrivateRoom', e.target.checked)}
                        className="w-5 h-5 accent-[#30d4cc] rounded cursor-pointer"
                      />
                    </div>

                    {/* Switch 7 */}
                    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl border border-gray-200">
                      <div>
                        <h4 className="font-bold text-gray-800 text-sm">اتاق اشتراکی (IsSubscribed)</h4>
                        <p className="text-xs text-gray-500 mt-0.5">وضعیت اشتراک ویژه برای اتاق</p>
                      </div>
                      <input
                        type="checkbox"
                        disabled={!canEdit}
                        checked={formData.isSubscribed}
                        onChange={e => handleInputChange('isSubscribed', e.target.checked)}
                        className="w-5 h-5 accent-[#30d4cc] rounded cursor-pointer"
                      />
                    </div>

                    {/* Switch 8 */}
                    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-2xl border border-gray-200">
                      <div>
                        <h4 className="font-bold text-gray-800 text-sm">نمایش اعضای VIP قبل از پایان جلسه (IsShowMemberVIP)</h4>
                        <p className="text-xs text-gray-500 mt-0.5">نمایش لیست اعضای ویژه قبل از بسته شدن جلسه</p>
                      </div>
                      <input
                        type="checkbox"
                        disabled={!canEdit}
                        checked={formData.isShowMemberVIPBeforeEndTheMeeting}
                        onChange={e => handleInputChange('isShowMemberVIPBeforeEndTheMeeting', e.target.checked)}
                        className="w-5 h-5 accent-[#30d4cc] rounded cursor-pointer"
                      />
                    </div>
                  </div>
                </div>
              )}

              {/* TAB 3: MEMBERS & ADMINS */}
              {activeTab === 'members' && (
                <div className="space-y-6 max-w-4xl">
                  <h3 className="text-lg font-bold text-gray-800 border-b pb-3 flex items-center gap-2">
                    <FaUsers className="text-[#30d4cc]" />
                    مدیریت اعضا و مدیران اتاق
                  </h3>

                  {/* Add Admin Form */}
                  {canEdit && (
                    <div className="bg-gray-50 p-5 rounded-2xl border border-gray-200">
                      <h4 className="font-bold text-gray-800 text-sm mb-3 flex items-center gap-2">
                        <FaUserPlus className="text-[#30d4cc]" />
                        افزودن مدیر جدید به اتاق (ROOMTOUSER_ADMIN = 551)
                      </h4>
                      <div className="flex gap-3">
                        <input
                          type="text"
                          value={addAdminInput}
                          onChange={e => setAddAdminInput(e.target.value)}
                          placeholder="نام کاربری، شماره تلفن یا UUID کاربر..."
                          className="flex-1 px-4 py-2 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc]"
                        />
                        <button
                          type="button"
                          onClick={handleAddAdmin}
                          className="px-5 py-2 bg-[#30d4cc] text-gray-900 font-bold rounded-xl hover:bg-[#2bc4bc] transition-all flex items-center gap-2"
                        >
                          <FaUserPlus />
                          افزودن مدیر
                        </button>
                      </div>

                      {addAdminList.length > 0 && (
                        <div className="mt-3 flex flex-wrap gap-2">
                          {addAdminList.map(u => (
                            <span key={u} className="bg-emerald-100 text-emerald-800 text-xs px-3 py-1.5 rounded-xl font-bold flex items-center gap-2">
                              + مدیر جدید: {u}
                              <button type="button" onClick={() => handleRemoveAddAdmin(u)} className="text-red-500 hover:text-red-700">
                                <FaTimes />
                              </button>
                            </span>
                          ))}
                        </div>
                      )}
                    </div>
                  )}

                  {/* Existing Members List */}
                  <div>
                    <h4 className="font-bold text-gray-800 text-sm mb-3">لیست اعضای فعلی اتاق</h4>
                    {roomData?.members && roomData.members.filter(m => m.status === 100 || m.status === 500).length > 0 ? (
                      <div className="overflow-x-auto border border-gray-200 rounded-2xl">
                        <table className="w-full text-sm text-right">
                          <thead className="bg-gray-100 text-gray-700 font-bold">
                            <tr>
                              <th className="p-3">نام و نام خانوادگی</th>
                              <th className="p-3">نام کاربری / UUID</th>
                              <th className="p-3">نقش در اتاق</th>
                              {canEdit && <th className="p-3 text-center">عملیات</th>}
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-gray-200">
                            {roomData.members.filter(m => m.status === 100 || m.status === 500).map(m => {
                              const isAdmin = m.type === 551;
                              const isMarkedForRemove = removeAdminList.includes(m.uuid);

                              return (
                                <tr key={m.id} className={isMarkedForRemove ? 'bg-red-50 opacity-60' : 'hover:bg-gray-50'}>
                                  <td className="p-3 font-semibold text-gray-800">{m.fullname}</td>
                                  <td className="p-3 text-gray-500 font-mono text-xs">{m.username || m.uuid}</td>
                                  <td className="p-3">
                                    {isAdmin ? (
                                      <span className="bg-blue-100 text-blue-800 font-bold text-xs px-2.5 py-1 rounded-lg border border-blue-200">
                                        ⭐ مدیر اتاق (551)
                                      </span>
                                    ) : (
                                      <span className="bg-gray-100 text-gray-700 text-xs px-2.5 py-1 rounded-lg">
                                        عضو اتاق
                                      </span>
                                    )}
                                  </td>
                                  {canEdit && (
                                    <td className="p-3 text-center">
                                      {isAdmin && (
                                        <button
                                          type="button"
                                          onClick={() => handleRemoveExistingAdmin(m.uuid)}
                                          className="text-red-500 hover:text-red-700 text-xs px-2 py-1 rounded-lg border border-red-200 hover:bg-red-100 transition-all"
                                        >
                                          <FaTrash className="inline ml-1" />
                                          حذف دسترسی مدیریت
                                        </button>
                                      )}
                                    </td>
                                  )}
                                </tr>
                              );
                            })}
                          </tbody>
                        </table>
                      </div>
                    ) : (
                      <div className="p-6 text-center text-gray-400 border border-dashed border-gray-200 rounded-2xl">
                        هیچ عضوی در این اتاق یافت نشد.
                      </div>
                    )}
                  </div>
                </div>
              )}

              {/* TAB 4: WAITING LIST */}
              {activeTab === 'waitingList' && (
                <div className="space-y-6 max-w-4xl">
                  <h3 className="text-lg font-bold text-gray-800 border-b pb-3 flex items-center gap-2">
                    <FaUsers className="text-[#30d4cc]" />
                    لیست انتظار ورود به اتاق
                  </h3>

                  <div className="bg-gray-50 p-4 rounded-2xl border border-gray-200">
                    <input
                      type="text"
                      value={waitingSearch}
                      onChange={e => setWaitingSearch(e.target.value)}
                      placeholder="جستجو بر اساس نام، کدملی یا موبایل..."
                      className="w-full px-4 py-2 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc]"
                    />
                  </div>

                  {loadingWaiting ? (
                    <div className="flex justify-center p-8 text-gray-500">
                      <FaSpinner className="animate-spin text-3xl text-[#30d4cc]" />
                    </div>
                  ) : (
                    <div>
                      {waitingMembers.filter(m => 
                        !waitingSearch || 
                        (m.fullname && m.fullname.includes(waitingSearch)) ||
                        (m.nationalCode && m.nationalCode.includes(waitingSearch)) ||
                        (m.mobile && m.mobile.includes(waitingSearch))
                      ).length > 0 ? (
                        <div className="overflow-x-auto border border-gray-200 rounded-2xl">
                          <table className="w-full text-sm text-right">
                            <thead className="bg-gray-100 text-gray-700 font-bold">
                              <tr>
                                <th className="p-3">نام و نام خانوادگی</th>
                                <th className="p-3">موبایل</th>
                                <th className="p-3">کدملی</th>
                                {canEdit && <th className="p-3 text-center">عملیات</th>}
                              </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-200">
                              {waitingMembers
                                .filter(m => 
                                  !waitingSearch || 
                                  (m.fullname && m.fullname.includes(waitingSearch)) ||
                                  (m.nationalCode && m.nationalCode.includes(waitingSearch)) ||
                                  (m.mobile && m.mobile.includes(waitingSearch))
                                )
                                .map(m => (
                                  <tr key={m.userId || m.id} className="hover:bg-gray-50">
                                    <td className="p-3 font-semibold text-gray-800">{m.fullname}</td>
                                    <td className="p-3 text-gray-600">{m.mobile}</td>
                                    <td className="p-3 text-gray-600">{m.nationalCode}</td>
                                    {canEdit && (
                                      <td className="p-3 text-center flex gap-2 justify-center">
                                        <button
                                          type="button"
                                          onClick={() => handleChangeMemberStatus(m.userId, 100)}
                                          className="px-3 py-1 bg-green-100 text-green-700 hover:bg-green-200 rounded-lg font-bold text-xs transition-all"
                                        >
                                          تایید
                                        </button>
                                        <button
                                          type="button"
                                          onClick={() => handleChangeMemberStatus(m.userId, 0)}
                                          className="px-3 py-1 bg-red-100 text-red-700 hover:bg-red-200 rounded-lg font-bold text-xs transition-all"
                                        >
                                          رد
                                        </button>
                                      </td>
                                    )}
                                  </tr>
                                ))}
                            </tbody>
                          </table>
                        </div>
                      ) : (
                        <div className="p-6 text-center text-gray-400 border border-dashed border-gray-200 rounded-2xl">
                          هیچ موردی در لیست انتظار یافت نشد.
                        </div>
                      )}
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        )}

        {/* Footer Actions */}
        <div className="bg-gray-100 border-t border-gray-200 px-8 py-4 flex items-center justify-between">
          <button
            type="button"
            onClick={onClose}
            className="px-6 py-2.5 text-gray-600 hover:text-gray-900 font-semibold rounded-xl hover:bg-gray-200 transition-all"
          >
            بستن
          </button>

          {canEdit && (
            <button
              type="button"
              onClick={handleSave}
              disabled={saving}
              className="px-8 py-2.5 bg-linear-to-r from-[#30d4cc] to-[#2bc4bc] text-gray-900 font-bold rounded-xl shadow-lg shadow-[#30d4cc]/30 hover:shadow-xl transition-all flex items-center gap-2 disabled:opacity-50"
            >
              <FaSave />
              {saving ? 'در حال ذخیره...' : 'ذخیره تغییرات اتاق'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default EditRoomLargeModal;
