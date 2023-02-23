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
import isEqual from 'lodash/isEqual';
import PrototypeFacetList from './PrototypeFacetList';
import PrototypeMapFacetModal from './PrototypeMapFacetModal';
import { MapItem } from '../PrototypePrepareNextSubjectPage';
import PrototypeUnmapFacetModal from './PrototypeUnmapFacetModal';

interface Props extends InjectedWizardProps {
  mappedItems: MapItem[];
  newItems: MapItem[];
  unmappedItems: MapItem[];
  name: string;
}

const PrototypePrepareNextSubjectStep2 = ({
  mappedItems: initialMappedItems,
  newItems: initialNewItems,
  unmappedItems: initialUnmappedItems,
  name,
  ...stepProps
}: Props) => {
  const { isActive, goToNextStep } = stepProps;
  const { isMounted } = useMounted();
  const [itemToMap, setItemToMap] = useState<MapItem | undefined>(undefined);
  const [itemToUnmap, setItemToUnmap] = useState<
    [MapItem, MapItem] | undefined
  >(undefined);
  const namePlural = `${name}s`;

  const grouping = name === 'location' ? 'Local authority' : 'Characteristic';

  const getCaption = (item: MapItem) =>
    name === 'location' ? `${item.code}, ${item.region}` : item.group;

  const [unmappedItems, setUnmappedItems] = useState<MapItem[]>(
    initialUnmappedItems,
  );
  const [newItems, setNewItems] = useState<MapItem[]>(initialNewItems);
  const [mappedItems, setMappedItems] = useState<[MapItem, MapItem][]>(
    initialMappedItems.map(item => {
      return [item, item];
    }),
  );

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
                  message: `All ${namePlural} from the current subject should be mapped to the next subject`,
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
                <Form id="form" showSubmitError>
                  <FormFieldset id="downloadFiles" legend={stepHeading}>
                    <>
                      <p>
                        Existing {namePlural} in the publication subject need to
                        be mapped to an equivalent location in the new subject.
                        We try to automatically match up {namePlural} that
                        appear to be a good fit, but you should double-check
                        these choices.
                      </p>
                      <p>
                        Any facets in the new subject that have not been mapped
                        will become new {namePlural} in the publication subject.
                      </p>
                    </>
                  </FormFieldset>

                  {unmappedItems.length > 0 && (
                    <>
                      <FormFieldset
                        id="unmapped"
                        legend={
                          <>
                            Unmapped existing {namePlural} (
                            {unmappedItems.length} of{' '}
                            {mappedItems.length + unmappedItems.length})
                          </>
                        }
                        legendSize="m"
                        error={
                          form.submitCount > 0 && unmappedItems.length
                            ? `All ${namePlural} from the current subject should be mapped to the next subject`
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
                    </>
                  )}

                  <h3>
                    New {namePlural} ({newItems.length})
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
                    grouped={name !== 'indicator'}
                  />

                  {mappedItems.length > 0 && (
                    <>
                      <h3>
                        Mapped existing {namePlural} ({mappedItems.length} of{' '}
                        {mappedItems.length + unmappedItems.length})
                      </h3>

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
                        grouped={name !== 'indicator'}
                        onClick={id =>
                          setItemToUnmap(
                            mappedItems.find(item => item[0].id === id),
                          )
                        }
                      />
                    </>
                  )}

                  <WizardStepFormActions {...stepProps} />
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
              const newItem = newItems.find(item => item.id === itemToMapToId);
              if (!newItem) {
                return;
              }
              setUnmappedItems(items =>
                items.filter(item => item.id !== itemToMap.id),
              );
              setNewItems(items =>
                items.filter(item => item.id !== itemToMapToId),
              );
              setMappedItems(items => [[itemToMap, newItem], ...items]);
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
              setMappedItems(
                mappedItems.filter(item => !isEqual(item, itemToUnmap)),
              );
              setUnmappedItems(items => [itemToUnmap[0], ...items]);
              setNewItems(items => [itemToUnmap[1], ...items]);
              setItemToUnmap(undefined);
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
      </SummaryList>
    </WizardStepSummary>
  );
};

export default PrototypePrepareNextSubjectStep2;
