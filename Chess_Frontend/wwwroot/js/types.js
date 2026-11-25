export function sameSquare(a, b) {
    return a.row === b.row && a.column === b.column;
}
export function squareName(position) {
    const file = "abcdefgh"[position.column] ?? "?";
    return file + (8 - position.row);
}
export function pieceAt(pieces, position) {
    return pieces.find((piece) => piece.row === position.row && piece.column === position.column);
}
