FROM node:10.14.2-alpine

RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app

ENV PATH /usr/src/app/node_modules/.bin:$PATH

COPY package.json .
RUN npm install

CMD ["npm", "start"]
