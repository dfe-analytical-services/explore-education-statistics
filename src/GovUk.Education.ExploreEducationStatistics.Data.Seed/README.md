# Explore education statistics service : The Seeder

## Requirements

- The seeder process requires a connection to an Azure Storage account in order to copy the files to be processed. If running storage locally then the system will only run on windows using the Azure Storage emulator.
- Ensure that you have the Azure tools plugin installed in Rider if you intend to seed locally. 
- Ideally install the Azure Storage Explorer tool https://azure.microsoft.com/en-gb/features/storage-explorer/

## Steps to seed a new statistics database.

In order to seed sample data into an EES Statistics SQL Server database:

- If seeding a remote DB i.e. the ```statistics``` & ```public-statistics``` DBs in Azure cloud then you will need to change the Azure deployment process to ensure that the Data Processor function is not
restarted during a currently running seed as a consequence of others performing further deployments (Login to Azure and edit the Release Task for the Data Processor Function)
- Download the sample csv files from from the following location (data file & accompanying meta csv files): https://github.com/dfe-analytical-services/ees-test-data/tree/master/csvs/large
- Create the following dir if not existing: 
```
~/projects\explore-education-statistics\src\GovUk.Education.ExploreEducationStatistics.Data.Seed\bin\Debug\netcoreapp3.0\Files
```
- Extract any csv file from the zip files & copy all csv files to the ```Files``` directory i.e. 
```
~\projects\explore-education-statistics\src\GovUk.Education.ExploreEducationStatistics.Data.Seed\bin\Debug\netcoreapp3.0\Files
```

- Modify the appsettings.json file in the Seed project so that ```CoreStorage``` connection string points to the correct storage.
- Delete the ```releases container``` from the Blob Storage if existing.
- Run the seeder locally.
- Once the seeder has run then check that the ```releases``` container has been created & that it contains the uploaded csv files for each release.
- Delete & re-create both the statistics & content DBs or clear the contents by running the following scripts:
```~\projects\explore-education-statistics\useful-scripts\sql\drop-all-statistics-db-objects.sql```
```~\projects\explore-education-statistics\useful-scripts\sql\drop-all-content-db-objects.sql```
- Run the admin app which will re-create the schemas & restart the Content api to re-create the content schema & add fixed sample data.
- Run the Processor function (if you are seeding locally)
- You can observe the progress of each processed subject by examing the ```Status``` column in the imports table using the Azure Storage Explorer tool.
- When all are flagged as COMPLETE then add the footnotes data by opening a connection to the statistics DB previously seeded & run the script: 
```~\projects\explore-education-statistics\useful-scripts\sql\FootnoteData.sql```
- Run the data-api tests in postman to check that all tests are passing with the new data.
- Repeat the process against the public-statistics DB (Only required if not seeding locally. You will need to restart the data-api to re-create the public-statistics schema,
 and change the Importer function configuration to use the public-statistics database. Don't forget to switch the connection string back once the import is complete.).
- In order for the public site to have visible links to the uploaded files in its download files section you will need to public the content (See as yet unwritten README for that :).