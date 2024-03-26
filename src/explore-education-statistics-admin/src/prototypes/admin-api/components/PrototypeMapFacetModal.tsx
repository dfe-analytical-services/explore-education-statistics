import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Button from '@common/components/Button';
import {
  Form,
  FormFieldCheckbox,
  FormFieldRadioGroup,
  FormTextSearchInput,
} from '@common/components/form';
import Yup from '@common/validation/yup';
import Modal from '@common/components/Modal';
import { groupBy } from 'lodash';
import { Formik } from 'formik';
import React, { useMemo, useState } from 'react';
import ButtonText from '@common/components/ButtonText';
import { MapItem } from '../PrototypePrepareNextSubjectPage';
import styles from './PrototypeMapFacetModal.module.scss';

interface FormValues {
  selectedItem: string;
}

interface Props {
  itemToMap: MapItem;
  name: string;
  newItems: MapItem[];
  onClose: () => void;
  onSubmit: (id?: string) => void;
}

const PrototypeMapFacetModal = ({
  itemToMap,
  name,
  newItems,
  onClose,
  onSubmit,
}: Props) => {
  const [searchTerm, setSearchTerm] = useState<string>();

  const groupByKey = name === 'location' ? 'region' : 'group';

  const groupedNewItems = useMemo(() => {
    if (searchTerm) {
      return groupBy(
        newItems.filter(loc =>
          loc.label.toLowerCase().includes(searchTerm.toLowerCase()),
        ),
        groupByKey,
      );
    }

    return groupBy(newItems, groupByKey);
  }, [groupByKey, newItems, searchTerm]);

  return (
    <Modal
      className="govuk-!-width-one-half"
      open
      title={`Map existing ${name}`}
      onExit={onClose}
    >
      <Formik<FormValues>
        initialValues={{
          selectedItem: '',
        }}
        validationSchema={Yup.object({
          selectedItem: Yup.string().required(`Choose a ${name}`),
        })}
        onSubmit={({ selectedItem }) => {
          onSubmit(selectedItem);
        }}
      >
        <Form id="form">
          <div className={styles.inner}>
            <h3>Current data set {name}</h3>
            <SummaryList className="govuk-!-margin-bottom-5">
              <SummaryListItem term="Label">{itemToMap.label}</SummaryListItem>
              {itemToMap.group && (
                <SummaryListItem term="Group">
                  {itemToMap.group}
                </SummaryListItem>
              )}
              {itemToMap.filter && (
                <SummaryListItem term="Filter">
                  {itemToMap.filter}
                </SummaryListItem>
              )}
              {itemToMap.code && (
                <SummaryListItem term="Code">{itemToMap.code}</SummaryListItem>
              )}
              {itemToMap.level && (
                <SummaryListItem term="Level">
                  {itemToMap.level}
                </SummaryListItem>
              )}
              {itemToMap.region && (
                <SummaryListItem term="Region">
                  {itemToMap.region}
                </SummaryListItem>
              )}
              <SummaryListItem term="Identifier">
                {itemToMap.id}
              </SummaryListItem>
            </SummaryList>
            <h3>Next data set {name}</h3>
            <p>
              Choose a {name} that will be mapped to the current data set
              location (see above).
            </p>
            <FormTextSearchInput
              className="govuk-!-margin-bottom-4"
              id="search"
              label="Search options"
              name="search"
              onChange={event => {
                setSearchTerm(event.target.value);
              }}
              onKeyPress={event => {
                if (event.key === 'Enter') {
                  event.preventDefault();
                }
              }}
              value={searchTerm}
              width={20}
            />
            {Object.entries(groupedNewItems).map(([key, items]) => {
              return (
                <FormFieldRadioGroup
                  showError={false}
                  id="items"
                  key={key}
                  legend={key}
                  legendSize="s"
                  name="selectedItem"
                  small
                  options={items.map(item => {
                    return {
                      label: item.label,
                      value: item.id,
                    };
                  })}
                />
              );
            })}
          </div>
          <hr className="govuk-!-margin-0" />
          <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
            <FormFieldCheckbox
              small
              name="no-mapping"
              label={`No ${name} mapping available`}
              className="govuk-!-margin-top-6"
            />
            <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
              <Button
                className="govuk-!-margin-bottom-0 govuk-!-margin-top-0"
                type="submit"
              >
                Update {name} mapping
              </Button>
              <ButtonText
                className="govuk-!-margin-left-3"
                onClick={() => onClose()}
              >
                Cancel
              </ButtonText>
            </div>

            {/* 
             <Button
              className="govuk-!-margin-bottom-0 govuk-!-margin-top-4 govuk-!-margin-left-4"
              onClick={() => onSubmit()}
            >
              No mapping available (MAJOR update)
            </Button>
            */}
          </div>
        </Form>
      </Formik>
    </Modal>
  );
};

export default PrototypeMapFacetModal;
