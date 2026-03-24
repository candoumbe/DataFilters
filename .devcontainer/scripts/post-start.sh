#!/bin/bash
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper function for formatted output
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Function to handle installation errors
install_with_error_handling() {
    local package_name=$1
    local install_command=$2
    local success_message=$3
    
    log_info "Installing $package_name..."
    if eval "$install_command"; then
        log_success "$success_message"
        return 0
    else
        log_error "Failed to install $package_name."
        return 1
    fi
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Main configuration section
log_info "=========================================="
log_info "DataFilters DevContainer Post-Start Setup"
log_info "=========================================="

# Update package manager
log_info "Updating package manager..."
sudo apt-get update > /dev/null 2>&1
log_success "Package manager updated."

# Install GitHub CLI if not present
if ! command_exists gh; then
    log_info "Installing GitHub CLI (gh)..."
    if sudo apt-get install -y gh > /dev/null 2>&1; then
        log_success "GitHub CLI installed successfully."
    else
        log_error "Failed to install GitHub CLI. Attempting alternative installation..."
        if curl --proto '=https' --tlsv1.2 -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg > /dev/null 2>&1 && \
           echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages focal main" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null && \
           sudo apt-get update > /dev/null 2>&1 && \
           sudo apt-get install -y gh > /dev/null 2>&1; then
            log_success "GitHub CLI installed successfully (from archive)."
        else
            log_warning "Could not install GitHub CLI. Some Squad features may be limited."
        fi
    fi
else
    log_success "GitHub CLI already installed."
fi

# Ensure npm is available
if ! command_exists npm; then
    log_warning "npm not found, attempting to install Node.js and npm..."
    sudo apt-get install -y nodejs npm > /dev/null 2>&1
    log_success "Node.js and npm installed."
else
    log_success "npm already available."
fi

# Update npm to latest version
log_info "Updating npm to latest version..."
npm install -g npm@latest > /dev/null 2>&1
log_success "npm updated."

# Ensure squad CLI is installed or updated
if command_exists squad; then
    log_info "Updating squad CLI to latest version..."
    npm install -g @bradygaster/squad-cli@latest > /dev/null 2>&1
    log_success "squad CLI updated to latest version."
else
    log_info "Installing squad CLI..."
    npm install -g @bradygaster/squad-cli > /dev/null 2>&1
    log_success "squad CLI installed successfully."
fi

# Restore NuGet packages and build
log_info "Restoring NuGet packages and building DataFilters..."
./build.sh restore > /dev/null 2>&1
log_success "NuGet packages restored and build completed."

# Check if Squad is already initialized in the workspace
if [ -d ".squad" ]; then
    log_success "Squad is already initialized in this workspace."
else
    log_info "Squad is not yet initialized."
    log_info ""
    log_info "To set up your Squad team, run:"
    log_info "  squad init"
    log_info ""
    log_warning "Note: You may need to authenticate with GitHub first:"
    log_warning "  gh auth login"
fi

# Check GitHub authentication status
log_info ""
log_info "Checking GitHub authentication status..."
if gh auth status > /dev/null 2>&1; then
    log_success "You are authenticated with GitHub."
    GH_USERNAME=$(gh api user -q '.login' 2>/dev/null || echo "unknown")
    log_info "Logged in as: $GH_USERNAME"
else
    log_warning "You are not authenticated with GitHub."
    log_info "To authenticate, run:"
    log_info "  gh auth login"
    log_info ""
    log_warning "This is required for Squad features like issue triage and PR integration."
fi

# Display squad version and status
log_info ""
log_info "Squad version information:"
squad --version 2>/dev/null || log_warning "Could not retrieve squad version"

# Final setup instructions
log_info ""
log_info "=========================================="
log_success "Post-start setup complete!"
log_info "=========================================="
log_info ""
log_info "Next steps:"
log_info "1. Initialize Squad (if not already done):"
log_info "   squad init"
log_info ""
log_info "2. Authenticate with GitHub:"
log_info "   gh auth login"
log_info ""
log_info "3. Open Copilot and select the Squad agent"
log_info "4. Start collaborating with your team!"
log_info ""
log_info "For more information, visit: https://github.com/bradygaster/squad"
