import { Form, FormRadioGroup, FormSelect } from '@common/components/form';
import { useMobileMedia } from '@common/hooks/useMedia';
import { PublicationSortOptions } from '@common/services/publicationService';
import { Formik } from 'formik';
import noop from 'lodash/noop';
import React from 'react';

const options = [
  { label: 'Newest', value: 'newest' },
  { label: 'Oldest', value: 'oldest' },
  { label: 'A to Z', value: 'alphabetical' },
];

interface FormValues {
  sortBy: PublicationSortOptions;
}

interface Props {
  initialValues: FormValues;
  onChange: (nextSortBy: PublicationSortOptions) => void;
}

const SortControls = ({ initialValues, onChange }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <Formik initialValues={initialValues} onSubmit={noop}>
      <Form id="sortControlsForm">
        {isMobileMedia ? (
          <FormSelect
            id="sortControls"
            inline
            name="sortBy"
            label="Sort results"
            options={options}
            order={[]}
            value={initialValues.sortBy}
            onChange={event =>
              onChange(event.target.value as PublicationSortOptions)
            }
          />
        ) : (
          <FormRadioGroup
            id="sortControls"
            inline
            legend="Sort results"
            legendSize="s"
            name="sortBy"
            options={options}
            order={[]}
            small
            value={initialValues.sortBy}
            onChange={event =>
              onChange(event.target.value as PublicationSortOptions)
            }
          />
        )}
        <button className="dfe-no-js" type="submit">
          Submit
        </button>
      </Form>
    </Formik>
  );
};

export default SortControls;
