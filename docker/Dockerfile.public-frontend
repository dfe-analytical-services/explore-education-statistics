FROM node:18.17.0-alpine AS base

ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"

RUN corepack enable

WORKDIR /app

FROM base AS builder

ARG BUILD_BUILDNUMBER
ENV BUILD_BUILDNUMBER=$BUILD_BUILDNUMBER

COPY .npmrc .
COPY package.json .
COPY pnpm-lock.yaml .
COPY pnpm-workspace.yaml .
COPY src/explore-education-statistics-common/ ./src/explore-education-statistics-common/
COPY src/explore-education-statistics-frontend/ ./src/explore-education-statistics-frontend/
COPY src/tsconfig.json ./src/tsconfig.json

RUN --mount=type=cache,id=pnpm,target=/pnpm/store \
    pnpm --filter=explore-education-statistics-frontend... --ignore-scripts --frozen-lockfile install

RUN pnpm --filter=explore-education-statistics-frontend build

FROM base AS runner

COPY --from=builder /app/.npmrc .
COPY --from=builder /app/package.json .
COPY --from=builder /app/pnpm-lock.yaml .
COPY --from=builder /app/pnpm-workspace.yaml .
COPY --from=builder /app/src/explore-education-statistics-common/ ./src/explore-education-statistics-common/
COPY --from=builder /app/src/explore-education-statistics-frontend/ ./src/explore-education-statistics-frontend/

RUN corepack enable

RUN --mount=type=cache,id=pnpm,target=/pnpm/store \
    pnpm --filter=explore-education-statistics-frontend... --prod --ignore-scripts --frozen-lockfile install

WORKDIR /app/src/explore-education-statistics-frontend

ENV NODE_ENV=production
EXPOSE 3000

USER node
CMD ["pnpm", "start"]
