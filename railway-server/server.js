const express = require('express');
const path = require('path');
const app = express();
const PORT = process.env.PORT || 3000;

const VERSION_TOKEN = process.env.VERSION_TOKEN || null;

app.use(express.json());

app.use((req, res, next) => {
  res.header('Access-Control-Allow-Origin', '*');
  res.header('Access-Control-Allow-Methods', 'GET, OPTIONS');
  res.header('Access-Control-Allow-Headers', 'Content-Type');
  next();
});

app.get('/version.json', (req, res) => {
  if (VERSION_TOKEN && req.query.token !== VERSION_TOKEN) {
    return res.status(403).json({ error: 'Unauthorized' });
  }
  res.setHeader('Content-Type', 'application/json');
  res.sendFile(path.join(__dirname, 'version.json'));
});

app.get('/', (req, res) => {
  res.json({ message: 'Rolus Version Server', endpoint: '/version.json' });
});

app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
});

