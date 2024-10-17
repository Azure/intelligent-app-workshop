# Simple Copilot Chat Frontend

A React-based chat application for interacting with an AI assistant (backend), designed for deployment on Azure Container Apps.

## Getting Started

### Prerequisites

- **Node.js** (v16 or higher)
- **npm** (v7 or higher)
- **Git**

### Installation

1. **Install Dependencies:**

   ```bash
   npm install
   ```

1. **Set Environment Variables:**

   Create a `.env` file in the root directory of frontend and add the following environment variables:

   ```bash
   API_URL=https://your-backend-api-url/chat
   PORT=3001
   ```

1. **Run the Application:**

   ```bash
   npm start
   ```

   The application will be available at `http://localhost:3001`.

### Docker Usage (Optional)

1. **Build the Docker Image:**

   ```bash
   docker build -t simple-copilot-frontend .
   ```

1. **Run the Docker Container:**

   ```bash
   docker run -p 3001:3001 -d simple-copilot-frontend
   ```