#!/usr/bin/env bash
#
# 3OMAL-Platform Developer CLI — Bash version for CI/Unix/macOS
# Usage: ./dev.sh <command> [subcommand] [options]
#
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACK_DIR="$ROOT_DIR/back"
FRONT_DIR="$ROOT_DIR/front"
API_DIR="$BACK_DIR/src/API"
SEEDER_DIR="$BACK_DIR/tools/Seeder"
SOLUTION_FILE="$BACK_DIR/back.slnx"
INFRA_DIR="$BACK_DIR/src/Infrastructure"

CI_MODE=false

# Colors
RED='\033[0;91m'
GREEN='\033[0;92m'
YELLOW='\033[0;93m'
BLUE='\033[0;94m'
CYAN='\033[0;96m'
DIM='\033[0;2m'
BOLD='\033[1m'
NC='\033[0m'

# ========== Utilities ==========
info()  { echo -e "${GREEN}[INFO]${NC} $1"; }
warn()  { echo -e "${YELLOW}[WARN]${NC} $1"; }
error() { echo -e "${RED}[ERROR]${NC} $1"; }
success() { echo -e "${GREEN}${BOLD}✓${NC} $1"; }
fail()    { echo -e "${RED}${BOLD}✗${NC} $1"; }
section() { echo -e "\n${BOLD}${BLUE}═══ $1 ═══${NC}"; }
hint()    { echo -e "${CYAN}  ? $1${NC}"; }

timer_start=$(date +%s)
start_timer() { timer_start=$(date +%s); }
stop_timer() {
    local elapsed=$(( $(date +%s) - timer_start ))
    echo -e "${DIM}Completed in ${elapsed}s${NC}"
}

confirm() {
    if [ "$CI_MODE" = true ]; then return 0; fi
    read -r -p "$1 (y/N): " reply
    case "$reply" in [yY]|[yY][eE][sS]) return 0;; *) return 1;; esac
}

confirm_text() {
    if [ "$CI_MODE" = true ]; then return 0; fi
    read -r -p "$1: " reply
    [ "$reply" = "$2" ]
}

get_env() {
    echo "${ASPNETCORE_ENVIRONMENT:-${DOTNET_ENVIRONMENT:-development}}"
}

get_conn_string() {
    local json_file="$API_DIR/appsettings.json"
    python3 -c "
import json,sys
try:
    with open(sys.argv[1]) as f:
        c = json.load(f)
    print(c.get('ConnectionStrings', {}).get('DefaultConnection', ''))
except: sys.exit(0)
" "$json_file" 2>/dev/null || echo ""
}

# ========== Help ==========
show_help() {
    cat <<'EOF'
3OMAL-Platform Developer CLI

Usage: ./dev.sh <command> [subcommand] [options]

Commands:
  setup         First-time environment setup (deps, DB, seed)
  build         Build [back|front|all] (default: all)
  test          Run tests [back|front|all] (default: all)
  seed          Seed database with fake data
  clean         Clean [all|back|front|packages|artifacts]
  reset         Drop DB → migrate → seed
  db            Database management (create|drop|migrate|add|remove|list|pending)
  run           Start dev servers [back|front|all]
  lint          Lint and format code
  health        System health check
  env           Environment management
  deps          Check dependencies
  audit         Package vulnerability scan
  config        Configuration management

Options:
  --ci          Non-interactive mode (no prompts, fail-fast)
  --help, -h    Show this help

Examples:
  ./dev.sh setup
  ./dev.sh build back --release
  ./dev.sh test all --coverage
  ./dev.sh seed --workers 100 --seed 42
  ./dev.sh db add InitialCreate
  ./dev.sh run all
EOF
}

# ========== Dependencies ==========
cmd_deps_check() {
    section "Dependency Check"
    start_timer
    local ok=true

    if command -v dotnet &>/dev/null; then
        local dv
        dv=$(dotnet --version)
        success ".NET SDK v$dv"
    else
        fail ".NET SDK not found"
        ok=false
    fi

    if command -v node &>/dev/null; then
        local nv
        nv=$(node --version)
        success "Node.js $nv"
    else
        fail "Node.js not found"
        ok=false
    fi

    if command -v npm &>/dev/null; then
        success "npm v$(npm --version)"
    fi

    stop_timer
    $ok && success "All dependencies satisfied" || warn "Some dependencies missing"
    $ok
}

# ========== Environment ==========
cmd_env_show() {
    section "Environment"
    echo "  Active:  $(get_env)"
    echo "  Backend URL:  http://localhost:5000"
    echo "  Frontend URL: http://localhost:4200"
}

cmd_env_switch() {
    local env_name="${1:-}"
    [ -z "$env_name" ] && { error "Usage: dev env switch <environment>"; exit 1; }
    export ASPNETCORE_ENVIRONMENT="$env_name"
    export DOTNET_ENVIRONMENT="$env_name"
    success "Switched to '$env_name' environment"
}

# ========== Build ==========
cmd_build() {
    local target="${1:-all}"
    local config="${2:-Release}"
    shift 2 2>/dev/null || true
    local watch_flag=false
    for arg in "$@"; do [ "$arg" = "--watch" ] && watch_flag=true; done

    case "$target" in
        all) cmd_build_back "$config"; cmd_build_front "$config" "$watch_flag";;
        back) cmd_build_back "$config";;
        front) cmd_build_front "$config" "$watch_flag";;
        *) error "Unknown target: $target"; exit 1;;
    esac
}

cmd_build_back() {
    local config="${1:-Release}"
    section "Building Backend ($config)"
    start_timer
    info "Restoring packages..."
    dotnet restore "$SOLUTION_FILE" 2>&1
    info "Building..."
    dotnet build "$SOLUTION_FILE" --no-restore --configuration "$config" 2>&1
    local exit_code=$?
    stop_timer
    [ $exit_code -eq 0 ] && success "Backend build succeeded" || { fail "Backend build failed"; exit 1; }
}

cmd_build_front() {
    local config="${1:-Release}"
    local watch="${2:-false}"
    section "Building Frontend ($config)"
    start_timer

    pushd "$FRONT_DIR" >/dev/null
    if [ ! -d "node_modules" ]; then
        info "Installing npm packages..."
        npm install 2>&1
    fi

    if [ "$watch" = true ]; then
        info "Starting watch mode..."
        npx ng build --watch --configuration development
        popd >/dev/null
        return
    fi

    local ng_config="production"
    [ "$config" = "Debug" ] && ng_config="development"
    npx ng build --configuration "$ng_config" 2>&1
    local exit_code=$?
    popd >/dev/null

    stop_timer
    [ $exit_code -eq 0 ] && success "Frontend build succeeded" || { fail "Frontend build failed"; exit 1; }
}

# ========== Test ==========
cmd_test() {
    local target="${1:-all}"
    shift 1 2>/dev/null || true
    local filter="" coverage=false
    while [ $# -gt 0 ]; do
        case "$1" in --filter) shift; filter="$1";; --coverage) coverage=true;; esac; shift
    done

    case "$target" in
        all) cmd_test_back "$filter" "$coverage"; cmd_test_front "$coverage";;
        back) cmd_test_back "$filter" "$coverage";;
        front) cmd_test_front "$coverage";;
        *) error "Unknown target: $target"; exit 1;;
    esac
}

cmd_test_back() {
    local filter="${1:-}" coverage="${2:-false}"
    section "Backend Tests"
    start_timer
    local args=("test" "$SOLUTION_FILE" "--verbosity" "normal")
    [ -n "$filter" ] && args+=("--filter" "$filter")
    if [ "$coverage" = true ]; then
        args+=("/p:CollectCoverage=true" "/p:CoverletOutputFormat=opencover" "/p:CoverletOutput=../../coverage/")
    fi
    dotnet "${args[@]}" 2>&1
    local exit_code=$?
    stop_timer
    [ $exit_code -eq 0 ] && success "Backend tests passed" || { fail "Backend tests failed"; exit 1; }
}

cmd_test_front() {
    local coverage="${1:-false}"
    section "Frontend Tests"
    start_timer
    pushd "$FRONT_DIR" >/dev/null
    local args=("vitest" "run")
    [ "$coverage" = true ] && args+=("--coverage")
    npx "${args[@]}" 2>&1
    local exit_code=$?
    popd >/dev/null
    stop_timer
    [ $exit_code -eq 0 ] && success "Frontend tests passed" || { fail "Frontend tests failed"; exit 1; }
}

# ========== Database ==========
cmd_db() {
    local action="${1:-}"
    shift 1 2>/dev/null || true
    case "$action" in
        create) cmd_db_create;;
        drop) cmd_db_drop;;
        migrate) cmd_db_migrate;;
        add) cmd_db_add "$@";;
        remove) cmd_db_remove;;
        list) cmd_db_list;;
        pending) cmd_db_pending;;
        *) error "Unknown db action: $action"; exit 1;;
    esac
}

cmd_db_create() {
    section "Create Database"
    start_timer
    dotnet ef database update --project "$INFRA_DIR" --startup-project "$API_DIR" 2>&1
    stop_timer
    [ $? -eq 0 ] && success "Database created" || { error "Failed to create database"; exit 1; }
}

cmd_db_drop() {
    section "Drop Database"
    warn "This will PERMANENTLY delete all data!"
    confirm "Are you sure?" || { info "Aborted"; return; }
    start_timer
    dotnet ef database drop --project "$INFRA_DIR" --startup-project "$API_DIR" --force 2>&1
    stop_timer
    [ $? -eq 0 ] && success "Database dropped" || { error "Failed to drop database"; exit 1; }
}

cmd_db_migrate() {
    section "Apply Migrations"
    start_timer
    dotnet ef database update --project "$INFRA_DIR" --startup-project "$API_DIR" 2>&1
    stop_timer
    [ $? -eq 0 ] && success "Migrations applied" || { error "Migration failed"; exit 1; }
}

cmd_db_add() {
    local name="${1:-}"
    [ -z "$name" ] && { error "Migration name required: dev db add <name>"; exit 1; }
    section "Add Migration: $name"
    start_timer
    dotnet ef migrations add "$name" --project "$INFRA_DIR" --startup-project "$API_DIR" 2>&1
    stop_timer
    [ $? -eq 0 ] && success "Migration '$name' added" || { error "Failed to add migration"; exit 1; }
}

cmd_db_remove() {
    section "Remove Last Migration"
    confirm "Remove the last migration?" || { info "Aborted"; return; }
    start_timer
    dotnet ef migrations remove --project "$INFRA_DIR" --startup-project "$API_DIR" 2>&1
    stop_timer
    [ $? -eq 0 ] && success "Migration removed" || { error "Failed to remove migration"; exit 1; }
}

cmd_db_list() {
    section "Migrations"
    dotnet ef migrations list --project "$INFRA_DIR" --startup-project "$API_DIR" 2>&1
}

cmd_db_pending() {
    section "Pending Migrations"
    local result
    result=$(dotnet ef migrations list --project "$INFRA_DIR" --startup-project "$API_DIR" 2>&1)
    local pending
    pending=$(echo "$result" | grep -E "^[0-9]{14}" || true)
    if [ -n "$pending" ]; then
        info "Pending migrations:"
        echo "$pending" | while IFS= read -r line; do echo "  - $line"; done
    else
        success "No pending migrations"
    fi
}

# ========== Seed ==========
cmd_seed() {
    section "Database Seeding"

    if [ ! -f "$SEEDER_DIR/Seeder.csproj" ]; then
        warn "Seeder project not found"
        confirm "Create Seeder project now?" || { error "Seeder required"; exit 1; }
    fi

    start_timer
    local args=("run" "--project" "$SEEDER_DIR" "--")
    local workers=50 customers=200 bookings=500 reviews=300 payments=400 invoices=400 conversations=200 messages=10000 attachments=500 seed_val=$RANDOM

    while [ $# -gt 0 ]; do
        case "$1" in
            --workers) shift; workers=$1;;
            --customers) shift; customers=$1;;
            --bookings) shift; bookings=$1;;
            --reviews) shift; reviews=$1;;
            --payments) shift; payments=$1;;
            --invoices) shift; invoices=$1;;
            --conversations) shift; conversations=$1;;
            --messages) shift; messages=$1;;
            --attachments) shift; attachments=$1;;
            --seed) shift; seed_val=$1;;
            --ci) CI_MODE=true;;
        esac
        shift
    done

    args+=("--seed" "$seed_val" "--workers" "$workers" "--customers" "$customers")
    args+=("--bookings" "$bookings" "--reviews" "$reviews" "--payments" "$payments")
    args+=("--invoices" "$invoices" "--conversations" "$conversations")
    args+=("--messages" "$messages" "--attachments" "$attachments")

    info "Seeding: workers=$workers customers=$customers bookings=$bookings messages=$messages"
    dotnet "${args[@]}" 2>&1
    local exit_code=$?
    stop_timer
    [ $exit_code -eq 0 ] && success "Seed complete" || { error "Seeding failed"; exit 1; }
}

# ========== Reset ==========
cmd_reset() {
    section "Reset Database"
    warn "This will DROP and recreate the database!"
    confirm_text "Type 'reset' to confirm" "reset" || { info "Aborted"; return; }

    start_timer
    dotnet ef database drop --project "$INFRA_DIR" --startup-project "$API_DIR" --force 2>&1 | true
    info "Database dropped"
    dotnet ef database update --project "$INFRA_DIR" --startup-project "$API_DIR" 2>&1
    info "Migrations applied"
    stop_timer
    success "Database reset complete"

    if [ "$CI_MODE" = false ]; then
        read -r -p "Seed database now? (Y/n): " s
        if [ "$s" != "n" ]; then cmd_seed; fi
    fi
}

# ========== Run ==========
cmd_run() {
    local target="${1:-all}"
    case "$target" in
        all)
            section "Starting Development Servers"
            info "Backend:  http://localhost:5000"
            info "Frontend: http://localhost:4200"
            info "Swagger:  http://localhost:5000/swagger"
            echo -e "${DIM}Press Ctrl+C to stop both servers${NC}"

            pushd "$API_DIR" >/dev/null
            ASPNETCORE_URLS="http://localhost:5000" dotnet run --urls "http://localhost:5000" &
            BACK_PID=$!
            popd >/dev/null

            pushd "$FRONT_DIR" >/dev/null
            npx ng serve &
            FRONT_PID=$!
            popd >/dev/null

            trap 'kill $BACK_PID $FRONT_PID 2>/dev/null; exit' INT TERM
            wait
            ;;
        back)
            section "Starting Backend"
            info "API: http://localhost:5000"
            pushd "$API_DIR" >/dev/null
            ASPNETCORE_URLS="http://localhost:5000" dotnet run --urls "http://localhost:5000"
            popd >/dev/null
            ;;
        front)
            section "Starting Frontend"
            info "Angular: http://localhost:4200"
            pushd "$FRONT_DIR" >/dev/null
            npx ng serve
            popd >/dev/null
            ;;
        *) error "Unknown target: $target"; exit 1;;
    esac
}

# ========== Lint ==========
cmd_lint() {
    local fix=false
    for arg in "$@"; do [ "$arg" = "--fix" ] && fix=true; done

    section "Linting & Formatting"
    start_timer

    info "Linting backend..."
    pushd "$BACK_DIR" >/dev/null
    local args=("format")
    $fix || args+=("--verify-no-changes")
    dotnet "${args[@]}" 2>&1 && success "Backend formatting OK" || { fail "Backend formatting issues"; }
    popd >/dev/null

    info "Linting frontend..."
    pushd "$FRONT_DIR" >/dev/null
    if $fix; then
        npx eslint src/ --fix 2>&1 || true
        npx prettier --write "src/" 2>&1 || true
    else
        npx eslint src/ 2>&1 && success "ESLint OK" || fail "ESLint issues"
        npx prettier --check "src/" 2>&1 && success "Prettier OK" || { fail "Prettier issues"; hint "Run 'dev lint --fix' to auto-format"; }
    fi
    popd >/dev/null

    stop_timer
}

# ========== Clean ==========
cmd_clean() {
    local scope="${1:-all}"
    confirm "Remove build artifacts and caches?" || { info "Aborted"; return; }

    section "Cleanup"
    start_timer

    case "$scope" in
        all|back)
            info "Removing bin/obj..."
            find "$BACK_DIR" -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true
            success "bin/obj removed"
            info "Clearing NuGet cache..."
            dotnet nuget locals all --clear 2>&1
            ;;
    esac

    case "$scope" in
        all|front)
            if [ -d "$FRONT_DIR/node_modules" ]; then rm -rf "$FRONT_DIR/node_modules"; success "node_modules removed"; fi
            if [ -d "$FRONT_DIR/dist" ]; then rm -rf "$FRONT_DIR/dist"; success "dist removed"; fi
            info "Clearing npm cache..."
            npm cache clean --force 2>&1 || true
            ;;
    esac

    case "$scope" in
        all|artifacts)
            find "$BACK_DIR" -type d -name TestResults -exec rm -rf {} + 2>/dev/null || true
            [ -d "$BACK_DIR/coverage" ] && rm -rf "$BACK_DIR/coverage"
            success "Test artifacts removed"
            ;;
    esac

    stop_timer
    success "Cleanup complete"
}

# ========== Health ==========
cmd_health() {
    section "System Health Check"
    start_timer
    local pass=0 fail=0

    if command -v dotnet &>/dev/null; then
        dv=$(dotnet --version)
        if [[ "$dv" == 10.* ]]; then
            echo -e "  ${GREEN}✓ PASS${NC}  dotnet-sdk: $dv"
            pass=$((pass + 1))
        else
            echo -e "  ${YELLOW}⚠ SKIP${NC}  dotnet-sdk: $dv (10.x recommended)"
            pass=$((pass + 1))
        fi
    else
        echo -e "  ${RED}✗ FAIL${NC}  dotnet-sdk: not found"
        fail=$((fail + 1))
    fi

    if command -v node &>/dev/null; then
        nv=$(node --version)
        echo -e "  ${GREEN}✓ PASS${NC}  nodejs: $nv"
        pass=$((pass + 1))
    else
        echo -e "  ${RED}✗ FAIL${NC}  nodejs: not found"
        fail=$((fail + 1))
    fi

    local cs
    cs=$(get_conn_string)
    if command -v sqlcmd &>/dev/null; then
        echo -e "  ${GREEN}✓ PASS${NC}  sqlcmd available"
        pass=$((pass + 1))
    elif [ -n "$cs" ]; then
        echo -e "  ${GREEN}✓ PASS${NC}  connection string configured"
        pass=$((pass + 1))
    else
        echo -e "  ${YELLOW}⚠ SKIP${NC}  sql-server: cannot verify"
        pass=$((pass + 1))
    fi

    local jwt_key=""
    local json_file="$API_DIR/appsettings.json"
    jwt_key=$(python3 -c "
import json,sys
try:
    with open(sys.argv[1]) as f:
        c = json.load(f)
    print(c.get('Jwt', {}).get('Key', '') or '')
except: sys.exit(0)
" "$json_file" 2>/dev/null || echo "")
    if [ -n "$jwt_key" ] && [ ${#jwt_key} -ge 16 ]; then
        echo -e "  ${GREEN}✓ PASS${NC}  jwt-key configured"
        pass=$((pass + 1))
    else
        echo -e "  ${RED}✗ FAIL${NC}  jwt-key: missing or too short"
        fail=$((fail + 1))
    fi

    stop_timer
    echo ""
    echo -e "  ${pass} passed, ${fail} failed"
    [ $fail -eq 0 ] && success "System healthy" || { warn "$fail checks failed"; [ "$CI_MODE" = true ] && exit 1; }
}

# ========== Audit ==========
cmd_audit() {
    section "Package Security Audit"
    start_timer

    info "Scanning NuGet packages..."
    pushd "$BACK_DIR" >/dev/null
    dotnet list package --vulnerable 2>&1 || true
    popd >/dev/null

    info "Scanning npm packages..."
    pushd "$FRONT_DIR" >/dev/null
    npm audit 2>&1 || true
    popd >/dev/null

    stop_timer
}

# ========== Config ==========
cmd_config() {
    local action="${1:-show}"
    shift 1 2>/dev/null || true
    case "$action" in
        show)
            section "Configuration"
            if [ -f "$API_DIR/appsettings.json" ]; then
                local json_file="$API_DIR/appsettings.json"
                python3 -c "
import json,sys
try:
    with open(sys.argv[1]) as f:
        c = json.load(f)
    for k, v in c.items():
        if k == 'Jwt' and 'Key' in v:
            v['Key'] = '******** (hidden)'
        print(f'  {k}: {json.dumps(v, ensure_ascii=False)}')
except: sys.exit(0)
" "$json_file" 2>/dev/null || true
            fi
            ;;
        backup)
            local bak_dir="$ROOT_DIR/config-backups/$(date +%Y%m%d-%H%M%S)"
            mkdir -p "$bak_dir"
            cp "$API_DIR/appsettings.json" "$bak_dir/" 2>/dev/null || true
            cp "$API_DIR/appsettings.Development.json" "$bak_dir/" 2>/dev/null || true
            cp "$FRONT_DIR/src/environments/environment.ts" "$bak_dir/" 2>/dev/null || true
            success "Config backed up to: $bak_dir"
            ;;
        *) error "Unknown config action: $action"; exit 1;;
    esac
}

# ========== Setup ==========
cmd_setup() {
    section "First-Time Setup"
    start_timer
    info "Setting up development environment..."

    cmd_deps_check

    info "Restoring packages..."
    dotnet restore "$SOLUTION_FILE" 2>&1
    pushd "$FRONT_DIR" >/dev/null
    npm install 2>&1
    popd >/dev/null
    success "Packages restored"

    if ! dotnet tool list --global 2>&1 | grep -q "dotnet-ef"; then
        info "Installing dotnet-ef..."
        dotnet tool install --global dotnet-ef 2>&1
    fi

    info "Creating database..."
    dotnet ef database update --project "$INFRA_DIR" --startup-project "$API_DIR" 2>&1 || warn "Database creation failed"

    stop_timer
    success "Setup complete! Run './dev.sh run' to start servers."
}

# ========== Main Dispatch ==========
main() {
    if [ $# -eq 0 ] || [ "$1" = "--help" ] || [ "$1" = "-h" ]; then
        show_help
        exit 0
    fi

    local args=()
    for arg in "$@"; do
        if [ "$arg" = "--ci" ]; then CI_MODE=true; else args+=("$arg"); fi
    done

    set -- "${args[@]}"
    local cmd="${1:-}"
    shift 1 2>/dev/null || true

    case "$cmd" in
        setup) cmd_setup;;
        build) cmd_build "${1:-all}" "${2:-Release}";;
        test) cmd_test "$@";;
        seed) cmd_seed "$@";;
        clean) cmd_clean "${1:-all}";;
        reset) cmd_reset;;
        db) cmd_db "$@";;
        run) cmd_run "${1:-all}";;
        start) cmd_run "${1:-all}";;
        lint) cmd_lint "$@";;
        health|status|doctor) cmd_health;;
        env)
            local action="${1:-show}"
            shift 1 2>/dev/null || true
            case "$action" in
                show) cmd_env_show;;
                switch) cmd_env_switch "$1";;
                *) error "Unknown env action: $action";;
            esac
            ;;
        deps) cmd_deps_check;;
        audit) cmd_audit;;
        config) cmd_config "$@";;
        help) show_help;;
        *) error "Unknown command: $cmd"; show_help; exit 1;;
    esac
}

main "$@"
