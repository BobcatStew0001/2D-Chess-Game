export type PieceColor = "White" | "Black";

export type PieceType = "Pawn" | "Rook" | "Knight" | "Bishop" | "Queen" | "King";

export type CurrentState =
  | "InProgress"
  | "Checkmate"
  | "Stalemate"
  | "Check"
  | "Draw"
  | "Resigned";

export interface Position {
  row: number;
  column: number;
}

export interface PieceDto {
  color: PieceColor;
  type: PieceType;
  row: number;
  column: number;
}

export interface MoveDto {
  from: Position;
  to: Position;
  isEnPassant: boolean;
  isCastle: boolean;
  isCapture: boolean;
  promotion?: PieceType | null;
}

export type GameMode = "Friend" | "Computer";

export interface GameStateDto {
  whoseTurn: PieceColor;
  moveHistory: MoveDto[];
  state: CurrentState;
  pieces: PieceDto[];
  mode: GameMode;
  you: string;
  yourColor: PieceColor | null;
  yourTurn: boolean;
  opponentJoined: boolean;
  whitePlayer: string | null;
  blackPlayer: string | null;
  resignedBy: string | null;
}

export interface LoginResult {
  token: string;
  username: string;
}

export function sameSquare(a: Position, b: Position): boolean {
  return a.row === b.row && a.column === b.column;
}

export function squareName(position: Position): string {
  const file = "abcdefgh"[position.column] ?? "?";
  return file + (8 - position.row);
}

export function pieceAt(pieces: PieceDto[], position: Position): PieceDto | undefined {
  return pieces.find((piece) => piece.row === position.row && piece.column === position.column);
}
