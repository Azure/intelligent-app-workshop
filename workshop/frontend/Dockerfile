# Build stage
FROM node:16 AS build

WORKDIR /app

COPY package*.json ./
RUN npm install
RUN npm ci

COPY . .
RUN npm run build

# Production stage
FROM node:16-alpine

WORKDIR /app

COPY --from=build /app/build ./build
COPY --from=build /app/package*.json ./
COPY --from=build /app/server.js ./

# Install production dependencies
RUN npm ci --only=production

# Set environment variables
ENV NODE_ENV=production
ENV PORT=80

# Expose the correct port
EXPOSE 80

# Create a shell script to run the server
RUN echo 'node server.js' > /app/run.sh
RUN chmod +x /app/run.sh

CMD ["/app/run.sh"]
