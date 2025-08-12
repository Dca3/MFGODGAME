# Mafia MMORPG Game

A web-based, multiplayer, mafia-themed MMORPG with real-time PvP duels, quests, and character progression.

## 🎮 Features

### Backend (.NET 9)
- **ASP.NET Core Web API** with Clean Architecture
- **ASP.NET Identity** + JWT Authentication
- **SignalR** for real-time communication
- **Redis** for matchmaking and caching
- **PostgreSQL** with Entity Framework Core
- **Server-authoritative combat** system
- **Repository Pattern** + Unit of Work
- **Integration Tests** with xUnit

### Frontend (Angular 17)
- **Angular 17** with Standalone Components
- **NgRx** for state management
- **Tailwind CSS** for styling
- **SignalR** client for real-time updates
- **PixiJS** for graphics (planned)

## 🏗️ Architecture

```
mafia-mmorpg-backend/
├── MafiaMMORPG.Domain/          # Entities, Value Objects
├── MafiaMMORPG.Application/     # Interfaces, DTOs, Services
├── MafiaMMORPG.Infrastructure/  # EF Core, Redis, External Services
├── MafiaMMORPG.Web/            # API Controllers, SignalR Hubs
└── MafiaMMORPG.Tests/          # Integration Tests

mafia-mmorpg/ (Frontend)
├── src/
│   ├── app/
│   │   ├── store/              # NgRx Store
│   │   ├── services/           # API Services
│   │   └── components/         # UI Components
│   └── environments/           # Configuration
```

## 🚀 Quick Start

### Backend Setup

1. **Prerequisites**
   - .NET 9 SDK
   - PostgreSQL
   - Redis

2. **Database Setup**
   ```bash
   cd mafia-mmorpg-backend
   dotnet ef database update
   ```

3. **Run Backend**
   ```bash
   cd MafiaMMORPG.Web
   dotnet run
   ```
   - API: http://localhost:8080
   - Swagger: http://localhost:8080/swagger

### Frontend Setup

1. **Prerequisites**
   - Node.js 18+
   - npm or yarn

2. **Install Dependencies**
   ```bash
   cd mafia-mmorpg
   npm install
   ```

3. **Run Frontend**
   ```bash
   ng serve
   ```
   - App: http://localhost:4200

## 🔐 Authentication

### Default Users (Development)
- **Admin**: admin@mafya.local / Admin123!
- **Demo Player**: player@mafya.local / Player123!

### API Endpoints
- `POST /auth/register` - User registration
- `POST /auth/login` - User login
- `POST /auth/refresh` - Token refresh
- `GET /me` - User profile
- `GET /me/stats` - Character stats
- `POST /me/stats/allocate` - Allocate stat points

## 🎯 Game Features

### Character System
- **4 Core Stats**: Karizma (Charisma), Güç (Strength), Zeka (Intelligence), Hayat (Health)
- **Level Progression**: Experience-based leveling
- **Stat Allocation**: Free points system

### PvP System
- **Real-time Duels**: Turn-based combat
- **MMR Matchmaking**: ELO-based rating system
- **Server-authoritative**: All calculations on server

### Items & Equipment
- **Rarity System**: Common, Rare, Epic, Legendary
- **Affixes**: Random stat bonuses
- **Set Bonuses**: Equipment combinations

### Quests & Progression
- **NPC Quests**: Story-driven content
- **Seasonal Rankings**: Competitive leaderboards
- **Economy System**: In-game currency

## 🛠️ Development

### Backend Development
```bash
# Run tests
dotnet test

# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Frontend Development
```bash
# Run tests
ng test

# Build for production
ng build --configuration production
```

## 📦 Technologies

### Backend
- **.NET 9** - Framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **SignalR** - Real-time communication
- **Redis** - Caching & matchmaking
- **PostgreSQL** - Database
- **xUnit** - Testing
- **Swagger** - API documentation

### Frontend
- **Angular 17** - Framework
- **NgRx** - State management
- **Tailwind CSS** - Styling
- **SignalR** - Real-time client
- **PixiJS** - Graphics (planned)

## 📝 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📞 Support

For questions and support, please open an issue on GitHub.
