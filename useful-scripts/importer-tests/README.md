## importer tests 
#

This test suite contains tests which test the importer functionality of EES. 

## Project structure 
* index.ts - main entrypoint / script responsible for running the importer tests
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


* `npm run watch` - Watches any typescript files for changes and compiled typescript files down to common js 


* `npm run build` - compiles the `index.ts` file down to Common JS. This is the command you should use if you want to run the importer script and are not adding new features to this script. 


* `npm run start` - Runs the compiled `index.js` file which will run the importer script (useful for when adding new features)

## Getting started 
* Install dependencies by running `npm i` in ~/importer-tests
* Place an zip file containing both a data file & a meta file with the name `archive.zip`
* copy the .env.example file to .env `cp .env .env.example`
* Fill out the .env file with the values based on what environment you are testing against (you can get these values from Admin EES)
* Run `npm run build` to compile typescript down to Common JS
* Run `npm run start` to run the script 
* Alternatively if you want to import more than one subject you can run the `multiple-publications` bash script with `./multiple-imports.sh`
