## Publisher tests 
#

This test suite contains a typescript script to test the publishing functionality of EES. 

## Project struture 
* index.ts - main entrypoint / script responsible for running the publisher test
* ~/utils - Contains helper functions that are used in `index.ts`
* types.ts - Contains type declarations used in `index.ts`


## Dependencies: 

* Dependencies are managed via npm 

  * Globby - handles file globs 
  * node-stream-zip - handles reading & extraction of zip files 
  * uuid - generates unique uuids 
  * node-stream-zip - handles compressing files to zip archives   
  * rimraf - handles deletion of directories / files 
  * form-data - handles the process of uploading form data to the API 


## Available NPM commands: 

* `npm run clean` - removes various directories, test files & transpiled javascript files 


* `npm run watch` - Watches any typescript files for changes and transpiles typescript files down to common js 


* `npm run build` - transpiles the `index.ts` file down to `index.js` (common JS). This is the command you should use if you want to run the publisher script and are not developing new features to this script. 


* `npm run start` - Runs the transpiled `index.js` file which will run the publisher script (useful for when developing new functionality)


## Getting started 
* Install dependencies by running `npm i` in ~/publisher-tests
* Place an zip file containing both a data file & a meta file with the name `archive.zip`
* copy the .env.example file to .env `cp .env .env.example`
* Fill out the .env file with the values based on what environment you are testing against (you can get these values from Admin EES)
* Run `npm run build` to generate the transpiled javascript 
* Run `npm run start` to run the script 
* Alternatively if you want to publish more than one release you can run the `multiple-publications` bash script with `./multiple-publications.sh`
