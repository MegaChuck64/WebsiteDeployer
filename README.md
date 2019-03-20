# WebsiteDeployer

This is a WPF app I made using LibGit2Sharp to simplify the git commands 
that I have to do to pull and push my github pages website. 

Now I don't need to use the command line, I can just open this app
press the download or upload button with the optional commit message text box

Credentials, for right now, are just held in a text file two levels above the highest project directory for this repo

There's also controls for entering username and password directly into app

Credentials: 
  gitCred.txt () 
  first line: username
  second line: password
  
  
Download: 
  Checks if we have the repo already on the computer
  If not clone it
  if we do, pull the latest 
  (Download will make your local repo look just like the remote repo)
  
Upload:
  Add all changed files (Staging)
  Commit with the commit message from text box
  Push to master
  (Upload will make the remote repo look just like your local one)
  
  
  

   
    
