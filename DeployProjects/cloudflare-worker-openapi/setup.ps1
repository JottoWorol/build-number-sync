<#
Cloudflare Worker Build Number Sync - Setup Script (Windows)
This script automates the setup process for the project
#>

Write-Host "🚀 Starting Cloudflare Worker Build Number Sync setup..."

# Step 1: Check for npm
try {
    npm --version | Out-Null
    Write-Host "✅ npm is available"
} catch {
    Write-Host "❌ Error: npm is not installed. Please install Node.js and npm first."
    exit 1
}

# Step 2: Install wrangler
try {
    npm list wrangler | Out-Null
    Write-Host "✅ Wrangler is already installed"
} catch {
    Write-Host "📦 Installing wrangler..."
    npm install -D wrangler@latest
}

# Step 3: Check wrangler login
try {
    npx wrangler whoami | Out-Null
    Write-Host "✅ You are already logged in to Cloudflare"
} catch {
    Write-Host "🔑 Please log in to Cloudflare..."
    npx wrangler login
}

# Step 4: Create KV namespace
$kvExists = npx wrangler kv namespace list | Select-String "BUILD_VERSION_SYNC"
if (-not $kvExists) {
    Write-Host "💾 Creating KV namespace..."
    
    # Create the namespace
    npx wrangler kv namespace create "BUILD_VERSION_SYNC"
    
    # Get the namespace ID from the list
    $kvList = npx wrangler kv namespace list
    $kvIdLine = $kvList | Select-String -Pattern '"id": "[^"]*"' -Context 0,1 | Where-Object { $_.Line -match "BUILD_VERSION_SYNC" }
    
    if (-not $kvIdLine) {
        Write-Host "❌ Error: Failed to create KV namespace or extract ID"
        exit 1
    }
    
    $kvId = $kvIdLine.Line -replace '.*"id": "([^"]*)".*', '$1'
    Write-Host "✅ Created KV namespace with ID: $kvId"
    
    # Check if KV namespace already exists in wrangler.jsonc
    $kvConfigExists = Get-Content wrangler.jsonc | Select-String "BUILD_VERSION_SYNC"
    if ($kvConfigExists) {
        Write-Host "✅ KV namespace already configured in wrangler.jsonc"
    } else {
        # Step 5: Update wrangler.jsonc with the new KV namespace ID
        Write-Host "📝 Updating wrangler.jsonc..."
        
        # Check if jq is available
        try {
            jq --version | Out-Null
        } catch {
            Write-Host "❌ Error: jq is required but could not be found"
            Write-Host "Please install jq manually and run this script again"
            Write-Host "You can install it from: https://stedolan.github.io/jq/download/"
            exit 1
        }
        
        # Create a temporary file without comments for jq to process
        (Get-Content wrangler.jsonc) -replace '^\s*/\\*.*$|^\s*\\*.*$|^\s*\\*.*$', '' | Out-File -FilePath wrangler.no_comments.jsonc -Encoding utf8
        
        # Update the KV namespace ID using jq
        jq ".kv_namespaces[0].id = `"$kvId`"" wrangler.no_comments.jsonc > wrangler.tmp.jsonc
        Move-Item -Force wrangler.tmp.jsonc wrangler.jsonc
        
        # Clean up temporary file
        Remove-Item -Force wrangler.no_comments.jsonc
        
        Write-Host "✅ Updated wrangler.jsonc with new KV namespace ID"
    }
    
    Write-Host "✅ Updated wrangler.jsonc with new KV namespace ID"
} else {
    Write-Host "✅ KV namespace already exists"
}

# Step 6: Deploy the project
Write-Host "🌍 Deploying the project..."
npx wrangler deploy

if ($LASTEXITCODE -eq 0) {
    Write-Host "🎉 Setup completed successfully!"
    Write-Host "Your Cloudflare Worker Build Number Sync is now deployed."
} else {
    Write-Host "❌ Error: Deployment failed"
    exit 1
}