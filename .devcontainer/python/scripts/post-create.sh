#!/bin/bash
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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

log_info "================================================"
log_info "DataFilters (Python) DevContainer Post-Create Setup"
log_info "================================================"

# Update package manager
log_info "Updating package manager..."
sudo apt-get update > /dev/null 2>&1
log_success "Package manager updated."

log_info "Python version: $(python --version)"
log_info "pip version: $(pip --version)"

# Install xdg-utils for opening links in the default browser
log_info "Installing xdg-utils..."
sudo apt-get install -y xdg-utils > /dev/null 2>&1
log_success "xdg-utils installed successfully."

# Install Python development dependencies
log_info "Installing Python development dependencies..."
cd src/python
pip install -e ".[dev]" > /dev/null 2>&1
cd ../..
log_success "Python development dependencies installed."

# Install squad CLI globally
log_info "Installing squad CLI globally..."
npm install -g @bradygaster/squad-cli > /dev/null 2>&1
log_success "squad CLI installed successfully."

log_info "Initialize squad configuration..."
squad init
log_success "squad CLI initialized successfully."

log_info ""
log_success "Post-create setup complete!"
log_info ""
log_info "Additional setup will run on container start (post-start)."
log_info "This includes:"
log_info "  • GitHub CLI checks"
log_info "  • Python dependency checks"
log_info "  • Squad configuration checks"
log_info ""
