# Public Data scripts

This project contains various useful scripts for the Public Data API.

To run any of these scripts, you can either:

1. Run directly via Rider. Set different script commands via 'Program arguments' in the run configuration.
2. Via the CLI e.g.

   ```bash
   dotnet run -- seed:data
   ```

## Seed data - `seed:data`

Data for the public data API can be seeded using this command.

### Prerequisites

Before using this command, you will first need to place any required seed CSVs in the `SeedFiles` 
directory. The seed CSVs can be found in Google Drive.

Once you have done this, ensure the public API Docker database is running:

```bash
# Via Docker Compose
docker compose up -d public-api-db

# Via project start script
pnpm start publicApiDb
```

### Usage

```bash
dotnet run -- seed:data
```

Additional options can be provided to the script after `seed:data` argument like:

```bash
dotnet run -- seed:data --dump-sql
```

### Options

- `--dump-sql` 
  - Creates an SQL dump of the seed data at `data/public-api-db/public_data.sql`
  - Useful for creating seed data dumps for Google Drive.
