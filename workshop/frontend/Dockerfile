# Build stage
FROM node:16 AS build

WORKDIR /app

COPY package*.json ./
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
ENV PORT=3001

# Expose the correct port
EXPOSE 3001

# Create a shell script to run the server
RUN echo '#!/bin/sh\n\nnode server.js' > /app/run.sh
RUN chmod +x /app/run.sh

CMD ["/app/run.sh"]
