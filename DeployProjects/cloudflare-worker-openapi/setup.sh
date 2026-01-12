#!/bin/bash

# Cloudflare Worker Build Number Sync - Setup Script
# This script automates the setup process for the project

echo "🚀 Starting Cloudflare Worker Build Number Sync setup..."

# Step 1: Check for npm
if ! command -v npm &> /dev/null; then
    echo "❌ Error: npm is not installed. Please install Node.js and npm first."
    exit 1
fi
echo "✅ npm is available"

# Step 2: Install wrangler
if ! npm list wrangler &> /dev/null; then
    echo "📦 Installing wrangler..."
    npm install -D wrangler@latest
else
    echo "✅ Wrangler is already installed"
fi

# Step 3: Check wrangler login
if ! npx wrangler whoami &> /dev/null; then
    echo "🔑 Please log in to Cloudflare..."
    npx wrangler login
else
    echo "✅ You are already logged in to Cloudflare"
fi

# Step 4: Create KV namespace
if ! npx wrangler kv namespace list | grep -q "BUILD_VERSION_SYNC"; then
    echo "💾 Creating KV namespace..."
    
    # Create the namespace
    npx wrangler kv namespace create "BUILD_VERSION_SYNC"
    
    # Get the namespace ID from the list (more reliable than parsing creation output)
    KV_ID=$(npx wrangler kv namespace list | grep -B 1 "BUILD_VERSION_SYNC" | grep '"id"' | cut -d'"' -f4)
    
    if [ -z "$KV_ID" ]; then
        echo "❌ Error: Failed to create KV namespace or extract ID"
        exit 1
    fi
    
    echo "✅ Created KV namespace with ID: $KV_ID"
    
    # Check if KV namespace already exists in wrangler.jsonc
    if grep -q "BUILD_VERSION_SYNC" wrangler.jsonc; then
        echo "✅ KV namespace already configured in wrangler.jsonc"
    else
        # Step 5: Update wrangler.jsonc with the new KV namespace ID
        echo "📝 Updating wrangler.jsonc..."
        
        # Use jq to update the JSON file (install jq if not available)
        if ! command -v jq &> /dev/null; then
            echo "📦 Installing jq for JSON manipulation..."
            if [[ "$OSTYPE" == "linux-gnu"* ]]; then
                sudo apt-get install -y jq
            elif [[ "$OSTYPE" == "darwin"* ]]; then
                brew install jq
            else
                echo "❌ Error: jq is required but could not be installed automatically on this OS"
                echo "Please install jq manually and run this script again"
                exit 1
            fi
        fi
        
        # Create a temporary file without comments for jq to process
        grep -v '^\s*/\*' wrangler.jsonc | grep -v '^\s*\*/' | grep -v '^\s*\*' > wrangler.no_comments.jsonc
        
        # Update the KV namespace ID using jq
        jq '.kv_namespaces[0].id = "'"$KV_ID"'"' wrangler.no_comments.jsonc > wrangler.tmp.jsonc && mv wrangler.tmp.jsonc wrangler.jsonc
        
        # Clean up temporary file
        rm -f wrangler.no_comments.jsonc
        
        echo "✅ Updated wrangler.jsonc with new KV namespace ID"
    fi
else
    echo "✅ KV namespace already exists"
fi

# Step 6: Deploy the project
echo "🌍 Deploying the project..."
npx wrangler deploy

if [ $? -eq 0 ]; then
    echo "🎉 Setup completed successfully!"
    echo "Your Cloudflare Worker Build Number Sync is now deployed."
else
    echo "❌ Error: Deployment failed"
    exit 1
fi