const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:63209';

const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
      "/tweets",
      "/dataHub",
      "/signalr"
    ],
    target: target,
    secure: false,
    logLevel: "debug",
    ws: true
  }
]

module.exports = PROXY_CONFIG;
