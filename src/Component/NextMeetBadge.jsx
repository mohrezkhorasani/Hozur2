// src/components/RoomCard/NextMeetingBadge.jsx
import React from 'react';
import { formatPersianDate, formatPersianDateTime } from '../Tools/tools';

const NextMeetingBadge = ({ plan }) => {
    if (!plan) return null;

    const hasTitle = plan.title && plan.title.trim() !== '';
    const hasTeacher = plan.teacher && plan.teacher.trim() !== '';
    const hasDabir = plan.dabir && plan.dabir.trim() !== '';

    return (
        <div className="mt-4 p-3 bg-gradient-to-r from-[#30d4cc]/10 to-[#30d4cc]/5 rounded-xl border border-[#30d4cc]/20">
            <div className="flex items-center justify-between mb-2">
                <span className="text-xs font-semibold text-[#30d4cc] bg-[#30d4cc]/10 px-2 py-1 rounded-lg">
                    جلسه بعدی
                </span>
                <span className="text-xs text-[#6b7280]">
                    {(() => {
                        if (!plan?.startAt) return 'تاریخ نامشخص';

                        try {
                            const date = new Date(plan.startAt);
                            const tehranOffset = 3.5 * 60 * 60 * 1000;
                            const tehranTime = formatPersianDateTime(new Date(date.getTime() - tehranOffset));
                            return tehranTime
                            // const year = tehranTime.getFullYear();
                            // const month = String(tehranTime.getMonth() + 1).padStart(2, '0');
                            // const day = String(tehranTime.getDate()).padStart(2, '0');
                            // const hours = String(tehranTime.getHours()).padStart(2, '0');
                            // const minutes = String(tehranTime.getMinutes()).padStart(2, '0');

                            // return `${year}/${month}/${day} ${hours}:${minutes}`;
                        } catch (error) {
                            return 'تاریخ نامشخص';
                        }
                    })()}   
                </span>
            </div>

            <div className="space-y-1.5 text-right">
                {hasTitle && (
                    <div className="flex items-center gap-2 text-sm">
                        <span className="text-[#6b7280] min-w-[50px] text-xs">موضوع:</span>
                        <span className="text-[#1f2937] font-medium">{plan.title}</span>
                    </div>
                )}

                {hasTeacher && (
                    <div className="flex items-center gap-2 text-sm">
                        <span className="text-[#6b7280] min-w-[50px] text-xs">استاد:</span>
                        <span className="text-[#1f2937]">{plan.teacher}</span>
                    </div>
                )}

                {hasDabir && (
                    <div className="flex items-center gap-2 text-sm">
                        <span className="text-[#6b7280] min-w-[50px] text-xs">دبیر:</span>
                        <span className="text-[#1f2937]">{plan.dabir}</span>
                    </div>
                )}

                {!hasTitle && !hasTeacher && !hasDabir && (
                    <div className="text-sm text-[#6b7280] text-center py-1">
                        جلسه برنامه‌ریزی شده
                    </div>
                )}
            </div>
        </div>
    );
};

export default NextMeetingBadge;