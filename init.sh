#!/usr/bin/env bash
set -e

# Usage: ./init.sh NewProjectName
# Example: ./init.sh MyShop

TARGET_NAME="$1"

if [ -z "$TARGET_NAME" ]; then
  echo "Usage: ./init.sh <NewProjectName>"
  echo "Example: ./init.sh MyCompany.Inventory"
  exit 1
fi

OLD_NAME="SynergyFlow"
OLD_NAME_LOWER="synergyflow"
TARGET_NAME_LOWER=$(echo "$TARGET_NAME" | tr '[:upper:]' '[:lower:]' | tr -cd '[:alnum:]')

echo "🔄 Initializing template with project name: '$TARGET_NAME'..."

# 1. Replace occurrences in files
echo "✏️ Replacing namespaces, usings, and configuration strings..."
find . -type f \
  ! -path '*/.*' \
  ! -path '*/bin/*' \
  ! -path '*/obj/*' \
  ! -path '*/TestResults/*' \
  ! -name 'init.sh' \
  -exec sed -i "s/$OLD_NAME/$TARGET_NAME/g" {} +

# Replace lowercase strings in compose and env files
find compose.yaml .env.example README.md -type f 2>/dev/null \
  -exec sed -i "s/$OLD_NAME_LOWER/$TARGET_NAME_LOWER/g" {} + || true

# 2. Rename files
echo "📁 Renaming files..."
find . -depth -name "*$OLD_NAME*" \
  ! -path '*/.git*' \
  ! -path '*/bin/*' \
  ! -path '*/obj/*' \
  | while read -r file; do
      new_file=$(echo "$file" | sed "s/$OLD_NAME/$TARGET_NAME/g")
      mv "$file" "$new_file"
    done

# 3. Rename directories
echo "📂 Renaming directories..."
find . -depth -type d -name "*$OLD_NAME*" \
  ! -path '*/.git*' \
  ! -path '*/bin/*' \
  ! -path '*/obj/*' \
  | while read -r dir; do
      new_dir=$(echo "$dir" | sed "s/$OLD_NAME/$TARGET_NAME/g")
      mv "$dir" "$new_dir"
    done

echo "✅ Project successfully renamed to $TARGET_NAME!"
echo "🚀 Run 'dotnet restore' and 'dotnet build' to get started."
