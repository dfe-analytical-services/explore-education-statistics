import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useMounted from '@common/hooks/useMounted';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import { Form, FormFieldset } from '@common/components/form';
import { Formik } from 'formik';
import React, { useState } from 'react';
import Yup from '@common/validation/yup';
import capitalize from 'lodash/capitalize';
import PrototypeFacetList from './PrototypeFacetList';
import PrototypeMapFacetModal from './PrototypeMapFacetModal';
import { MapItem } from '../PrototypePrepareNextSubjectPage';
import PrototypeUnmapFacetModal from './PrototypeUnmapFacetModal';
import PrototypeRemoveNoMappingFacetModal from './PrototypeRemoveNoMappingFacetModal';
import {
  FacetType,
  Items,
  usePrototypeNextSubjectContext,
} from '../contexts/PrototypeNextSubjectContext';

interface Props extends InjectedWizardProps {
  name: FacetType;
}

const PrototypePrepareNextSubjectStep2 = ({ name, ...stepProps }: Props) => {
  const namePlural = `${name}s`;
  const { isActive, goToNextStep } = stepProps;
  const { isMounted } = useMounted();

  const state = usePrototypeNextSubjectContext();
  const allItems = state[namePlural as keyof typeof state] as Items;

  const { mappedItems, unmappedItems, newItems, noMappingItems } = allItems;
  const { onAddNoMappingItem, onMapItem, onRemoveNoMappingItem, onUnmapItem } =
    state;

  const [itemToMap, setItemToMap] = useState<MapItem | undefined>(undefined);
  const [itemToUnmap, setItemToUnmap] = useState<
    [MapItem, MapItem] | undefined
  >(undefined);
  const [itemToRemoveNoMapping, setItemToRemoveNoMapping] = useState<
    MapItem | undefined
  >(undefined);

  const grouping = name === 'location' ? 'Local authority' : 'Characteristic';

  const getCaption = (item: MapItem) =>
    name === 'location' ? `${item.code}, ${item.region}` : item.group;

  let nextStep = 'view changelog';

  if (name === 'location') {
    nextStep = 'filters';
  }
  if (name === 'filter') {
    nextStep = 'indicators';
  }

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      Map {namePlural}
    </WizardStepHeading>
  );

  if (isActive && isMounted) {
    return (
      <>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-full">
            <Formik
              initialValues={{
                unmapped: unmappedItems,
              }}
              validationSchema={Yup.object({
                unmapped: Yup.array().test({
                  name: 'whatevs',
                  message: `All ${namePlural} from the current data set should be mapped to the next data set`,
                  test() {
                    if (!unmappedItems.length) {
                      return true;
                    }
                    return false;
                  },
                }),
              })}
              onSubmit={() => {
                goToNextStep();
              }}
            >
              {form => (
                <Form id="form">
                  <FormFieldset id="downloadFiles" legend={stepHeading}>
                    <>
                      <p>
                        Existing {namePlural} in the publication data set need
                        to be mapped to an equivalent location in the new data
                        set. We try to automatically match up {namePlural} that
                        appear to be a good fit, but you should double-check
                        these choices.
                      </p>
                      <p>
                        Any facets in the new data set that have not been mapped
                        will become new {namePlural} in the publication data
                        set.
                      </p>
                    </>
                  </FormFieldset>

                  <h3>
                    New {namePlural} ({newItems.length}){' '}
                    <span className="govuk-tag govuk-tag--grey">
                      No action required
                    </span>
                  </h3>

                  <PrototypeFacetList
                    heading={`${grouping} (${newItems.length})`}
                    items={newItems.map(item => [
                      { label: '' },
                      {
                        label: item.label,
                        id: item.id,
                        caption: getCaption(item),
                      },
                    ])}
                    type="new"
                    grouped
                  />

                  {unmappedItems.length > 0 && (
                    <FormFieldset
                      className="govuk-!-margin-top-6"
                      id="unmapped"
                      legend={
                        <>
                          <span style={{ textTransform: 'capitalize' }}>
                            {namePlural}{' '}
                          </span>
                          that cannot be found in the new data set (
                          {unmappedItems.length} of{' '}
                          {mappedItems.length + unmappedItems.length}){' '}
                          <span className="govuk-tag govuk-tag--red">
                            Requires action
                          </span>
                        </>
                      }
                      legendSize="m"
                      error={
                        form.submitCount > 0 && unmappedItems.length
                          ? `All ${namePlural} from the current data set should be mapped to the next data set`
                          : undefined
                      }
                    >
                      <PrototypeFacetList
                        heading={`${grouping} (${unmappedItems.length})`}
                        items={unmappedItems.map(item => [
                          {
                            label: item.label,
                            id: item.id,
                            caption: getCaption(item),
                          },
                          { label: 'Not mapped' },
                        ])}
                        type="unmapped"
                        onClick={id =>
                          setItemToMap(
                            unmappedItems.find(item => item.id === id),
                          )
                        }
                        grouped={name !== 'indicator'}
                      />
                    </FormFieldset>
                  )}

                  <h2>Summary of {name} changes in this data set</h2>

                  {mappedItems.length > 0 && (
                    <>
                      <h3>
                        {capitalize(namePlural)} mapped to the new data set (
                        {mappedItems.length} of{' '}
                        {mappedItems.length + unmappedItems.length}){' '}
                        <span className="govuk-tag">
                          Minor updates - Please check
                        </span>
                      </h3>

                      <div className="govuk-hint">
                        If you need to make any changes to the list below,
                        selecting a {name} will allow you to remove the mapping
                        and choose a new one.
                      </div>

                      <PrototypeFacetList
                        heading={`${grouping} (${mappedItems.length})`}
                        items={mappedItems.map(item => [
                          {
                            label: item[0].label,
                            id: item[0].id,
                            caption: getCaption(item[0]),
                          },
                          {
                            label: item[1].label,
                            id: item[1].id,
                            caption: getCaption(item[1]),
                          },
                        ])}
                        type="mapped"
                        grouped
                        onClick={id =>
                          setItemToUnmap(
                            mappedItems.find(item => item[0].id === id),
                          )
                        }
                      />
                    </>
                  )}

                  {noMappingItems.length > 0 && (
                    <>
                      <h3>
                        {capitalize(namePlural)} with no mappings available (
                        {noMappingItems.length}){' '}
                        <span className="govuk-tag">
                          Major updates - please check
                        </span>
                      </h3>

                      <div className="govuk-hint">
                        The list below shows {namePlural} that do not have
                        mappings available in the new data set, please be aware
                        this will create a major version update, this could
                        create breaking changes for users of this new data set.
                        If you need to make any changes to the list below,
                        selecting a {name} will allow you to reset and then
                        choose a mapping if appropriate.
                      </div>

                      <PrototypeFacetList
                        heading={`${grouping} (${noMappingItems.length})`}
                        items={noMappingItems.map(item => [
                          {
                            label: item.label,
                            id: item.id,
                            caption: getCaption(item),
                          },
                          { label: 'No mapping available' },
                        ])}
                        type="noMappings"
                        grouped
                        onClick={id =>
                          setItemToRemoveNoMapping(
                            noMappingItems.find(item => item.id === id),
                          )
                        }
                      />
                    </>
                  )}

                  <WizardStepFormActions
                    {...stepProps}
                    submitText={`Next -  ${nextStep}`}
                  />
                </Form>
              )}
            </Formik>
          </div>
        </div>

        {itemToMap && (
          <PrototypeMapFacetModal
            itemToMap={itemToMap}
            name={name}
            newItems={newItems}
            onClose={() => setItemToMap(undefined)}
            onSubmit={itemToMapToId => {
              // no mapping available
              if (!itemToMapToId) {
                onAddNoMappingItem(itemToMap, name);
              } else {
                onMapItem(itemToMapToId, itemToMap, name);
              }
              setItemToMap(undefined);
            }}
          />
        )}

        {itemToUnmap && (
          <PrototypeUnmapFacetModal
            itemToUnmap={itemToUnmap}
            name={name}
            onClose={() => setItemToUnmap(undefined)}
            onSubmit={() => {
              onUnmapItem(itemToUnmap, name);
              setItemToUnmap(undefined);
            }}
          />
        )}

        {itemToRemoveNoMapping && (
          <PrototypeRemoveNoMappingFacetModal
            item={itemToRemoveNoMapping}
            name={name}
            onClose={() => setItemToRemoveNoMapping(undefined)}
            onSubmit={() => {
              onRemoveNoMappingItem(itemToRemoveNoMapping, name);
              setItemToRemoveNoMapping(undefined);
            }}
          />
        )}
      </>
    );
  }

  return (
    <WizardStepSummary {...stepProps} goToButtonText={`Edit ${name} mappings`}>
      {stepHeading}

      <SummaryList noBorder>
        <SummaryListItem term={`Total ${namePlural}`}>
          {mappedItems.length + newItems.length}
        </SummaryListItem>
        <SummaryListItem term={`Mapped ${namePlural}`}>
          {mappedItems.length}
        </SummaryListItem>
        <SummaryListItem term={`New ${namePlural}`}>
          {newItems.length}
        </SummaryListItem>
        <SummaryListItem
          term={`${capitalize(namePlural)} with no mappings available`}
        >
          {noMappingItems.length}
        </SummaryListItem>
      </SummaryList>
    </WizardStepSummary>
  );
};

export default PrototypePrepareNextSubjectStep2;
