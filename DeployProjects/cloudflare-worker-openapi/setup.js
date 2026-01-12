#!/usr/bin/env node

/**
 * Cloudflare Worker Build Number Sync - Unified Setup Script
 * This script detects the operating system and runs the appropriate setup script
 */

const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

console.log('🚀 Starting Cloudflare Worker Build Number Sync setup...');

// Detect operating system
const isWindows = process.platform === 'win32';
const isMac = process.platform === 'darwin';
const isLinux = process.platform === 'linux';

// Check for npm
try {
    execSync('npm --version', { stdio: 'pipe' });
    console.log('✅ npm is available');
} catch (error) {
    console.error('❌ Error: npm is not installed. Please install Node.js and npm first.');
    process.exit(1);
}

// Determine which script to run
let scriptPath;
let scriptName;

if (isWindows) {
    scriptPath = path.join(__dirname, 'setup.ps1');
    scriptName = 'setup.ps1';
} else {
    scriptPath = path.join(__dirname, 'setup.sh');
    scriptName = 'setup.sh';
}

// Check if the script exists
if (!fs.existsSync(scriptPath)) {
    console.error(`❌ Error: ${scriptName} not found`);
    process.exit(1);
}

console.log(`📜 Running ${scriptName}...`);

try {
    if (isWindows) {
        // For Windows, we need to run PowerShell
        execSync(`powershell -ExecutionPolicy Bypass -File "${scriptPath}"`, { 
            stdio: 'inherit',
            shell: true
        });
    } else {
        // For Unix-like systems, make sure the script is executable
        try {
            fs.chmodSync(scriptPath, '755');
        } catch (error) {
            // Ignore chmod errors, just try to run it
        }
        
        execSync(`./${scriptName}`, { 
            stdio: 'inherit',
            shell: true,
            cwd: __dirname
        });
    }
    
    console.log('🎉 Setup completed successfully!');
} catch (error) {
    console.error('❌ Error: Setup failed');
    process.exit(1);
}