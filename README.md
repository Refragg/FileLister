# FileLister

A simple file server with a web directory viewer

## Deployment:

- Install docker on the target machine
- Clone this repository
- Set the TITLE_SUFFIX and FILES_ROOT_DIRECTORY environment variables
- Change the file at `FileLister/wwwroot/header-img.png` to add your own branding

- Run `docker compose up -d` in the repo's root directory, (-d makes it run in the background)
- Run `docker compose down` to stop the server

This assumes that you have a reverse proxy setup that will forward the requests to this server