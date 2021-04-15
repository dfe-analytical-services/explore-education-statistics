## importer / publisher test suite
#

Test suite to test the importing / publishing functionality of EES

## Project structure 
* index.ts - main entrypoint / script responsible for running the test suite
* ~/utils - Contains helper functions that are used in `index.ts`


## Dependencies: 

* Dependencies are managed via npm 

  * Globby - handles file globs 
  * node-stream-zip - handles reading & extraction of zip files 
  * uuid - generates unique uuids 
  * node-stream-zip - handles compressing files to zip archives   
  * rimraf - handles deletion of directories / files 
  * form-data - handles the process of uploading form data to the API 


## Available NPM commands: 

* `npm run clean` - removes various directories, test files & compiled javascript files 


* `npm run watch` - Watches any typescript files for changes and compiles typescript files down to common js 


* `npm run build` - compiles the `index.ts` file down to Common JS. This is the command you should use if you want to run the test and are not adding new features to the test suite. 


* `npm run test:importer` - runs the importer test 

* `npm run test:publisher` - runs the publisher test 

## Getting started 
* Ensure you have Node v12 or higher installed
* Install dependencies by running `npm i` in ~/importer-publisher-tests
* Place an zip file containing both a data file & a meta file with the name `archive.zip` in the root of the project
* copy the .env.example file to .env `cp .env .env.example`
* Fill out the .env file with the values based on what environment you are testing against (you can get these values from Admin EES)
* Run `npm run watch` to watch typescript files for changes (useful when adding new functionality)
* Run `npm run build` to compile typescript down to common JS
* Run `npm run test:importer` to start the importer test 
* Run `npm run test:publisher` to start the publisher test 
* Alternatively if you want to import or publish more than one subject you can run the `multiple-import-publication.sh` bash script with `./multiple-import-publication.sh -n <num of subjects to upload> <importer or publisher>`
