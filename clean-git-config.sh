#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(git rev-parse --show-toplevel 2>/dev/null || true)"

if [[ -z "${ROOT_DIR}" ]]; then
  echo "Error: not inside a Git repository."
  exit 1
fi

if [[ "$(pwd)" != "${ROOT_DIR}" ]]; then
  echo "Error: run this script from the repository root: ${ROOT_DIR}"
  exit 1
fi

CURRENT_BRANCH="$(git branch --show-current)"

if [[ -z "${CURRENT_BRANCH}" ]]; then
  echo "Error: unable to determine current branch."
  exit 1
fi

KEYS=(
  "branch.${CURRENT_BRANCH}.github-pr-owner-number"
  "branch.${CURRENT_BRANCH}.github-pr-base-branch"
  "branch.${CURRENT_BRANCH}.vscode-merge-base"
)

print_effective_values() {
  local key="$1"
  local values
  values="$(git config --get-all "${key}" 2>/dev/null || true)"

  if [[ -z "${values}" ]]; then
    echo "  effective values: none"
    return
  fi

  echo "  effective values:"
  while IFS= read -r line; do
    echo "    - ${line}"
  done <<< "${values}"
}

print_all_origins() {
  local key="$1"
  local origins
  origins="$(git config --show-origin --show-scope --get-all "${key}" 2>/dev/null || true)"

  if [[ -z "${origins}" ]]; then
    echo "  origins: none"
    return
  fi

  echo "  origins:"
  while IFS= read -r line; do
    echo "    - ${line}"
  done <<< "${origins}"
}

count_effective_values() {
  local key="$1"
  local count
  count="$(git config --get-all "${key}" 2>/dev/null | wc -l | tr -d ' ')"
  echo "${count}"
}

echo "Repository root : ${ROOT_DIR}"
echo "Current branch  : ${CURRENT_BRANCH}"
echo
echo "Configuration audit (all sources):"

duplicates_found=0

for key in "${KEYS[@]}"; do
  echo
  echo "Key: ${key}"
  print_all_origins "${key}"
  print_effective_values "${key}"

  count="$(count_effective_values "${key}")"
  if [[ "${count}" -gt 1 ]]; then
    duplicates_found=1
    echo "  status: duplicate values detected (${count})"
  else
    echo "  status: ok (${count})"
  fi
done

echo
if [[ "${duplicates_found}" -eq 0 ]]; then
  echo "No duplicates detected for the current branch keys. Nothing to clean."
  exit 0
fi

echo "The script will perform this cleanup after confirmation:"
echo "  1) keep one canonical value per key"
echo "  2) remove the same key from local/worktree/global scopes"
echo "  3) re-add the canonical value in local scope (.git/config)"
echo

read -r -p "Proceed with cleanup? [y/N] " answer

if [[ ! "${answer}" =~ ^[Yy]$ ]]; then
  echo "Cleanup canceled."
  exit 0
fi

echo
echo "Applying cleanup..."

for key in "${KEYS[@]}"; do
  canonical="$(git config --get-all "${key}" 2>/dev/null | head -n 1 || true)"

  if [[ -z "${canonical}" ]]; then
    echo "- ${key}: no value found, skipping"
    continue
  fi

  git config --local --unset-all "${key}" 2>/dev/null || true
  git config --worktree --unset-all "${key}" 2>/dev/null || true
  git config --global --unset-all "${key}" 2>/dev/null || true

  git config --local --add "${key}" "${canonical}"

  echo "- ${key}: cleaned, canonical value restored in local scope"
done

echo
echo "Post-cleanup verification:"

remaining_duplicates=0

for key in "${KEYS[@]}"; do
  echo
  echo "Key: ${key}"
  print_all_origins "${key}"
  print_effective_values "${key}"

  count="$(count_effective_values "${key}")"
  if [[ "${count}" -gt 1 ]]; then
    remaining_duplicates=1
    echo "  status: still duplicated (${count})"
  else
    echo "  status: ok (${count})"
  fi
done

echo
if [[ "${remaining_duplicates}" -eq 1 ]]; then
  echo "Warning: duplicates still exist in effective config (possibly system scope or include files)."
  echo "Inspect with: git config --show-origin --show-scope --list"
  exit 2
fi

echo "Cleanup completed successfully."