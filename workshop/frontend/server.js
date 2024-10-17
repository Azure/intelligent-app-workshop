const express = require('express');
const cors = require('cors');
const axios = require('axios');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3001;

app.use(cors());
app.use(express.json());

// Serve static files from the React app
app.use(express.static(path.join(__dirname, 'build')));

const API_URL = process.env.API_URL || 'https://ca-web-5ssljjzhgthbc.agreeablesea-63caf67a.eastus2.azurecontainerapps.io/chat';

app.post('/api/chat', async (req, res) => {
  try {
    const response = await axios.post(API_URL, req.body, {
      headers: {
        'Content-Type': 'application/json',
        'accept': 'text/plain'
      }
    });
    res.json(response.data);
  } catch (error) {
    console.error('Error:', error.message);
    res.status(500).json({ error: 'An error occurred while processing your request' });
  }
});

// The "catchall" handler: for any request that doesn't
// match one above, send back React's index.html file.
app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'build', 'index.html'));
});

app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
  console.log(`API URL: ${API_URL}`);
});
