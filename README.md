# ChessGame

A full-stack web-based chess application built with a **C# ASP.NET Core Web API** backend and an **HTML/CSS/JavaScript** frontend. Supports two player local play and single player vs an AI opponent powered by the **Minimax algorithm**.

---

## Project Structure

```
ChessGame/
├── Chess_Backend/               # ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── AIController.cs      # Endpoints for AI move generation
│   │   ├── GameController.cs    # Endpoints for game flow and state
│   │   ├── MoveController.cs    # Endpoints for submitting and validating moves
│   │   └── SaveController.cs    # Endpoints for saving and loading games
│   ├── Logic/
│   │   ├── ChessAI.cs           # Minimax algorithm and move selection
│   │   ├── GameState.cs         # Turn tracking, check/checkmate/stalemate detection
│   │   └── MoveValidator.cs     # Move legality validation
│   ├── Models/
│   │   ├── Pieces/
│   │   │   ├── Bishop.cs
│   │   │   ├── King.cs
│   │   │   ├── Knight.cs
│   │   │   ├── Pawn.cs
│   │   │   ├── Queen.cs
│   │   │   └── Rook.cs
│   │   ├── Board.cs             # 8x8 grid and piece positions
│   │   ├── Move.cs              # Move representation (from, to, special flags)
│   │   └── Piece.cs             # Base class for all pieces
│   └── Program.cs               # App entry point and middleware configuration
│
└── Chess_Frontend/              # Static web frontend
    ├── wwwroot/
    │   ├── index.html           # Main game page
    │   ├── css/                 # Stylesheets
    │   └── js/                  # Game rendering and API communication
    └── Program.cs               # Static file server configuration
```

---

## Features

- **Two Player Local** — Play against a friend on the same machine
- **Single Player vs AI** — Play against an AI opponent powered by the Minimax algorithm
- **Full Move Validation** — Legal move enforcement including check, castling, en passant, and pawn promotion
- **Save & Load** — Save a game and load it on any machine
- **Cross Platform** — Runs in any modern web browser on Windows, Linux, or ChromeOS

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | HTML, CSS, JavaScript |
| Backend | C# ASP.NET Core Web API (.NET 9) |
| AI | Minimax Algorithm |
| Communication | REST API / JSON |
| IDE | JetBrains Rider |

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- A modern web browser
- JetBrains Rider or Visual Studio (optional)

### Running the Backend

```bash
cd Chess_Backend
dotnet run
```

The API will start on `https://localhost:5001` by default.

### Running the Frontend

Once the backend is running, open your browser and navigate to the frontend URL configured in `Chess_Frontend/Program.cs`, or open `index.html` directly during development.

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/game/start` | Start a new game |
| GET | `/api/game/state` | Get the current board state |
| POST | `/api/move` | Submit a player move |
| GET | `/api/ai/move` | Request the AI to calculate its move |
| POST | `/api/save` | Save the current game |
| GET | `/api/save/load` | Load a saved game |

---

## Roadmap

- [x] Project structure and architecture
- [ ] Core models (Board, Piece, Move)
- [ ] Piece movement rules
- [ ] Move validation
- [ ] Game state tracking (check, checkmate, stalemate)
- [ ] REST API controllers
- [ ] Minimax AI implementation
- [ ] Frontend board rendering
- [ ] Save/Load functionality
- [ ] Deployment

---

## License

This project is for educational purposes.