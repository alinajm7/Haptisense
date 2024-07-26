#!/bin/bash

# Define the repository URL
REPO_URL="https://github.com/alinajm7/Haptisense"

# Generate the tree view with links in Markdown format
generate_tree() {
  local dir="$1"
  local prefix="$2"
  
  for item in "$dir"/*; do
    if [ -d "$item" ]; then
      echo "${prefix}- [$(basename "$item")]($REPO_URL/tree/main/${item#./})/"
      generate_tree "$item" "$prefix  "
    else
      echo "${prefix}- [$(basename "$item")]($REPO_URL/blob/main/${item#./})"
    fi
  done
}

# Create the output file
OUTPUT_FILE="project_tree_view.md"
echo "# Project Tree View" > "$OUTPUT_FILE"
generate_tree "." "" >> "$OUTPUT_FILE"

# Output result
echo "Tree view has been generated in $OUTPUT_FILE"
