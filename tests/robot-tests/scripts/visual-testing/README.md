# Visual testing - tables, charts and permalinks

As well as the standard Robot UI tests, we also have a suite of tests to enable visual comparison testing of tables, charts and 
permalinks. We capture "before" and "after" images of tables and charts and use scripts to automatically compare them visually. 
We also capture "before" and "after" HTML of the tables and charts for use when visual comparison is not desirable.

## Tables and Charts

### Overview of the process

#### Generate the CSV

We use a SQL query to generate a CSV of Data Blocks in a given environment. This then gives us the information we need to visit 
each of these Data Blocks in the public site and view them in situ (e.g. as a Key Stat tile, as a Data Block embedded in Content, 
as a Highlight Table, etc). We use this same CSV file to generate a Robot test suite based off a test suite template that visits
each Release with Data Blocks as its own test case. 

#### Generate the test script

With the ability to view each of these tables and charts in situ, we're then able to generate snapshot images of them, as well as 
their HTML.

#### Capture "before" and "after" images

After a code change is deployed, a migration applied or other change is performed on an environment whereby we want to sanity check that 
no changes to the existing tables and charts have occurred (or that only certain expected changes have occurred), we can then run the
tests again and capture snapshot images of the "after" state of each table and chart.

#### Visually compare images

Once we have a set of "before" and "after" snapshot images, we're able to visually compare them to ensure that only expected changes are 
present.

### Running the process

#### Generate the CSV

On the Content DB of the target environment, run the following SQL statement and save the results as a CSV:

```sql
SELECT ContentBlock.Id                                      AS ContentBlockId,
  Releases.Id                                          AS ReleaseId,
  Releases.Slug                                        AS ReleaseSlug,
  Publications.Id                                      AS PublicationId,
  Publications.Slug                                    AS PublicationSlug,
  ContentSections.Id                                   AS ContentSectionId,
  ContentSections.Heading                              AS ContentSectionHeading,
  ContentSections.[Type]                               AS ContentSectionType,
  ContentSections.[Order]                              AS ContentSectionOrder,
  (
    SELECT MIN(cs.[Order]) 
    FROM ContentSections cs 
    JOIN ReleaseContentSections rcs ON rcs.ContentSectionId = cs.Id 
    AND rcs.ReleaseId = Releases.Id
    AND cs.Type = ContentSections.Type
  ) AS MinContentSectionOrder,
  ContentBlock.[Order]                                 AS ContentBlockOrder,
  (
    SELECT MIN(cb.[Order]) 
    FROM ContentBlock cb 
    WHERE cb.ContentSectionId = ContentBlock.ContentSectionId
  )                                                    AS MinContentBlockOrder,
  DataBlock_HighlightName                              AS HighlightName,
  JSON_VALUE([DataBlock_Query], '$.SubjectId')			AS SubjectId,
  JSON_VALUE([DataBlock_Charts], '$[0].Title')         AS ChartTitle,
  JSON_VALUE([DataBlock_Charts], '$[0].Type')          AS ChartType,
  DataBlock_Table										AS TableConfig
FROM ContentBlock
JOIN ReleaseContentBlocks ON ContentBlock.Id = ReleaseContentBlocks.ContentBlockId
JOIN Releases ON ReleaseContentBlocks.ReleaseId = Releases.Id
JOIN Publications ON Publications.Id = Releases.PublicationId 
LEFT JOIN ContentSections ON ContentSections.Id = ContentSectionId
WHERE ContentBlock.Type = 'DataBlock'
  AND Releases.Published IS NOT NULL
  AND Releases.SoftDeleted = 0
  AND (
    -- Include DataBlocks that are linked to Content Sections
    ContentSectionId IS NOT NULL
    -- Include DataBlocks that are used for Featured Tables
    OR (DataBlock_HighlightName IS NOT NULL
        AND DataBlock_HighlightName <> ''))
  -- Include only DataBlocks that are from the latest published Release
  AND NOT EXISTS(
    SELECT 1
    FROM Releases PublicationReleases
    WHERE PublicationReleases.PreviousVersionId = Releases.Id
	  AND PublicationReleases.Published IS NOT NULL
      AND PublicationReleases.SoftDeleted = 0
  );
```
This produces a CSV that allow the UI tests to locate each published Data Block in its correct context.

#### Generate the test script

We now use the CSV to generate a Robot test suite whereby each unique Release with Data Blocks is 
represented as its own test case within the suite.

The reason why we generate it in this fashion is that it allows us to rerun the test suite with the 
`--rerun-failed-tests` option to handle any intermittent failures we encounter, allowing us to get 
the most complete results we can for a given environment.

The test script template is the `visually_check_tables_and_charts.template.robot` file and test suites can 
be generated via the `generate_tables_and_charts_test_cases.py` file.

1. Save the CSV file captured by the first step into the `tests/robot-tests/scripts/visual-testing` folder 
   e.g. `datablocks-dev.csv`.
2. Run the following command:
   
   ```bash
   cd tests/robot-tests
   pipenv run python scripts/visual-testing/generate_tables_and_charts_test_cases.py --file scripts/visual-testing/datablocks-dev.csv --target visually_check_tables_and_charts.dev.robot 
   ```
   
   This will generate a test suite for the Data Blocks (for the Dev environment in this example). In this suite will be an individual test case
   for each Release with published Data Blocks for that environment.

#### Capture "before" and "after" images

Now that we have a generated test script specific to an environment and the data on it, we can run the tests to capture "before" images on that environment.
Using the example of a Dev environment-specific test script above, we can run:

```bash
cd tests/robot-tests
pipenv run python run_tests.py -f tests/visual_testing/visually_check_tables_and_charts.dev.robot -e dev
```

This will iterate through Releases on the environment, capturing images of all of the tables and charts per Release, saving them into a
`test-results/snapshots/%{RUN_IDENTIFIER}` folder. These will be the "before" snapshots. HTML of the tables and charts will also be captured.

Copy the `test-results/snapshots/%{RUN_IDENTIFIER}` folder to a safe location as a subsequent run will cause the folder to be deleted.

When the change under test is applied to the environment, then we can capture the "after" snapshots in the same way as above. These will again be captures
within another `test-results/snapshots/%{RUN_IDENTIFIER}` folder.

Again, copy the `test-results/snapshots/%{RUN_IDENTIFIER}` folder to a safe location as a subsequent run will cause the folder to be deleted.

If any intermittent failures occur whilst running the test suite, it can be rerun using the `--rerun-failed-tests` flag to try the failing Releases again, e.g:

```bash
cd tests/robot-tests
pipenv run python run_tests.py -f tests/visual_testing/visually_check_tables_and_charts.dev.robot -e dev --rerun-failed-tests
```

#### Visually compare images

Now that we have a set of "before" and "after" snapshot images in the two snapshot folders, we're able to visually compare them to 
ensure that only expected changes are present.

From the `tests/robot-tests` folder, run:

```bash
pipenv run python scripts/visual-testing/compare_image_folder_trees.py --first </path/to/before/snapshots> --second </path/to/after/snapshots> --diff </path/to/diffs/folder>
```

This will compare the images in the `</path/to/before/snapshots>` folder with the images in the `</path/to/after/snapshots>` folder.

Any differences will be output to the provided `</path/to/diffs/folder>` folder. This folder will be created if it does not already exist. If 
differences are found, the original, the new, and the diff images are all output to allow manual visual comparisons.

## Permalinks

The process for visually testing Permalinks is the same as with tables and charts above, with the exceptions that the CSV that drives the test is different, 
and that the command to generate the test script is slightly different.

### CSV

The CSV for Permalink testing is a single column of Permalink Ids. For each Id, a test case will be generated. There's no process for generating a CSV of Permalink 
Ids for an environment at this point in time whilst they remain in Table Storage, but known Ids can be supplied in a `permalinks-dev.csv` as an example for the Dev
environment.

### Test script generation

To generate a test script for an environment, e.g. Dev environment using the CSV above, run:

```bash
cd tests/robot-tests
pipenv run python scripts/visual-testing/generate_permalink_test_cases.py --file permalinks-dev.csv --target visually_check_permalinks.dev.robot 
```

This test script can then be run for befores and afters and then images visually compared as per the tables and charts example. The command for running these tests 
for the Dev environment example would be:

```bash
cd tests/robot-tests
pipenv run python run_tests.py -f tests/visual_testing/visually_check_permalinks.dev.robot -e dev
```

## Limitations of visual testing

Currently there are a few limitations with this process:

1. Comparing images of different dimensions - the `compare_image_folder_trees.py` script will attempt to resize images prior to comparing. Currently there are 
   occasional errors whilst resizing images of particular dimensions.
2. Occasional capture of a table and chart before all styling has been applied - this will lead to visual differences due to, for instance, the correct font not 
   being applied yet, due to a flash of unstyled content.
3. Tables being too large for image capture - in some environments (prior to the maximum table cell limit being applied), some tables are so huge that Selenium
   is unable to successfully take a snapshot, or is unable to maximise a browser enough to take a full snapshot without scrollable content. The former case will 
   result in a Selenium error, and the latter will only capture a partially visible table. The HTML will be captured still however.

Note that we capture HTML alongside the visual images. This may prove to be more useful than the snapshots in a lot of scenarios.

## Working examples

### Tables and Charts

A [generated test suite for Tables and Charts](tests/robot-tests/tests/visual_testing/visually_check_tables_and_charts.seed_data.robot)
is included as part of the overall UI test suite when run in its entirety against Local and Dev. 

Its [associated CSV file](tests/robot-tests/tests/visual_testing/visually_check_tables_and_charts.seed_data.robot.csv) is included alongside
it.

It uses a [csv generated from our seed data](tests/robot-tests/scripts/visual-testing/seed-data-datablocks.csv) as a basis to generate the test
script and csv above.

These have been generated by running:

```bash
cd tests/robot-tests/scripts/visual-testing
pipenv run python generate_tables_and_charts_test_cases.py --file seed-data-datablocks.csv --target visually_check_tables_and_charts.seed_data.robot
```

And then as a manual step, the `Force Tags` setting in the 
[generated Robot test](tests/robot-tests/tests/visual_testing/visually_check_tables_and_charts.seed_data.robot) has been manually updated to allow it to
run as part of the test suite against Local and Dev, where this seed data is present.

This is then runnable individually by running:

```bash
cd tests/robot-tests
pipenv run python run_tests.py -f tests/visual_testing/visually_check_tables_and_charts.seed_data.robot
```

### Permalinks

We don't have any Permalinks set up in our seed data, as this relies on Permalinks being set up in Table Storage and we currently have no process for
seeding environments with these.

A [generated test suite for Permalinks](tests/robot-tests/tests/visual_testing/visually_check_permalinks.dev.robot) has been included however 
using a [manually generated set of Permalink Ids](tests/robot-tests/scripts/visual-testing/dev-permalinks.csv) on the Dev environment.

The above test script has been generated by running:

```bash
cd tests/robot-tests/scripts/visual-testing
pipenv run python generate_permalink_test_cases.py --file dev-permalinks.csv --target visually_check_permalinks.dev.robot
```

And then as a manual step, the `Force Tags` setting in the
[generated Robot test](tests/robot-tests/tests/visual_testing/visually_check_permalinks.dev.robot) has been manually updated to allow it to
run as part of the test suite against Dev, where these manually generated Permalinks are present.

This test will be run as part of the UI test suite against Dev, and can also be run manually using:

```bash
cd tests/robot-tests
pipenv run python run_tests.py -f tests/visual_testing/visually_check_permalinks.dev.robot -e dev
```

## Who should I talk to?

Luke Howsam  
Mark Youngman
Duncan Watson