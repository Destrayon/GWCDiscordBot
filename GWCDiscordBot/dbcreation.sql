CREATE TABLE IF NOT EXISTS OffendingUsers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DiscordId INTEGER UNIQUE NOT NULL,
    PingAmount INTEGER NOT NULL,
    HasEscalated INTEGER NOT NULL
);