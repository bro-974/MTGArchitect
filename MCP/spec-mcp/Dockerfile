FROM node:22-alpine

WORKDIR /app

COPY package.json .
RUN npm install

COPY server.js .

# Specs are mounted at runtime, not baked in
VOLUME ["/app/specs"]

CMD ["node", "server.js"]