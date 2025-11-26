# Rolus Version Server

Simple Express server to host the `version.json` file for the Rolus launcher.

## Setup on Railway

1. **Create a new project on Railway:**
   - Go to [railway.app](https://railway.app)
   - Click "New Project"
   - Select "Deploy from GitHub repo" (or "Empty Project" and upload files)

2. **If using GitHub:**
   - Create a new repository or use an existing one
   - Push this `railway-server` folder to your repo
   - Connect the repo to Railway

3. **If uploading directly:**
   - Create a new project on Railway
   - Upload the files in this folder
   - Railway will auto-detect Node.js and install dependencies

4. **Add your version.json:**
   - Copy your `version.json` file into this `railway-server` folder
   - Commit and push (or upload if not using Git)

5. **Set up public access:**
   - Click "Generate Domain" in the Networking section
   - Railway will create a URL like: `https://rolus-production-xxxx.up.railway.app`
   - Your version.json will be at: `https://rolus-production-xxxx.up.railway.app/version.json`

6. **Optional: Add token security (recommended for extra privacy):**
   - In Railway, go to your service → Variables
   - Add a new variable: `VERSION_TOKEN` = `your-secret-token-here`
   - Update the launcher URL to include the token: `https://your-project.up.railway.app/version.json?token=your-secret-token-here`

7. **Update the launcher:**
   - Edit `LauncherService.cs` in the launcher project
   - Update `VERSION_URL` to your Railway URL (with token if you added one)

## Updating version.json

Just update the `version.json` file in this folder and push to Railway. The launcher will automatically fetch the new version on next check.

## Custom Domain (Optional)

Railway allows you to add a custom domain:
- Go to your project settings
- Add a custom domain
- Update the `VERSION_URL` in the launcher to use your custom domain

