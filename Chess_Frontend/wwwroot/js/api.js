const API_BASE = `${window.location.protocol}//${window.location.hostname}:5245`;
const TOKEN_KEY = "bubs-chess-token";
export class ApiError extends Error {
    constructor(message, status) {
        super(message);
        this.status = status;
    }
}
export function getToken() {
    return sessionStorage.getItem(TOKEN_KEY);
}
export function setToken(token) {
    if (token) {
        sessionStorage.setItem(TOKEN_KEY, token);
    }
    else {
        sessionStorage.removeItem(TOKEN_KEY);
    }
}
async function readBody(response) {
    const text = await response.text();
    if (!text) {
        return null;
    }
    try {
        return JSON.parse(text);
    }
    catch {
        return text;
    }
}
function messageFrom(body, fallback) {
    if (typeof body === "string" && body.trim()) {
        return body;
    }
    return fallback;
}
let requestQueue = Promise.resolve();
function enqueue(job) {
    const result = requestQueue.then(job, job);
    requestQueue = result.then(() => undefined, () => undefined);
    return result;
}
function headers(json) {
    const result = {};
    const token = getToken();
    if (token) {
        result.Authorization = `Bearer ${token}`;
    }
    if (json) {
        result["Content-Type"] = "application/json";
    }
    return result;
}
async function request(path, init, json = false) {
    return enqueue(() => send(path, init, json));
}
async function send(path, init, json) {
    let response;
    try {
        response = await fetch(`${API_BASE}${path}`, {
            ...init,
            headers: { ...headers(json), ...init?.headers },
        });
    }
    catch {
        throw new ApiError(`Cannot reach the chess server at ${API_BASE}. Start Chess_Backend, then try again.`, 0);
    }
    const body = await readBody(response);
    if (!response.ok) {
        throw new ApiError(messageFrom(body, response.statusText), response.status);
    }
    return body;
}
export function login(username, password) {
    return request("/api/auth/login", { method: "POST", body: JSON.stringify({ username, password }) }, true);
}
export function logout() {
    return request("/api/auth/logout", { method: "POST" });
}
export function currentUser() {
    return request("/api/auth/me");
}
export function playFriend() {
    return request("/api/game/play/friend", { method: "POST" });
}
export function playComputer() {
    return request("/api/game/play/computer", { method: "POST" });
}
export function getState() {
    return request("/api/game/state");
}
export function startGame() {
    return request("/api/game/start", { method: "POST" });
}
export function resign() {
    return request("/api/game/resign", { method: "POST" });
}
export function makeMove(move) {
    return request("/api/move/make", { method: "POST", body: JSON.stringify(move) }, true);
}
export async function validateMove(from, to) {
    const body = await request("/api/move/validate", {
        method: "POST",
        body: JSON.stringify({
            from,
            to,
            isEnPassant: false,
            isCastle: false,
            isCapture: false,
        }),
    }, true);
    return body === true;
}
