import Link from '@admin/components/Link';
import ButtonLink from '@admin/components/ButtonLink';
import ApiDataSetUnmappedAndManuallyMappedLocationsTable
  from '@admin/pages/release/data/components/ApiDataSetUnmappedAndManuallyMappedLocationsTable';
import ApiDataSetNewLocationsTable from '@admin/pages/release/data/components/ApiDataSetNewLocationsTable';
import ApiDataSetAutoMappedLocationsTable
  from '@admin/pages/release/data/components/ApiDataSetAutoMappedLocationsTable';
import getApiDataSetLocationMappings from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import getUnmappedLocationErrors from '@admin/pages/release/data/utils/getUnmappedLocationErrors';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import apiDataSetVersionQueries from '@admin/queries/apiDataSetVersionQueries';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import Tag from '@common/components/Tag';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ErrorSummary from '@common/components/ErrorSummary';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import PageNav, {NavItem} from '@common/components/PageNav';
import locationLevelsMap from '@common/utils/locationLevelsMap';
import {useQuery} from '@tanstack/react-query';
import React from 'react';
import {generatePath, useParams} from 'react-router-dom';
import camelCase from 'lodash/camelCase';

export default function ReleaseApiDataSetLocationsMappingPage() {
  const {dataSetId, releaseVersionId, publicationId} =
    useParams<ReleaseDataSetRouteParams>();

  const {data: dataSet, isLoading: isLoadingDataSet} = useQuery(
    apiDataSetQueries.get(dataSetId),
  );

  const {
    data: locationsMapping = {levels: {}},
    isLoading: isLoadingMapping,
  } = useQuery({
    ...apiDataSetVersionQueries.getLocationsMapping(
      dataSet?.draftVersion?.id ?? '',
    ),
    enabled: !!dataSet?.draftVersion?.id,
  });

  const {
    autoMappedLocations,
    newLocationCandidates,
    unmappedAndManuallyMappedLocations,
  } = getApiDataSetLocationMappings(locationsMapping);

  const totalNewLocationCandidates = Object.values(
    newLocationCandidates,
  ).flatMap(key => key).length;

  const unmappedLocationErrors = getUnmappedLocationErrors(
    unmappedAndManuallyMappedLocations,
  );

  const navItems: NavItem[] = [
    {
      id: 'unmapped-locations',
      text: 'Locations not found in the new data set',
      subNavItems: Object.keys(unmappedAndManuallyMappedLocations).map(
        level => {
          return {
            id: `unmapped-${level}`,
            text: locationLevelsMap[camelCase(level)]?.plural ?? level,
          };
        },
      ),
    },
    {
      id: 'new-locations',
      text: 'New locations',
      subNavItems: Object.keys(newLocationCandidates).map(level => {
        return {
          id: `new-${level}`,
          text: locationLevelsMap[camelCase(level)]?.plural ?? level,
        };
      }),
    },
    {id: 'auto-mapped-locations', text: 'Auto mapped locations'},
  ];

  return (
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath<ReleaseDataSetRouteParams>(
          releaseApiDataSetDetailsRoute.path,
          {
            publicationId,
            releaseVersionId,
            dataSetId,
          },
        )}
      >
        Back
      </Link>
      <LoadingSpinner loading={isLoadingDataSet || isLoadingMapping}>
        <div className="govuk-grid-row">
          <PageNav items={navItems}/>
          <div className="govuk-grid-column-three-quarters">
            <span className="govuk-caption-l">Map locations</span>
            <h2>{dataSet?.title}</h2>
            {!!unmappedLocationErrors.length && (
              <ErrorSummary
                errors={unmappedLocationErrors}
                headingTag="h3"
                title="Unmapped locations"
                updateDocumentTitle={false}
              />
            )}
            <p>
              Existing locations in the publication data set should be mapped to
              an equivalent location in the new data set. We try to
              automatically match up locations that have the same name, but you
              can double-check these choices.
            </p>
            <p>
              Any locations in the new data set that have not been mapped will
              show as new locations in the publication data set.
            </p>

            <h3 className="govuk-heading-l" id="unmapped-locations">
              Locations not found in the new data set
            </h3>

            {Object.keys(unmappedAndManuallyMappedLocations).length > 0 ? (
              <>
                {Object.keys(unmappedAndManuallyMappedLocations).map(level => (
                  <ApiDataSetUnmappedAndManuallyMappedLocationsTable
                    key={`unmapped-${level}`}
                    level={level}
                    locations={unmappedAndManuallyMappedLocations[level]}
                  />
                ))}
              </>
            ) : (
              <p>No locations not found in the new data set.</p>
            )}

            <h3 className="govuk-heading-l" id="new-locations">
              New locations ({totalNewLocationCandidates}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>

            {totalNewLocationCandidates > 0 ? (
              <Accordion id="new-locations">
                {Object.keys(newLocationCandidates).map(level => {
                  return (
                    <AccordionSection
                      goToTop={false}
                      // TODO remove camelCase when levels are camelCase in BE
                      heading={`${
                        locationLevelsMap[camelCase(level)]?.plural ?? level
                      } (${newLocationCandidates[level].length})`}
                      headingTag="h4"
                      id={`new-${level}`}
                      key={`new-${level}`}
                    >
                      <ApiDataSetNewLocationsTable
                        level={level}
                        locations={newLocationCandidates[level]}
                      />
                    </AccordionSection>
                  );
                })}
              </Accordion>
            ) : (
              <p>No new locations.</p>
            )}

            <h3 className="govuk-heading-l" id="auto-mapped-locations">
              Auto mapped locations ({autoMappedLocations.length}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>
            {autoMappedLocations.length > 0 ? (
              <Accordion id="auto-mapped" showOpenAll={false}>
                <AccordionSection
                  goToTop={false}
                  heading="View and search auto mapped locations"
                  headingTag="h4"
                >
                  <ApiDataSetAutoMappedLocationsTable
                    locations={autoMappedLocations}
                  />
                </AccordionSection>
              </Accordion>
            ) : (
              <p>No auto mapped locations.</p>
            )}

            {dataSet && (
              <ButtonLink
                className="govuk-!-margin-top-4"
                to={generatePath<ReleaseDataSetRouteParams>(
                  releaseApiDataSetDetailsRoute.path,
                  {
                    publicationId,
                    releaseVersionId,
                    dataSetId: dataSet.id,
                  },
                )}
              >
                Continue
              </ButtonLink>
            )}
          </div>
        </div>
      </LoadingSpinner>
    </>
  );
}
