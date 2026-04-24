const path = require('path');
module.exports = {
  entry: './ChatBot.js',
  output: {
    filename: 'chatbot.bundle.js',
    path: path.resolve(__dirname, '.'),
  },
  mode: 'production',
};