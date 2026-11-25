import { pieceAt, sameSquare } from "./types.js";
const SHAPES = {
    Pawn: `
    <circle cx="40" cy="22" r="11"/>
    <path d="M26 70h28l-6-26a14 14 0 0 0-16 0z"/>
    <rect x="22" y="64" width="36" height="8" rx="3"/>`,
    Rook: `
    <path d="M16 16h10v8h7v-8h14v8h7v-8h10v16H16z"/>
    <path d="M24 32h32v28H24z"/>
    <rect x="16" y="60" width="48" height="10" rx="2"/>`,
    Knight: `
    <path d="M22 70h36l-4-18 12-6-6-8 8-8c-4-12-18-16-28-10l-8 6-6-4 2 14-12 8 2 8z"/>
    <rect x="16" y="64" width="42" height="8" rx="2"/>`,
    Bishop: `
    <circle cx="40" cy="15" r="8"/>
    <path d="M40 8v14"/>
    <path d="M27 70h26l-4-18c9-5 12-16 5-24-9 7-19 7-28 0-7 8-4 19 5 24z"/>
    <rect x="22" y="64" width="36" height="8" rx="2"/>`,
    Queen: `
    <circle cx="18" cy="18" r="5"/>
    <circle cx="40" cy="12" r="5"/>
    <circle cx="62" cy="18" r="5"/>
    <path d="M14 26l10 38h32l10-38-16 12L40 20 30 38z"/>
    <rect x="20" y="62" width="40" height="8" rx="2"/>`,
    King: `
    <path d="M36 4h8v8h10v8H44v8h-8v-8H26v-8h10z"/>
    <path d="M24 70h32l-5-30c11-4 14-16 7-24-10 8-22 8-32 0-7 8-4 20 7 24z"/>
    <rect x="20" y="64" width="40" height="8" rx="2"/>`,
};
export function renderBoard(board, pieces, selected, legalMoves) {
    board.replaceChildren();
    for (let row = 0; row < 8; row++) {
        for (let column = 0; column < 8; column++) {
            const position = { row, column };
            const square = document.createElement("button");
            square.type = "button";
            square.className = (row + column) % 2 === 0 ? "square white-square" : "square black-square";
            square.dataset.row = String(row);
            square.dataset.column = String(column);
            square.setAttribute("aria-label", squareLabel(pieces, position, selected, legalMoves));
            if (selected && sameSquare(selected, position)) {
                square.classList.add("selected");
            }
            if (legalMoves.some((move) => sameSquare(move, position))) {
                square.classList.add("legal");
            }
            const piece = pieceAt(pieces, position);
            if (piece) {
                const glyph = document.createElement("span");
                glyph.className = piece.color === "White" ? "piece white-piece" : "piece black-piece";
                glyph.innerHTML = pieceMarkup(piece.type, piece.color);
                square.append(glyph);
            }
            else if (square.classList.contains("legal")) {
                const marker = document.createElement("span");
                marker.className = "legal-marker";
                marker.setAttribute("aria-hidden", "true");
                square.append(marker);
            }
            board.append(square);
        }
    }
}
function pieceMarkup(type, color) {
    const fill = color === "White" ? "#ffffff" : "#111111";
    const stroke = color === "White" ? "#111111" : "#ffffff";
    return `<svg viewBox="0 0 80 80" focusable="false" aria-hidden="true">
    <g fill="${fill}" stroke="${stroke}" stroke-width="3.5" stroke-linejoin="round" stroke-linecap="round">
      ${SHAPES[type]}
    </g>
  </svg>`;
}
function squareLabel(pieces, position, selected, legalMoves) {
    const file = "abcdefgh"[position.column];
    const rank = 8 - position.row;
    const piece = pieceAt(pieces, position);
    const name = piece ? `${piece.color} ${piece.type}` : "empty";
    const marks = [];
    if (selected && sameSquare(selected, position)) {
        marks.push("selected");
    }
    if (legalMoves.some((move) => sameSquare(move, position))) {
        marks.push("legal destination");
    }
    const suffix = marks.length ? `, ${marks.join(", ")}` : "";
    return `${file}${rank}, ${name}${suffix}`;
}
export function readSquare(target) {
    if (!(target instanceof Element)) {
        return null;
    }
    const square = target.closest(".square");
    if (!square?.dataset.row || !square.dataset.column) {
        return null;
    }
    return {
        row: Number(square.dataset.row),
        column: Number(square.dataset.column),
    };
}
