# syntax=docker/dockerfile:1

FROM node:20-alpine AS build
ARG APP_NAME
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npx nx build "${APP_NAME}" --configuration=production

FROM nginx:alpine AS final
ARG APP_NAME

COPY nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/apps/${APP_NAME}/browser /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
