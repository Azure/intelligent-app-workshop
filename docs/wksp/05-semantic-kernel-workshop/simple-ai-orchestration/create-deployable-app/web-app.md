# Creating simple Web UI

The UI was created using React JS and a Node JS proxy to the API. Here is a highlight of key files:

* `workshop/frontend`
  * `Dockerfile` - Dockerfile for building and deploying web app
  * `.env` - local file used to provide configuration values (e.g. url)
  * `package.json` - required package dependencies
  * `server.js` - NodeJS application code
  * `src` - React JS application source code directory
    * `App.tsx` - main application code
    * `index.tsx` - application entry point

## Running Web UI locally

### Build Web UI

1. Go to the frontend directory

   ```bash
   cd workshop/frontend
   ```

1. Run `npm install` to get required dependencies

1. Run `npm run build` to build the React application

### Run Web UI

1. Create `.env` file in `frontend` directory and provide the following required values:
    1. `API_URL` - URL to the backend API
    1. `PORT` - port where React app is running
    1. `REACT_APP_PROXY_URL` - url to the Node JS proxy

    ```shell
    PORT=3001
    REACT_APP_PROXY_URL=/api/chat
    ```

1. Start backend API using `dotnet run`

1. On a separate terminal start React application using `npm start`

1. On a separate terminal export the following required variables for NodeJS application:

    ```bash
    PORT=3001
    API_URL=http://localhost:5000/chat
    ```

    1. Start the NodeJS application using `node server.js`

1. Navigate to browser on <http://localhost:3001> and test the chat application.
