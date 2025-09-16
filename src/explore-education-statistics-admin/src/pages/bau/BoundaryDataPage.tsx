import Page from '@admin/components/Page';
import LoadingSpinner from '@common/components/LoadingSpinner';
import typedKeys from '@common/utils/object/typedKeys';
import React, { Fragment } from 'react';
import FormattedDate from '@common/components/FormattedDate';
import Details from '@common/components/Details';
import Link from '@admin/components/Link';
import { useQuery } from '@tanstack/react-query';
import boundaryDataQueries from '@admin/queries/boundaryDataQueries';
import boundaryTypesMap from '@common/utils/boundaryTypesMap';
import locationLevelsMap from '@common/utils/locationLevelsMap';
import InsetText from '@common/components/InsetText';
import classNames from 'classnames';
import styles from './BoundaryDataPage.module.scss';

const BoundaryDataPage = () => {
  const { data: boundaryLevels = [], isLoading: isLoadingBoundaryLevels } =
    useQuery(boundaryDataQueries.getBoundaryLevels);

  return (
    <Page
      title="Boundary data"
      caption="Manage Boundary data"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Boundary data' },
      ]}
    >
      <LoadingSpinner
        loading={isLoadingBoundaryLevels}
        text="Loading boundary levels"
      >
        <Details summary="Glossary" id="glossary">
          <h2 className="govuk-heading-lg">Boundary types</h2>
          <dl
            className={classNames(styles.glossarySection, styles.boundaryTypes)}
          >
            {Object.keys(boundaryTypesMap).map(key => {
              return (
                <Fragment key={key}>
                  <dt>{boundaryTypesMap[key].label}</dt>
                  <dd>{boundaryTypesMap[key].description}</dd>
                </Fragment>
              );
            })}
          </dl>
          <h2 className="govuk-heading-lg">Level codes</h2>
          <dl
            className={classNames(
              styles.glossarySection,
              styles.boundaryLevels,
            )}
          >
            {typedKeys(locationLevelsMap).map(key => {
              return (
                <Fragment key={key}>
                  <dt>{locationLevelsMap[key].code}</dt>
                  <dd>{locationLevelsMap[key].label}</dd>
                </Fragment>
              );
            })}
          </dl>
        </Details>

        <InsetText>
          <h2>New data uploads: before you start</h2>
          <p>
            If the GeoJSON file has been downloaded direcly from the Open
            Geography Portal then the coordinate system may need converting from
            EPSG:27700 to EPSG:4326 (WGS84) before using this tool. The file can
            be converted using "mapshaper", following the below steps:
          </p>
          <ol>
            <li>
              Visit{' '}
              <a
                href="https://mapshaper.org/"
                target="_blank"
                rel="noreferrer noopener nofollow"
              >
                https://mapshaper.org/ (opens in new tab)
              </a>
            </li>
            <li>Import the original GeoJSON file</li>
            <li>Select "Console" from the navigation options</li>
            <li>
              Enter "<code>-proj from=EPSG:27700 crs=EPSG:4326</code>" and press
              ENTER
            </li>
            <li>Select "Export" from the navigation options</li>
            <li>
              Ensure the "GeoJSON" option is selected, then click "Export" to
              save the file to your local machine
            </li>
            <li>
              Rename the downloaded file's extension from .json to .geojson
            </li>
          </ol>
        </InsetText>

        <Link to="boundary-data/upload" className="govuk-button">
          Upload boundary data file
        </Link>

        <table>
          <thead>
            <tr>
              <th scope="col">Level</th>
              <th scope="col">Label</th>
              <th scope="col">Published</th>
              <th scope="col">Actions</th>
            </tr>
          </thead>
          {boundaryLevels && (
            <tbody>
              {boundaryLevels.map(boundaryLevel => (
                <tr key={boundaryLevel.id}>
                  <td>{locationLevelsMap[boundaryLevel.level].code}</td>
                  <td>{boundaryLevel.label}</td>
                  <td>
                    <FormattedDate format="d MMM yyyy">
                      {boundaryLevel.published}
                    </FormattedDate>
                  </td>
                  <td>
                    <Link
                      to={`boundary-data/boundary-level/${boundaryLevel.id}`}
                    >
                      Edit
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          )}
        </table>
      </LoadingSpinner>
    </Page>
  );
};

export default BoundaryDataPage;
