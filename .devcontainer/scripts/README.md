# DevContainer Scripts

This directory contains initialization scripts for the DataFilters development container.

## Scripts Overview

### `post-create.sh`
**When**: Runs once when the container is first created  
**Purpose**: Initial environment setup and dependencies installation  
**Tasks**:
- Updates package manager
- Installs `xdg-utils` (for opening links in default browser)
- Installs/updates Node.js and npm
- Installs squad CLI globally

### `post-start.sh`
**When**: Runs every time the container starts  
**Purpose**: Ensures environment is ready and project is configured  
**Tasks**:
- Ensures GitHub CLI (`gh`) is installed
- Updates npm to latest version
- Ensures squad CLI is installed/updated to latest
- Restores NuGet packages and builds the DataFilters project
- Checks Squad initialization status (`.squad/` directory)
- Verifies GitHub authentication status
- Provides setup instructions if needed

## Setup Workflow

```
Container Created → post-create.sh → Container Started → post-start.sh
     ↓                    ↓              ↓                    ↓
  Fresh OS         Core tools         Deps up-to-date   Ready to work
                   installed          Build prepared
```

## Squad Configuration

After the container starts, you can initialize Squad for your project:

```bash
# Initialize a new team
squad init

# Authenticate with GitHub (required for issues, PRs, triage)
gh auth login

# Verify setup
squad doctor

# Start working with your team
squad
```

## Available Commands

Once Squad is initialized:

- `squad init` - Initialize Squad in current directory
- `squad upgrade` - Update Squad to latest version
- `squad status` - Show active squad status
- `squad triage` - Watch issues and auto-triage
- `squad copilot` - Manage Copilot coding agent
- `squad doctor` - Diagnose setup issues
- `squad` - Interactive shell to work with your team
- `gh auth login` - Authenticate with GitHub
- `gh auth status` - Check GitHub authentication

## Troubleshooting

### GitHub CLI not working
```bash
gh auth login
```

### Squad CLI outdated
```bash
npm install -g @bradygaster/squad-cli@latest
squad upgrade
```

### Project build issues
```bash
./build.sh restore
./build.sh
```

### Check environment
```bash
squad doctor
node --version
npm --version
git --version
gh --version
```

## Notes

- Both scripts use colored output for better readability
- Scripts include error handling and graceful degradation
- Empty `.squad/` directory indicates Squad hasn't been initialized yet
- GitHub authentication is optional but required for full Squad features
- The build system uses Nuke - see `build/Build.cs`
