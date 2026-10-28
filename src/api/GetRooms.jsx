import Cookies from 'js-cookie';
import { BaseURL, NormalHeaderAPI } from './apiNeed';
import { combinePath } from '../Tools/tools';

export async function fetchRooms(uname) {
    const res = await fetch(combinePath(BaseURL, `/user/GetRooms?${uname ? `uname=${uname}` : ""}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}
export async function fetchRoomsPublic(uname = null) {
    const res = await fetch(combinePath(BaseURL, `/user/GetRoomsPublic?username=${uname == null ? "" : uname}`), {
        method: "GET",
        headers: NormalHeaderAPI,

    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}

export async function GetRoomCalendarActivity(friendlyID, month = "", year = 1405, signal = null) {
    const res = await fetch(combinePath(BaseURL, `/user/GetRoomCalendarActivity?${friendlyID ? `friendlyID=${friendlyID}&` : ""}month=${month}&year=${year}`), {
        method: "GET",
        headers: NormalHeaderAPI,
        signal: signal
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}


export async function GetMembersOfMeet(meetID) {
    const res = await fetch(combinePath(BaseURL, `/user/GetMembersOfMeet?${meetID ? `idMeet=${meetID}` : ""}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.members; // چون ساختار ریسپانس تو همین شکله
}

export async function GetMembersOfMeetOnlyCertificated(meetID) {
    const res = await fetch(combinePath(BaseURL, `/room/GetMembersOfMeetOnlyCertificated/${meetID}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.certificates; // چون ساختار ریسپانس تو همین شکله
}
export async function GetLatestMeetings(uname = null) {
    const res = await fetch(combinePath(BaseURL, `/user/GetLatestMeetings`), {
        method: "GET",
        headers: NormalHeaderAPI,

    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}

export async function GenerateCertificateWithUserMeetID(uid, options = {}) {
    if (typeof options === 'object' && Object.keys(options).length > 0) {
        const res = await fetch(combinePath(BaseURL, `/room/GenerateCertificateWithUserMeetID`), {
            method: "POST",
            headers: {
                ...NormalHeaderAPI,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                id: uid || 0,
                nationalCode: options.nationalCode || "",
                phone: options.phone || "",
                isMale: options.isMale !== undefined ? options.isMale : true,
                meetID: options.meetID || "",
                name: options.name || ""
            })
        });

        if (!res.ok) {
            let errMsg = "خطا در صدور گواهینامه";
            try {
                const errJson = await res.json();
                if (errJson && errJson.message) errMsg = errJson.message;
            } catch (e) { }
            throw new Error(errMsg);
        }

        const json = await res.json();
        return json.link;
    }

    const res = await fetch(combinePath(BaseURL, `/room/GenerateCertificateWithUserMeetID/${uid}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) {
        let errMsg = "خطا در دریافت گواهینامه";
        try {
            const errJson = await res.json();
            if (errJson && errJson.message) errMsg = errJson.message;
        } catch (e) { }
        throw new Error(errMsg);
    }

    const json = await res.json();
    return json.link;
}

export async function fetchRoom(friendlyID) {
    const res = await fetch(combinePath(BaseURL, `/user/GetRoomByFriendlyID?q=${friendlyID}`), {
        method: "GET",
        headers: NormalHeaderAPI,

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return {
        ... json.data,
        subscriptionStatus: json.subscriptionStatus,
        isSubscribedByUser: json.isSubscribedByUser,

    }; // چون ساختار ریسپانس تو همین شکله
}
export async function fetchRoomLive() {
    const res = await fetch(combinePath(BaseURL, `/user/GetLives?q=`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}

export async function GetReservation(pid) {
    const res = await fetch(combinePath(BaseURL, `/user/GetReservation/${pid}`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}

export async function NewReservation(pid, nick, phone, na) {
    const url = combinePath(BaseURL, `/user/NewReservation/${pid}/${nick}?phone=${phone}&na=${na}`);

    const res = await fetch(url, {
        method: "POST",
        headers: {
            "accept": "*/*",
            "Content-Type": "application/json",
            "vc2AuthNorm": Cookies.get("vc2AuthNorm")

        },

    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to login");
    let json = await res.json()
    if (json.status != 200)
        throw new Error(json.message)
    return json;
}
export async function fetchRoomLivesPublic() {
    const res = await fetch(combinePath(BaseURL, `/user/GetLivesPublic?q=`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}
export async function fetchRoomLivesPublicWithID(roomID) {
    const res = await fetch(combinePath(BaseURL, `/user/GetLiveWithID?idRoom=${roomID}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}
export async function getProfile(uname) {
    const res = await fetch(combinePath(BaseURL, `/user/GetProfileInfo?username=${uname}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}


export async function getSlideshows(page = null) {
    const res = await fetch(combinePath(BaseURL, `/user/GetSlideShow?${page == null ? `` : `page=${page}`}`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}
// https://core.hekmateislami.com/user/GetHekmatShobe
export async function GetHekmatShobe() {
    const res = await fetch(combinePath(BaseURL, `/user/GetHekmatShobe`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}

export async function GetPlanRoomPublic(page, count, order = 100) {
    const url = combinePath(BaseURL, `/user/GetPlanRoomPublic?page=${page}&count=${count}&o=${order}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    //console.log("getplans", res)
    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}


export async function getEmojis(name) {
    const res = await fetch(combinePath(BaseURL, `/user/GetListRoomIcon?path=${name}`), {
        method: "GET",
        headers: NormalHeaderAPI

    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json; // چون ساختار ریسپانس تو همین شکله
}

export async function GetTeacherList(name) {
    const res = await fetch(combinePath(BaseURL, `/user/GetTeacherList?q=${name}`), {
        method: "GET",
        headers: NormalHeaderAPI

    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}




export async function addNewPayment(data) {
    const url = combinePath(BaseURL, `/task/addNewPayment/`);

    const res = await fetch(url, {
        method: "POST",
        headers: {
            "accept": "*/*",
            "Content-Type": "application/json",
            "vc2AuthNorm": Cookies.get("vc2AuthNorm")

        },
        body: JSON.stringify(data)
    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to login");
    let json = await res.json()
    if (json.status != 200)
        throw new Error(json.message)
    return json;
}

export async function GetPaymentList() {
    const res = await fetch(combinePath(BaseURL, `/task/GetPaymentList`), {
        method: "POST",
        headers: NormalHeaderAPI
    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    //console.log(json)

    return json.data; // چون ساختار ریسپانس تو همین شکله
}

export async function ChangeStatusOfPayment(data) {
    // Build a proper headers object
    const headers = {
        ...NormalHeaderAPI,          // NormalHeaderAPI should be an object
        "vc2AuthNorm": Cookies.get("vc2AuthNorm") || "", // fallback if cookie missing
    };

    // Ensure Content-Type is set (if not already included)
    if (!headers["Content-Type"]) {
        headers["Content-Type"] = "application/json";
    }

    const res = await fetch(combinePath(BaseURL, `/task/ChangeStatusOfPayment`), {
        method: "POST",
        headers,
        body: JSON.stringify(data),
    });

    if (!res.ok) throw new Error(`Failed to update payment status: ${res.status}`);

    const json = await res.json();
    return json.data;
}

export async function GetPDFOfPayment(id) {
    const res = await fetch(combinePath(BaseURL, `/task/GetPDFOfPayment?id=${id}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    //console.log(json)

    return json.path; // چون ساختار ریسپانس تو همین شکله
}
export async function GetUserMeetLogs(umid, page, count) {
    const res = await fetch(combinePath(BaseURL, `/usermeet/GetUserMeetLogs?umid=${umid}&page=${page}&count=${count}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    //console.log(json)

    return json; // چون ساختار ریسپانس تو همین شکله
}

export async function GetTopTeachers(fid) {
    const res = await fetch(combinePath(BaseURL, `/room/GetTopTeachers?fid=${fid}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch top teachers");
    const json = await res.json();
    return json.data;
}

export async function GetMeetingDetail(fid) {
    const res = await fetch(combinePath(BaseURL, `/room/GetMeetingDetail?fid=${fid}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch meeting detail");
    const json = await res.json();
    return json.data;
}

export async function GetRoomDetails(fid) {
    const res = await fetch(combinePath(BaseURL, `/room/getDetails?fid=${fid}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch room details");
    const json = await res.json();
    console.log(json)
    return {
        ...json.data
    };
}
export async function GetRoomMeetingHistory(fid, page = 1, count = 10) {
    const res = await fetch(combinePath(BaseURL, `/room/GetRoomMeetingHistory?fid=${fid}&page=${page}&count=${count}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch room meeting history");
    const json = await res.json();
    return json.data;
}

export async function GetUpcomingPlans(fid) {
    const res = await fetch(combinePath(BaseURL, `/room/GetUpcomingPlans?fid=${fid}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });
    if (!res.ok) throw new Error("Failed to fetch upcoming plans");
    const json = await res.json();
    return json.data;
}