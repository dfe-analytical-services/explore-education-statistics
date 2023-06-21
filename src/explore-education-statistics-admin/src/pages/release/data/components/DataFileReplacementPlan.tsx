import ButtonLink from '@admin/components/ButtonLink';
import mergeReplacementFootnoteFilters from '@admin/pages/release/data/components/utils/mergeReplacementFootnoteFilters';
import {
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
  ReleaseFootnoteRouteParams,
  releaseFootnotesEditRoute,
} from '@admin/routes/releaseRoutes';
import dataBlockService from '@admin/services/dataBlockService';
import dataReplacementService, {
  DataBlockReplacementPlan,
  FootnoteReplacementPlan,
} from '@admin/services/dataReplacementService';
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
import React, { useMemo, useState } from 'react';
import { generatePath } from 'react-router';

interface Props {
  publicationId: string;
  releaseId: string;
  fileId: string;
  replacementFileId: string;
  onCancel?: () => void;
  onReplacement?: () => void;
}

const DataFileReplacementPlan = ({
  publicationId,
  releaseId,
  fileId,
  replacementFileId,
  onCancel,
  onReplacement,
}: Props) => {
  const [isSubmitting, toggleSubmitting] = useToggle(false);

  const [deleteDataBlock, setDeleteDataBlock] = useState<
    DataBlockReplacementPlan
  >();
  const [deleteFootnote, setDeleteFootnote] = useState<
    FootnoteReplacementPlan
  >();

  const isMounted = useMountedRef();

  const {
    value: plan,
    isLoading,
    error,
    retry: reloadPlan,
  } = useAsyncRetry(
    () =>
      dataReplacementService.getReplacementPlan(
        releaseId,
        fileId,
        replacementFileId,
      ),
    [releaseId, fileId, replacementFileId],
  );

  const hasInvalidDataBlocks = useMemo<boolean>(
    () => plan?.dataBlocks.some(block => !block.valid) ?? false,
    [plan],
  );

  const hasInvalidFootnotes = useMemo<boolean>(
    () => plan?.footnotes.some(footnote => !footnote.valid) ?? false,
    [plan],
  );

  if (error) {
    return (
      <>
        <WarningMessage>
          There was a problem loading the data replacement information.
        </WarningMessage>

        <Button onClick={onCancel}>Cancel data replacement</Button>
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
                            releaseId,
                            dataBlockId: dataBlock.id,
                          },
                        )}
                      >
                        Edit data block
                      </ButtonLink>
                    )}
                    <Button
                      variant="warning"
                      onClick={() => {
                        setDeleteDataBlock(dataBlock);
                      }}
                    >
                      Delete data block
                    </Button>
                  </ButtonGroup>
                </>
              )}
            </Details>
          ))}

          <h3 className="govuk-heading-m">
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
                summary={footnote.content}
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
                            releaseId,
                            footnoteId: footnote.id,
                          },
                        )}
                      >
                        Edit footnote
                      </ButtonLink>
                      <Button
                        variant="warning"
                        onClick={() => {
                          setDeleteFootnote(footnote);
                        }}
                      >
                        Delete footnote
                      </Button>
                    </ButtonGroup>
                  </>
                )}
              </Details>
            );
          })}

          <ButtonGroup className="govuk-!-margin-top-8">
            {plan.valid && (
              <Button
                disabled={isSubmitting}
                onClick={async () => {
                  toggleSubmitting.on();

                  await dataReplacementService.replaceData(
                    releaseId,
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

            <Button variant="secondary" onClick={onCancel}>
              Cancel data replacement
            </Button>
          </ButtonGroup>

          {deleteDataBlock && (
            <ModalConfirm
              title="Delete data block"
              onExit={() => setDeleteDataBlock(undefined)}
              onConfirm={async () => {
                await dataBlockService.deleteDataBlock(
                  releaseId,
                  deleteDataBlock.id,
                );
                setDeleteDataBlock(undefined);
                reloadPlan();
              }}
            >
              <p>
                Are you sure you want to delete{' '}
                <strong>'{deleteDataBlock?.name}'</strong>?
              </p>
            </ModalConfirm>
          )}

          {deleteFootnote && (
            <ModalConfirm
              title="Delete footnote"
              onExit={() => setDeleteFootnote(undefined)}
              onConfirm={async () => {
                await footnoteService.deleteFootnote(
                  releaseId,
                  deleteFootnote.id,
                );
                setDeleteFootnote(undefined);
                reloadPlan();
              }}
            >
              <p>Are you sure you want to delete the following footnote?</p>

              <InsetText>
                <p>{deleteFootnote?.content}</p>
              </InsetText>
            </ModalConfirm>
          )}
        </>
      )}
    </LoadingSpinner>
  );
};

export default DataFileReplacementPlan;
