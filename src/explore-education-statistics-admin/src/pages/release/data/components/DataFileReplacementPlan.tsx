import ButtonLink from '@admin/components/ButtonLink';
import mergeReplacementFootnoteFilters from '@admin/pages/release/data/components/utils/mergeReplacementFootnoteFilters';
import {
  releaseApiDataSetDetailsRoute,
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
  ReleaseFootnoteRouteParams,
  releaseFootnotesEditRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import dataBlockService from '@admin/services/dataBlockService';
import dataReplacementService from '@admin/services/dataReplacementService';
import footnoteService from '@admin/services/footnoteService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import CollapsibleList from '@common/components/CollapsibleList';
import Details from '@common/components/Details';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useToggle from '@common/hooks/useToggle';
import useMountedRef from '@common/hooks/useMountedRef';
import React, { ReactNode, useMemo } from 'react';
import { generatePath } from 'react-router';
import sanitizeHtml from '@common/utils/sanitizeHtml';
import { useAuthContext } from '@admin/contexts/AuthContext';
import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';

interface Props {
  cancelButton: ReactNode;
  publicationId: string;
  releaseVersionId: string;
  fileId: string;
  replacementFileId: string;
  onReplacement?: () => void;
}

const DataFileReplacementPlan = ({
  cancelButton,
  publicationId,
  releaseVersionId,
  fileId,
  replacementFileId,
  onReplacement,
}: Props) => {
  const [isSubmitting, toggleSubmitting] = useToggle(false);

  const isMounted = useMountedRef();

  const {
    value: plan,
    isLoading,
    error,
    retry: reloadPlan,
  } = useAsyncRetry(
    () =>
      dataReplacementService.getReplacementPlan(
        releaseVersionId,
        fileId,
        replacementFileId,
      ),
    [releaseVersionId, fileId, replacementFileId],
  );

  const hasInvalidDataBlocks = useMemo<boolean>(
    () => plan?.dataBlocks.some(block => !block.valid) ?? false,
    [plan],
  );

  const hasInvalidFootnotes = useMemo<boolean>(
    () => plan?.footnotes.some(footnote => !footnote.valid) ?? false,
    [plan],
  );
  const {
    enableReplacementOfPublicApiDataSets: isNewReplaceDsvFeatureEnabled,
  } = useConfig();

  const {
    hasDataSetVersionPlan,
    hasIncompleteLocationMapping,
    hasIncompleteFilterMapping,
    isNotReadyToPublish,
    hasMajorVersionUpdate,
  } = useMemo(() => {
    if (!isNewReplaceDsvFeatureEnabled) {
      return {
        hasDataSetVersionPlan: false,
        hasIncompleteLocationMapping: false,
        hasIncompleteFilterMapping: false,
        isNotReadyToPublish: false,
        hasMajorVersionUpdate: false,
      };
    }

    return {
      hasDataSetVersionPlan: !!plan?.apiDataSetVersionPlan,
      hasIncompleteLocationMapping:
        !plan?.apiDataSetVersionPlan?.mappingStatus?.locationsComplete,
      hasIncompleteFilterMapping:
        !plan?.apiDataSetVersionPlan?.mappingStatus?.filtersComplete,
      isNotReadyToPublish: !plan?.apiDataSetVersionPlan?.readyToPublish,
      hasMajorVersionUpdate:
        plan?.apiDataSetVersionPlan?.mappingStatus?.hasMajorVersionUpdate,
    };
  }, [plan, isNewReplaceDsvFeatureEnabled]);

  const { user } = useAuthContext();
  const dataSetId = plan?.apiDataSetVersionPlan?.dataSetId;

  const releaseRouteParams = useMemo<ReleaseDataSetRouteParams | undefined>(
    () =>
      dataSetId
        ? {
            releaseVersionId,
            publicationId,
            dataSetId,
          }
        : undefined,
    [releaseVersionId, publicationId, dataSetId],
  );

  const apiDataSetsTabRoute =
    user?.permissions.isBauUser && releaseRouteParams
      ? `${generatePath<ReleaseDataSetRouteParams>(
          releaseApiDataSetDetailsRoute.path,
          releaseRouteParams,
        )}`
      : undefined;

  if (error) {
    return (
      <>
        <WarningMessage>
          There was a problem loading the data replacement information.
        </WarningMessage>

        {cancelButton}
      </>
    );
  }

  const notPresentTag = (
    <Tag colour="red" className="govuk-!-margin-left-2">
      {' not present'}
    </Tag>
  );

  return (
    <LoadingSpinner loading={isLoading}>
      {plan && (
        <>
          <WarningMessage>
            Please check the information below before confirming the data
            replacement as making this change may affect existing data blocks
            and footnotes.
          </WarningMessage>

          <h3 className="govuk-heading-m">
            <Tag colour={hasInvalidDataBlocks ? 'red' : 'green'}>
              {`Data blocks: ${hasInvalidDataBlocks ? 'ERROR' : 'OK'}`}
            </Tag>
          </h3>

          {hasInvalidDataBlocks ? (
            <p>
              One or more data blocks will be invalidated by this data
              replacement. The list below shows the affected data blocks, you
              can either delete or edit these if you wish to continue with this
              data replacement.
            </p>
          ) : (
            <p>
              {plan.dataBlocks.length > 0
                ? 'All existing data blocks can be replaced by this data replacement.'
                : 'No data blocks to replace.'}
            </p>
          )}

          {plan.dataBlocks.map(dataBlock => (
            <Details
              key={dataBlock.id}
              summary={dataBlock.name}
              summaryAfter={
                <Tag
                  className="govuk-!-margin-left-2"
                  colour={dataBlock.valid ? 'green' : 'red'}
                >
                  <VisuallyHidden>:</VisuallyHidden>

                  {dataBlock.valid ? ' OK' : ' ERROR'}
                </Tag>
              }
            >
              {dataBlock.valid ? (
                <p>This data block has no conflicts and can be replaced.</p>
              ) : (
                <>
                  <SummaryList smallKey>
                    <SummaryListItem term="Locations">
                      <SummaryList noBorder compact>
                        {Object.entries(dataBlock.locations).map(
                          ([levelKey, level]) => (
                            <SummaryListItem key={levelKey} term={level.label}>
                              <ul className="govuk-list">
                                {level.locationAttributes.map(location => (
                                  <li key={location.code}>
                                    {location.label}
                                    {!location.valid && notPresentTag}
                                  </li>
                                ))}
                              </ul>
                            </SummaryListItem>
                          ),
                        )}
                      </SummaryList>
                    </SummaryListItem>

                    <SummaryListItem term="Time periods">
                      <SummaryList noBorder compact>
                        <SummaryListItem term="Start date">
                          {dataBlock.timePeriods?.start.label}
                          {!dataBlock.timePeriods?.start.valid && notPresentTag}
                        </SummaryListItem>
                        <SummaryListItem term="End date">
                          {dataBlock.timePeriods?.end.label}
                          {!dataBlock.timePeriods?.end.valid && notPresentTag}
                        </SummaryListItem>
                      </SummaryList>
                    </SummaryListItem>

                    {Object.values(dataBlock.filters).map(filter => (
                      <SummaryListItem key={filter.id} term={filter.label}>
                        <SummaryList noBorder compact>
                          {Object.values(filter.groups).map(group => (
                            <SummaryListItem key={group.id} term={group.label}>
                              <ul className="govuk-list">
                                {group.filters.map(filterItem => (
                                  <li key={filterItem.id}>
                                    {filterItem.label}
                                    {!filterItem.valid && notPresentTag}
                                  </li>
                                ))}
                              </ul>
                            </SummaryListItem>
                          ))}
                        </SummaryList>
                      </SummaryListItem>
                    ))}

                    <SummaryListItem term="Indicators">
                      <SummaryList noBorder compact>
                        {Object.values(dataBlock.indicatorGroups).map(
                          indicatorGroup => (
                            <SummaryListItem
                              key={indicatorGroup.id}
                              term={indicatorGroup.label}
                            >
                              <ul className="govuk-list">
                                {indicatorGroup.indicators.map(indicator => (
                                  <li key={indicator.id}>
                                    {indicator.label}
                                    {!indicator.valid && notPresentTag}
                                  </li>
                                ))}
                              </ul>
                            </SummaryListItem>
                          ),
                        )}
                      </SummaryList>
                    </SummaryListItem>
                  </SummaryList>

                  <ButtonGroup>
                    {dataBlock.fixable && (
                      <ButtonLink
                        to={generatePath<ReleaseDataBlockRouteParams>(
                          releaseDataBlockEditRoute.path,
                          {
                            publicationId,
                            releaseVersionId,
                            dataBlockId: dataBlock.id,
                          },
                        )}
                      >
                        Edit data block
                      </ButtonLink>
                    )}

                    <ModalConfirm
                      title="Delete data block"
                      triggerButton={
                        <Button variant="warning">Delete data block</Button>
                      }
                      onConfirm={async () => {
                        await dataBlockService.deleteDataBlock(
                          releaseVersionId,
                          dataBlock.id,
                        );
                        reloadPlan();
                      }}
                    >
                      <p>
                        Are you sure you want to delete{' '}
                        <strong>'{dataBlock?.name}'</strong>?
                      </p>
                    </ModalConfirm>
                  </ButtonGroup>
                </>
              )}
            </Details>
          ))}

          <h3 className="govuk-heading-m govuk-!-padding-top-4">
            <Tag colour={hasInvalidFootnotes ? 'red' : 'green'}>
              {`Footnotes: ${hasInvalidFootnotes ? 'ERROR' : 'OK'}`}
            </Tag>
          </h3>

          {hasInvalidFootnotes ? (
            <p>
              One or more footnotes will be invalidated by this data
              replacement. The list below shows the affected footnotes, you can
              either delete or edit these if you wish to continue with this data
              replacement.
            </p>
          ) : (
            <p>
              {plan.footnotes.length > 0
                ? 'All existing footnotes can be replaced by this data replacement.'
                : 'No footnotes to replace.'}
            </p>
          )}

          {plan.footnotes.map(footnote => {
            const mergedFilters = mergeReplacementFootnoteFilters(footnote);

            return (
              <Details
                key={footnote.id}
                summary={sanitizeHtml(footnote.content, { allowedTags: [] })}
                summaryAfter={
                  <Tag
                    className="govuk-!-margin-left-2"
                    colour={footnote.valid ? 'green' : 'red'}
                  >
                    <VisuallyHidden>:</VisuallyHidden>
                    {footnote.valid ? ' OK' : ' ERROR'}
                  </Tag>
                }
              >
                {footnote.valid ? (
                  <p>This footnote has no conflicts and can be replaced.</p>
                ) : (
                  <>
                    <SummaryList smallKey>
                      {Object.values(mergedFilters).map(filter => (
                        <SummaryListItem key={filter.id} term={filter.label}>
                          {filter.isAllSelected ? (
                            '(All selected)'
                          ) : (
                            <SummaryList noBorder compact>
                              {Object.values(filter.groups).map(group => (
                                <SummaryListItem
                                  key={group.id}
                                  term={group.label}
                                >
                                  {group.isAllSelected ? (
                                    '(All selected)'
                                  ) : (
                                    <ul className="govuk-list">
                                      {group.filters.map(filterItem => (
                                        <li key={filterItem.id}>
                                          {filterItem.label}
                                          {!filterItem.valid && notPresentTag}
                                        </li>
                                      ))}
                                    </ul>
                                  )}
                                </SummaryListItem>
                              ))}
                            </SummaryList>
                          )}
                        </SummaryListItem>
                      ))}

                      <SummaryListItem term="Indicators">
                        <SummaryList noBorder compact>
                          {Object.values(footnote.indicatorGroups).map(
                            indicatorGroup => (
                              <SummaryListItem
                                key={indicatorGroup.id}
                                term={indicatorGroup.label}
                              >
                                <CollapsibleList
                                  id="indicatorsList"
                                  itemName="indicator"
                                  itemNamePlural="indicators"
                                >
                                  {indicatorGroup.indicators.map(indicator => (
                                    <li key={indicator.id}>
                                      {indicator.label}
                                      {!indicator.valid && notPresentTag}
                                    </li>
                                  ))}
                                </CollapsibleList>
                              </SummaryListItem>
                            ),
                          )}
                        </SummaryList>
                      </SummaryListItem>
                    </SummaryList>

                    <ButtonGroup>
                      <ButtonLink
                        to={generatePath<ReleaseFootnoteRouteParams>(
                          releaseFootnotesEditRoute.path,
                          {
                            publicationId,
                            releaseVersionId,
                            footnoteId: footnote.id,
                          },
                        )}
                      >
                        Edit footnote
                      </ButtonLink>

                      <ModalConfirm
                        title="Delete footnote"
                        triggerButton={
                          <Button variant="warning">Delete footnote</Button>
                        }
                        onConfirm={async () => {
                          await footnoteService.deleteFootnote(
                            releaseVersionId,
                            footnote.id,
                          );
                          reloadPlan();
                        }}
                      >
                        <p>
                          Are you sure you want to delete the following
                          footnote?
                        </p>

                        <InsetText>
                          <p>{footnote?.content}</p>
                        </InsetText>
                      </ModalConfirm>
                    </ButtonGroup>
                  </>
                )}
              </Details>
            );
          })}

          {hasDataSetVersionPlan && (
            <>
              {hasMajorVersionUpdate ? (
                <>
                  <h3 className="govuk-heading-m govuk-!-padding-top-4">
                    <Tag colour={hasMajorVersionUpdate ? 'red' : 'green'}>
                      {`API data set status: ${
                        hasMajorVersionUpdate ? 'ERROR' : 'OK'
                      }`}
                    </Tag>
                  </h3>
                  <p>
                    Please cancel this data replacement and upload a new data
                    file that doesn't create a breaking change. To see the
                    breaking changes, please{' '}
                    {apiDataSetsTabRoute && (
                      <Link to={apiDataSetsTabRoute} unvisited>
                        go to the API data sets tab (This page is currently only
                        accessible to certain users, please contact the EES team
                        for support with API data sets)
                      </Link>
                    )}
                    .
                  </p>
                </>
              ) : (
                <>
                  <h3 className="govuk-heading-m govuk-!-padding-top-4">
                    <Tag colour={hasIncompleteFilterMapping ? 'red' : 'green'}>
                      {`API data set Filters: ${
                        hasIncompleteFilterMapping ? 'ERROR' : 'OK'
                      }`}
                    </Tag>
                  </h3>

                  {hasIncompleteFilterMapping ? (
                    <p>
                      Please{' '}
                      {apiDataSetsTabRoute && (
                        <Link to={apiDataSetsTabRoute} unvisited>
                          go to the API data sets tab (This page is currently
                          only accessible to certain users, please contact the
                          EES team for support with API data sets)
                        </Link>
                      )}{' '}
                      and complete manual mapping process for filters.
                    </p>
                  ) : (
                    <p>No manual mapping required for API data set filters.</p>
                  )}

                  <h3 className="govuk-heading-m govuk-!-padding-top-4">
                    <Tag
                      colour={hasIncompleteLocationMapping ? 'red' : 'green'}
                    >
                      {`API data set Locations: ${
                        hasIncompleteLocationMapping ? 'ERROR' : 'OK'
                      }`}
                    </Tag>
                  </h3>

                  {hasIncompleteLocationMapping ? (
                    <p>
                      Please{' '}
                      {apiDataSetsTabRoute && (
                        <Link to={apiDataSetsTabRoute} unvisited>
                          go to the API data sets tab (This page is currently
                          only accessible to certain users, please contact the
                          EES team for support with API data sets)
                        </Link>
                      )}{' '}
                      and complete manual mapping process for locations.
                    </p>
                  ) : (
                    <p>
                      No manual mapping required for API data set locations.
                    </p>
                  )}
                  <h3 className="govuk-heading-m govuk-!-padding-top-4">
                    <Tag colour={isNotReadyToPublish ? 'red' : 'green'}>
                      {`API data set has to be finalized: ${
                        isNotReadyToPublish ? 'ERROR' : 'OK'
                      }`}
                    </Tag>
                  </h3>

                  {isNotReadyToPublish ? (
                    <p>
                      Please{' '}
                      {apiDataSetsTabRoute && (
                        <Link to={apiDataSetsTabRoute} unvisited>
                          go to the API data sets tab (This page is currently
                          only accessible to certain users, please contact the
                          EES team for support with API data sets)
                        </Link>
                      )}{' '}
                      and finalize the data set version mapping process.
                    </p>
                  ) : (
                    <p>No actions required for API data set version mapping.</p>
                  )}
                </>
              )}
            </>
          )}

          <ButtonGroup className="govuk-!-margin-top-8">
            {plan.valid && (
              <Button
                disabled={isSubmitting}
                onClick={async () => {
                  toggleSubmitting.on();

                  await dataReplacementService.replaceData(
                    releaseVersionId,
                    fileId,
                    replacementFileId,
                  );

                  if (onReplacement) {
                    onReplacement();
                  }
                  if (isMounted.current) {
                    toggleSubmitting.off();
                  }
                }}
              >
                Confirm data replacement
              </Button>
            )}
            {cancelButton}
          </ButtonGroup>
        </>
      )}
    </LoadingSpinner>
  );
};

export default DataFileReplacementPlan;
