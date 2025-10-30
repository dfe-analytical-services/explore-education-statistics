# Seed data

## Overview

Scripts in this folder allow us to generate seed data on environments so that we are able to run our read-only UI tests
against those environments.

The following seed data scripts are responsible for setting up the following data:

- `generate_seed_data_theme_1.robot` - everything under the "UI test theme - SEED DATA - Pupils and schools" Theme.
- `generate_seed_data_theme_2.robot` - everything under the "UI test theme - SEED DATA - Publication and Release UI Permissions" Theme.

## Generating seed data on a target environment

In order to create seed data for a target environment, follow these steps: 

1. Remove any existing seed data themes from that environment that you are wishing to replace. For example, if wanting to generate
   "UI test theme - SEED DATA - Pupils and schools" against an environment, firstly remove the existing theme from the environment
   if it already exists there.
   - This is achieved by using the `DELETE /api/themes/{theme id}` Admin API endpoint.
2. Clear public and private caches - this prevents any issues with new seed data slotting into place under existing cache folders and 
   it being unclear on what is old cache files and what is new. This is particularly important for public cache folders which are 
   slug-based paths, and can lead to situations whereby old and new cached data will exist in these folders after seeding is complete.
3. Run the following command to generate the seed data (in this example, to seed "UI test theme - SEED DATA - Pupils and schools"):
  ```bash
  pipenv run python3 run_tests.py -e <environment> -i robot --reseed -f tests/seed_data/generate_seed_data_theme_1.robot
  ```
  The script will run to completion and then the theme and all of its data (including real data files) will be available in the
  target environment.

## Regenerating local seed data and distributing new database dump files and storage files

In order to provide a new local seed data dump, we will generally follow the same steps as above, firstly by clearing out any existing 
themes that we wish to replace, and then running the scripts to regenerate them.

There are some additional clean-up tasks that are also useful to do prior to issuing a new dump file, in order to keep it as clean as
possible.

We will also be generating a CSV which allows us to upload seed data files to the local storage emulator. 

### Generating a new database dump

As a full run-through, we can do the following steps to provide a new and clean data dump:

1. Fire up the environment locally, with a fresh set of the latest data dump in our dbs.
2. Delete theme "UI test theme - SEED DATA - Pupils and schools".
3. Delete theme "UI test theme - SEED DATA - Publication and Release UI Permissions".
4. Clear out old Subject data from the `statistics` database. Against the `statistics` database, run the following:
   1. Mark any remaining Subjects as soft-deleted - `UPDATE Subject SET SoftDeleted = 1;`
   2. Run the stored procedure to delete any soft-deleted Subjects - `EXEC RemoveSoftDeletedSubjects 1000000, 10000, 10000;`
   3. Remove any existing Locations - `DELETE FROM Location;`
5. At this point, we should have a minimal set of data in our databases. We can now recover space:
   1. Against the `statistics` database, run:
      ```sql
      ALTER DATABASE [statistics] SET RECOVERY SIMPLE;
      DBCC SHRINKFILE (statistics_log, 1);
      DBCC SHRINKFILE ([statistics], 1);
      ALTER DATABASE [statistics] SET RECOVERY FULL;
      ```
   1. Against the `content` database, run:
      ```sql
      ALTER DATABASE [content] SET RECOVERY SIMPLE;
      DBCC SHRINKFILE (content_log, 1);
      DBCC SHRINKFILE ([content], 1);
      ALTER DATABASE [content] SET RECOVERY FULL;
      ```
6. At this point, we now have database files with minimal footprint. Now we can generate the seed data with the following
   steps:
   1. Restart the environment locally - this helps to prevent any issues with any cached data that no longer exists in the 
      underlying database due to it having been cleaned down manually. An example of this would be the cache of existing 
      Locations held in the Importer project.
   2. Ensure that the latest `seed-data-files.zip` is downloaded and located in the 
      [tests/robot-tests/tests/files](tests/robot-tests/tests/files) folder.
   3. Run:
      ```bash
      pipenv run python3 run_tests.py -e local -i robot --reseed -f tests/seed_data/generate_seed_data_theme_1.robot
      ```
   4. Run:
       ```bash
       pipenv run python3 run_tests.py -e local -i robot --reseed -f tests/seed_data/generate_seed_data_theme_2.robot
       ```
7. At this point, we now have databases with the correct seed data imported. We can now take a backup of these database files
   from [data/ees-mssql](data/ees-mssql) as a new `ees-mssql-data-<version>.zip` file, as per our standard approach. 

Note that before running the UI tests against this new dump, it is best to clear out public and private caches form storage so 
as to prevent any confusion with old and new cache files existing under similar folders.

### Generating a new seed data files CSV for local storage

In order to allow us to upload real files to our local storage emulators, we need to provide a CSV that will allow the
[create_emulator_release_files.py](tests/robot-tests/tests/libs/create_emulator_release_files.py) to upload correct files into
the appropriate locations in storage, as these are tied to ids which will change each time that seed data is regenerated.

To do this, perform the following steps:

1. Run the following SQL against the `content` database:
   ```sql
   SELECT ReleaseFiles.ReleaseVersionId, Files.Id, Files.Filename, Files.Type
   FROM Files
   JOIN ReleaseFiles ON Files.Id = ReleaseFiles.FileId
   AND Files.Type IN ('Data', 'Metadata')
   ORDER BY ReleaseFiles.ReleaseVersionId, Files.Filename, Files.Type;
   ```
2. Save the contents to 
   [tests/robot-tests/tests/seed_data/seed_data_emulator_files.csv](tests/robot-tests/tests/seed_data/seed_data_emulator_files.csv)
   and ensure that it is saved with a CSV heading row, as this is what is expected.

The next time that someone runs the UI tests locally, the run tests process will upload any missing files to storage.

Note that the process that uploads files into local storage when the tests run will expect to find files with matching filenames
in `seed-data-files.zip`, which is then unpacked into the 
[.unzipped-seed-data-files folder](tests/robot-tests/tests/files/.unzipped-seed-data-files). If new files have been included in an 
update to our seed data process, they must be added 

## Regenerating visual testing scripts based on seed data

Locally we run a [visual testing script](tests/robot-tests/tests/visual_testing/visually_check_tables_and_charts.seed_data.robot)
to ensure our visual testing process is maintained and able to be used when we need it.

This is reliant on ids from seed data, and so after regenerating seed data for a local dump, we also need to update this script
and its seed data csv also.

To do this, complete the steps in [the visual testing README](tests/robot-tests/scripts/visual-testing/README.md) for the local 
environment. This will produce an updated
[visual testing script](tests/robot-tests/tests/visual_testing/visually_check_tables_and_charts.seed_data.robot)
and [visual testing CSV](tests/robot-tests/tests/visual_testing/visually_check_tables_and_charts.seed_data.robot.csv) that
represent the new seed data ids.
