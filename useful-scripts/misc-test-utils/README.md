## misc test utils 

A cli tool to automate the miscellaneous testing of EES.


### structure 
This tool consists of 3 areas.

* `index.ts` - Entrypoint file. Contains inquirer.js code responsible for asking users questions, getting input etc.

* `modules` - utilities responsible for making calls to API and performing functions that the user requests in the entrypoint file

* `utils` - contains various utilities that the files in `modules` use.


### Getting Started
* Ensure you have Node / NPM installed and are using at least Node V12
* copy the `.env.example` file to `.env` (`cp .env.example .env`) and fill out with your own values.
* if you want to upload a subject or publish a new release place a file containing a data and a meta file named `archive.zip` in the root of this directory (`~misc-test-utils/`)
* Install dependencies: `npm ci`
* Start the tool: `npm run start`
* clean test data, node_modules etc.: `npm run clean`
* generate new `.env` types: `npm run gen-env`
