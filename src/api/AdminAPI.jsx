import Cookies from 'js-cookie';
import { BaseURL, NormalHeaderAPI } from './apiNeed';
import { combinePath } from '../Tools/tools';



export async function GetUserList(page, count, order = 100) {
    const url = combinePath(BaseURL, `/user/AdminGetUserList?page=${page - 1}&count=${count}&o=${order}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}


export async function ChangeRole(userID, roleNumber) {
    const url = combinePath(BaseURL, `/user/AdminChangeRole`);

    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify({
            Role: roleNumber,   // توجه: با حرف بزرگ R
            UserId: userID      // توجه: با حرف بزرگ U
        })
    });

    if (!res.ok) {
        const errorText = await res.text();
        throw new Error(`Failed to change role: ${res.status} ${errorText}`);
    }

    return await res.json();
}

export async function ChangeStatus(userID, status) {
    const url = combinePath(BaseURL, `/user/AdminChangeStatus`);

    const res = await fetch(url, {
        method: "POST",
        headers: NormalHeaderAPI,
        body: JSON.stringify({
            status: status, // masalan 100 = Enabled -100 = disable
            //              , Pin = 999
            //  , Disable = -100
            //  , Canceled = -101
            //  , Delete = -400
            //  , Forbidden = -403
            //  , Ban = -403 ,

            userId: userID
        })
    });

    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}

export async function GetRoomListOfUser(uid, page, count, order = 100) {
    const url = combinePath(BaseURL, `/user/AdminGetUserRoomList?uid=${uid}&page=${page}&count=${count}&o=${order}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}
export async function RemoveAdminGetUserRoomList(uid, roomID) {
    const url = combinePath(BaseURL, `/user/RemoveAdminGetUserRoomList?uid=${uid}&rid=${roomID}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}


export async function AdminCreateRoom(roomData) {
    const url = combinePath(BaseURL, `/user/AdminCreateRoom`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify(roomData)
    });
    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد اتاق");
    return json;
}
export async function GetPollListAdmin() {
    const res = await fetch(combinePath(BaseURL, `/admin/poll/GetPollListAdmin`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}

export async function AddPollQuiz(data) {
    //console.log(data)
    const url = combinePath(BaseURL, `/admin/poll/AddPollQuiz`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد اتاق");
    return json;
}

export async function UpdatePollQuiz(id,data) {
    //console.log(data)
    const url = combinePath(BaseURL, `/admin/poll/UpdatePollQuiz/${id}`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد اتاق");
    return json;
}

export async function DeletePollQuiz(id) {
    //console.log(data)
    const url = combinePath(BaseURL, `/admin/poll/DeletePollQuiz/${id}`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        }
    });
    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد اتاق");
    return json;
}

export async function NewPlanRoom(data) {
    //console.log(data)
    const url = combinePath(BaseURL, `/user/NewPlanRoom`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
        },
        body: data
    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد اتاق");
    return json;
}
export async function DeletePlanRoom(idplan) {
    //console.log(`Deleting plan room with ID: ${idplan}`);
    const url = combinePath(BaseURL, `/user/DeletePlanRoom/${idplan}`);
    const res = await fetch(url, {
        method: "DELETE",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json',
        },
    });
    if (!res.ok) throw new Error("Failed to delete plan room");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در حذف جلسه");
    return json;
}


export async function GetPlanRoom(uid, page, count, order = 100) {
    const url = combinePath(BaseURL, `/user/GetPlanRoom?friendlyID=${uid}&page=${page}&count=${count}&o=${order}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    //console.log("getplans", res)
    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}


export async function InsertSlideShow(formData) {
    const url = combinePath(BaseURL, `/user/InsertSlideShow`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            // حذف 'Content-Type' تا مرورگر خودکار آن را برای FormData تنظیم کند
        },
        body: formData // ارسال مستقیم FormData
    });
    if (!res.ok) throw new Error("Failed to create slideshow");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد اسلایدشو");
    return json;
}

export async function EditSlideShow(data) {
    //console.log(data)
    const url = combinePath(BaseURL, `/user/EditSlideShow`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify(data)
    });
    //console.log(res)

    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    //console.log(json)
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد sاسلایدشو");
    return json;
}



export async function ChangeAvatarOfRoom(data) {
    //console.log(data)
    const url = combinePath(BaseURL, `/user/ChangeAvatarOfRoom`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify(data)
    });
    //console.log(res)

    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    //console.log(json)
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد sاسلایدشو");
    return json;
}

export async function RoomRelatedBy(uid) {
    const url = combinePath(BaseURL, `/user/RoomRelatedBy?uid=${uid}`);
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    //console.log("RoomRelatedBy", res);
    if (!res.ok) throw new Error("Failed to get related rooms");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در دریافت اتاق‌های مرتبط");
    return json;
}

export async function AddToRooms(uid, friendlyIDs) {
    const url = combinePath(BaseURL, `/user/AddToRooms?uid=${uid}`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            friendlyIDs: friendlyIDs,
            type: 551
        })
    });
    //console.log("AddToRooms", res);
    if (!res.ok) throw new Error("Failed to add rooms");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در اتصال اتاق‌ها");
    return json;
}

export async function GetAllRooms(username="", q="") {
    const url = combinePath(BaseURL, `/admin/GetAllRooms?username=${username}&q=${q}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    //console.log("GetAllRooms", res)
    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}
export async function fetchRoomLiveAdmin() {
    const res = await fetch(combinePath(BaseURL, `/admin/GetLives?q=`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}


export async function GetAllRoomsMini(username="", q="") {
    const url = combinePath(BaseURL, `/admin/GetAllRoomsMini?username=${username}&q=${q}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    //console.log("GetAllRooms", res)
    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}

// Start of hooks

// Get all webhooks list
export async function GetWebhooksList(meetingID = "") {
    const url = combinePath(BaseURL, `/bbb/hooks/list${meetingID ? `?meetingID=${meetingID}` : ''}`);
    
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to fetch webhooks");
    const json = await res.json();
    return json; // returns { hooks: [...] }
}

// Get webhook info by ID
export async function GetWebhookInfo(hookID) {
    const url = combinePath(BaseURL, `/bbb/hooks/info/${hookID}`);
    
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to fetch webhook info");
    const json = await res.json();
    return json;
}

// Create new webhook
// Add this function to your AdminAPI.js file

// Create webhook with specific events (send null for all events)
export async function CreateWebhookWithEvents(callbackURL, eventIDs, meetingID = null) {
    let url = combinePath(BaseURL, `/bbb/hooks/create?callbackURL=${encodeURIComponent(callbackURL)}`);
    if (meetingID) url += `&meetingID=${encodeURIComponent(meetingID)}`;
    // فقط اگر eventIDs وجود داشته باشه و آرایه خالی نباشه، اضافه کن
    if (eventIDs && eventIDs.length > 0) {
        url += `&eventIDs=${encodeURIComponent(eventIDs.join(','))}`;
    }
    // اگر eventIDs === null یا آرایه خالی، پارامتر eventIDs ارسال نمیشه
    // که به معنی "همه رویدادها" هست
    
    const res = await fetch(url, {
        method: "POST",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to create webhook");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد هوک");
    return json;
}   

// Create webhook for all events
export async function CreateWebhookAllEvents(callbackURL, meetingID = null) {
    let url = combinePath(BaseURL, `/bbb/hooks/create-all?callbackURL=${encodeURIComponent(callbackURL)}`);
    if (meetingID) url += `&meetingID=${encodeURIComponent(meetingID)}`;
    
    const res = await fetch(url, {
        method: "POST",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to create webhook");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد هوک");
    return json;
}

// Create webhook for recording events
export async function CreateWebhookRecording(callbackURL, meetingID = null) {
    let url = combinePath(BaseURL, `/bbb/hooks/create-recording?callbackURL=${encodeURIComponent(callbackURL)}`);
    if (meetingID) url += `&meetingID=${encodeURIComponent(meetingID)}`;
    
    const res = await fetch(url, {
        method: "POST",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to create webhook");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد هوک");
    return json;
}

// Create webhook for participant events
export async function CreateWebhookParticipant(callbackURL, meetingID = null) {
    let url = combinePath(BaseURL, `/bbb/hooks/create-participant?callbackURL=${encodeURIComponent(callbackURL)}`);
    if (meetingID) url += `&meetingID=${encodeURIComponent(meetingID)}`;
    
    const res = await fetch(url, {
        method: "POST",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to create webhook");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد هوک");
    return json;
}

// Destroy webhook
export async function DestroyWebhook(hookID) {
    const url = combinePath(BaseURL, `/bbb/hooks/destroy/${hookID}`);
    
    const res = await fetch(url, {
        method: "DELETE",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to destroy webhook");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در حذف هوک");
    return json;
}

// Destroy all webhooks for a meeting
export async function DestroyAllWebhooksForMeeting(meetingID) {
    const url = combinePath(BaseURL, `/bbb/hooks/destroy-all/${meetingID}`);
    
    const res = await fetch(url, {
        method: "DELETE",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to destroy webhooks");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در حذف هوک‌ها");
    return json;
}

// Update webhook callback URL
export async function UpdateWebhookCallbackURL(hookID, newCallbackURL) {
    const url = combinePath(BaseURL, `/bbb/hooks/update/${hookID}?newCallbackURL=${encodeURIComponent(newCallbackURL)}`);
    
    const res = await fetch(url, {
        method: "PUT",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to update webhook");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در بروزرسانی هوک");
    return json;
}



export async function GetPollReport(meetID, qid = null, userid = null) {
    let url = `${BaseURL}admin/poll/GetPollReport?MeetID=${meetID}`;
    
    if (qid && qid !== null) {
        url += `&QID=${qid}`;
    }
    
    if (userid && userid !== null) {
        url += `&UserID=${userid}`;
    }
    
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to fetch poll report");
    const json = await res.json();
    return json; 
}

export async function EditRoom(friendlyID, roomTitle, logoFile = null, najmIDBoss = null) {
    const url = combinePath(
        BaseURL,
        `/user/EditRoom?FriendlyID=${encodeURIComponent(friendlyID)}&RoomTitle=${encodeURIComponent(roomTitle)}`
    );

    const formData = new FormData();
    if (logoFile) {
        formData.append("LogoFile", logoFile);
    } 
    formData.append("FriendlyID", friendlyID);
    formData.append("RoomTitle", roomTitle);
    if (najmIDBoss !== null && najmIDBoss !== undefined) {
        formData.append("NajmIDBoss", najmIDBoss);
    }

    const res = await fetch(url, {
        method: "POST",
        headers: {
            "accept": "*/*",
            "vc2AuthNorm": Cookies.get("vc2AuthNorm") || ""
        },
        body: formData
    });

    if (!res.ok) {
        throw new Error(`Failed to edit room: ${res.status}`);
    }

    const json = await res.json();
    return json;
}

export async function GetNajmBosses(q = "") {
    try {
        const url = combinePath(BaseURL, `/room/GetNajmBosses?q=${encodeURIComponent(q || "")}`);
        const res = await fetch(url, {
            method: "GET",
            headers: {
                "accept": "application/json",
                "vc2AuthNorm": Cookies.get("vc2AuthNorm") || ""
            }
        });
        if (res.ok) {
            const data = await res.json();
            return data;
        }
    } catch (e) {
        console.warn("Backend GetNajmBosses fetch failed, trying direct LAN endpoint:", e);
    }

    // Direct LAN fallback
    try {
        const tokenRes = await fetch("http://192.168.0.4:1940/api/token/generate?minutes=1");
        const tokenData = await tokenRes.json();
        if (tokenData?.token) {
            const bossesRes = await fetch("http://192.168.0.4:1940/api/groups/bosses", {
                headers: {
                    "X-Api-Token": tokenData.token
                }
            });
            const bossesData = await bossesRes.json();
            return {
                status: 200,
                data: (bossesData?.Data || []).map(b => ({
                    najmId: String(b.NajmId || ""),
                    fullName: b.FullName || "",
                    firstName: b.FirstName || "",
                    lastname: b.Lastname || "",
                    nationalCode: b.NationalCode || "",
                    bbbID: b.bbbID || ""
                }))
            };
        }
    } catch (err) {
        console.error("Direct LAN Najm fetch failed:", err);
    }

    return { status: 500, data: [], message: "خطا در دریافت لیست مسئولین نجم" };
}

export async function SetRoomNajmBoss(friendlyID, najmId) {
    const url = combinePath(BaseURL, `/room/SetRoomNajmBoss`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "accept": "application/json",
            "vc2AuthNorm": Cookies.get("vc2AuthNorm") || ""
        },
        body: JSON.stringify({
            friendlyID: friendlyID,
            najmId: najmId ? String(najmId) : null
        })
    });

    const json = await res.json().catch(() => ({}));
    if (!res.ok) {
        throw new Error(json?.message || `خطا در تنظیم مسئول اتاق: ${res.status}`);
    }
    return json;
}

export async function EditUserInfo(userData) {
    const url = combinePath(BaseURL, `/user/EditUserInfo`);

    const formData = new FormData();
    formData.append("UserId", userData.userId || userData.id);
    if (userData.firstname !== undefined && userData.firstname !== null) formData.append("Firstname", userData.firstname);
    if (userData.lastname !== undefined && userData.lastname !== null) formData.append("LastName", userData.lastname);
    if (userData.isHekmatShobe !== undefined) formData.append("IsHekmatShobe", userData.isHekmatShobe);
    if (userData.isMemberVIP !== undefined) formData.append("IsMemberVIP", userData.isMemberVIP);
    if (userData.status !== undefined && userData.status !== null) formData.append("Status", userData.status);
    if (userData.avatarFile) {
        formData.append("AvatarFile", userData.avatarFile);
    } else if (userData.avatarPath !== undefined && userData.avatarPath !== null) {
        formData.append("AvatarPath", userData.avatarPath);
    }

    const res = await fetch(url, {
        method: "POST",
        headers: {
            "accept": "*/*",
            "vc2AuthNorm": Cookies.get("vc2AuthNorm") || ""
        },
        body: formData
    });

    if (!res.ok) {
        const errorText = await res.text();
        throw new Error(`Failed to edit user info: ${res.status} ${errorText}`);
    }

    return await res.json();
}

export async function GetAllMeeting(page = 1, count = 20, q = "", username = "") {
    let url = combinePath(BaseURL, `/admin/GetAllMeeting?page=${page}&count=${count}`);
    if (q) url += `&q=${encodeURIComponent(q)}`;
    if (username) url += `&username=${encodeURIComponent(username)}`;

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) {
        const errorText = await res.text();
        throw new Error(`Failed to fetch meetings: ${res.status} ${errorText}`);
    }

    return await res.json();
}

export async function GetAdminMeetings(page = 1, count = 15, q = "", type = null, ownerId = null, teacherId = null) {
    let url = combinePath(BaseURL, `/admin/GetAdminMeetings?page=${page}&count=${count}`);
    if (q) url += `&q=${encodeURIComponent(q)}`;
    if (type !== null && type !== undefined) url += `&type=${type}`;
    if (ownerId) url += `&ownerId=${ownerId}`;
    if (teacherId) url += `&teacherId=${teacherId}`;

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) {
        const errorText = await res.text();
        throw new Error(`Failed to fetch admin meetings: ${res.status} ${errorText}`);
    }
    const json = await res.json();
    return json.data;
}

export async function GetMeetingStats(type = null, q = "", ownerId = null, teacherId = null) {
    let url = combinePath(BaseURL, `/admin/GetMeetingStats?`);
    if (q) url += `&q=${encodeURIComponent(q)}`;
    if (type !== null && type !== undefined) url += `&type=${type}`;
    if (ownerId) url += `&ownerId=${ownerId}`;
    if (teacherId) url += `&teacherId=${teacherId}`;

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) {
        return null;
    }
    const json = await res.json();
    return json.data;
}

export async function UpdateMeetingMedia(data) {
    const url = combinePath(BaseURL, `/admin/UpdateMeetingMedia`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });

    if (!res.ok) {
        const errorText = await res.text();
        throw new Error(`Failed to update meeting media: ${res.status} ${errorText}`);
    }

    return await res.json();
}

export async function SaveMeetingMediaLog(data) {
    const url = combinePath(BaseURL, `/admin/SaveMeetingMediaLog`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });

    if (!res.ok) {
        const errorText = await res.text();
        throw new Error(`Failed to save media log: ${res.status} ${errorText}`);
    }

    return await res.json();
}

export async function GetMeetingMediaLogs(meetingId) {
    const url = combinePath(BaseURL, `/admin/GetMeetingMediaLogs/${meetingId}`);
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) {
        const errorText = await res.text();
        throw new Error(`Failed to fetch media logs: ${res.status} ${errorText}`);
    }

    return await res.json();
}

export async function DeleteMeetingMediaLog(logId) {
    const url = combinePath(BaseURL, `/admin/DeleteMeetingMediaLog/${logId}`);
    const res = await fetch(url, {
        method: "DELETE",
        headers: NormalHeaderAPI
    });

    if (!res.ok) {
        const errorText = await res.text();
        throw new Error(`Failed to delete media log: ${res.status} ${errorText}`);
    }

    return await res.json();
}

export async function GetRoomFullDetails(friendlyId) {
    const url = combinePath(BaseURL, `/room/GetRoomFullDetails/${friendlyId}`);
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch room full details");
    return await res.json();
}

export async function EditRoomFull(data) {
    const url = combinePath(BaseURL, `/room/EditRoomFull`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error("Failed to edit room");
    return await res.json();
}

export async function GetRoomMetadata(friendlyId, key) {
    const url = combinePath(BaseURL, `/room/GetRoomMetadata?friendlyId=${friendlyId}&key=${key}`);
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch room metadata");
    return await res.json();
}

export async function SetRoomMetadata(friendlyId, key, value, dataType = 141) {
    const url = combinePath(BaseURL, `/room/SetRoomMetadata`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ friendlyId, key, value, dataType })
    });
    if (!res.ok) throw new Error("Failed to set room metadata");
    return await res.json();
}

export async function GetRoomWaitingMembers(roomId) {
    const url = combinePath(BaseURL, `/admin/GetRoomWaitingMembers?roomId=${roomId}`);
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to get waiting members");
    return await res.json();
}

export async function ChangeRoomMemberStatus(roomId, userId, status) {
    const url = combinePath(BaseURL, `/admin/ChangeRoomMemberStatus`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ roomId, userId, status })
    });
    if (!res.ok) throw new Error("Failed to change status");
    return await res.json();
}

export async function GetParticipants(page = 1, count = 10, q = "") {
    const params = new URLSearchParams();
    if (page) params.append("page", page);
    if (count) params.append("count", count);
    if (q) params.append("q", q);

    const url = combinePath(BaseURL, `/admin/GetParticipants?${params.toString()}`);
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch participants");
    const json = await res.json();
    return json.data;
}

export async function GetParticipantMeetings(userId, q = "") {
    const params = new URLSearchParams();
    if (userId) params.append("userId", userId);
    if (q) params.append("q", q);

    const url = combinePath(BaseURL, `/admin/GetParticipantMeetings?${params.toString()}`);
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch participant meetings");
    const json = await res.json();
    return json.data;
}

export async function GetCacheDetails() {
    const url = combinePath(BaseURL, `/admin/cache/details`);
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch cache details");
    const json = await res.json();
    return json;
}

export async function ClearCache(keys = null) {
    const url = combinePath(BaseURL, `/admin/cache/clear`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ keys: keys && keys.length > 0 ? keys : null })
    });
    if (!res.ok) throw new Error("Failed to clear cache");
    const json = await res.json();
    return json;
}

export async function ReCache(keys = null) {
    const url = combinePath(BaseURL, `/admin/cache/recache`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ keys: keys && keys.length > 0 ? keys : null })
    });
    if (!res.ok) throw new Error("Failed to re-cache");
    const json = await res.json();
    return json;
}



export async function GetMeetingRecordings(meetingId) {
    const url = combinePath(BaseURL, `/admin/meetings/${meetingId}/recordings`);
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch meeting recordings");
    const json = await res.json();
    return json;
}

export async function SaveRecordingSettings(payload) {
    const url = combinePath(BaseURL, `/admin/meetings/recordings/settings`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
    });
    if (!res.ok) throw new Error("Failed to save recording settings");
    const json = await res.json();
    return json;
}

export async function PublishRecording(payload) {
    const url = combinePath(BaseURL, `/admin/meetings/recordings/publish`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
    });
    if (!res.ok) throw new Error("Failed to change recording publish status");
    const json = await res.json();
    return json;
}

export async function DeleteRecording(payload) {
    const url = combinePath(BaseURL, `/admin/meetings/recordings/delete`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
    });
    if (!res.ok) throw new Error("Failed to delete recording");
    const json = await res.json();
    return json;
}

export async function GetRecordingTracks(recordId) {
    const url = combinePath(BaseURL, `/admin/meetings/recordings/tracks?recordId=${encodeURIComponent(recordId)}`);
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch recording text tracks");
    const json = await res.json();
    return json;
}
