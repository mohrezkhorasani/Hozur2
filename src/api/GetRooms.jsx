import Cookies from 'js-cookie';
import { BaseURL, NormalHeaderAPI } from './apiNeed';
import { combinePath } from '../Tools/tools';

export async function fetchRooms(uname) {
    const res = await fetch(combinePath(BaseURL,`/user/GetRooms?${uname?`uname=${uname}`:""}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}
export async function fetchRoomsPublic(uname =null) {
    const res = await fetch(combinePath(BaseURL,`/user/GetRoomsPublic?username=${uname==null?"":uname}`), {
        method: "GET",
        headers: NormalHeaderAPI,

    });
    console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}

export async function GetRoomCalendarActivity(friendlyID) {
    const res = await fetch(combinePath(BaseURL,`/user/GetRoomCalendarActivity?${friendlyID?`friendlyID=${friendlyID}`:""}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}


export async function GetMembersOfMeet(meetID) {
    const res = await fetch(combinePath(BaseURL,`/user/GetMembersOfMeet?${meetID?`idMeet=${meetID}`:""}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.members; // چون ساختار ریسپانس تو همین شکله
}

export async function GetLatestMeetings(uname =null) {
    const res = await fetch(combinePath(BaseURL,`/user/GetLatestMeetings`), {
        method: "GET",
        headers: NormalHeaderAPI,

    });
    console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}

export async function fetchRoom(friendlyID) {
    const res = await fetch(combinePath(BaseURL,`/user/GetRoomByFriendlyID?q=${friendlyID}`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}
export async function fetchRoomLive() {
    const res = await fetch(combinePath(BaseURL,`/user/GetLives?q=`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}
export async function fetchRoomLivesPublic() {
    const res = await fetch(combinePath(BaseURL,`/user/GetLivesPublic?q=`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}
export async function fetchRoomLivesPublicWithID(roomID) {
    const res = await fetch(combinePath(BaseURL,`/user/GetLiveWithID?idRoom=${roomID}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}
export async function getProfile(uname) {
    const res = await fetch(combinePath(BaseURL,`/user/GetProfileInfo?username=${uname}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}


export async function getSlideshows(page =null) {
    const res = await fetch(combinePath(BaseURL,`/user/GetSlideShow?${page==null?``:`page=${page}`}`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}
// https://core.hekmateislami.com/user/GetHekmatShobe
export async function GetHekmatShobe() {
    const res = await fetch(combinePath(BaseURL,`/user/GetHekmatShobe`), {
        method: "GET",
        headers: NormalHeaderAPI

    });

    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}

export async function GetPlanRoomPublic( page, count, order = 100) {
    const url = combinePath(BaseURL, `/user/GetPlanRoomPublic?page=${page}&count=${count}&o=${order}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    console.log("getplans",res)
    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}


export async function getEmojis(name) {
    const res = await fetch(combinePath(BaseURL,`/user/GetListRoomIcon?path=${name}`), {
        method: "GET",
        headers: NormalHeaderAPI

    });
    console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json; // چون ساختار ریسپانس تو همین شکله
}

export async function GetTeacherList(name) {
    const res = await fetch(combinePath(BaseURL,`/user/GetTeacherList?q=${name}`), {
        method: "GET",
        headers: NormalHeaderAPI

    });
    console.log(res)
    if (!res.ok) throw new Error("Failed to fetch rooms");

    const json = await res.json();
    return json.data; // چون ساختار ریسپانس تو همین شکله
}