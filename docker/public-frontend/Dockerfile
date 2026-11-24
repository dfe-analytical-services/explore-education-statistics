FROM node:22.23.4-alpine

ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"

RUN npm i -g corepack@latest
RUN corepack enable

WORKDIR /app

# The public site is built by the pipeline before this image is built (see the "PublicSiteBuild" job in
# azure-pipelines-main.yml), so this image only packages that build output along with production
# dependencies, rather than building it a second time.
COPY package.json .
COPY pnpm-lock.yaml .
COPY pnpm-workspace.yaml .
COPY src/explore-education-statistics-common/ ./src/explore-education-statistics-common/
COPY src/explore-education-statistics-frontend/ ./src/explore-education-statistics-frontend/

RUN test -f ./src/explore-education-statistics-frontend/.next/BUILD_ID || \
    (echo "No public site build output found - run 'pnpm --filter=explore-education-statistics-frontend build' before building this image." && exit 1)

RUN --mount=type=cache,id=pnpm,target=/pnpm/store \
    pnpm --filter=explore-education-statistics-frontend... --prod --ignore-scripts --frozen-lockfile install

WORKDIR /app/src/explore-education-statistics-frontend

ENV NODE_ENV=production
EXPOSE 3000

USER node
CMD ["pnpm", "start"]
