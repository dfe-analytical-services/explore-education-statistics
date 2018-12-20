FROM node:10.14.2-alpine

# set working directory
RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app

# add `/usr/src/app/node_modules/.bin` to $PATH
ENV PATH /usr/src/app/node_modules/.bin:$PATH

COPY package.json .
RUN npm install

CMD ["npm", "start"]
