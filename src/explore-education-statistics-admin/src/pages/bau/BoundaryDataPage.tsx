import Page from '@admin/components/Page';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';
import FormattedDate from '@common/components/FormattedDate';
import Details from '@common/components/Details';
import Link from '@admin/components/Link';
import { useQuery } from '@tanstack/react-query';
import boundaryDataQueries from '@admin/queries/boundaryDataQueries';
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
            <dt>BFC</dt>
            <dd>
              Full resolution - clipped to the coastline (Mean High Water mark)
            </dd>

            <dt>BFE</dt>
            <dd>
              Full resolution - extent of the realm (usually this is the Mean
              Low Water mark but, in some cases, boundaries extend beyond this
              to include offshore islands)
            </dd>

            <dt>BGC</dt>
            <dd>
              Generalised (20m) - clipped to the coastline (Mean High Water
              mark)
            </dd>

            <dt>BUC</dt>
            <dd>
              Ultra Generalised (500m) - clipped to the coastline (Mean High
              Water mark)
            </dd>
          </dl>

          <h2 className="govuk-heading-lg">Level codes</h2>
          <dl
            className={classNames(
              styles.glossarySection,
              styles.boundaryLevels,
            )}
          >
            <dt>EDA</dt>
            <dd>English devolved area</dd>

            <dt>INST</dt>
            <dd>Institution</dd>

            <dt>LA</dt>
            <dd>Local authority</dd>

            <dt>LAD</dt>
            <dd>Local authority district</dd>

            <dt>LEP</dt>
            <dd>Local enterprise partnership</dd>

            <dt>LSIP</dt>
            <dd>Local skills improvement plan area</dd>

            <dt>MCA</dt>
            <dd>Mayoral combined authority</dd>

            <dt>MAT</dt>
            <dd>Multi academy trust</dd>

            <dt>NAT</dt>
            <dd>National</dd>

            <dt>OA</dt>
            <dd>Opportunity area</dd>

            <dt>PA</dt>
            <dd>Planning area</dd>

            <dt>PCON</dt>
            <dd>Parliamentary constituency</dd>

            <dt>PROV</dt>
            <dd>Provider</dd>

            <dt>REG</dt>
            <dd>Regional</dd>

            <dt>RSC</dt>
            <dd>Regional School Commissioner region</dd>

            <dt>SCH</dt>
            <dd>School</dd>

            <dt>SPON</dt>
            <dd>Sponsor</dd>

            <dt>WARD</dt>
            <dd>Ward</dd>
          </dl>
        </Details>

        <Details
          summary="New data uploads: before you start"
          id="beforeYouStart"
        >
          <p>
            If the GeoJSON file has been downloaded direcly from the Open
            Geography Portal then the coordinate system may need converting from
            EPSG:27700 to EPSG:4326 (WGS84) before using this tool. The file can
            be converted using "mapshaper", following the below steps:
          </p>
          <ol>
            <li>
              Visit{' '}
              <a href="https://mapshaper.org/" target="_blank" rel="noreferrer">
                https://mapshaper.org/
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
        </Details>

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
                  <td>{boundaryLevel.level}</td>
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
