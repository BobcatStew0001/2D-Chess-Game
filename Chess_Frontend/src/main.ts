import * as api from "./api.js";
import { ApiError } from "./api.js";
import { readSquare, renderBoard } from "./board.js";
import {
  pieceAt,
  sameSquare,
  squareName,
  type CurrentState,
  type GameStateDto,
  type MoveDto,
  type PieceColor,
  type PieceType,
  type Position,
} from "./types.js";

const gate = document.querySelector<HTMLElement>("#gate");
const loginForm = document.querySelector<HTMLFormElement>("#login-form");
const loginMessage = document.querySelector<HTMLElement>("#login-message");
const usernameInput = document.querySelector<HTMLInputElement>("#username");
const passwordInput = document.querySelector<HTMLInputElement>("#password");
const lobby = document.querySelector<HTMLElement>("#lobby");
const welcome = document.querySelector<HTMLElement>("#welcome");
const shareAddress = document.querySelector<HTMLElement>("#share-address");
const playFriendButton = document.querySelector<HTMLButtonElement>("#play-friend");
const playComputerButton = document.querySelector<HTMLButtonElement>("#play-computer");
const logoutButton = document.querySelector<HTMLButtonElement>("#logout");

const gameScreen = document.querySelector<HTMLElement>("#game-screen");
const boardElement = document.querySelector<HTMLElement>("#board");
const statusElement = document.querySelector<HTMLElement>("#status");
const detailElement = document.querySelector<HTMLElement>("#detail");
const historyElement = document.querySelector<HTMLOListElement>("#history");
const messageElement = document.querySelector<HTMLElement>("#message");
const newGameButton = document.querySelector<HTMLButtonElement>("#new-game");
const resignButton = document.querySelector<HTMLButtonElement>("#resign");
const backButton = document.querySelector<HTMLButtonElement>("#back-to-menu");
const promotionDialog = document.querySelector<HTMLElement>("#promotion");

if (
  !gate ||
  !loginForm ||
  !loginMessage ||
  !usernameInput ||
  !passwordInput ||
  !lobby ||
  !welcome ||
  !shareAddress ||
  !playFriendButton ||
  !playComputerButton ||
  !logoutButton ||
  !gameScreen ||
  !boardElement ||
  !statusElement ||
  !detailElement ||
  !historyElement ||
  !messageElement ||
  !newGameButton ||
  !resignButton ||
  !backButton ||
  !promotionDialog
) {
  throw new Error("The chess page is missing part of its layout.");
}

const gatePanel: HTMLElement = gate;
const loginFormPanel: HTMLFormElement = loginForm;
const loginMessagePanel: HTMLElement = loginMessage;
const usernameField: HTMLInputElement = usernameInput;
const passwordField: HTMLInputElement = passwordInput;
const lobbyPanel: HTMLElement = lobby;
const welcomeText: HTMLElement = welcome;
const gamePanel: HTMLElement = gameScreen;
const promotionPanel: HTMLElement = promotionDialog;

const board: HTMLElement = boardElement;
const status: HTMLElement = statusElement;
const detail: HTMLElement = detailElement;
const history: HTMLOListElement = historyElement;
const message: HTMLElement = messageElement;

let game: GameStateDto | null = null;
let selected: Position | null = null;
let legalMoves: Position[] = [];
let scanId = 0;
let busy = false;
let pollTimer = 0;

shareAddress.textContent = window.location.origin;

if ("serviceWorker" in navigator) {
  window.addEventListener("load", () => {
    void navigator.serviceWorker.register("/sw.js");
  });
}

loginForm.addEventListener("submit", (event) => {
  event.preventDefault();
  void signIn();
});

logoutButton.addEventListener("click", () => {
  void signOut();
});

playFriendButton.addEventListener("click", () => {
  void enterGame("Opening the friend game...", () => api.playFriend());
});

playComputerButton.addEventListener("click", () => {
  void enterGame("Opening the computer game...", () => api.playComputer());
});

backButton.addEventListener("click", () => {
  stopPolling();
  clearSelection();
  game = null;
  showLobby();
});

newGameButton.addEventListener("click", () => {
  void runAction("Starting a new game...", () => api.startGame());
});

resignButton.addEventListener("click", () => {
  if (!game || !canMove(game.state)) {
    setMessage("This game is already over.");
    return;
  }

  void runAction("Sending the resignation...", () => api.resign());
});

board.addEventListener("click", (event) => {
  void onSquareClick(readSquare(event.target));
});

void restoreSession();

async function restoreSession(): Promise<void> {
  if (!api.getToken()) {
    showLogin();
    return;
  }

  try {
    const me = await api.currentUser();
    showLobby(me.username);
  } catch {
    api.setToken(null);
    showLogin();
  }
}

async function signIn(): Promise<void> {
  loginMessagePanel.textContent = "Checking the name and password...";
  try {
    const result = await api.login(usernameField.value, passwordField.value);
    api.setToken(result.token);
    passwordField.value = "";
    showLobby(result.username);
  } catch (error) {
    loginMessagePanel.textContent = error instanceof Error ? error.message : "Sign in failed.";
  }
}

async function signOut(): Promise<void> {
  try {
    await api.logout();
  } catch {
    // The local sign-in is cleared either way.
  }
  api.setToken(null);
  stopPolling();
  game = null;
  showLogin();
}

async function enterGame(pending: string, action: () => Promise<GameStateDto>): Promise<void> {
  welcomeText.textContent = pending;
  try {
    const next = await action();
    showGame(next);
  } catch (error) {
    handleAuth(error);
    welcomeText.textContent = error instanceof Error ? error.message : "Could not open the game.";
  }
}

async function onSquareClick(position: Position | null): Promise<void> {
  if (!position || !game || busy || !game.yourTurn || !game.yourColor) {
    return;
  }

  const piece = pieceAt(game.pieces, position);
  if (piece?.color === game.yourColor) {
    if (selected && sameSquare(selected, position)) {
      clearSelection();
      draw();
      return;
    }

    selected = position;
    legalMoves = [];
    draw();
    void highlightLegalMoves(position, ++scanId);
    return;
  }

  if (!selected) {
    setMessage("Select one of your pieces.");
    return;
  }

  const moving = pieceAt(game.pieces, selected);
  const promotion = moving?.type === "Pawn" && (position.row === 0 || position.row === 7)
    ? await choosePromotion(promotionPanel)
    : null;
  const move = buildMove(selected, position, promotion);
  const current = game;
  clearSelection();
  const next = await runAction("Sending the move...", async () => {
    try {
      return await api.makeMove(move);
    } catch (error) {
      if (error instanceof ApiError && error.status === 400) {
        return current;
      }
      throw error;
    }
  });

  if (next === current) {
    setMessage("That move is not legal.");
  }
}

async function highlightLegalMoves(from: Position, id: number): Promise<void> {
  const found: Position[] = [];
  for (const to of squaresAround(from)) {
    if (id !== scanId) {
      return;
    }

    try {
      if (await api.validateMove(from, to)) {
        found.push(to);
        if (id === scanId && selected && sameSquare(selected, from)) {
          legalMoves = [...found];
          draw();
        }
      }
    } catch (error) {
      handleAuth(error);
      return;
    }
  }
}

async function runAction(
  pendingMessage: string,
  action: () => Promise<GameStateDto | null>
): Promise<GameStateDto | null> {
  if (busy) {
    return null;
  }

  busy = true;
  setMessage(pendingMessage);
  setControlsEnabled(false);

  try {
    const next = await action();
    if (next && next !== game) {
      applyState(next);
    } else if (next) {
      applyState(next);
    }
    return next;
  } catch (error) {
    handleAuth(error);
    return null;
  } finally {
    busy = false;
    setControlsEnabled(true);
  }
}

async function poll(): Promise<void> {
  if (busy || !game) {
    return;
  }

  try {
    const next = await api.getState();
    if (signature(next) !== signature(game)) {
      clearSelection();
      applyState(next);
    }
  } catch (error) {
    handleAuth(error);
  }
}

function showLogin(): void {
  gatePanel.hidden = false;
  loginFormPanel.hidden = false;
  lobbyPanel.hidden = true;
  gamePanel.hidden = true;
  loginMessagePanel.textContent = "Sign in as Bub or Friend.";
}

function showLobby(username?: string): void {
  const name = username ?? welcomeText.dataset.username ?? "";
  welcomeText.dataset.username = name;
  welcomeText.textContent = `Signed in as ${name}.`;
  gatePanel.hidden = false;
  loginFormPanel.hidden = true;
  lobbyPanel.hidden = false;
  gamePanel.hidden = true;
}

function showGame(next: GameStateDto): void {
  gatePanel.hidden = true;
  gamePanel.hidden = false;
  applyState(next);
  startPolling();
}

function applyState(next: GameStateDto): void {
  game = next;
  if (!next.yourTurn) {
    clearSelection();
  }
  draw();
  setMessage(defaultHint(next));
}

function draw(): void {
  if (!game) {
    return;
  }

  renderBoard(board, game.pieces, selected, legalMoves);
  status.textContent = statusText(game);
  detail.textContent = detailText(game);
  history.replaceChildren();

  if (game.moveHistory.length === 0) {
    const item = document.createElement("li");
    item.className = "empty-history";
    item.textContent = "No moves yet.";
    history.append(item);
    return;
  }

  for (const move of game.moveHistory) {
    const item = document.createElement("li");
    item.textContent = `${squareName(move.from)} → ${squareName(move.to)}`;
    history.append(item);
  }
}

function buildMove(from: Position, to: Position, promotion: PieceType | null): MoveDto {
  const target = game ? pieceAt(game.pieces, to) : undefined;
  return {
    from,
    to,
    isEnPassant: false,
    isCastle: false,
    isCapture: Boolean(target),
    promotion,
  };
}

function choosePromotion(dialog: HTMLElement): Promise<PieceType> {
  dialog.hidden = false;
  return new Promise((resolve) => {
    const onClick = (event: Event) => {
      const button = event.target instanceof Element ? event.target.closest("button") : null;
      const piece = button?.dataset.piece;
      if (piece !== "Queen" && piece !== "Rook" && piece !== "Bishop" && piece !== "Knight") {
        return;
      }

      dialog.hidden = true;
      dialog.removeEventListener("click", onClick);
      resolve(piece);
    };

    dialog.addEventListener("click", onClick);
  });
}

function clearSelection(): void {
  selected = null;
  legalMoves = [];
  scanId++;
}

function canMove(state: CurrentState): boolean {
  return state === "InProgress" || state === "Check";
}

function statusText(current: GameStateDto): string {
  if (current.state === "Resigned" && current.resignedBy) {
    return `${current.resignedBy} resigned`;
  }

  switch (current.state) {
    case "Check":
      return `${current.whoseTurn} is in check`;
    case "Checkmate":
      return `Checkmate — ${opponent(current.whoseTurn)} wins`;
    case "Stalemate":
      return "Stalemate";
    case "Draw":
      return "Draw";
    case "Resigned":
      return "Resigned";
    default:
      return current.yourTurn ? "Your turn" : `${current.whoseTurn} to move`;
  }
}

function detailText(current: GameStateDto): string {
  const you = current.yourColor ? `You are ${current.you}, playing ${current.yourColor}.` : `You are ${current.you}.`;
  if (current.mode === "Friend" && !current.opponentJoined) {
    const other = current.you === "Bub" ? "Friend" : "Bub";
    return `${you} Waiting for ${other} to join from the other device.`;
  }

  if (current.mode === "Computer") {
    return `${you} You play White. The computer plays Black.`;
  }

  const white = current.whitePlayer ?? "White";
  const black = current.blackPlayer ?? "Black";
  return `${you} ${white} has White and ${black} has Black.`;
}

function defaultHint(current: GameStateDto): string {
  if (current.state === "Resigned" && current.resignedBy) {
    const winner = current.resignedBy === current.whitePlayer ? current.blackPlayer : current.whitePlayer;
    return `${winner ?? "The other player"} wins. The server kept this game.`;
  }
  if (!canMove(current.state)) {
    return "This game is over. Start a new game when you are both ready.";
  }
  if (!current.opponentJoined) {
    return "The board is saved. Moves open when the other player joins.";
  }
  if (!current.yourTurn) {
    return current.mode === "Computer" ? "The computer is moving." : "Waiting for the other player.";
  }
  return "Select a piece to see where it can move. The server saves every move.";
}

function opponent(color: PieceColor): PieceColor {
  return color === "White" ? "Black" : "White";
}

function signature(current: GameStateDto): string {
  const pieces = current.pieces.map((piece) => `${piece.color}${piece.type}${piece.row}${piece.column}`).join("|");
  return `${current.state}:${current.whoseTurn}:${current.moveHistory.length}:${pieces}:${current.opponentJoined}`;
}

function squaresAround(from: Position): Position[] {
  const squares: Position[] = [];
  for (let radius = 1; radius <= 7; radius++) {
    for (let rowOffset = -radius; rowOffset <= radius; rowOffset++) {
      for (let columnOffset = -radius; columnOffset <= radius; columnOffset++) {
        if (Math.max(Math.abs(rowOffset), Math.abs(columnOffset)) !== radius) {
          continue;
        }

        const row = from.row + rowOffset;
        const column = from.column + columnOffset;
        if (row >= 0 && row < 8 && column >= 0 && column < 8) {
          squares.push({ row, column });
        }
      }
    }
  }
  return squares;
}

function setMessage(text: string): void {
  message.textContent = text;
}

function handleAuth(error: unknown): void {
  if (error instanceof ApiError && error.status === 401) {
    api.setToken(null);
    stopPolling();
    showLogin();
    loginMessagePanel.textContent = error.message;
    return;
  }

  setMessage(error instanceof Error ? error.message : "Something went wrong.");
}

function setControlsEnabled(enabled: boolean): void {
  newGameButton!.disabled = !enabled;
  resignButton!.disabled = !enabled;
  backButton!.disabled = !enabled;
}

function startPolling(): void {
  stopPolling();
  pollTimer = window.setInterval(() => {
    void poll();
  }, 2000);
}

function stopPolling(): void {
  window.clearInterval(pollTimer);
  pollTimer = 0;
}
