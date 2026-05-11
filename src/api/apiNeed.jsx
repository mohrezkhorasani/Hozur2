import Cookies from 'js-cookie';

export const BaseURL = "https://core.hekmateislami.com/"
// export const BaseURL = "https://localhost:7087/"
export const BaseURLFiles = "https://vcfile.hekmateislami.com/"

export const NormalHeaderAPI = {
    "accept": "*/*",
    "vc2AuthNorm": Cookies.get("vc2AuthNorm")
}