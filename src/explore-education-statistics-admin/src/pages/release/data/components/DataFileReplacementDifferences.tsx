import { IndicatorSource } from '@admin/services/apiDataSetVersionService';
import {
  IndicatorMappingWithKey,
  IndicatorsMappingsPlan,
  MappingWithKey,
  PlanMappings,
  UpdateMappingPayload,
  UpdateMappingPayloadMultiple,
} from '@admin/services/dataReplacementService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import FormProvider from '@common/components/form/FormProvider';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import classNames from 'classnames';
import mapValues from 'lodash/mapValues';
import pickBy from 'lodash/pickBy';
import startCase from 'lodash/startCase';
import React, { useCallback, useMemo, useState } from 'react';
import { useImmer } from 'use-immer';

interface Props {
  releaseVersionId: string;
  fileId: string;
  mappings: PlanMappings;
}

type SourceItem = IndicatorSource /* | FilterSource | LocationSource */;

export default function DataFileReplacementDifferences({
  releaseVersionId,
  fileId,
  mappings,
}: Props) {
  const [planMappings, updatePlanMappings] = useImmer<PlanMappings>(mappings);

  const handleIndicatorsMappingUpdate = useCallback(
    async (payload: UpdateMappingPayloadMultiple) => {
      console.log({ payload });

      updatePlanMappings(draft => {
        payload.forEach(({ sourceKey, candidateKey }) => {
          draft.indicators.mappings[sourceKey].candidateKey = candidateKey;
          draft.indicators.mappings[sourceKey].type = 'ManuallySet';
          return draft;
        });
      });

      /* const freshIndicatorsMappingsPlan: PlanMappings['indicators'] =
        await new Promise(() => {
          //TODO 6913
          // send updates and get fresh indicators mappings plan
        });

      updatePlanMappings(draft => {
        // overwrite
        return { indicators: freshIndicatorsMappingsPlan };
      }); */
    },
    [updatePlanMappings],
  );

  const tableId = 'replacements-differences-table';

  const mappingCounts: { indicators: { mapped: number; unmapped: number } } =
    useMemo(() => {
      return mapValues(planMappings, itemType => {
        const nonAutoMappedMappings = Object.values(itemType.mappings).filter(
          ({ type }) => type !== 'AutoSet',
        );
        const manuallyMappedCount = nonAutoMappedMappings.filter(
          ({ type }) => type === 'ManuallySet',
        ).length;
        const unmappedCount =
          nonAutoMappedMappings.length - manuallyMappedCount;
        return { mapped: manuallyMappedCount, unmapped: unmappedCount };
      });
    }, [planMappings]);

  return (
    <>
      <h3>Missing Dependencies</h3>

      <p>
        The following items were not found in the replacement data and were
        previously used in existing datablocks or footnotes.
        <br /> Please map these items to new items that appear in the
        replacement data or select "No mapping" for items that are no longer
        represented.
      </p>

      <table
        className="dfe-table--vertical-align-middle dfe-table--row-highlights"
        id={tableId}
        data-testid={tableId}
      >
        <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
          {Object.entries(mappingCounts).map(
            ([itemType, { mapped, unmapped }]) => (
              <span key={itemType}>
                {startCase(itemType)}{' '}
                <TagGroup className="govuk-!-margin-left-2">
                  {unmapped > 0 && (
                    <Tag colour="red">
                      {`${unmapped} unmapped ${
                        unmapped > 1 ? itemType : itemType.slice(0, -1)
                      } `}
                    </Tag>
                  )}
                  {mapped > 0 && (
                    <Tag colour="blue">
                      {`${mapped} mapped ${
                        mapped > 1 ? itemType : itemType.slice(0, -1)
                      }`}
                    </Tag>
                  )}
                </TagGroup>
              </span>
            ),
          )}
        </caption>
        <thead>
          <VisuallyHidden as="tr">
            <th className="govuk-!-width-one-quarter">Item Type</th>
            <th className="govuk-!-width-one-quarter">Original Item</th>
            <th className="govuk-!-width-one-quarter">Mapping</th>
            <th className="govuk-!-text-align-right">Actions</th>
          </VisuallyHidden>
        </thead>
        <tbody data-testid={`${tableId}-body`}>
          <DifferencesMappingTableRows
            itemType="indicator"
            mappings={planMappings.indicators}
            onUpdate={handleIndicatorsMappingUpdate}
          />
        </tbody>
      </table>
    </>
  );
}

/* =-=-=-=-=-=-=-= DifferencesMappingTableRows =-=-=-=-=-=-=-= */

interface RowsProps {
  itemType: 'indicator' | 'filter' | 'location';
  mappings: IndicatorsMappingsPlan /* | FiltersMappingsPlan | LocationsMappingsPlan */;
  onUpdate: (payload: UpdateMappingPayloadMultiple) => Promise<void>;
}

function DifferencesMappingTableRows({
  itemType,
  mappings: { candidates, mappings },
  onUpdate,
}: RowsProps) {
  const [currentMappingItem, setCurrentMappingItem] = useState<
    IndicatorMappingWithKey | undefined
  >();
  const [loadingMappings, setLoadingMappings] = useState<
    Set<UpdateMappingPayload['sourceKey']>
  >(new Set());

  const handleModalMappingSubmit = useCallback(
    (payload: UpdateMappingPayloadMultiple) => {
      const updateMappingKeys = payload.map(({ sourceKey }) => sourceKey);
      setLoadingMappings(prev => {
        const next = new Set(prev);
        updateMappingKeys.forEach(key => next.add(key));
        return next;
      });

      onUpdate(payload).finally(() =>
        setLoadingMappings(prev => {
          const next = new Set(prev);
          updateMappingKeys.forEach(key => next.delete(key));
          return next;
        }),
      );
    },
    [],
  );

  const {
    allCandidates,
    unmappedCandidates,
  }: {
    allCandidates: Dictionary<SourceItem>;
    unmappedCandidates: Dictionary<SourceItem>;
  } = useMemo(() => {
    // add key inside to each item
    const allCandidatesWithKeys = mapValues(
      candidates,
      ({ label }, candidateKey) => ({ candidateKey, label }),
    );

    const unmappedCandidatesWithKeys = pickBy(
      allCandidatesWithKeys,
      ({ candidateKey }) =>
        // filter out candidates that exist in mappings
        !Object.values(mappings).some(mapping => {
          return mapping.candidateKey === candidateKey;
        }),
    );

    return {
      allCandidates: allCandidatesWithKeys,
      unmappedCandidates: unmappedCandidatesWithKeys,
    };
  }, [candidates, mappings]);

  const mappingsToList: IndicatorMappingWithKey[] = Object.entries(mappings)
    .map(([sourceKey, mapping]) => ({ ...mapping, sourceKey }))
    .filter(({ type }) => type !== 'AutoSet');

  return (
    <>
      {currentMappingItem && (
        <DifferencesItemMappingModal
          itemType={itemType}
          allCandidateOptions={allCandidates}
          unmappedCandidateOptions={unmappedCandidates}
          currentItem={currentMappingItem}
          onSubmit={handleModalMappingSubmit}
          onClose={() => setCurrentMappingItem(undefined)}
        />
      )}

      {mappingsToList.map((mapping, index) => {
        const { source, sourceKey, type } = mapping;
        const isUnset = type === 'Unset';
        const itemCurrentMapping =
          (mapping.candidateKey &&
            allCandidates[mapping.candidateKey]?.label) ??
          'No mapping';
        const isLoading = loadingMappings.has(mapping.sourceKey);

        return (
          <tr
            key={`mapping-${sourceKey}`}
            className={classNames({
              'rowHighlight--alert': isUnset,
            })}
          >
            <td>
              <strong>
                {index === 0 ? (
                  startCase(itemType)
                ) : (
                  <VisuallyHidden>{itemType}</VisuallyHidden>
                )}
              </strong>
            </td>
            <td>{source.label}</td>
            <td>
              {isUnset ? (
                <Tag colour="red">not present</Tag>
              ) : (
                itemCurrentMapping
              )}
            </td>
            <td className="govuk-!-text-align-right">
              <LoadingSpinner
                loading={isLoading}
                alert
                hideText
                inline
                size="sm"
                text={`Updating mapping for ${mapping.source.label}`}
              >
                {mapping.type === 'Unset' && (
                  <ButtonText
                    onClick={() =>
                      handleModalMappingSubmit([
                        {
                          sourceKey: mapping.sourceKey,
                          candidateKey: undefined, // no mapping
                        },
                      ])
                    }
                  >
                    No mapping{' '}
                    <VisuallyHidden>for {mapping.source.label}</VisuallyHidden>
                  </ButtonText>
                )}

                <ButtonText
                  className="govuk-!-margin-left-2"
                  onClick={() => setCurrentMappingItem(mapping)}
                >
                  Map item{' '}
                  <VisuallyHidden>{mapping.source.label}</VisuallyHidden>
                </ButtonText>
              </LoadingSpinner>
            </td>
          </tr>
        );
      })}
    </>
  );
}

interface ModalProps {
  itemType: string;
  allCandidateOptions: Dictionary<SourceItem>;
  unmappedCandidateOptions: Dictionary<SourceItem>;
  currentItem: MappingWithKey<SourceItem>;
  onSubmit: (payload: UpdateMappingPayloadMultiple) => void;
  onClose: () => void;
}

interface FormValues {
  selectedCandidate: string;
}

function DifferencesItemMappingModal({
  currentItem,
  itemType,
  allCandidateOptions,
  unmappedCandidateOptions,
  onSubmit,
  onClose,
}: ModalProps) {
  const noMappingValue = 'noMapping';

  const currentCandidate = !currentItem.candidateKey
    ? undefined
    : {
        key: currentItem.candidateKey,
        label: allCandidateOptions[currentItem.candidateKey].label,
      };

  const handleSubmit = ({ selectedCandidate }: FormValues) => {
    onSubmit([
      {
        sourceKey: currentItem.sourceKey,
        candidateKey:
          selectedCandidate !== noMappingValue ? selectedCandidate : undefined,
      },
    ]);
    onClose();
  };

  const options: RadioOption<string>[] = [
    {
      label: 'No mapping available',
      value: noMappingValue,
      divider: `Select ${itemType}:`,
    },
    ...(currentCandidate
      ? [{ label: currentCandidate.label, value: currentCandidate.key }]
      : []),
    ...Object.entries(unmappedCandidateOptions).map(
      ([candidateName, candidate]) => {
        return {
          label: candidate.label,
          value: candidateName,
        };
      },
    ),
  ];

  return (
    <Modal open onExit={onClose} title={`Map existing ${itemType}`}>
      <h3>Current data set {itemType}</h3>
      <SummaryList>
        <SummaryListItem term="Name">{currentItem?.sourceKey}</SummaryListItem>
        <SummaryListItem term="Label">
          {currentItem?.source.label}
        </SummaryListItem>
      </SummaryList>
      <FormProvider
        initialValues={{ selectedCandidate: currentItem.candidateKey }}
        validationSchema={Yup.object({
          selectedCandidate: Yup.string().required(
            `Select the next data set ${itemType}`,
          ),
        })}
      >
        {({ formState }) => {
          return (
            <Form id="mapping-form" onSubmit={handleSubmit}>
              <FormFieldRadioSearchGroup<FormValues>
                alwaysShowOptions={[noMappingValue]}
                hint={`Choose a ${itemType} that will be mapped to the current data set ${itemType} (see above).`}
                legend={`Next data set ${itemType}`}
                name="selectedCandidate"
                options={options}
                order={[]}
                searchLabel={`Search ${itemType}s`}
                small
              />
              <ButtonGroup>
                <Button disabled={formState.isSubmitting} type="submit">
                  {`Update ${itemType} mapping`}
                </Button>
                <ButtonText disabled={formState.isSubmitting} onClick={onClose}>
                  Cancel
                </ButtonText>
              </ButtonGroup>
            </Form>
          );
        }}
      </FormProvider>
    </Modal>
  );
}
