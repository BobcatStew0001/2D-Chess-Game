import type { GameStateDto, LoginResult, MoveDto, Position } from "./types.js";

const API_BASE = `${window.location.protocol}//${window.location.hostname}:5245`;
const TOKEN_KEY = "bubs-chess-token";

export class ApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

export function getToken(): string | null {
  return sessionStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string | null): void {
  if (token) {
    sessionStorage.setItem(TOKEN_KEY, token);
  } else {
    sessionStorage.removeItem(TOKEN_KEY);
  }
}

async function readBody(response: Response): Promise<unknown> {
  const text = await response.text();
  if (!text) {
    return null;
  }

  try {
    return JSON.parse(text) as unknown;
  } catch {
    return text;
  }
}

function messageFrom(body: unknown, fallback: string): string {
  if (typeof body === "string" && body.trim()) {
    return body;
  }
  return fallback;
}

let requestQueue: Promise<void> = Promise.resolve();

function enqueue<T>(job: () => Promise<T>): Promise<T> {
  const result = requestQueue.then(job, job);
  requestQueue = result.then(
    () => undefined,
    () => undefined
  );
  return result;
}

function headers(json: boolean): HeadersInit {
  const result: Record<string, string> = {};
  const token = getToken();
  if (token) {
    result.Authorization = `Bearer ${token}`;
  }
  if (json) {
    result["Content-Type"] = "application/json";
  }
  return result;
}

async function request(path: string, init?: RequestInit, json = false): Promise<unknown> {
  return enqueue(() => send(path, init, json));
}

async function send(path: string, init: RequestInit | undefined, json: boolean): Promise<unknown> {
  let response: Response;
  try {
    response = await fetch(`${API_BASE}${path}`, {
      ...init,
      headers: { ...headers(json), ...init?.headers },
    });
  } catch {
    throw new ApiError(
      `Cannot reach the chess server at ${API_BASE}. Start Chess_Backend, then try again.`,
      0
    );
  }

  const body = await readBody(response);
  if (!response.ok) {
    throw new ApiError(messageFrom(body, response.statusText), response.status);
  }

  return body;
}

export function login(username: string, password: string): Promise<LoginResult> {
  return request(
    "/api/auth/login",
    { method: "POST", body: JSON.stringify({ username, password }) },
    true
  ) as Promise<LoginResult>;
}

export function logout(): Promise<unknown> {
  return request("/api/auth/logout", { method: "POST" });
}

export function currentUser(): Promise<{ username: string }> {
  return request("/api/auth/me") as Promise<{ username: string }>;
}

export function playFriend(): Promise<GameStateDto> {
  return request("/api/game/play/friend", { method: "POST" }) as Promise<GameStateDto>;
}

export function playComputer(): Promise<GameStateDto> {
  return request("/api/game/play/computer", { method: "POST" }) as Promise<GameStateDto>;
}

export function getState(): Promise<GameStateDto> {
  return request("/api/game/state") as Promise<GameStateDto>;
}

export function startGame(): Promise<GameStateDto> {
  return request("/api/game/start", { method: "POST" }) as Promise<GameStateDto>;
}

export function resign(): Promise<GameStateDto> {
  return request("/api/game/resign", { method: "POST" }) as Promise<GameStateDto>;
}

export function makeMove(move: MoveDto): Promise<GameStateDto> {
  return request(
    "/api/move/make",
    { method: "POST", body: JSON.stringify(move) },
    true
  ) as Promise<GameStateDto>;
}

export async function validateMove(from: Position, to: Position): Promise<boolean> {
  const body = await request(
    "/api/move/validate",
    {
      method: "POST",
      body: JSON.stringify({
        from,
        to,
        isEnPassant: false,
        isCastle: false,
        isCapture: false,
      }),
    },
    true
  );

  return body === true;
}
