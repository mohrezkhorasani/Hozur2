// src/components/AttachRoomsModal.jsx
import React, { useState, useEffect, useMemo } from 'react';
import { AddToRooms, GetAllRooms, RoomRelatedBy } from '../../api/AdminAPI';
import { useToast } from '../../Tools/ToastProvider';

const AttachRoomsModal = ({
    isOpen,
    onClose,
    userId,
    onAttachRooms,
}) => {
    const [allRooms, setAllRooms] = useState([]);
    const [relatedRooms, setRelatedRooms] = useState([]);
    const [selectedRooms, setSelectedRooms] = useState(new Set());
    const [loading, setLoading] = useState(false);
    const [searchTerm, setSearchTerm] = useState('');
    const [error, setError] = useState(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [successMessage, setSuccessMessage] = useState(null);
    const toast = useToast();

    // Load data when modal opens
    useEffect(() => {
        if (isOpen && userId) {
            loadData();
        }
    }, [isOpen, userId]);

    // Reset states when modal closes
    useEffect(() => {
        if (!isOpen) {
            setSelectedRooms(new Set());
            setSearchTerm('');
            setError(null);
            setSuccessMessage(null);
        }
    }, [isOpen]);

    const loadData = async () => {
        setLoading(true);
        setError(null);
        try {
            // Load all rooms
            const allRoomsResponse = await GetAllRooms();
            const allRoomsData = allRoomsResponse.data || [];
            setAllRooms(allRoomsData);

            // Load user's related rooms
            const relatedResponse = await RoomRelatedBy(userId);
            const relatedData = relatedResponse.rels || [];
            setRelatedRooms(relatedData);

            // Pre-select rooms that are already related
            const relatedIds = new Set(relatedData.map(room => room.friendlyID));
            setSelectedRooms(relatedIds);
        } catch (err) {
            console.error('Error loading data:', err);
            setError('خطا در دریافت اطلاعات');
            toast.error('خطا در دریافت اطلاعات');
        } finally {
            setLoading(false);
        }
    };

    const handleRoomToggle = (friendlyID) => {
        setSelectedRooms(prev => {
            const newSet = new Set(prev);
            if (newSet.has(friendlyID)) {
                newSet.delete(friendlyID);
            } else {
                newSet.add(friendlyID);
            }
            return newSet;
        });
    };

    const handleSelectAll = () => {
        const filteredRooms = getFilteredRooms();
        const allFilteredIds = new Set(filteredRooms.map(room => room.friendlyID || room.roomID));
        setSelectedRooms(allFilteredIds);
    };

    const handleDeselectAll = () => {
        const filteredRooms = getFilteredRooms();
        const filteredIds = new Set(filteredRooms.map(room => room.friendlyID || room.roomID));
        setSelectedRooms(prev => {
            const newSet = new Set(prev);
            filteredIds.forEach(id => newSet.delete(id));
            return newSet;
        });
    };

    const handleSubmit = async () => {
        if (selectedRooms.size === 0) {
            setError('لطفاً حداقل یک اتاق را انتخاب کنید');
            toast.warning('لطفاً حداقل یک اتاق را انتخاب کنید');
            return;
        }

        setIsSubmitting(true);
        setError(null);
        setSuccessMessage(null);

        try {
            const friendlyIDs = Array.from(selectedRooms);
            await AddToRooms(userId, friendlyIDs);

            setSuccessMessage(`✅ ${friendlyIDs.length} اتاق با موفقیت متصل شدند`);
            toast.success(`${friendlyIDs.length} اتاق با موفقیت متصل شدند`);

            // Reload data to reflect changes
            await loadData();

            // Call callback
            if (onAttachRooms) {
                onAttachRooms(friendlyIDs);
            }

            // Close modal after delay
            setTimeout(() => {
                onClose();
            }, 1500);
        } catch (err) {
            console.error('Error attaching rooms:', err);
            setError(err.message || 'خطا در اتصال اتاق‌ها');
            toast.error(err.message || 'خطا در اتصال اتاق‌ها');
        } finally {
            setIsSubmitting(false);
        }
    };

    // Enhanced search function that searches by title, ID, and owner
    const getFilteredRooms = () => {
        if (!searchTerm.trim()) return getSortedRooms(allRooms);

        const term = searchTerm.trim().toLowerCase();
        const filtered = allRooms.filter(room => {
            const title = (room.title || '').toLowerCase();
            const id = (room.friendlyID || room.roomID || '').toLowerCase();
            const ownerName = (room.owner?.fullname || '').toLowerCase();
            const ownerUsername = (room.owner?.username || '').toLowerCase();
            
            return title.includes(term) || 
                   id.includes(term) || 
                   ownerName.includes(term) ||
                   ownerUsername.includes(term);
        });
        
        return getSortedRooms(filtered);
    };

    // Sort rooms: related rooms first, then others
    const getSortedRooms = (rooms) => {
        const relatedIds = new Set(relatedRooms.map(room => room.friendlyID));
        
        return [...rooms].sort((a, b) => {
            const aId = a.friendlyID || a.roomID;
            const bId = b.friendlyID || b.roomID;
            const aIsRelated = relatedIds.has(aId);
            const bIsRelated = relatedIds.has(bId);
            
            // If both are related or both are not related, sort by title
            if (aIsRelated === bIsRelated) {
                return (a.title || '').localeCompare(b.title || '', 'fa');
            }
            
            // Related rooms come first
            return aIsRelated ? -1 : 1;
        });
    };

    const filteredRooms = getFilteredRooms();
    const isRoomSelected = (room) => {
        const id = room.friendlyID || room.roomID;
        return selectedRooms.has(id);
    };

    const isRoomRelated = (room) => {
        const id = room.friendlyID || room.roomID;
        return relatedRooms.some(r => r.friendlyID === id);
    };

    // Get count of related rooms in filtered list
    const relatedCountInFiltered = useMemo(() => {
        const relatedIds = new Set(relatedRooms.map(room => room.friendlyID));
        return filteredRooms.filter(room => {
            const id = room.friendlyID || room.roomID;
            return relatedIds.has(id);
        }).length;
    }, [filteredRooms, relatedRooms]);

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
            <div className="bg-white rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] flex flex-col">
                {/* Header */}
                <div className="sticky top-0 bg-white border-b px-6 py-4 rounded-t-2xl flex-shrink-0">
                    <div className="flex justify-between items-center">
                        <div>
                            <h2 className="text-xl font-bold text-gray-800">
                                اتصال اتاق‌ها به کاربر
                            </h2>
                            <p className="text-sm text-gray-500 mt-1">
                                {userId && `شناسه کاربر: ${userId}`}
                            </p>
                        </div>
                        <button
                            onClick={onClose}
                            className="text-gray-400 hover:text-gray-600 transition-colors"
                            disabled={isSubmitting}
                        >
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>
                    </div>
                </div>

                {/* Body */}
                <div className="flex-1 overflow-hidden flex flex-col">
                    {/* Loading State */}
                    {loading && (
                        <div className="flex-1 flex items-center justify-center p-8">
                            <div className="text-center">
                                <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-[#30d4cc]"></div>
                                <p className="mt-2 text-gray-500">در حال بارگذاری...</p>
                            </div>
                        </div>
                    )}

                    {/* Content */}
                    {!loading && (
                        <>
                            {/* Search and Actions Bar */}
                            <div className="px-6 py-4 border-b bg-gray-50 flex-shrink-0">
                                <div className="flex flex-col sm:flex-row gap-3">
                                    <div className="flex-1 relative">
                                        <input
                                            type="text"
                                            value={searchTerm}
                                            onChange={(e) => setSearchTerm(e.target.value)}
                                            placeholder="جستجو بر اساس عنوان، شناسه یا نام مالک..."
                                            className="w-full px-4 py-2.5 pr-10 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all"
                                            disabled={isSubmitting}
                                        />
                                        <svg
                                            className="absolute left-3 top-3 w-5 h-5 text-gray-400"
                                            fill="none"
                                            stroke="currentColor"
                                            viewBox="0 0 24 24"
                                        >
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                                        </svg>
                                    </div>
                                    <div className="flex gap-2">
                                        <button
                                            onClick={handleSelectAll}
                                            className="px-4 py-2.5 text-sm bg-blue-50 text-blue-600 rounded-xl hover:bg-blue-100 transition-colors"
                                            disabled={isSubmitting || filteredRooms.length === 0}
                                        >
                                            انتخاب همه
                                        </button>
                                        <button
                                            onClick={handleDeselectAll}
                                            className="px-4 py-2.5 text-sm bg-gray-100 text-gray-600 rounded-xl hover:bg-gray-200 transition-colors"
                                            disabled={isSubmitting || filteredRooms.length === 0}
                                        >
                                            لغو انتخاب
                                        </button>
                                    </div>
                                </div>
                                {/* Search hint */}
                                <div className="mt-1 text-xs text-gray-400">
                                    💡 می‌توانید بر اساس عنوان، شناسه یا نام مالک جستجو کنید
                                </div>
                            </div>

                            {/* Stats Bar */}
                            <div className="px-6 py-2 bg-gray-50 border-b flex-shrink-0">
                                <div className="flex flex-wrap items-center gap-4 text-sm">
                                    <span className="text-gray-600">
                                        <span className="font-semibold">{selectedRooms.size}</span> اتاق انتخاب شده
                                    </span>
                                    <span className="text-gray-400">|</span>
                                    <span className="text-gray-600">
                                        <span className="font-semibold">{relatedRooms.length}</span> اتاق متصل (کل)
                                    </span>
                                    <span className="text-gray-400">|</span>
                                    <span className="text-gray-600">
                                        <span className="font-semibold">{relatedCountInFiltered}</span> اتاق متصل در لیست فعلی
                                    </span>
                                    <span className="text-gray-400">|</span>
                                    <span className="text-gray-600">
                                        <span className="font-semibold">{filteredRooms.length}</span> اتاق در لیست
                                    </span>
                                    {searchTerm && (
                                        <span className="text-blue-500">
                                            جستجو: "{searchTerm}"
                                        </span>
                                    )}
                                </div>
                            </div>

                            {/* Room List */}
                            <div className="flex-1 overflow-y-auto p-4">
                                {filteredRooms.length === 0 ? (
                                    <div className="text-center py-12">
                                        <svg className="w-16 h-16 mx-auto text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                                        </svg>
                                        <p className="mt-4 text-gray-500">
                                            {searchTerm ? 'هیچ اتاقی با این جستجو یافت نشد' : 'هیچ اتاقی موجود نیست'}
                                        </p>
                                    </div>
                                ) : (
                                    <>
                                        {/* Show related rooms count badge */}
                                        {relatedCountInFiltered > 0 && searchTerm && (
                                            <div className="mb-3 px-3 py-2 bg-blue-50 border border-blue-200 rounded-lg text-sm text-blue-700">
                                                🏷️ {relatedCountInFiltered} اتاق متصل در نتایج جستجو یافت شد
                                            </div>
                                        )}
                                        
                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                                            {filteredRooms.map((room) => {
                                                const roomId = room.friendlyID || room.roomID;
                                                const isSelected = selectedRooms.has(roomId);
                                                const isRelated = isRoomRelated(room);
                                                const isDisabled = isSubmitting;

                                                return (
                                                    <div
                                                        key={roomId}
                                                        onClick={() => !isDisabled && handleRoomToggle(roomId)}
                                                        className={`
                                                            group relative p-4 border-2 rounded-xl cursor-pointer transition-all
                                                            ${isSelected
                                                                ? 'border-[#30d4cc] bg-[#30d4cc]/5 shadow-md'
                                                                : isRelated
                                                                    ? 'border-blue-300 bg-blue-50/50 hover:bg-blue-100/50'
                                                                    : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'
                                                            }
                                                            ${isDisabled ? 'opacity-50 cursor-not-allowed' : ''}
                                                        `}
                                                    >
                                                        <div className="flex items-start gap-3">
                                                            {/* Checkbox */}
                                                            <div className="flex-shrink-0 mt-0.5">
                                                                <div className={`
                                                                    w-5 h-5 rounded border-2 flex items-center justify-center transition-all
                                                                    ${isSelected 
                                                                        ? 'bg-[#30d4cc] border-[#30d4cc]' 
                                                                        : isRelated
                                                                            ? 'border-blue-400 bg-blue-100'
                                                                            : 'border-gray-300 bg-white group-hover:border-gray-400'
                                                                    }
                                                                `}>
                                                                    {isSelected && (
                                                                        <svg className="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                                                                        </svg>
                                                                    )}
                                                                    {!isSelected && isRelated && (
                                                                        <div className="w-2 h-2 rounded-full bg-blue-500"></div>
                                                                    )}
                                                                </div>
                                                            </div>

                                                            {/* Room Info */}
                                                            <div className="flex-1 min-w-0">
                                                                <div className="flex items-center gap-2">
                                                                    <h3 className={`font-semibold truncate ${isSelected ? 'text-gray-800' : isRelated ? 'text-blue-800' : 'text-gray-700'}`}>
                                                                        {room.title || 'بدون عنوان'}
                                                                    </h3>
                                                                    {isRelated && (
                                                                        <span className="text-xs bg-blue-100 text-blue-600 px-2 py-0.5 rounded-full whitespace-nowrap">
                                                                            متصل
                                                                        </span>
                                                                    )}
                                                                    {isSelected && !isRelated && (
                                                                        <span className="text-xs bg-green-100 text-green-600 px-2 py-0.5 rounded-full whitespace-nowrap">
                                                                            جدید
                                                                        </span>
                                                                    )}
                                                                </div>
                                                                <div className="flex flex-wrap items-center gap-2 mt-1 text-xs text-gray-500">
                                                                    <span className={`px-2 py-0.5 rounded font-mono ${isRelated ? 'bg-blue-100' : 'bg-gray-100'}`}>
                                                                        {roomId}
                                                                    </span>
                                                                    {room.owner?.fullname && (
                                                                        <>
                                                                            <span className="text-gray-300">|</span>
                                                                            <span className="flex items-center gap-1">
                                                                                <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                                                                                </svg>
                                                                                {room.owner.fullname}
                                                                            </span>
                                                                        </>
                                                                    )}
                                                                    {room.owner?.username && (
                                                                        <span className="text-gray-400">
                                                                            (@{room.owner.username})
                                                                        </span>
                                                                    )}
                                                                </div>
                                                                {room.latestTitle && (
                                                                    <p className="mt-1 text-sm text-gray-600 truncate">
                                                                        📌 {room.latestTitle}
                                                                    </p>
                                                                )}
                                                            </div>
                                                        </div>
                                                    </div>
                                                );
                                            })}
                                        </div>
                                    </>
                                )}
                            </div>
                        </>
                    )}

                    {/* Error/Success Messages */}
                    {error && (
                        <div className="px-6 py-3 border-t flex-shrink-0">
                            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-2 rounded-xl">
                                ❌ {error}
                            </div>
                        </div>
                    )}
                    {successMessage && (
                        <div className="px-6 py-3 border-t flex-shrink-0">
                            <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-2 rounded-xl">
                                {successMessage}
                            </div>
                        </div>
                    )}
                </div>

                {/* Footer */}
                <div className="sticky bottom-0 bg-white border-t px-6 py-4 rounded-b-2xl flex-shrink-0">
                    <div className="flex flex-col sm:flex-row gap-3">
                        <button
                            onClick={handleSubmit}
                            disabled={isSubmitting || selectedRooms.size === 0}
                            className={`
                                flex-1 py-2.5 rounded-xl font-semibold transition-all
                                ${!isSubmitting && selectedRooms.size > 0
                                    ? 'bg-linear-to-r from-[#30d4cc] to-[#2bc4bc] text-[#000000] hover:shadow-lg'
                                    : 'bg-gray-200 text-gray-400 cursor-not-allowed'
                                }
                            `}
                        >
                            {isSubmitting ? (
                                <span className="flex items-center justify-center gap-2">
                                    <span className="inline-block animate-spin rounded-full h-4 w-4 border-b-2 border-white"></span>
                                    در حال اتصال...
                                </span>
                            ) : (
                                `🔗 اتصال ${selectedRooms.size} اتاق انتخاب شده`
                            )}
                        </button>
                        <button
                            onClick={onClose}
                            className="px-6 py-2.5 text-gray-400 hover:text-gray-600 transition-colors"
                            disabled={isSubmitting}
                        >
                            انصراف
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AttachRoomsModal;